using Godot;
using System;

public partial class TutorialEyeHitbox : HurtboxEnemyParent
{

	/// <summary>
	/// Tutorial eye this is attatched to.
	/// </summary>
	private TutorialEye tutorial_eye;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tutorial_eye = GetParent<TutorialEye>();
	}

	public override bool Accept_Hitbox(HitboxParent hitbox, int damage = 1)
	{
		tutorial_eye.Hurt();
		return false;
	}
}
