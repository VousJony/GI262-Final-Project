using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Enemy : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float health;

    [Header("Movement")]
    [SerializeField] private float moveHowFar = 1f;
    [SerializeField] private float moveDelay = 1f;

    [Header("Attack")]
    [SerializeField] private float attackDamage = 10f;

    [Header("Drop")]
    [SerializeField] private int score = 10;
    [SerializeField] private int dropExp = 20;

    [Header("Drop Config (For Boss)")]
    [Tooltip("โอกาสดรอป (0-100%)")]
    [Range(0, 100)]
    [SerializeField] private float dropChance = 100f;

    [Tooltip("Prefab ของ Item ที่จะดรอป (ใส่ EquipmentData ไว้ใน Prefab นั้นเลย)")]
    [SerializeField] private GameObject pickupPrefab;

    private Rigidbody2D rb;
    private Collider2D col;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 0;
        col.isTrigger = true;

        ResetHp();

        StartCoroutine(Move());
    }

    public void ResetHp()
    {
        health = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Defeat();
        }
    }

    private void Defeat()
    {
        if (Character.instance != null)
        {
            Character.instance.AddScore(score);
            Character.instance.AddExp(dropExp);
        }

        TryDropItem();

        Die();
    }

    private void TryDropItem()
    {
        // เช็คแค่ Prefab อย่างเดียว
        if (pickupPrefab != null)
        {
            float randomValue = Random.Range(0f, 100f);
            if (randomValue <= dropChance)
            {
                // สร้างของตกที่ตำแหน่งศัตรู (Prefab จัดการตัวเองต่อใน Start)
                Instantiate(pickupPrefab, transform.position, Quaternion.identity);
            }
        }
    }

    private void Die()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.UpdateWaveProgress();
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Character>(out Character character))
        {
            character.TakeDamage(attackDamage);
            Die();
        }
    }

    private IEnumerator Move()
    {
        WaitForSeconds wait = new WaitForSeconds(moveDelay);

        while (transform.position.y > -6f)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y - moveHowFar);
            yield return wait;
        }

        Die();
    }
}