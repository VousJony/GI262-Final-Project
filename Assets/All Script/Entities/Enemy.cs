using UnityEngine;

[System.Serializable]
public class LootData
{
    [Tooltip("Prefab ของที่จะดรอป")] public GameObject itemPrefab;
    [Tooltip("โอกาสออก (Weight)")] public float weight = 10f;
}

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Enemy : MonoBehaviour
{
    #region Fields
    [Header("HP & Status")]
    [SerializeField] private float maxHealth = 100;
    private float _currentHealth;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float dodgeSpeed = 4f;
    [SerializeField] private float xBoundary = 2.5f;

    [Header("AI Obstacle Avoidance")]
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private float detectionWidth = 0.8f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Attack & Rewards")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private int score = 10;
    [SerializeField] private int dropExp = 20;

    [Header("Drop System")]
    [Range(0, 100)]
    [SerializeField] private float dropChance = 30f;
    [SerializeField] private LootData[] lootTable;

    [Header("VFX & SFX")]
    [SerializeField] private GameObject deathVFX;
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private AudioClip deathSFX;
    [SerializeField] private AudioClip hitSFX;

    // Components & Cache
    private Transform _transform;
    private Vector2 _detectionBoxSize;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        GetComponent<Rigidbody2D>().gravityScale = 0;
        GetComponent<Collider2D>().isTrigger = true;
        _transform = transform;
    }

    private void Start()
    {
        _currentHealth = maxHealth;
        _detectionBoxSize = new Vector2(detectionWidth, 0.5f);
    }

    private void Update()
    {
        if (MenuManager.instance != null && MenuManager.instance.currentState != GameState.Playing) return;

        HandleMovementAndAvoidance();
        CheckOutOfBounds();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Character>(out Character character))
        {
            character.TakeDamage(attackDamage);
            Die(true); // ชนผู้เล่นแล้วตายทันที
        }
    }
    #endregion

    #region Core Logic
    /// <summary>
    /// คำนวณการเคลื่อนที่และหลบสิ่งกีดขวาง
    /// </summary>
    private void HandleMovementAndAvoidance()
    {
        float deltaTime = Time.deltaTime;
        Vector3 currentPos = _transform.position;
        float newY = currentPos.y - (moveSpeed * deltaTime);
        float moveX = currentPos.x;

        // ยิง Raycast เช็คสิ่งกีดขวางด้านล่าง
        RaycastHit2D hit = Physics2D.BoxCast(currentPos, _detectionBoxSize, 0, Vector2.down, detectionRange, obstacleLayer);

        if (hit.collider != null)
        {
            // ถ้าเจอสิ่งกีดขวาง ให้หลบไปทางตรงข้าม
            float directionToDodge = (hit.transform.position.x > currentPos.x) ? -1f : 1f;
            float potentialTargetX = moveX + (directionToDodge * dodgeSpeed * deltaTime);

            // ตรวจสอบขอบจอ
            if (potentialTargetX >= -xBoundary && potentialTargetX <= xBoundary)
            {
                moveX = potentialTargetX;
            }
        }

        _transform.position = new Vector3(moveX, newY, 0);
    }

    private void CheckOutOfBounds()
    {
        // ถ้าหลุดขอบจอล่าง (เลยตัวผู้เล่นไป)
        float deadZoneY = (Character.instance != null) ? Character.instance.transform.position.y : -6f;

        if (_transform.position.y < deadZoneY)
        {
            // ถ้าผู้เล่นยังอยู่ ให้ทำดาเมจใส่ผู้เล่นด้วย
            if (Character.instance != null) Character.instance.TakeDamage(attackDamage);
            Die(false); // ตายแบบไม่มีเอฟเฟกต์/ของรางวัล
        }
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;

        if (AudioManager.instance != null && hitSFX != null)
            AudioManager.instance.PlaySFX(hitSFX);

        if (hitVFX != null)
        {
            GameObject vfx = Instantiate(hitVFX, _transform.position, Quaternion.identity);
            Destroy(vfx, 1f);
        }

        if (_currentHealth <= 0) Defeat();
    }

    /// <summary>
    /// จัดการเมื่อศัตรูตายจากการถูกยิง (ได้คะแนน/ของ)
    /// </summary>
    private void Defeat()
    {
        if (Character.instance != null)
        {
            Character.instance.AddScore(score);
            Character.instance.AddExp(dropExp);
        }

        TryDropItem();
        Die(true);
    }

    /// <summary>
    /// ทำลาย Object และเล่นเอฟเฟกต์
    /// </summary>
    private void Die(bool playEffect)
    {
        if (playEffect)
        {
            if (AudioManager.instance != null && deathSFX != null)
                AudioManager.instance.PlaySFX(deathSFX);

            if (deathVFX != null)
            {
                GameObject vfx = Instantiate(deathVFX, _transform.position, Quaternion.identity);
                Destroy(vfx, 2f);
            }
        }

        if (GameManager.instance != null)
            GameManager.instance.UpdateWaveProgress();

        Destroy(gameObject);
    }
    #endregion

    #region Drop System
    /// <summary>
    /// คำนวณสุ่มของดรอปตาม Weight
    /// </summary>
    private void TryDropItem()
    {
        if (Random.value > (dropChance / 100f)) return;

        GameObject itemToSpawn = GetRandomItemFromTable();
        if (itemToSpawn != null)
        {
            Instantiate(itemToSpawn, _transform.position, Quaternion.identity);
        }
    }

    private GameObject GetRandomItemFromTable()
    {
        if (lootTable == null || lootTable.Length == 0) return null;

        // คำนวณน้ำหนักรวม
        float totalWeight = 0f;
        foreach (var loot in lootTable) totalWeight += loot.weight;

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeightSum = 0f;

        // หาไอเทมที่ตกอยู่ในช่วง Random
        foreach (var loot in lootTable)
        {
            currentWeightSum += loot.weight;
            if (randomValue <= currentWeightSum) return loot.itemPrefab;
        }

        return lootTable[0].itemPrefab;
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // วาดเส้น Debug BoxCast
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.down * (detectionRange / 2), new Vector3(detectionWidth, detectionRange, 0));
    }
#endif
}