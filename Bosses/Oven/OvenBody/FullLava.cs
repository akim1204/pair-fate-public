using Godot;
using System;

public partial class FullLava : TextureRect
{
	private float timer = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		timer += (float)delta / 2;
		if (timer >= 2 * Mathf.Pi)
		{
			timer -= 2 * Mathf.Pi;
		}

		this.Position = new Vector2(-40, 180) + new Vector2(5 * Mathf.Cos(timer), 5 * Mathf.Sin(timer + 0.5f));
	}
}
