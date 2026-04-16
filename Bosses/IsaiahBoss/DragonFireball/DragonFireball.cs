using Godot;
using System;

public partial class DragonFireball : Node2D
{
	private float _speed;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Initialize(float angle, float speed)
	{
		this.Rotation = angle;
		this._speed = speed;
	}
}
