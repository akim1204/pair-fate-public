using Godot;
using System;

public partial class JelloBodyHurtbox : HurtboxEnemyParent
{

    /// <summary>
    /// Jello this hurtbox is attatched to
    /// </summary>
    private JelloNetworked jello;


    public override void _Ready()
    {
        /* Get parent of this hurtbox */
        this.jello = GetParent().GetParent<JelloNetworked>();
    }

    /// <summary>
    /// Accepts interaction with a given hitbox and updates accordingly
    /// </summary>
    /// <param name="hitbox">The given hitbox that is being accepted</param>
    /// <param name="damage">The 'damage'</param>
    /// <returns>Whether the given accepting should destroy the hitbox</returns>
    public override bool Accept_Hitbox(HitboxParent hitbox, int damage = 1)
    {
        Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Accept Called on Jello Body Hurtbox");
        /* Only accept authority hitboxes */
        if (hitbox.Authority_Hitbox())
        {
            jello.Hurt(1);
        }
        return false;
    }
}
