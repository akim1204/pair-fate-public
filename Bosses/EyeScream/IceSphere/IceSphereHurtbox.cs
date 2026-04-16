using Godot;
using System;

public partial class IceSphereHurtbox : HurtboxEnemyParent
{
	private IceSphere parent;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		parent = GetParent<IceSphere>();
	}

	/// <summary>
	/// Accepts interaction with a given hitbox and updates accordingly
	/// </summary>
	/// <param name="hitbox">The given hitbox that is being accepted</param>
	/// <param name="damage">The 'damage'</param>
	/// <returns>Whether the given accepting should destroy the hitbox</returns>
	public override bool Accept_Hitbox(HitboxParent hitbox, int damage = 1)
	{
		//Logger.Instance.Log(Logger.LOG_LEVELS.DEBUG, "Accept Called on Ice Sphere Hurtbox");
		parent.Hurt(damage);
		return false;
	}
}
