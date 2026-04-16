using Godot;
using System;

public partial class TutorialGumdropHitbox : HurtboxEnemyParent
{

	/// <summary> Gumdrop object this hitbox is attatched to. </summary>
	private TutorialGumdrop gumdrop;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		gumdrop = GetParent<TutorialGumdrop>();
	}


	public override bool Accept_Hitbox(HitboxParent hitbox, int damage = 1)
	{
		gumdrop.Hurt();
		return false;
	}

}
