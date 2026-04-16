using Godot;
using System;

public partial class TrainingDummyHitbox : HurtboxEnemyParent
{
	private int hp;
	private Sprite2D dummy_sprite;
	private Sprite2D dummy_body;
	private AnimationPlayer dummy_animation_player;

	private SoundPlayer sound_player;

	private float sway_timer = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		hp = 10;
		dummy_sprite = GetParent<Sprite2D>();
		dummy_body = dummy_sprite.GetNode<Sprite2D>("DummyBody");
		dummy_animation_player = dummy_sprite.GetNode<AnimationPlayer>("AnimationPlayer");
		dummy_animation_player.Play("Healthy");
		sound_player = dummy_sprite.GetNode<SoundPlayer>("SoundPlayer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		/*
		if (hp <= 0) {
			EmitSignal(SignalName.EndPhaseOne);
			GetParent().QueueFree();
		}*/

		if (sway_timer > 0)
		{
			sway_timer = Mathf.Max(0, sway_timer - (float)delta);
		}
		if (sway_timer < 0)
		{
			sway_timer = Mathf.Min(0, sway_timer + (float)delta);
		}
		if (hp > 0)
		{
			dummy_body.Rotation = Mathf.Sin(sway_timer * 8) * Mathf.Sqrt(Mathf.Abs(sway_timer) / 4);
		}
	}

	public override bool Accept_Hitbox(HitboxParent hitbox, int damage = 1)
	{
		Rpc("Trigger_Hit", damage);
		return false;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Trigger_Hit(int damage)
	{
		hp -= damage;
		if (sway_timer > 0) sway_timer = -3;
		else sway_timer = 3;
		/* Play sound &*/
		if (hp >= 0)
			sound_player.Play_Effect("Hit", -20);
		if (hp > 6)
		{
			dummy_animation_player.Play("HealthyHurt");
			hp -= 1;
		}
		else if (hp > 3)
		{
			dummy_animation_player.Play("HurtHurt");
			hp -= 1;
		}
		else if (hp > 0)
		{
			dummy_animation_player.Play("BrokenHurt");
		}
		else if (hp == 0)
		{
			dummy_body.QueueFree();
		}
	}
}
