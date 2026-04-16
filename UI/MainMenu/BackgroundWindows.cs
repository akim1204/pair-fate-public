using Godot;
using System;

public partial class BackgroundWindows : Sprite2D
{
	float timer = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		timer += (float)delta / 2.5f;
		if (timer > 2 * Mathf.Pi)
		{
			timer -= 2 * Mathf.Pi;
		}
		this.Modulate = new Color(1, 1, 1, Mathf.Cos(timer) * 2);
	}
}
