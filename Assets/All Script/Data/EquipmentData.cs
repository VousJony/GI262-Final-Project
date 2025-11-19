using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/New Equipment")]
public class EquipmentData : ScriptableObject
{
    public string equipmentName;
    public Sprite icon; // เผื่อใช้แสดงใน UI

    [Header("Bonus Stats")]
    public Status statusBonus;

    [Header("Special Settings")]
    [Tooltip("จำนวนกระสุนที่ยิงออกไปต่อ 1 นัด (เช่น ลูกซอง = 5)")]
    public int projectilesPerShot = 1;

    [Tooltip("Prefab กระสุนเฉพาะของปืนนี้ (ถ้าไม่ใส่จะใช้ Default ของตัวละคร)")]
    public GameObject bulletPrefabOverride;

    // ฟังก์ชันตาม Diagram (แต่เราดึงจากตัวแปร statusBonus โดยตรงได้เลย)
    public Status GetStatusBonus()
    {
        return statusBonus;
    }
}