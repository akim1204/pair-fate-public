using Godot;
using System;

public partial class DragonArmHitbox : Area2D
{
	private bool is_active = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void _on_collision_area_area_entered(Area2D area)
	{
		if (!this.is_active) {
			return;
		}
		// If it is a hurtbox
		if (area.GetType().IsAssignableTo(typeof(PlayerHurtbox)))
		{
			/* Cast to hurtboxenemyparent */
			((PlayerHurtbox)area).Hurt(1);
		}

	}
	public void Set_Active(bool status) 
	{
		this.is_active = status;
	}
	public bool Get_Active()
	{
		return is_active;
	}
}
