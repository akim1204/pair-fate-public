using Godot;
using System;

public partial class TutorialGumdrop : Sprite2D
{

	/// <summary> Bag of all players. </summary>
	private PlayerBag player_bag;

	/// <summary>
	/// The packed scene for the gumdrop profile.
	/// </summary>
	private PackedScene gumdrop_projectile;

	/// <summary>
	///  Boolean representing if its authority.
	/// </summary>
	bool authority = false;

	/// <summary>
	/// How much hp the gumdrop has.
	/// </summary>
	private int gumdrop_hp = 5;

	/// <summary> Timer until the gumdrops next bullet. </summary>
	private float fire_timer = 0;

	/// <summary>
	/// How far away the gumdrop can fire.
	/// </summary>
	private const float FIRE_RANGE = 500f;

	/// <summary> The time between each gumdrop bullet.  </summary>
	private const float FIRE_INTERVAL = 2.5f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player_bag = GetNode<PlayerBag>("/root/PlayerBag");
		gumdrop_projectile = GD.Load<PackedScene>("res://BasicEnemies/TutorialGumdrop/TutorialGumdropProjectile.tscn");


		/* Figure out if this is authority */
		if (Multiplayer.GetUniqueId() == 1)
		{
			authority = true;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		/* Locate the nearest player */
		float maxDist = FIRE_RANGE;
		Player closest_player = null;
		foreach (Player player in player_bag.GetActivePlayers())
		{
			if ((this.GlobalPosition - player.GlobalPosition).Length() < FIRE_RANGE)
			{
				maxDist = (this.GlobalPosition - player.GlobalPosition).Length();
				closest_player = player;
			}
		}

		/* If there is a closest player */
		if (closest_player != null)
		{
			fire_timer -= (float)delta;
			if (fire_timer <= 0)
			{
				fire_timer = FIRE_INTERVAL;
				if (authority)
				{
					Rpc("Fire_Projectile", closest_player.GlobalPosition);
				}
			}
		}
		else
		{
			fire_timer = FIRE_INTERVAL;
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Fire_Projectile(Vector2 target_position)
	{
		/* Instantiate projectile */
		var projectile_inst = gumdrop_projectile.Instantiate<TutorialGumdropProjectile>();
		AddChild(projectile_inst);
		projectile_inst.GlobalPosition = this.GlobalPosition;
		projectile_inst.Set_Direction(target_position - this.GlobalPosition);
	}

	public void Hurt()
	{
		Rpc("Trigger_Hurt");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Trigger_Hurt()
	{
		gumdrop_hp -= 1;
		if (gumdrop_hp <= 0)
		{
			QueueFree();
		}
	}
}
