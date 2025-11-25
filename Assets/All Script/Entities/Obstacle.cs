using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Obstacle : MonoBehaviour
{
    #region Fields
    [Header("Status")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private float destroyYThreshold = -10f;

    [Header("VFX & SFX")]
    [SerializeField] private GameObject crashVFX;
    [SerializeField] private AudioClip crashSFX;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Update()
    {
        // เคลื่อนที่ลงและทำลายเมื่อหลุดจอ
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        if (transform.position.y < destroyYThreshold)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Character>(out Character character))
        {
            character.TakeDamage(damage);

            if (AudioManager.instance != null && crashSFX != null)
                AudioManager.instance.PlaySFX(crashSFX);

            if (crashVFX != null)
            {
                GameObject vfx = Instantiate(crashVFX, transform.position, Quaternion.identity);
                Destroy(vfx, 1.5f);
            }

            Destroy(gameObject);
        }
    }
    #endregion
}