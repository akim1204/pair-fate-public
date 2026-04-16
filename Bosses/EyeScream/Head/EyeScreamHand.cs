using Godot;
using System;
using System.Collections;

public partial class EyeScreamHand : Node2D
{
	KnightArm finger1;
	KnightArm finger2;
	KnightArm finger3;
	KnightArm shadow_finger1;
	KnightArm shadow_finger2;
	KnightArm shadow_finger3;
	Node2D shadow_center;
	private float timer = 0;
	Sprite2D sweep_shadow;
	public enum HAND_STATES
	{
		FIST,
		IDLE,
		PUSH,
		STALE,
	}
	public HAND_STATES hand_state = HAND_STATES.IDLE;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		finger1 = GetNode<KnightArm>("Finger1");
		finger2 = GetNode<KnightArm>("Finger2");
		finger3 = GetNode<KnightArm>("Finger3");
		shadow_center = GetNode<Node2D>("ShadowCenter");
		shadow_finger1 = shadow_center.GetNode<KnightArm>("ShadowFinger1");
		shadow_finger2 = shadow_center.GetNode<KnightArm>("ShadowFinger2");
		shadow_finger3 = shadow_center.GetNode<KnightArm>("ShadowFinger3");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		switch (hand_state)
		{
			case HAND_STATES.STALE:
				break;
			case HAND_STATES.FIST:
				//finger1.Set_Handprint_Helper(GlobalPosition + 80 * Vector2.Up + -20 * Vector2.Left * finger2.orientation);
				//finger2.Set_Handprint_Helper(GlobalPosition + 80 * Vector2.Down + 20 * Vector2.Left * finger2.orientation);
				//finger3.Set_Handprint_Helper(GlobalPosition + 80 * Vector2.Up + -40 * Vector2.Left * finger2.orientation);
				finger1.Set_Handprint_Relative(-120 * Vector2.Down * finger1.orientation);
				finger2.Set_Handprint_Relative(-120 * Vector2.Down * finger2.orientation);
				finger3.Set_Handprint_Relative(-120 * Vector2.Down * finger3.orientation);
				break;
			case HAND_STATES.PUSH:
				finger1.Set_Handprint_Helper(GlobalPosition + 160 * Vector2.Down + 40 * Vector2.Right * finger1.orientation);
				finger2.Set_Handprint_Helper(GlobalPosition + 160 * Vector2.Up - 60 * Vector2.Right * finger2.orientation);
				finger3.Set_Handprint_Helper(GlobalPosition + 160 * Vector2.Down + 20 * Vector2.Right * finger3.orientation);
				break;
			case HAND_STATES.IDLE:
				timer += 2 * (float)delta;
				if (timer > Mathf.Pi * 2) timer -= Mathf.Pi * 2;
				finger1.Set_Handprint_Helper(GlobalPosition - 40 * Vector2.Down + (190 + 20 * Mathf.Cos(timer)) * Vector2.Right * finger1.orientation);
				finger2.Set_Handprint_Helper(GlobalPosition - 40 * Vector2.Up - (190 - 20 * Mathf.Cos(timer)) * Vector2.Right * finger2.orientation);
				finger3.Set_Handprint_Helper(GlobalPosition - 40 * Vector2.Down + (190 + 20 * Mathf.Cos(timer)) * Vector2.Right * finger3.orientation);
				break;
		}
	}

	public void Fist() { hand_state = HAND_STATES.FIST; }
	public void Idle() { hand_state = HAND_STATES.IDLE; }
	public void Sweep() { hand_state = HAND_STATES.PUSH; }
	public void Stale() { hand_state = HAND_STATES.STALE; }

	public void Hide_Sweeo() { sweep_shadow.Hide(); }
	public void Show_Sweep() { sweep_shadow.Show(); }
}
