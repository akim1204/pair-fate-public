using Godot;
using System;

public partial class DragonArmHurtbox : HurtboxEnemyParent
{
	private Arm dragon_arm;
	private bool is_active;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		is_active = false;
		dragon_arm = GetParent<Arm>();
	}

	public override bool Accept_Hitbox(HitboxParent hitbox, int damage = 1)
	{
		if (!is_active)
		{
			return false;
		}
		Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Accept Called on Arm Hurtbox");
		dragon_arm.Register_Hit(hitbox.Get_Player());
		return false;
	}
	public void _on_entered_frosting_patch()
	{
		dragon_arm.Stick_Arm();
	}
	public void Set_Active(bool status)
	{
		this.is_active = status;
	}
	public bool Get_Active()
	{
		return this.is_active;
	}
}
