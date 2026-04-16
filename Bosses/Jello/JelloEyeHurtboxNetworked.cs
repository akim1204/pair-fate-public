using Godot;
using System;

public partial class JelloEyeHurtboxNetworked : HurtboxEnemyParent
{
    /// <summary>
    /// Jello eye this hurtbox is attatched to
    /// </summary>
    private JelloEyeNetworked jello_eye;
    public override void _Ready()
    {
        /* Get parent of this hurtbox */
        this.jello_eye = GetParent().GetParent<JelloEyeNetworked>();
    }


    /// <summary>
    /// Accepts interaction with a given hitbox and updates accordingly
    /// </summary>
    /// <param name="hitbox">The given hitbox that is being accepted</param>
    /// <param name="damage">The 'damage'</param>
    /// <returns>Whether the given accepting should destroy the hitbox</returns>
    public override bool Accept_Hitbox(HitboxParent hitbox, int damage = 1)
    {
        Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Accept Called on Eye Hurtbox");
        jello_eye.Hurt(1);
        return false;
    }
}
