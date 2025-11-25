using UnityEngine;

[System.Serializable]
public class Status
{
    [Header("Combat")]
    public float attack;
    public float defense;
    [Range(0, 100)] public float critChance;

    [Header("Weapon")]
    public float rpm;
    public float angle;
    public float projectileSpeed;

    [Header("Movement")]
    public float moveSpeed;

    /// <summary>
    /// Overload Operator + ให้สามารถบวก Status สองตัวเข้าด้วยกันได้ง่ายๆ
    /// </summary>
    public static Status operator +(Status a, Status b)
    {
        return new Status
        {
            attack = a.attack + b.attack,
            defense = a.defense + b.defense,
            critChance = a.critChance + b.critChance,
            rpm = a.rpm + b.rpm,
            angle = a.angle + b.angle,
            projectileSpeed = a.projectileSpeed + b.projectileSpeed,
            moveSpeed = a.moveSpeed + b.moveSpeed
        };
    }
}