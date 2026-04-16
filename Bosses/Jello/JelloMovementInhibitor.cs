using Godot;
using System;

public partial class JelloMovementInhibitor : MovementInhibitor
{
    private JelloResidue jello_residue;

    private BossController boss_controller;
    /// <summary> Maximum speed the player can travel on jello </summary>
    private const float MAX_JELLO_SPEED = 225;

    /// <summary> Friction applied by being on jello. </summary>
    private const float JELLO_FRICTION = 5000f;

    public override void _Ready()
    {
    }

    public override void Seek()
    {
        /* Get the jello residue node */
        jello_residue = GetNode<JelloResidue>("../bossController0/JelloResidue");

        /* Get the boss controller node */
        boss_controller = GetNode<BossController>("../bossController0");
    }

    /// <summary>
    /// Places any restrictions on the player movement due to environment.
    /// </summary>
    /// <param name="delta"> The time since the previous frame. </param>
    /// <param name="cur_velocity">The current velocity of the player. </param>
    /// <param name="cur_position">The current position of the player. </param>
    /// <returns>The new velocity of the player. </returns>
    public override Vector2 Inhibit_Movement(float delta, Vector2 cur_velocity, Vector2 cur_position)
    {
        /* Apply slowdown to player */
        if (jello_residue.Get_Residue(cur_position) > 0.5f)
        {
            float cur_speed = cur_velocity.Length();
            if (cur_speed > MAX_JELLO_SPEED)
            {
                if (cur_speed < MAX_JELLO_SPEED + JELLO_FRICTION * delta)
                {
                    return cur_velocity.Normalized() * MAX_JELLO_SPEED;
                }
                else
                {
                    return cur_velocity - cur_velocity.Normalized() * JELLO_FRICTION * delta;
                }
            }
        }
        /* If player is locked in place */
        if (boss_controller.Get_Player_Lock())
        {
            cur_velocity = Vector2.Zero;
        }
        return cur_velocity;
    }

}
