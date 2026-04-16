using Godot;
using System;

public partial class PlayerShieldHitbox : Area2D
{
    private Item shield;

    public override void _Ready()
    {
        /* Getting the parent shield */
        this.shield = GetParent<Item>();

        /* Check its type */
        if (this.shield.Get_Type() != Item.ItemTypes.Shield)
        {
            //Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Shield hitbox not attatched to a shield");
        }
    }

    /// <summary>
    /// Returns the points of the parent shield.
    /// </summary>
    /// <returns></returns>
    public Vector2[] Get_Points()
    {
        return shield.Get_Points();
    }

    public bool Get_Active()
    {
        return shield.Get_Active();
    }

    public Item Get_Parent()
    {
        return this.shield;
    }
}
