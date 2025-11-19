using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Projectile : MonoBehaviour
{
    private float _speed;

    private float _damage;
    private float _critChance;

    private Rigidbody2D rb;

    public void Setup(float damage, float critChance, Vector3 direction, float speed)
    {
        _damage = damage;
        _critChance = critChance;
        _speed = speed;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();

        rb.gravityScale = 0;

        // ใช้ค่า speed ที่รับมา
        rb.linearVelocity = direction.normalized * _speed;

        // หมุนกระสุนไปตามทิศทาง
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Enemy>(out Enemy enemy))
        {
            float finalDamage = _damage;
            // คำนวณ Critical
            bool isCritical = Random.Range(0f, 100f) <= _critChance;

            if (isCritical)
            {
                finalDamage *= 2f;
                //Debug.Log("Critical Hit!");
            }

            enemy.TakeDamage(finalDamage);
            Destroy(gameObject);
        }

    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}