using Godot;
using System;

public partial class OvenFireArm : Node2D
{

	/// <summary> Distance between joints of a leg </summary>
	private float LEG_LENGTH = 100;

	/// <summary> Sprite2D for top of arm  </summary>
	private Sprite2D brachium;

	/// <summary> Sprite2D for bottom of arm </summary>
	private Sprite2D forearm;
	/// <summary> Location for leg foot </summary>
	private Vector2 footprint = Vector2.Zero;

	/// <summary> Whether the leg is right or left. </summary>
	[Export]
	public float Orientation = 1;

	/// <summary> If the footprint is currently in the range </summary>
	public bool In_Range;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Getting nodes */
		brachium = GetNode<Sprite2D>("Brachium");
		forearm = brachium.GetNode<Sprite2D>("Forearm");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		/* Base Angle */
		float ang = GetAngleTo(footprint);

		/* Calculating leg angles */
		float step = GlobalPosition.DistanceTo(footprint);

		float brach_ang;

		/* If outside range */
		if (step > LEG_LENGTH * 2)
		{
			brach_ang = 0;
			In_Range = false;
		}
		else
		{
			brach_ang = -Mathf.Acos(step / LEG_LENGTH / 2);
			In_Range = true;
		}

		brachium.Rotation = ang + Orientation * brach_ang;
		forearm.Rotation = Orientation * (2 * Mathf.Pi - brach_ang * 2);
	}

	/// <summary>
	/// Sets the footprint of this body.
	/// </summary>
	/// <param name="position"> The new footprint position </param>
	public void Set_Footprint(Vector2 position)
	{
		footprint = position;
	}
}
