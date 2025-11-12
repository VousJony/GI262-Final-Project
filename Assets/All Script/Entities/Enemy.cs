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
        Character.instance.AddScore(score);
        Character.instance.AddExp(dropExp);
        GameManager.instance.UpdateWaveProgress();
        Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Character character = collision.GetComponent<Character>();
        if (character != null)
        {
            character.TakeDamage(attackDamage);
            Die();
        }

    }

    private IEnumerator Move()
    {
        while (transform.position.y > -5f)
        {
            transform.position = new Vector2(transform.position.x,transform.position.y - moveHowFar);
            yield return new WaitForSeconds(moveDelay);
        }

    }

}
