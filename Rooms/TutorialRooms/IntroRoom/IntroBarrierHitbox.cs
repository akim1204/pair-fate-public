using Godot;
using System;

public partial class IntroBarrierHitbox : HurtboxEnemyParent
{
	private SoundPlayer sound_player;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sound_player = GetNode<SoundPlayer>("SoundPlayer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override bool Accept_Hitbox(HitboxParent hitbox, int damage = 1)
	{
		Rpc("Destroy");
		return false;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Destroy()
	{
		// TODO: AWKWARD PROBLEM?
		sound_player.Play_Effect("Break", -25);
		this.GetParent<Node2D>().QueueFree(); //TODO: PLAY ANIMATION BEFORE DESTROYING
	}
}
