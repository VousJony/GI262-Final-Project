using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    #region Singleton
    public static Character instance;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        InitializeCharacter();
    }
    #endregion

    #region Fields
    private const float GOLDEN_RATIO = 1.618034f;

    [Header("Base Status")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private Status baseStatus;
    private Status _initialBaseStatus;
    private float currentHealth;

    [Header("Equipment & Combat")]
    [SerializeField] private EquipmentData currentEquipment;
    [SerializeField] private GameObject defaultBulletPrefab;
    [SerializeField] private Transform firePoint;

    // ตัวแปรสำหรับระบบอาวุธชั่วคราว
    private EquipmentData _startingEquipment;
    private float _equipmentDurationTimer;
    private float _equipmentMaxDuration;
    private bool _isUsingTemporaryWeapon = false;
    private float _nextFireTime = 0f;

    [Header("Movement")]
    [SerializeField] private float xLimit = 5f;
    private Vector2 _moveInput;
    private Vector3 _startPosition;
    private Vector3 _initialScale;
    private Camera _mainCam;

    [Header("UI References")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider expBar;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image weaponIconUI;
    [SerializeField] private Image weaponCooldownUI;

    [Header("Audio & VFX")]
    [SerializeField] private AudioClip defaultShootSFX;
    [SerializeField] private AudioClip hitSFX;
    [SerializeField] private AudioClip levelUpSFX;
    [SerializeField] private AudioClip gameOverSFX;
    [SerializeField] private ParticleSystem levelUpVFX;

    // Runtime Data
    [SerializeField] private Status finalStatus;
    private int score = 0;
    private int level = 1;
    private int exp = 0;
    private int expToNextLevel = 100;
    #endregion

    #region Unity Methods
    private void Update()
    {
        // หยุดการทำงานถ้าเกมไม่ได้กำลังเล่นอยู่
        if (MenuManager.instance != null && MenuManager.instance.currentState != GameState.Playing)
        {
            if (AudioManager.instance != null) AudioManager.instance.StopWeaponLoop();
            return;
        }

        HandleMovement();
        HandleShooting();
        HandleEquipmentDuration();
    }
    #endregion

    #region Initialization & Setup
    private void InitializeCharacter()
    {
        _mainCam = Camera.main;
        _startPosition = transform.position;
        _initialScale = transform.localScale;
        _startingEquipment = currentEquipment;

        // เก็บค่า Status เริ่มต้นไว้สำหรับ Reset เกมใหม่
        _initialBaseStatus = new Status();
        CopyStatus(baseStatus, _initialBaseStatus);

        CalculateStats();
        EquipWeapon(_startingEquipment);
    }

    /// <summary>
    /// รีเซ็ตค่าทั้งหมดเพื่อเริ่มเกมใหม่ (Score, Level, HP, Position)
    /// </summary>
    public void SetUpNewGame()
    {
        score = 0;
        level = 1;
        exp = 0;
        expToNextLevel = 100;
        transform.position = _startPosition;
        transform.localScale = _initialScale;

        if (AudioManager.instance != null) AudioManager.instance.StopWeaponLoop();

        CopyStatus(_initialBaseStatus, baseStatus);
        EquipWeapon(_startingEquipment);

        SetScoreText(score);
        UpdateExpBar();
        SetLevelText();
        ResetHp();
    }
    #endregion

    #region Movement Logic
    /// <summary>
    /// รับค่า Input จากระบบ New Input System
    /// </summary>
    public void Move(InputAction.CallbackContext context)
    {
        if (MenuManager.instance.currentState != GameState.Playing) return;

        if (context.performed) _moveInput = context.ReadValue<Vector2>();
        else if (context.canceled) _moveInput = Vector2.zero;
    }

    private void HandleMovement()
    {
        // ตรวจสอบการสัมผัสหน้าจอ (Touch/Mouse)
        bool isPointerActive = Pointer.current != null && Pointer.current.press.isPressed;

        if (isPointerActive)
        {
            // เคลื่อนที่ตามนิ้วที่แตะ
            Vector2 pointerScreenPos = Pointer.current.position.ReadValue();
            Vector3 targetPos = _mainCam.ScreenToWorldPoint(pointerScreenPos);
            float targetX = Mathf.Clamp(targetPos.x, -xLimit, xLimit);

            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(targetX, transform.position.y, 0),
                finalStatus.moveSpeed * 2f * Time.deltaTime
            );
        }
        else if (_moveInput.x != 0)
        {
            // เคลื่อนที่ด้วยคีย์บอร์ด/จอย
            float movement = (_moveInput.x > 0 ? 1f : -1f) * finalStatus.moveSpeed * Time.deltaTime;
            float newX = Mathf.Clamp(transform.position.x + movement, -xLimit, xLimit);
            transform.position = new Vector3(newX, transform.position.y, 0);
        }
    }
    #endregion

    #region Combat & Equipment Logic
    private void HandleShooting()
    {
        // ยิงเฉพาะเมื่อมีศัตรูอยู่ในฉาก
        bool hasTargets = (EnemyManager.instance != null && EnemyManager.instance.transform.childCount > 0);
        bool isLooping = currentEquipment != null && currentEquipment.isLoopingSFX;

        // จัดการเสียงปืนแบบ Loop (เช่น ปืนกล)
        if (isLooping)
        {
            if (hasTargets)
            {
                AudioClip clipToUse = (currentEquipment.shootSFXOverride != null) ? currentEquipment.shootSFXOverride : defaultShootSFX;
                if (AudioManager.instance != null) AudioManager.instance.PlayWeaponLoop(clipToUse);
            }
            else
            {
                if (AudioManager.instance != null) AudioManager.instance.StopWeaponLoop();
                return;
            }
        }
        else if (!hasTargets) return;

        // คำนวณเวลายิงนัดถัดไปตาม RPM
        if (Time.time >= _nextFireTime)
        {
            Shoot();
            float rpm = Mathf.Max(finalStatus.rpm, 1f);
            _nextFireTime = Time.time + (60f / rpm);
        }
    }

    private void Shoot()
    {
        GameObject bulletToSpawn = (currentEquipment != null && currentEquipment.bulletPrefabOverride != null)
            ? currentEquipment.bulletPrefabOverride
            : defaultBulletPrefab;

        if (bulletToSpawn == null) return;

        // เล่นเสียงยิงแบบ OneShot (สำหรับปืนที่ไม่ใช่ Loop)
        if (currentEquipment == null || !currentEquipment.isLoopingSFX)
        {
            AudioClip clipToUse = (currentEquipment != null && currentEquipment.shootSFXOverride != null)
                ? currentEquipment.shootSFXOverride
                : defaultShootSFX;
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX(clipToUse);
        }

        // แสดงเอฟเฟกต์ประกายไฟปากกระบอกปืน
        if (currentEquipment != null && currentEquipment.muzzleFlashVFX != null)
        {
            Transform spawnPoint = firePoint != null ? firePoint : transform;
            GameObject vfx = Instantiate(currentEquipment.muzzleFlashVFX, spawnPoint.position, spawnPoint.rotation, spawnPoint);
            Destroy(vfx, 1f);
        }

        // คำนวณการกระจายตัวของกระสุน (Spread)
        int projectileCount = (currentEquipment != null) ? currentEquipment.projectilesPerShot : 1;
        float spreadAngle = finalStatus.angle;
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        for (int i = 0; i < projectileCount; i++)
        {
            float currentAngle = 0f;
            if (projectileCount > 1)
            {
                // กระจายมุมเท่าๆ กัน
                float step = spreadAngle / (projectileCount - 1);
                currentAngle = (-spreadAngle / 2f) + (step * i);
            }
            else if (spreadAngle > 0)
            {
                // สุ่มมุมภายใน range (สำหรับปืนลูกซองนัดเดียว)
                currentAngle = Random.Range(-spreadAngle / 2f, spreadAngle / 2f);
            }

            Vector3 direction = Quaternion.Euler(0, 0, currentAngle) * Vector3.up;
            GameObject bulletObj = Instantiate(bulletToSpawn, spawnPos, Quaternion.identity);

            if (bulletObj.TryGetComponent<Projectile>(out Projectile p))
            {
                float speed = (finalStatus.projectileSpeed > 0) ? finalStatus.projectileSpeed : 10f;
                p.Setup(finalStatus.attack, finalStatus.critChance, direction, speed);
            }
        }
    }

    /// <summary>
    /// สวมใส่อาวุธใหม่ และตั้งค่า Timer ถ้าเป็นอาวุธชั่วคราว
    /// </summary>
    public void EquipWeapon(EquipmentData newWeapon)
    {
        currentEquipment = newWeapon;
        if (AudioManager.instance != null) AudioManager.instance.StopWeaponLoop();

        bool isStartingWeapon = (newWeapon == _startingEquipment);
        bool isInfiniteData = (currentEquipment != null && currentEquipment.duration <= 0);

        if (isStartingWeapon || isInfiniteData)
        {
            _isUsingTemporaryWeapon = false;
            _equipmentDurationTimer = 0;
        }
        else
        {
            _isUsingTemporaryWeapon = true;
            _equipmentMaxDuration = currentEquipment.duration;
            _equipmentDurationTimer = _equipmentMaxDuration;
        }

        CalculateStats();
        UpdateWeaponUI();
    }

    private void HandleEquipmentDuration()
    {
        if (_isUsingTemporaryWeapon)
        {
            _equipmentDurationTimer -= Time.deltaTime;

            // อัปเดตหลอด Cooldown UI
            if (weaponCooldownUI != null)
                weaponCooldownUI.fillAmount = _equipmentDurationTimer / _equipmentMaxDuration;

            // หมดเวลา คืนอาวุธเริ่มต้น
            if (_equipmentDurationTimer <= 0)
                EquipWeapon(_startingEquipment);
        }
    }
    #endregion

    #region Stats & UI Updates
    /// <summary>
    /// คำนวณค่า Status รวม (Base + Equipment)
    /// </summary>
    private void CalculateStats()
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
            finalStatus += currentEquipment.GetStatusBonus();

        if (statusText != null)
            statusText.text = $"ATK: {finalStatus.attack:F0}\nDEF: {finalStatus.defense:F0}\nCRIT: {finalStatus.critChance:F1}%\n";
    }

    private void UpdateWeaponUI()
    {
        if (weaponIconUI != null)
        {
            bool hasIcon = currentEquipment != null && currentEquipment.icon != null;
            weaponIconUI.sprite = hasIcon ? currentEquipment.icon : null;
            weaponIconUI.enabled = hasIcon;
            weaponIconUI.color = Color.white;
            weaponIconUI.preserveAspect = true;
        }

        if (weaponCooldownUI != null)
        {
            weaponCooldownUI.enabled = _isUsingTemporaryWeapon;
            weaponCooldownUI.fillAmount = _isUsingTemporaryWeapon ? 1f : 0f;
        }
    }

    private void CopyStatus(Status source, Status destination)
    {
        destination.attack = source.attack;
        destination.defense = source.defense;
        destination.critChance = source.critChance;
        destination.rpm = source.rpm;
        destination.moveSpeed = source.moveSpeed;
        destination.angle = source.angle;
        destination.projectileSpeed = source.projectileSpeed;
    }
    #endregion

    #region Health, Score & Exp
    public void TakeDamage(float damage)
    {
        if (MenuManager.instance.currentState != GameState.Playing) return;

        float damageTaken = Mathf.Max(damage - finalStatus.defense, 1f);
        currentHealth -= damageTaken;
        UpdateHpBar();

        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(hitSFX);

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlaySFX(gameOverSFX);
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

    public void AddScore(int amount)
    {
        score += amount;
        SetScoreText(score);
    }

    public int GetScore() => score;

    private void SetScoreText(int val)
    {
        if (scoreText != null) scoreText.text = "Score\n" + val;
    }

    public void AddExp(int amount)
    {
        exp += amount;

        // เช็ค Level Up
        if (exp >= expToNextLevel)
        {
            level++;
            exp -= expToNextLevel;

            // เพิ่มเพดาน EXP ด้วย Golden Ratio
            expToNextLevel = Mathf.RoundToInt(expToNextLevel * GOLDEN_RATIO);

            // เพิ่ม Stat พื้นฐานเมื่อเลเวลอัป
            baseStatus.attack += 5 * GOLDEN_RATIO;
            baseStatus.defense += 1 * GOLDEN_RATIO;

            CalculateStats();
            SetLevelText();

            if (AudioManager.instance != null) AudioManager.instance.PlaySFX(levelUpSFX);
            if (levelUpVFX != null) levelUpVFX.Play();
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
    #endregion
}