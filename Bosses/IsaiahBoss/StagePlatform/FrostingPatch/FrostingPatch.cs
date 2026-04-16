using Godot;
using System;

public partial class FrostingPatch : Node2D
{
	InteractablePatchSpawner spawner;
	private AnimatedSprite2D _animator;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		spawner = GetParent<InteractablePatchSpawner>();
		_animator = GetNode<AnimatedSprite2D>("Sprite2D");
		_animator.Frame = new Random().Next(0,3);
	}
	public void Initialize(Vector2 pos)
	{
		this.GlobalPosition = pos;
	}

	public void Destroy()
	{
		this.QueueFree();
	}
	public void _on_fist_entered(Area2D area)
	{
		if (area.GetType().IsAssignableTo(typeof(DragonArmHurtbox))) {
			DragonArmHurtbox arm_hurtbox = area as DragonArmHurtbox;
			if (arm_hurtbox.Get_Active()) {
				Rpc("Fist_Entered", arm_hurtbox);
			}
		}
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Fist_Entered(DragonArmHurtbox arm_hurtbox)
	{
		arm_hurtbox._on_entered_frosting_patch();
		/* TODO: Play squish animation and sound */
		this.spawner.Delete_Patch();	
	}

}
