using Godot;
using System;

public partial class CandyScytheArc : Node2D
{
	double life_timer = .25;
	bool active = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (life_timer <= 0)
		{
			QueueFree();
		}
		if (active)
		{
			life_timer -= delta;
			float alpha = (float)(life_timer / 0.25f);
			if (life_timer > 0.2)
			{
				alpha = 0;
			}
			this.Modulate = new Color(1, 1, 1, alpha);
		}

	}
	public void Initialize(Vector2 pos, float angle)
	{
		this.GlobalPosition = pos;
		this.GlobalRotation = angle;
		Show();

		active = true;

		// GD.Print("Arc!");
	}
}
