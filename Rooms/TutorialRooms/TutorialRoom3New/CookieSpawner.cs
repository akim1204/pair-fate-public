using Godot;
using System;

public partial class CookieSpawner : Node2D
{


	private bool authority = false;

	private float spawn_timer = 0;

	[Export]
	private float spawn_rate = 2;

	/// <summary> Packed scene for the cookie boulders. </summary>
	private PackedScene cookie_boulder;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Get prefab */
		cookie_boulder = GD.Load<PackedScene>("res://Rooms/TutorialRooms/TutorialRoom3New/CookieBoulder.tscn");

		/* Figure out if this is authority */
		if (Multiplayer.GetUniqueId() == 1)
		{
			authority = true;
			this.spawn_timer = GD.Randf() * spawn_rate;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (authority)
		{
			spawn_timer += (float)delta;
			if (spawn_timer > spawn_rate)
			{
				spawn_timer -= spawn_rate;
				spawn_timer -= GD.Randf() / 4 * spawn_rate;
				Rpc("Spawn_Boulder");
			}
		}
	}

	/// <summary>
	/// Rpc method to spawn a boulder
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Spawn_Boulder()
	{
		var inst = cookie_boulder.Instantiate<CookieBoulder>();
		CallDeferred("add_child", inst);
	}
}
