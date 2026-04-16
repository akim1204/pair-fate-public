using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

public partial class FallingHurtHitbox : HurtboxEnemyParent
{
	private FallingObject object_owner;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		object_owner = this.GetParent<FallingObject>();
	}

	public override bool Accept_Hitbox(HitboxParent hitbox, int damage = 1)
	{
		if (!object_owner.Get_Crashing()) {
			return false;
		}
		object_owner.Start_Destroy();
		return false;
	}
	public override void _Process(double delta)
	{
		if (this.HasOverlappingAreas()) {
			for (int i=0; i<this.GetOverlappingAreas().Count;i++) {
				Area2D area = this.GetOverlappingAreas()[i];
				if (area.GetType().IsAssignableTo(typeof(PlayerHurtbox))) {
					/* Cast to hurtboxenemyparent */
					Handle_Player_Areas((PlayerHurtbox)area);
				}
			}
		}
	}
    	public void Handle_Player_Areas(PlayerHurtbox area)
	{
		if (!object_owner.Get_Crashing()) {
			return;
		}
		// If it is a hurtbox
		/* Cast to hurtboxenemyparent */
		(area).Hurt(1);
	}
}
