using Godot;
using System;

public partial class ArmBossHeadHurtbox : HurtboxEnemyParent
{
	private ArmBossHead owner_head;
	private DragonBoss owner_boss;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		owner_boss = GetParent().GetParent<DragonBoss>();
		owner_head = GetParent<ArmBossHead>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public override bool Accept_Hitbox(HitboxParent hitbox, int damage = 1)
	{
		if (!owner_head.Get_Is_Vuln()) {
			return false;
		}
		Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Accept Called on Cake Hurtbox");
		owner_boss.Hurt_Hp(1);
		Rpc("Play_Hurt_Animation");
		return false;
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Play_Hurt_Animation()
	{
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(owner_boss, "modulate", new Color(2,2,2,2), .1f)
			.SetTrans(Tween.TransitionType.Elastic);
		tween.TweenProperty(owner_boss, "modulate", new Color(1f,1f,1f,1f), .02f);
	}
}
