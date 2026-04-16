using Godot;
using System;
using System.Collections.Generic;

public partial class KnightCavity : Node2D
{
	private PackedScene arm_prefab;

	private List<KnightArm> arms = new List<KnightArm>();

	private const int ARM_COUNT = 1;

	private float timer = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		arm_prefab = GD.Load<PackedScene>("res://Bosses/Knight/KnightArm/KnightArm.tscn");

		for (int i = 0; i < ARM_COUNT; i++)
		{
			var arm_inst = arm_prefab.Instantiate<KnightArm>();
			this.AddChild(arm_inst);
			arms.Add(arm_inst);

			/* Set orientation and values */
			float arm_angle = 2 * Mathf.Pi / ARM_COUNT * i;
			arm_inst.Position = Vector2.FromAngle(arm_angle) * 50;
			arm_inst.Scale = new Vector2(0.2f, 0.2f);
			if (arm_angle < Mathf.Pi / 2 || arm_angle > 3 * Mathf.Pi / 2) {
				arm_inst.orientation = -1;
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		timer += (float)delta;
		for (int i = 0; i < ARM_COUNT; i++)
		{
			float arm_angle = 2 * Mathf.Pi / ARM_COUNT * i;
			float sin_val = Mathf.Sin(timer) + 1;
			if (Mathf.Cos(timer) <= 0) {
				arms[i].Set_Handprint(Vector2.FromAngle(arm_angle) * (80 + 300 * sin_val));
			}
			else {
				arms[i].Set_Handprint(Vector2.FromAngle(arm_angle) * (80 + 300* sin_val) + 200 * Vector2.Up * Mathf.Cos(timer));
			}
		}
	}
}
