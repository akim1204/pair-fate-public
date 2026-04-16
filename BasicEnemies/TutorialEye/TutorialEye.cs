using Godot;
using System;

public partial class TutorialEye : Node2D
{
	/// <summary> Animation player for the eye.</summary>
	private AnimationPlayer eye_animator;

	/// <summary> Bag of all players. </summary>
	private PlayerBag player_bag;

	/// <summary> Boolean representing if its authority. </summary>
	bool authority = false;

	/// <summary> How far the eye can fire </summary>
	private const float FIRE_RANGE = 800f;

	/// <summary> If the eye is currently aiming </summary>
	private float aiming = 0;

	/// <summary> Base hp of the eye.</summary>
	public int hp = 2;

	private float hurt_timer = 0;

	private SoundPlayer sound_player;
	private PackedScene slime_projectile_prefab;
	private PackedScene eye_death_prefab;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Get player bag */
		player_bag = GetNode<PlayerBag>("/root/PlayerBag");

		/* Get initial animation */
		eye_animator = GetNode<AnimationPlayer>("EyeBodyAnimator");
		eye_animator.Play("Closed");

		slime_projectile_prefab = GD.Load<PackedScene>("res://BasicEnemies/TutorialEye/SlimeProjectile.tscn");
		eye_death_prefab = GD.Load<PackedScene>("res://Bosses/Jello/JelloEyeDeath.tscn");
		sound_player = GetNode<SoundPlayer>("SoundPlayer");

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

		hurt_timer = Mathf.Max(hurt_timer - (float)delta, 0);

		/* If there is a player */
		if (closest_player != null)
		{
			/* If beginning to aim */
			if (aiming == 0)
			{
				eye_animator.Play("Open");
			}

			aiming = Mathf.Min(2, aiming + (float)delta);
			/* Looking at player */
			if (aiming > 0.3 && hurt_timer <= 0)
			{
				/* Look at player */
				float angle = this.GetAngleTo(closest_player.GlobalPosition);
				if (angle > Mathf.Pi * 3 / 5)
				{
					eye_animator.Play("Left");
				}
				else if (angle > Mathf.Pi * 2 / 5)
				{
					eye_animator.Play("Down");
				}
				else
				{
					eye_animator.Play("Right");
				}
			}

			/* Only fire in authority */
			if (authority)
			{
				if (aiming > 1)
				{
					aiming -= 0.5f;
					Rpc("Spawn_Projectile", this.GlobalPosition + 380 * Vector2.Right +
					(-25 + GD.Randf() * 50) * Vector2.Up,
					closest_player.GlobalPosition);
				}
			}
		}
		else
		{
			if (aiming != 0)
			{
				eye_animator.Play("Close");
			}
			aiming = 0;
		}
	}


	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Spawn_Projectile(Vector2 source, Vector2 destination)
	{
		var inst = slime_projectile_prefab.Instantiate<SlimeProjectile>();
		AddChild(inst);
		inst.GlobalPosition = source;
		inst.Set_Direction(destination - source);
	}

	public void Hurt()
	{
		Rpc("Synced_Hurt");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Synced_Hurt()
	{
		this.hp -= 1;
		eye_animator.Play("Hurt");
		hurt_timer = 0.5f;
		if (hp < 0)
		{
			sound_player.Play_Effect("Death", -20);
			var inst = eye_death_prefab.Instantiate<JelloEyeDeath>();
			sound_player.Play_Effect("Death", -30);
			inst.GlobalPosition = this.GlobalPosition + 70 * Vector2.Down;
			inst.Scale = new Vector2(3, 3);
			GameManager.Instance.Get_World().AddChild(inst);
			QueueFree();
		}
		else
		{
			sound_player.Play_Effect("Hit", -20);
		}
	}
}