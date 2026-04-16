using Godot;
using System;

public partial class CookieBoulder : Node2D
{
	/// <summary> Main body of the cookie</summary>
	private Sprite2D cookie_body;

	/// <summary> Controlling the boulder bounce </summary>
	private float bounce_timer;

	/// <summary> How often it bounces, </summary>
	private const float BOUNCE_RATE = 4;

	/// <summary> How high it bounces </summary>
	private const float BOUNCE_HEIGHT = 12;

	/// <summary> Offset of body sprite. </summary>
	private const float SPRITE_OFFSET = 26;

	private const float TRAVEL_SPEED = 400;

	private const float TRAVEL_CUTOFF = 1400;

	private SoundPlayer sound_player;

	private float rotation_offset;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Get sprite and set random frame */
		cookie_body = GetNode<Sprite2D>("CookieBody");
		cookie_body.Frame = (int)(GD.Randi() % 4);
		rotation_offset = GD.Randf() * Mathf.Pi * 2;

		sound_player = GetNode<SoundPlayer>("SoundPlayer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		bounce_timer += (float)delta * BOUNCE_RATE;
		if (bounce_timer > Mathf.Pi * 2)
		{
			bounce_timer -= Mathf.Pi * 2;
		}

		/* Set rotation */
		cookie_body.Rotation = bounce_timer + rotation_offset;
		cookie_body.Position = new Vector2(0, -(SPRITE_OFFSET
		  + Mathf.Abs(Mathf.Sin(bounce_timer) * BOUNCE_HEIGHT)));

		/* Playing sound */
		if (bounce_timer % Mathf.Pi < (bounce_timer - delta * BOUNCE_RATE + Mathf.Pi) % Mathf.Pi)
		{
			/* Only play ocassionally */
			if (GD.Randi() % 2 == 0)
			{
				sound_player.Play_Effect("Bounce", -45);
			}
		}

		/* Travel */
		this.GlobalPosition += new Vector2(0, TRAVEL_SPEED * (float)delta);

		if (this.GlobalPosition.Y > 1400)
		{
			this.QueueFree();
		}

	}

	/// <summary>
	/// Colliding with player
	/// </summary>
	/// <param name="area"></param>
	public void _on_cookie_collision_area_entered(Area2D area)
	{

		// If it is a hurtbox
		if (area.GetType().IsAssignableTo(typeof(PlayerHurtbox)))
		{
			/* Cast to hurtboxenemyparent */
			((PlayerHurtbox)area).Hurt(1);
		}

	}
}
