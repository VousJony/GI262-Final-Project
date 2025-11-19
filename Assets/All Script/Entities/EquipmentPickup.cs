using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class EquipmentPickup : MonoBehaviour
{
    [Header("Item Data")]
    [Tooltip("ใส่ข้อมูลอาวุธที่จะให้ดรอปตรงนี้เลย (ใน Prefab)")]
    [SerializeField] private EquipmentData equipmentData;

    [Header("Visual Settings")]
    [SerializeField] private float rotateSpeed = 50f;

    [Header("Movement Settings")]
    [Tooltip("ความเร็วในการลอยลงมา (เหมือน Obstacle)")]
    [SerializeField] private float fallSpeed = 3f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void Start()
    {
        // Setup ตัวเองทันทีถ้ามีข้อมูลอยู่แล้ว
        if (equipmentData != null)
        {
            Setup(equipmentData);
        }
    }

    private void Update()
    {
        HandleRotation();
        MoveDown(); // เคลื่อนที่ลงอย่างเดียว
        CheckOutOfBounds();
    }

    private void HandleRotation()
    {
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
    }

    private void MoveDown()
    {
        // เคลื่อนที่ลงด้านล่าง (Space.World เพื่อให้ทิศทางลงเสมอไม่ว่าจะหมุนยังไง)
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
    }

    private void CheckOutOfBounds()
    {
        if (transform.position.y < -10f) Destroy(gameObject);
    }

    public void Setup(EquipmentData data)
    {
        equipmentData = data;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer != null && equipmentData != null && equipmentData.icon != null)
        {
            spriteRenderer.sprite = equipmentData.icon;
            transform.localScale = Vector3.one * 0.8f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Character>(out Character character))
        {
            if (equipmentData != null)
            {
                character.EquipWeapon(equipmentData);
                Destroy(gameObject);
            }
        }
    }
}