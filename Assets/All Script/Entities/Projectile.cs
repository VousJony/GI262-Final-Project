using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float damage = 25f;
    [SerializeField] private float speed = 10f;

    private Rigidbody2D rb;
    private Collider2D col;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 0;
        col.isTrigger = true;

        Move();
    }

    private void Move()
    {
        rb.linearVelocity = transform.up * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        Destroy(gameObject);
    }

}
