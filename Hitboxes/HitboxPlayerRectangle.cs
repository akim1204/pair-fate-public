using Godot;
using System;
using System.Collections.Generic;

public partial class HitboxPlayerRectangle : HitboxParent
{
    /// <summary>
    /// Type of hitbox, used by accepting hurtbox
    /// </summary>
    public override string hitbox_type { get { return "Rect"; } }

    /// <summary>
    /// Collision shape of the hitbox.
    /// </summary>
    private CollisionShape2D collider;

    /// <summary>
    /// Hashset of all Area2D this hitbox has already interacted with.
    /// </summary>
    private HashSet<Godot.Area2D> encountered_areas = new HashSet<Godot.Area2D>();

    /// <summary>
    /// How long the hitbox will last.
    /// </summary>
    private float lifetime = 0.1f;

    /// <summary>
    /// scale of the hitbox
    /// </summary>
    private Vector2 scale = Vector2.Zero;
    public override void _Ready()
    {

    }

    public void _on_area_entered(Godot.Area2D area)
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
        lifetime -= (float)delta;
        /* If lifetime ends, delete hitbox */
        if (lifetime <= 0)
        {
            QueueFree();
        }
    }

    public void Initialize(Player owner_player, float width, float height, Vector2 position, float rotation, float lifetime = 0.1f)
    {
        this.owner_player = owner_player;
        this.lifetime = lifetime;
        this.collider = GetNode<CollisionShape2D>("HitboxShape");
        this.collider.Scale = new Vector2(width, height);
        this.scale = new Vector2(width, height);
        this.Rotation = rotation;

        this.GlobalPosition = position;
    }

    /// <summary>
    /// Returns the top left and bottom right point of the hitbox
    /// </summary>
    /// <returns>Array of vector2s</returns>
    public override Vector2[] Get_Points()
    {
        return new Vector2[]{this.Position - this.scale.Rotated(this.Rotation), this.Position - new Vector2(this.scale.X, -this.scale.Y).Rotated(this.Rotation),
        this.Position + this.scale.Rotated(this.Rotation), this.Position - new Vector2(-this.scale.X, this.scale.Y).Rotated(this.Rotation)};
    }

}
