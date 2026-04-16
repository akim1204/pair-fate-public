using Godot;
using System;

public partial class OvenBodyFire : Sprite2D
{

	private OvenController oven_controller;

	private AnimationPlayer animation_player;

	private OvenFireArm left_arm;
	private Sprite2D left_hand;
	private Vector2 left_center = new Vector2(0, 60);
	private OvenFireArm right_arm;
	private SoundPlayer sound_player;
	private Sprite2D right_hand;
	private Vector2 right_center = new Vector2(380, 60);
	private const float LAVA_TILESIZE = 320;
	private const float LAVA_PULLSPEED = 64;
	private float lava_offset = 0;
	private TextureRect oven_lava;

	/// <summary>
	/// Whether it is currently ignited 
	/// </summary>
	private bool ignited = false;

	private float timer = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		animation_player = GetNode<AnimationPlayer>("OvenBodyFireAnimation");
		left_arm = GetNode<OvenFireArm>("LeftArm");
		right_arm = GetNode<OvenFireArm>("RightArm");
		left_hand = left_arm.GetNode<Sprite2D>("LeftHand");
		right_hand = right_arm.GetNode<Sprite2D>("RightHand");
		sound_player = GetNode<SoundPlayer>("SoundPlayer");
		left_arm.Visible = false;
		right_arm.Visible = false;
		left_center += this.GlobalPosition;
		right_center += this.GlobalPosition;

		oven_lava = GetNode<TextureRect>("OvenLava");
		oven_lava.Visible = false;
	}

	public void Activate()
	{
		oven_lava.Visible = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (ignited)
		{
			timer += 3 * (float)delta;
			if (timer > 2 * Mathf.Pi)
			{
				timer -= 2 * Mathf.Pi;
			}

			/* Move arms one at a time */
			if (Mathf.Sin(timer) > 0)
			{
				left_arm.Set_Footprint(left_center + 200 * Mathf.Sin(timer) * Vector2.Down + Mathf.Max(0, Mathf.Cos(timer)) * 80 * Vector2.Left);
				left_hand.GlobalPosition = left_center + 250 * Mathf.Sin(timer) * Vector2.Down + Mathf.Max(0, Mathf.Cos(timer)) * 100 * Vector2.Left;

				left_hand.Rotation = Mathf.Pi * Mathf.Max(0.5f, Mathf.Sin(timer));

				left_arm.Visible = true;
				right_arm.Visible = false;
			}
			else
			{
				left_arm.Visible = false;
				right_arm.Set_Footprint(right_center - 200 * Mathf.Sin(timer) * Vector2.Down + Mathf.Max(0, -Mathf.Cos(timer)) * 80 * Vector2.Right);
				right_hand.GlobalPosition = right_center - 250 * Mathf.Sin(timer) * Vector2.Down + Mathf.Max(0, -Mathf.Cos(timer)) * 100 * Vector2.Right;

				right_hand.Rotation = -Mathf.Pi * Mathf.Max(0.5f, -Mathf.Sin(timer));

				right_arm.Visible = true;
			}


			/* Move if sin decreasing */
			if (Mathf.Sign(Mathf.Cos(timer)) != Mathf.Sign(Mathf.Sin(timer)))
			{
				/* play sound on change */
				if (Mathf.Sign(Mathf.Cos(timer - (float)delta)) == Mathf.Sign(Mathf.Sin(timer - (float)delta)))
				{
					sound_player.Play_Effect("pull", -30, 0.5f);
				}
				oven_controller.Pull_Platform((float)delta);
				oven_lava.GlobalPosition += (float)delta * Vector2.Up * LAVA_PULLSPEED;
				lava_offset += (float)delta * LAVA_PULLSPEED;
				if (lava_offset > LAVA_TILESIZE)
				{
					oven_lava.GlobalPosition -= LAVA_TILESIZE * Vector2.Up;
					lava_offset -= LAVA_TILESIZE;
				}
			}
		}
	}

	/// <summary>
	/// Sets the oven controller for this fire
	/// </summary>
	public void Set_Controller(OvenController controller)
	{
		this.oven_controller = controller;
	}

	/// <summary>
	/// Play the ignited animation
	/// </summary>
	public void Ignite()
	{
		animation_player.Play("IgnitedClose");
		timer = 0;
		ignited = true;
	}

	/// <summary>
	/// just plays the ignited animation
	/// </summary>
	public void Fake_Ignite()
	{
		animation_player.Play("IgnitedClose");
	}

	/// <summary>
	/// Play the extinguished animation
	/// </summary>
	public void Extinguish()
	{
		animation_player.Play("Extinguished");
		ignited = false;
		left_arm.Visible = false;
		right_arm.Visible = false;
	}

	public void _on_oven_body_fire_animation_animation_finished(string anim_name)
	{
		if (anim_name == "IgnitedClose")
		{
			animation_player.Play("IgnitedOpen");
			/* Chomp */
			oven_controller.Eat_Platform();
		}
		if (anim_name == "IgnitedOpen")
		{
			animation_player.Play("IgnitedClose");
		}
	}
}
