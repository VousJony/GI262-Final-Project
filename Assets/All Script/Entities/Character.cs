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

    [Header("Attack")]
    //[SerializeField] private Equipment equipment = 1;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 1f;

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
    
    private void SetUpNewGame()
    {
        SetScoreText(score);
        UpdateExpBar();
        SetLevelText();
        ResetHp();
    }
    #region Score Management
    private void SetScoreText(float score)
    {
        scoreText.text = score.ToString();
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
        // Handle character death (e.g., game over)
        Debug.Log("Character has died.");
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
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
        if (context.started)
        {
            Vector2 input = context.ReadValue<Vector2>();
            float moveDirection = 0f;
            if (input.x > 0 && transform.position.x < 5f)
            {
                moveDirection = 1f;
            }
            else if (input.x < 0 && transform.position.x > -5f)
            {
                moveDirection = -1f;
            }

            transform.position = new Vector2(transform.position.x + moveDirection, transform.position.y);
        }

    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.started && bulletPrefab != null)
        {
            GameObject newBullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Destroy(newBullet, 2f);
        }
    }
    #endregion  

}
