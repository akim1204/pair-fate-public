using Godot;
using System;

public partial class OvenSnakeSprite : Node2D
{
	public AnimationPlayer animator;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		animator = GetNode<AnimationPlayer>("Animator");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Play a given animation
	/// </summary>
	/// <param name="animation"></param>
	public void Play_Animation(string animation)
	{
		animator.Play(animation);
	}
}
