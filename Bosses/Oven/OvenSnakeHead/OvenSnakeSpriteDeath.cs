using Godot;
using System;

public partial class OvenSnakeSpriteDeath : Sprite2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<AnimationPlayer>("AnimationPlayer").Play("Break");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void _on_animation_player_animation_finished(String anim_name)
	{
		QueueFree();
	}
}
