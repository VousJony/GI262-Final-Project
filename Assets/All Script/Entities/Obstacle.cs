using UnityEngine;

// บังคับให้มี Components ที่จำเป็นเพื่อป้องกัน Human Error
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))] // หรือ Collider2D อื่นๆ
public class Obstacle : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float damage = 20f;

    [Header("Settings")]
    [Tooltip("ทำลายตัวเองเมื่อตำแหน่ง Y น้อยกว่าค่านี้")]
    [SerializeField] private float destroyYThreshold = -10f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // ตั้งค่า Physics ให้เหมาะสมกับสิ่งกีดขวางที่เคลื่อนที่ผ่านเฉยๆ
        rb.gravityScale = 0;

        // [แก้ไข] เปลี่ยนจาก rb.isKinematic = true; เป็น bodyType
        rb.bodyType = RigidbodyType2D.Kinematic;

        // ตรวจสอบ Collider
        if (TryGetComponent<Collider2D>(out Collider2D col))
        {
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        Move();
        CheckOutOfBounds();
    }

    private void Move()
    {
        // เคลื่อนที่ลงด้านล่างแบบ Smooth ตาม Time.deltaTime
        transform.Translate(Vector3.down * speed * Time.deltaTime);
    }

    private void CheckOutOfBounds()
    {
        // Simple logic ตรวจสอบตำแหน่งเพื่อ Destroy
        if (transform.position.y < destroyYThreshold)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Optimization: ใช้ TryGetComponent เพื่อความเร็วสูงสุดและลด GC
        if (collision.TryGetComponent<Character>(out Character character))
        {
            character.TakeDamage(damage);
            Destroy(gameObject); // ทำลาย Obstacle ทันทีที่ชนผู้เล่น
        }

    }
}