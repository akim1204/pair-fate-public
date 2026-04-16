using Godot;
using System;
using System.Collections;

public partial class SlimeProjectile : Sprite2D
{
	/// <summary> direction the projectile is currently traveling. </summary>
	private Vector2 direction = Vector2.Zero;

	private const float LEFT_BOUND = -64;
	private const float RIGHT_BOUND = 2620;
	private const float TOP_BOUND = 0;
	private const float BOTTOM_BOUND = 1536;

	private float TRAVEL_SPEED = 800f;

	private AnimationPlayer animation_player;

	bool firing = false;

	bool destroyed = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Find animation player */
		animation_player = GetNode<AnimationPlayer>("AnimationPlayer");

		/* Play spawn animation */
		animation_player.Play("Shoot");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (firing)
		{
			this.GlobalPosition += (float)delta * this.direction * TRAVEL_SPEED;
		}
		/* Check for collisions */
		if (this.GlobalPosition.X < LEFT_BOUND - 10 ||
		this.GlobalPosition.X > RIGHT_BOUND + 10 ||
		this.GlobalPosition.Y < TOP_BOUND - 10 ||
		this.GlobalPosition.Y > BOTTOM_BOUND + 10)
		{
			if (!destroyed)
			{
				Rpc("Destroy");
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

	public void _on_animation_player_animation_finished(string anim_name)
	{
		if (!firing)
		{
			firing = true;
		}
		if (destroyed)
		{
			QueueFree();
		}
	}

	public void _on_collision_area_area_entered(Area2D area)
	{
		if (!destroyed)
		{
			// If it is a hurtbox
			if (area.GetType().IsAssignableTo(typeof(PlayerHurtbox)))
			{
				/* Cast to hurtboxenemyparent */
				((PlayerHurtbox)area).Hurt(1);
				Rpc("Destroy");

			}

			// If it is a shield hitbox
			if (area.GetType().IsAssignableTo(typeof(PlayerShieldHitbox)))
			{
				PlayerShieldHitbox shield_hitbox = (PlayerShieldHitbox)area;
				if (shield_hitbox.Get_Active())
				{
					Rpc("Destroy");
				}
			}
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Destroy()
	{
		if (!destroyed)
		{

			GetNode<SoundPlayer>("SoundPlayer").Play_Effect("Destroy", -35);
		}
		animation_player.Play("Destroy");
		destroyed = true;
		TRAVEL_SPEED = 200;
	}
}
