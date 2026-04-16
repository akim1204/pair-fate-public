using Godot;
using System;

public partial class ExitBarrier : StaticBody2D
{
	DragonBoss boss;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		boss = GetTree().Root.GetNode<DragonBoss>("World/DragonBoss");
		if (boss == null) {
			GD.Print("Couldn't find boss");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (boss.Get_Boss_State() == DragonBoss.BossStates.DEAD) {
			this.QueueFree();
		}
		this.GlobalPosition = new Vector2(768f, 633f);
	}
}
