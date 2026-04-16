using Godot;
using System;

public partial class EyeScreamDroplet : Sprite2D
{
	private const float ROOM_BOTTOM = EyeScreamController.ROOM_BOTTOM;

	private const float drop_speed = 500;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		this.GlobalPosition += drop_speed * (float)delta * Vector2.Down;

		/* Destroying once out of screen */
		if (this.GlobalPosition.Y > ROOM_BOTTOM + 100)
		{
			QueueFree();
		}
	}

	public void _on_area_2d_area_entered(Area2D area)
	{

		// If it is a hurtbox
		if (area.GetType().IsAssignableTo(typeof(PlayerHurtbox)))
		{
			/* Cast to hurtboxenemyparent */
			((PlayerHurtbox)area).Hurt(1);
		}

		// If its a shield 
		if (area.GetType().IsAssignableTo(typeof(PlayerShieldHitbox)))
		{
			PlayerShieldHitbox shield_hitbox = (PlayerShieldHitbox)area;
			if (shield_hitbox.Get_Active())
			{
				Rpc("Destroy");
			}
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Destroy()
	{
		QueueFree();
	}
}
