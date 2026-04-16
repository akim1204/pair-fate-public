using Godot;
using System;

public partial class JelloHurtbox : HurtboxEnemyParent
{

    /// <summary>
    /// Jello object this hurtbox is attatched to
    /// </summary>
    private Jello jello;

    public override void _Ready()
    {
        /* Get parent of this hurtbox */
        this.jello = GetParent<Jello>();
    }

    /// <summary>
    /// Accepts interaction with a given hitbox and updates accordingly
    /// </summary>
    /// <param name="hitbox">The given hitbox that is being accepted</param>
    /// <param name="damage">The 'damage'</param>
    /// <returns>Whether the given accepting should destroy the hitbox</returns>
    public override bool Accept_Hitbox(HitboxParent hitbox, int damage = 1)
    {
        Logger.Instance.Log(Logger.LOG_LEVELS.DEBUG, "Accept Called on Jello Hurtbox");
        return false;
    }
}
