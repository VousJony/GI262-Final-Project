using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Projectile : MonoBehaviour
{
    #region Fields
    private float _speed;
    private float _damage;
    private float _critChance;

    [SerializeField] private GameObject impactVFX;
    #endregion

    #region Public Methods
    /// <summary>
    /// ตั้งค่ากระสุนก่อนยิง (Damage, Speed, Direction)
    /// </summary>
    public void Setup(float damage, float critChance, Vector3 direction, float speed)
    {
        _damage = damage;
        _critChance = critChance;
        _speed = speed;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.linearVelocity = direction.normalized * _speed;

        // หมุนหัวกระสุนไปตามทิศทาง
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    #endregion

    #region Unity Methods
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Enemy>(out Enemy enemy))
        {
            // คำนวณ Critical Hit
            float finalDamage = _damage;
            bool isCritical = Random.Range(0f, 100f) <= _critChance;
            if (isCritical) finalDamage *= 2f;

            enemy.TakeDamage(finalDamage);

            // แสดงเอฟเฟกต์เมื่อชน
            if (impactVFX != null)
            {
                GameObject vfx = Instantiate(impactVFX, transform.position, Quaternion.identity);
                Destroy(vfx, 1f);
            }

            Destroy(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
    #endregion
}