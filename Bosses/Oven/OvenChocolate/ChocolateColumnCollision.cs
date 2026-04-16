using Godot;
using System;

public partial class ChocolateColumnCollision : HurtboxEnemyParent
{

	private ChocolateColumn column;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		column = GetParent<ChocolateColumn>();
	}

	/// <summary>
	/// Accepts interaction with a given hitbox and updates accordingly
	/// </summary>
	/// <param name="hitbox">The given hitbox that is being accepted</param>
	/// <param name="damage">The 'damage'</param>
	/// <returns>Whether the given accepting should destroy the hitbox</returns>
	public override bool Accept_Hitbox(HitboxParent hitbox, int damage = 1)
	{
		// Extinguish if hit by splash
		if (hitbox.hitbox_type == "Splash")
		{
			column.Solidify(hitbox.Get_Points());
		}
		return false;
	}
}
