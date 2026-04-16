using Godot;
using System;

public partial class CandleMissileHitbox : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
    	public void _on_area_entered(Area2D area) 
	{
		GD.Print("Crater collided with player");
		// If it is a hurtbox
		if (area.GetType().IsAssignableTo(typeof(PlayerHurtbox)))
		{
			/* Cast to hurtboxenemyparent */
			((PlayerHurtbox)area).Hurt(1);
		}

	}
}
