using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

public partial class CraterHitbox : Area2D
{
	private Node2D arm_crater;
	private double life_timer = .5;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		arm_crater = GetParent<Node2D>();
	}

	public override void _Process(double delta)
	{

		Color col = new Color(0,0,0,.8f/(float) this.life_timer * (float) delta);
		arm_crater.Modulate -= col;

		this.life_timer -= delta;
		if (life_timer <= 0) {
			arm_crater.QueueFree();
		}
	}
    	public void _on_area_entered(Area2D area) 
	{
		// If it is a hurtbox
		if (area.GetType().IsAssignableTo(typeof(PlayerHurtbox)))
		{
			/* Cast to hurtboxenemyparent */
			((PlayerHurtbox)area).Hurt(1);
		}

	}
}
