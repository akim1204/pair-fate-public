using Godot;
using System;

public partial class JelloResidueHurtbox : HurtboxEnemyParent
{

    /// <summary>
    /// Jello Residue object this hurtbox is attatched to
    /// </summary>
    private JelloResidue jello_residue;

    public override void _Ready()
    {
        /* Get parent of this hurtbox */
        this.jello_residue = GetParent<JelloResidue>();
    }

    /// <summary>
    /// Accepts interaction with a given hitbox and updates accordingly
    /// </summary>
    /// <param name="hitbox">The given hitbox that is being accepted</param>
    /// <param name="damage">The 'damage'</param>
    /// <returns>Whether the given accepting should destroy the hitbox</returns>
    public override bool Accept_Hitbox(HitboxParent hitbox, int damage = 1)
    {
        Logger.Instance.Log(Logger.LOG_LEVELS.DEBUG, "Accept Called on Jello Residue Hurtbox");
        // Curently only works with rectangular types
        if (hitbox.hitbox_type == "Rect")
        {
            Vector2[] hitbox_corners = hitbox.Get_Points();
            /* Quick check */
            if (hitbox_corners.Length != 2)
            {
                Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Invalid Points Recieved from Hitbox");
            }
            /* Clear the corresponding grid within the residue */
            this.jello_residue.Update_Grid_Rect(hitbox_corners[0], hitbox_corners[1], 0);
        }
        return false;
    }
}
