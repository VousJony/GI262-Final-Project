using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class EquipmentPickup : MonoBehaviour
{
    #region Fields
    [Header("Item Data")]
    [SerializeField] private EquipmentData equipmentData;

    [Header("Movement")]
    [SerializeField] private float rotateSpeed = 50f;
    [SerializeField] private float fallSpeed = 3f;

    [Header("Effects")]
    [SerializeField] private GameObject pickupVFX;
    [SerializeField] private AudioClip pickupSFX;

    private SpriteRenderer _spriteRenderer;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void Start()
    {
        if (equipmentData != null) Setup(equipmentData);
    }

    private void Update()
    {
        // หมุนและตกลงมาเรื่อยๆ
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        // ทำลายทิ้งเมื่อตกเลยขอบจอ
        if (transform.position.y < -10f) Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Character>(out Character character))
        {
            if (equipmentData != null)
            {
                character.EquipWeapon(equipmentData);
                PlayPickupEffects();
                Destroy(gameObject);
            }
        }
    }
    #endregion

    #region Public Methods & Helper
    /// <summary>
    /// ตั้งค่าไอเทม (ใช้กรณี Spawn แบบ Runtime)
    /// </summary>
    public void Setup(EquipmentData data)
    {
        equipmentData = data;
        if (_spriteRenderer != null && data.icon != null)
        {
            _spriteRenderer.sprite = data.icon;
            transform.localScale = Vector3.one * 0.8f;
        }
    }

    private void PlayPickupEffects()
    {
        if (AudioManager.instance != null && pickupSFX != null)
            AudioManager.instance.PlaySFX(pickupSFX);

        if (pickupVFX != null)
        {
            GameObject vfx = Instantiate(pickupVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 1.5f);
        }
    }
    #endregion
}