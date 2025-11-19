using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    public static Character instance;

    [Header("Base Status")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private Status baseStatus; // ค่าพลังพื้นฐาน

    [Header("Equipment")]
    [SerializeField] private EquipmentData currentEquipment;

    [Header("Current Real-time Status")]
    [SerializeField] private Status finalStatus; // ค่าพลังสุทธิ

    private float currentHealth;

    [Header("UI References")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider expBar;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;
    [Tooltip("Image UI สำหรับแสดงไอคอนอาวุธปัจจุบัน")]
    [SerializeField] private Image weaponIconUI; // [เพิ่ม] ช่องใส่ UI Icon

    [Header("Movement Settings")]
    [SerializeField] private float xLimit = 5f;
    private Vector2 _moveInput;
    private Vector3 _startPosition;

    [Header("Combat Settings")]
    [SerializeField] private GameObject defaultBulletPrefab;
    private bool _isShooting = false;
    private float _nextFireTime = 0f;

    private int score = 0;
    private int level = 1;
    private int exp = 0;
    private int expToNextLevel = 100;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        _startPosition = transform.position;
        CalculateStats();
        UpdateWeaponUI(); // [เพิ่ม] อัปเดต UI ตั้งแต่เริ่ม
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (MenuManager.instance.currentState != GameState.Playing) return;

        HandleMovement();
        HandleShooting();
    }

    public void CalculateStats()
    {
        finalStatus = new Status
        {
            attack = baseStatus.attack,
            defense = baseStatus.defense,
            critChance = baseStatus.critChance,
            rpm = baseStatus.rpm,
            moveSpeed = baseStatus.moveSpeed,
            angle = baseStatus.angle,
            projectileSpeed = baseStatus.projectileSpeed
        };

        if (currentEquipment != null)
        {
            finalStatus += currentEquipment.GetStatusBonus();
        }
    }

    public void EquipWeapon(EquipmentData newWeapon)
    {
        currentEquipment = newWeapon;
        CalculateStats();
        UpdateWeaponUI(); // [เพิ่ม] อัปเดต UI เมื่อเปลี่ยนอาวุธ
    }

    // [เพิ่ม] ฟังก์ชันจัดการ UI อาวุธ
    private void UpdateWeaponUI()
    {
        if (weaponIconUI != null)
        {
            if (currentEquipment != null && currentEquipment.icon != null)
            {
                weaponIconUI.sprite = currentEquipment.icon;
                weaponIconUI.color = Color.white; // ทำให้มองเห็น (เผื่อถูกตั้งค่า Alpha ไว้)
                weaponIconUI.enabled = true;

                // Optional: รักษาอัตราส่วนภาพไม่ให้เบี้ยว
                weaponIconUI.preserveAspect = true;
            }
            else
            {
                // ถ้าไม่มีอาวุธ หรือไม่มีไอคอน ให้ซ่อน Image ไปเลย
                weaponIconUI.enabled = false;
            }
        }
    }

    public void SetUpNewGame()
    {
        score = 0;
        level = 1;
        exp = 0;
        expToNextLevel = 100;
        transform.position = _startPosition;

        CalculateStats();
        UpdateWeaponUI(); // [เพิ่ม] รีเซ็ต UI ตอนเริ่มเกมใหม่

        SetScoreText(score);
        UpdateExpBar();
        SetLevelText();
        ResetHp();
    }

    // --- Combat Logic ---

    private void HandleShooting()
    {
        if (_isShooting && Time.time >= _nextFireTime)
        {
            Shoot();

            float rpm = Mathf.Max(finalStatus.rpm, 1f);
            float fireDelay = 60f / rpm;

            _nextFireTime = Time.time + fireDelay;
        }
    }

    private void Shoot()
    {
        // 1. เลือก Prefab (รูปร่างกระสุน)
        GameObject bulletToSpawn = defaultBulletPrefab;
        if (currentEquipment != null && currentEquipment.bulletPrefabOverride != null)
        {
            bulletToSpawn = currentEquipment.bulletPrefabOverride;
        }

        if (bulletToSpawn == null) return;

        int projectileCount = (currentEquipment != null) ? currentEquipment.projectilesPerShot : 1;
        float spreadAngle = finalStatus.angle;

        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = 0f;

            if (projectileCount > 1)
            {
                float startAngle = -spreadAngle / 2f;
                float step = spreadAngle / (projectileCount - 1);
                currentAngle = startAngle + (step * i);
            }
            else if (spreadAngle > 0)
            {
                currentAngle = Random.Range(-spreadAngle / 2f, spreadAngle / 2f);
            }

            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            Vector3 direction = rotation * Vector3.up;

            GameObject bulletObj = Instantiate(bulletToSpawn, transform.position, Quaternion.identity);

            if (bulletObj.TryGetComponent<Projectile>(out Projectile p))
            {
                float speed = (finalStatus.projectileSpeed > 0) ? finalStatus.projectileSpeed : 10f;
                p.Setup(finalStatus.attack, finalStatus.critChance, direction, speed);
            }
        }
    }

    // --- Movement Logic ---

    public void Move(InputAction.CallbackContext context)
    {
        if (MenuManager.instance.currentState != GameState.Playing) return;
        if (context.performed) _moveInput = context.ReadValue<Vector2>();
        else if (context.canceled) _moveInput = Vector2.zero;
    }

    private void HandleMovement()
    {
        if (_moveInput.x != 0)
        {
            float moveDirection = (_moveInput.x > 0) ? 1f : -1f;
            float movement = moveDirection * finalStatus.moveSpeed * Time.deltaTime;
            float newX = Mathf.Clamp(transform.position.x + movement, -xLimit, xLimit);
            transform.position = new Vector3(newX, transform.position.y, 0);
        }
    }

    // --- Stats Management ---

    public void TakeDamage(float damage)
    {
        if (MenuManager.instance.currentState != GameState.Playing) return;

        float damageTaken = Mathf.Max(damage - finalStatus.defense, 1f);

        currentHealth -= damageTaken;
        UpdateHpBar();
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        MenuManager.instance.TriggerGameOver();
    }

    public void ResetHp()
    {
        currentHealth = maxHealth;
        UpdateHpBar();
    }

    private void UpdateHpBar()
    {
        if (hpBar != null) hpBar.value = currentHealth / maxHealth;
    }

    public int GetScore() => score;

    public void AddScore(int amount)
    {
        score += amount;
        SetScoreText(score);
    }

    private void SetScoreText(int val)
    {
        if (scoreText != null) scoreText.text = "Score\n" + val;
    }

    public void AddExp(int amount)
    {
        exp += amount;
        if (exp >= expToNextLevel)
        {
            level++;
            exp -= expToNextLevel;
            expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.5f);

            baseStatus.attack += 5;
            CalculateStats();

            SetLevelText();
        }
        UpdateExpBar();
    }

    private void UpdateExpBar()
    {
        if (expBar != null) expBar.value = (float)exp / expToNextLevel;
    }

    private void SetLevelText()
    {
        if (levelText != null) levelText.text = "Lvl: " + level;
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (MenuManager.instance.currentState != GameState.Playing) return;
        if (context.performed) _isShooting = true;
        else if (context.canceled) _isShooting = false;
    }
}