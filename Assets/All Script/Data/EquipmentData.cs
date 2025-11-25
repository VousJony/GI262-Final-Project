using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/New Equipment")]
public class EquipmentData : ScriptableObject
{
    public string equipmentName;
    public Sprite icon;

    [Header("Duration Settings")]
    [Tooltip("วินาที (ใส่ 0 หรือน้อยกว่า = อาวุธถาวร)")]
    public float duration = 10f;

    [Header("Bonus Stats")]
    public Status statusBonus;

    [Header("Combat Specs")]
    public int projectilesPerShot = 1;
    public GameObject bulletPrefabOverride;

    [Header("Audio & Visuals")]
    [Tooltip("เปิดถ้าต้องการเสียงแบบ Loop (เช่นปืนกล)")]
    public bool isLoopingSFX;
    public AudioClip shootSFXOverride;
    public GameObject muzzleFlashVFX;

    public Status GetStatusBonus()
    {
        return statusBonus;
    }
}