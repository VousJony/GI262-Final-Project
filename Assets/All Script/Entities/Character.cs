using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    public static Character instance;

    [Header("HP")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float health;
    [SerializeField] private Slider hpBar;

    [Header("Score")]
    [SerializeField] private int score = 0;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Level & Exp")]
    [SerializeField] private int exp = 0;
    [SerializeField] private int expToNextLevel = 100;
    [SerializeField] private Slider expBar;
    [SerializeField] private int level = 1;
    [SerializeField] private TextMeshProUGUI levelText;

    //[SerializeField] private Status status = 1;

    [Header("Movement")]
    [SerializeField] private float xLimit = 5f;
    [SerializeField] private float moveDelay = 0.2f;
    private Vector2 _moveInput;
    private float _lastMoveTime;

    [Header("Attack")]
    //[SerializeField] private Equipment equipment = 1;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 1f;
    private bool _isShooting = false;
    private float _lastShotTime;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetUpNewGame();
    }

    private void Update()
    {
        HandleContinuousMovement();
        HandleContinuousShooting();
    }

    private void SetUpNewGame()
    {
        SetScoreText(score);
        UpdateExpBar();
        SetLevelText();
        ResetHp();
    }

    #region Score Management
    private void SetScoreText(int score)
    {
        scoreText.text = "Score\n" + score.ToString();
    }

    public void AddScore(int amount)
    {
        score += amount;
        SetScoreText(score);
    }
    #endregion

    #region Level & Exp Management
    public void SetLevelText()
    {
        levelText.text = "Lvl: " + level.ToString();
    }

    private void UpdateExpBar()
    {
        expBar.value = (float)exp / expToNextLevel;
    }

    public void AddExp(int amount)
    {
        exp += amount;
        if (exp >= expToNextLevel)
        {
            level++;
            exp -= expToNextLevel;
            expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.5f);
            SetLevelText();
        }
        UpdateExpBar();
    }
    #endregion

    #region HP Management
    private void Die()
    {
        Debug.Log("Character has died.");
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        UpdateHpBar();
        if (health <= 0)
        {
            Die();
        }
    }

    public void ResetHp()
    {
        health = maxHealth;
        UpdateHpBar();
    }

    public void UpdateHpBar()
    {
        if (hpBar != null)
        {
            hpBar.value = health / maxHealth;
        }
    }
    #endregion

    #region Actions

    public void Move(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _moveInput = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            _moveInput = Vector2.zero;
        }
    }

    private void HandleContinuousMovement()
    {
        // ตรวจสอบว่ามีการอินพุตการเคลื่อนที่ และเวลาหน่วง (Cooldown) ได้ผ่านไปแล้ว
        if (_moveInput.x != 0 && Time.time >= _lastMoveTime + moveDelay)
        {
            float moveDirection = 0f;

            if (_moveInput.x > 0)
            {
                moveDirection = 1f;
            }
            else if (_moveInput.x < 0)
            {
                moveDirection = -1f;
            }

            // คำนวณตำแหน่งใหม่แบบ "ทันที 1 หน่วย"
            float newXPosition = transform.position.x + moveDirection;

            // จำกัดตำแหน่ง
            newXPosition = Mathf.Clamp(newXPosition, -xLimit, xLimit);

            // กำหนดตำแหน่งใหม่ให้วัตถุ
            transform.position = new Vector2(newXPosition, transform.position.y);

            // บันทึกเวลาที่ทำการก้าวครั้งล่าสุด เพื่อเริ่มนับเวลาหน่วงใหม่
            _lastMoveTime = Time.time;
        }
    }

    private void InstantiateBullet()
    {
        GameObject newBullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // ทำลายกระสุนหลังจาก 2 วินาที
        Destroy(newBullet, 2f);
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isShooting = true;
        }
        else if (context.canceled)
        {
            _isShooting = false;
        }
    }

    private void HandleContinuousShooting()
    {
        if (_isShooting && bulletPrefab != null && Time.time >= _lastShotTime + (1f / fireRate))
        {
            InstantiateBullet();

            _lastShotTime = Time.time;
        }
    }

    #endregion

}