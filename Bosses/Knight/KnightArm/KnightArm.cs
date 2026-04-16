using Godot;
using System;

public partial class KnightArm : Node2D
{

	/// <summary> Length of arm </summary>
	[Export]
	public float ARM_LENGTH = 240;

	/// <summary> Three individual parts of the arm </summary>
	private Node2D Arm1;
	private Node2D Arm2;
	private Node2D Arm3;
	[Export]
	public int orientation = 1;
	private Vector2 handprint_goal = Vector2.Zero;
	private Vector2 current_handprint = Vector2.Zero;
	private const float MOVE_SPEED = 500;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Arm1 = GetNode<Node2D>("Arm1");
		Arm2 = Arm1.GetNode<Node2D>("Arm2");
		Arm3 = Arm2.GetNode<Node2D>("Arm3");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (handprint_goal.DistanceTo(current_handprint) < (float)delta * MOVE_SPEED)
		{
			Set_Handprint(handprint_goal);
		}
		else
		{
			Set_Handprint(current_handprint + (handprint_goal - current_handprint).Normalized() * (float)delta * MOVE_SPEED);
		}
	}

	public void Set_Handprint_Helper(Vector2 global_position)
	{
		handprint_goal = ToLocal(global_position);
	}

	public void Set_Handprint_Relative(Vector2 local_position)
	{
		handprint_goal = local_position;
	}

	public void Set_Handprint(Vector2 position)
	{
		current_handprint = position;
		/* Scale to arm length of one */
		position /= ARM_LENGTH;

		/* Total offset of hand */
		float offset = position.Length();

		/* Case when too long */
		if (offset > 3)
		{
			/* Set angles */
			Arm2.Rotation = 0;
			Arm3.Rotation = 0;
			Arm1.Rotation = position.Angle();
			return;
		}
		/* Calculate individual angles */
		float remainder = offset - 1;
		/* First angle */
		float theta = Mathf.Acos(remainder / 2);

		/* Set angles */
		Arm2.Rotation = -theta * orientation;
		Arm3.Rotation = -theta * orientation;

		/* Final angle */
		Arm1.Rotation = position.Angle() + theta * orientation;
	}
}
