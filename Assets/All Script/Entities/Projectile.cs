using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Projectile : MonoBehaviour
{
    #region Fields
    private float _speed;
    private float _damage;
    private float _critChance;

    [Header("References")]
    [SerializeField] private GameObject impactVFX;

    [Header("Settings")]
    [Tooltip("อายุขัยสูงสุดของกระสุน เพื่อป้องกันบัคกระสุนค้างในฉาก")]
    [SerializeField] private float maxLifeTime = 5f;
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

        // [Fail-safe] ตั้งเวลาทำลายตัวเองล่วงหน้า รับประกันว่ากระสุนจะไม่ลอยค้างตลอดกาลแน่นอน
        Destroy(gameObject, maxLifeTime);
    }
    #endregion

    #region Unity Methods
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // กรณีชนศัตรู
        if (collision.TryGetComponent<Enemy>(out Enemy enemy))
        {
            // คำนวณ Critical Hit
            float finalDamage = _damage;
            bool isCritical = Random.Range(0f, 100f) <= _critChance;
            if (isCritical) finalDamage *= 2f;

            enemy.TakeDamage(finalDamage);
            SpawnImpactVFX();

            Destroy(gameObject);
        }
        // กรณีชนกำแพง หรือสิ่งกีดขวางอื่นๆ (สมมติว่าเซ็ต Tag เป็น "Obstacle" หรือ "Wall")
        //else if (collision.CompareTag("Obstacle") || collision.CompareTag("Wall"))
        //{
        //    SpawnImpactVFX();
        //    Destroy(gameObject);
        //}
    }

    private void OnBecameInvisible()
    {
        // ทำลายตัวเองเมื่อออกนอกจอ (จะทำงานได้ดีเมื่อเล่นจริง แต่ตอนรันใน Editor ระวังกล้อง Scene View มองเห็นอยู่)
        Destroy(gameObject);
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// แยก Logic การสร้าง VFX ออกมาตามหลัก Single Responsibility Principle (SRP)
    /// </summary>
    private void SpawnImpactVFX()
    {
        if (impactVFX != null)
        {
            GameObject vfx = Instantiate(impactVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 1f); // ทำลาย VFX ทิ้งหลังจาก 1 วินาที
        }
    }
    #endregion
}