using UnityEngine;

[System.Serializable]
public class Status
{
    [Header("Combat Stats")]
    public float attack;       // พลังโจมตี
    public float defense;      // พลังป้องกัน (ลดดาเมจ)
    [Range(0, 100)]
    public float critChance;   // โอกาสคริติคอล (%)

    [Header("Weapon Stats")]
    public float rpm;          // Rounds Per Minute (อัตราการยิง)
    public float angle;        // องศาการกระจาย (สำหรับลูกซอง)
    public float projectileSpeed; // [เพิ่ม] ความเร็วกระสุน

    [Header("Movement")]
    public float moveSpeed;    // ความเร็วการเคลื่อนที่

    // ฟังก์ชันสำหรับบวกค่า Stat (Base + Equipment)
    public static Status operator +(Status a, Status b)
    {
        Status result = new Status();
        result.attack = a.attack + b.attack;
        result.defense = a.defense + b.defense;
        result.critChance = a.critChance + b.critChance;
        result.rpm = a.rpm + b.rpm;
        result.angle = a.angle + b.angle;
        result.projectileSpeed = a.projectileSpeed + b.projectileSpeed; // บวกความเร็วกระสุน
        result.moveSpeed = a.moveSpeed + b.moveSpeed;
        return result;
    }
}