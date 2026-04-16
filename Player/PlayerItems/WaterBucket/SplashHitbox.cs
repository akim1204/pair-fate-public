using Godot;
using System;
using System.Collections.Generic;

public partial class SplashHitbox : HitboxParent
{
    /// <summary>
    /// Type of hitbox, used by accepting hurtbox
    /// </summary>
    public override string hitbox_type { get { return "Splash"; } }

    /// <summary> Size of the splash hitbox in pixels </summary>
    private const float WIDTH = 300;
    private const float HEIGHT = 200;

    /// <summary>
    /// Collision shape of the hitbox.
    /// </summary>
    private CollisionShape2D collider;

    /// <summary>
    /// Hashset of all Area2D this hitbox has already interacted with.
    /// </summary>
    private HashSet<Area2D> encountered_areas = new HashSet<Area2D>();


    /// <summary>
    /// scale of the hitbox
    /// </summary>
    private Vector2 scale = Vector2.Zero;
    public override void _Ready()
    {
    }

    public void _on_area_entered(Area2D area)
    {
        /* Only interact with each area once over lifetime */
        if (!encountered_areas.Contains(area))
        {
            encountered_areas.Add(area);
            // If it is a hurtbox
            if (area.GetType().IsAssignableTo(typeof(HurtboxEnemyParent)))
            {
                /* Cast to hurtboxenemyparent */
                ((HurtboxEnemyParent)area).Accept_Hitbox(this, 1);
            }
        }
    }

    public override void _Process(double delta)
    {
    }

    /// <summary>
    /// Returns two corners of the same side of the hitbox and a vector perpendicular to this side.
    /// </summary>
    /// <returns>Array of vector2s</returns>
    public override Vector2[] Get_Points()
    {
        float true_rotation = this.GetParent<Node2D>().Rotation;
        return new Vector2[]{this.GlobalPosition,
        Vector2.FromAngle(true_rotation) * HEIGHT,
        Vector2.FromAngle(true_rotation) * HEIGHT + Vector2.FromAngle(true_rotation + Mathf.Pi / 2) * WIDTH / 2,
        Vector2.FromAngle(true_rotation) * HEIGHT + Vector2.FromAngle(true_rotation - Mathf.Pi / 2) * WIDTH / 2};
    }
}
