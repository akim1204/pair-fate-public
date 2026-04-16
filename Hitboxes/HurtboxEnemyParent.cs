using Godot;
using System;

public partial class HurtboxEnemyParent : Area2D
{
    /// <summary>
    /// Accepts interaction with a given hitbox and updates accordingly
    /// </summary>
    /// <param name="hitbox">The given hitbox that is being accepted</param>
    /// <param name="damage">The 'damage'</param>
    /// <returns>Whether the given accepting should destroy the hitbox</returns>
    public virtual bool Accept_Hitbox(HitboxParent hitbox, int damage = 1)
    {
        Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Accept Called on Invalid Hurtbox");
        return false;
    }
}
