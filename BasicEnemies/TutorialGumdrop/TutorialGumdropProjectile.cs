using Godot;
using System;

public partial class TutorialGumdropProjectile : Sprite2D
{
	/// <summary> direction the projectile is currently traveling. </summary>
	private Vector2 direction = Vector2.Zero;

	private const float LEFT_BOUND = 0;
	private const float RIGHT_BOUND = 2560;
	private const float TOP_BOUND = 0;
	private const float BOTTOM_BOUND = 1536;

	private const float TRAVEL_SPEED = 300f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		this.GlobalPosition += (float)delta * this.direction * TRAVEL_SPEED;

		/* Check for collisions */
		if (this.GlobalPosition.X < LEFT_BOUND - 10 ||
		this.GlobalPosition.X > RIGHT_BOUND + 10 ||
		this.GlobalPosition.Y < TOP_BOUND - 10 ||
		this.GlobalPosition.Y > BOTTOM_BOUND + 10)
		{
			QueueFree();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="area"></param>
	public void _on_tutorial_gumdrop_projectile_hitbox_area_entered(Area2D area)
	{
		// If it is a hurtbox
		if (area.GetType().IsAssignableTo(typeof(PlayerHurtbox)))
		{
			/* Cast to hurtboxenemyparent */
			((PlayerHurtbox)area).Hurt(1);
		}

		// If it is a shield hitbox
		if (area.GetType().IsAssignableTo(typeof(PlayerShieldHitbox)))
		{
			PlayerShieldHitbox shield_hitbox = (PlayerShieldHitbox)area;
			if (shield_hitbox.Get_Active())
			{
				QueueFree();
			}
		}
	}

	/// <summary>
	/// Sets the direction that the projectile is currently traveling.
	/// </summary>
	/// <param name="direction"> Direction to set.</param>
	public void Set_Direction(Vector2 shoot_direction)
	{
		this.direction = shoot_direction.Normalized();
		/* Edge case */
		if (this.direction == Vector2.Zero)
		{
			this.direction = Vector2.Right;
		}

		/* Set rotation */
		this.Rotation = this.direction.Angle() + Mathf.Pi / 2;
	}
}
