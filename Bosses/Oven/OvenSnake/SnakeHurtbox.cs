using Godot;
using System;

public partial class SnakeHurtbox : HurtboxEnemyParent
{
	/// <summary>
	/// Snake head this hurtbox is attatched to
	/// </summary>
	private OvenSnakeBody snake_body;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		snake_body = GetParent<OvenSnakeBody>();
	}

	/// <summary>
	/// Accepts interaction with a given hitbox and updates accordingly
	/// </summary>
	/// <param name="hitbox">The given hitbox that is being accepted</param>
	/// <param name="damage">The 'damage'</param>
	/// <returns>Whether the given accepting should destroy the hitbox</returns>
	public override bool Accept_Hitbox(HitboxParent hitbox, int damage = 1)
	{
		/* Only damaged by hammer */
		if (hitbox.hitbox_type == "Rect")
		{
			snake_body.Hurt(1);
		}
		return false;
	}
}
