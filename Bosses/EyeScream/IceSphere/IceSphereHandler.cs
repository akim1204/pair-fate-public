using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

public partial class IceSphereHandler : Node2D
{
	/* If the ice spheres are hidden or active */
	public bool active = false;
	/// <summary> Boundaries of the room </summary>
	private const float ROOM_LEFT = EyeScreamController.ROOM_LEFT,
	ROOM_RIGHT = EyeScreamController.ROOM_RIGHT,
	ROOM_TOP = EyeScreamController.ROOM_TOP,
	ROOM_BOTTOM = EyeScreamController.ROOM_BOTTOM;

	private Random rand = new Random();
	private PackedScene fragment_prefab;
	private EyeScreamController controller;
	private PlayerBag player_bag = GameManager.Instance.Get_Player_Bag();

	/// <summary> Radius of the sphere and the size of region mappings </summary>
	public const int SPHERE_RADIUS = 160;
	private const int SPHERE_RADIUS2 = SPHERE_RADIUS * SPHERE_RADIUS;
	private const int COL_COUNT = (int)(ROOM_RIGHT - ROOM_LEFT) / SPHERE_RADIUS + 1;
	/// <summary> Array of vectors that are 'adjacent' to a given position </summary>
	private Vector2[] adjacency_vectors = new Vector2[] { new Vector2(0, 0),
				new Vector2(SPHERE_RADIUS, 0), new Vector2(-SPHERE_RADIUS, 0),
				new Vector2(0, SPHERE_RADIUS), new Vector2(0, -SPHERE_RADIUS),
				new Vector2(SPHERE_RADIUS, SPHERE_RADIUS), new Vector2(-SPHERE_RADIUS, SPHERE_RADIUS),
				new Vector2(SPHERE_RADIUS, -SPHERE_RADIUS), new Vector2(-SPHERE_RADIUS, -SPHERE_RADIUS)  };

	/// <summary> Stored lists of ice spheres </summary>
	private Dictionary<int, IceSphere> ice_spheres = new Dictionary<int, IceSphere>();

	/// <summary> Dictionary mapping areas of the world to overlapping ice spheres </summary>
	private Dictionary<int, List<int>> ice_regions = new Dictionary<int, List<int>>();

	/// <summary> Number of ice spheres </summary>
	private const int SPHERE_COUNT = 35;

	/// <summary> List of possible colors  </summary>
	private Color[] colors = new Color[]{
		Colors.DarkRed, Colors.Blue, Colors.Yellow, Colors.Orange,
		Colors.Brown, Colors.DeepPink, Colors.Crimson, Colors.MediumAquamarine,
	};

	///						--------- TRACKING DROPPED REGIONS-----------

	/// <summary>
	/// Outermost spheres 
	/// </summary>
	private int[] outer_edge = new int[] { 12, 11, 35, 23, 24, 32 };
	private int[] middle_edge = new int[] { 13, 14, 15, 21, 22, 25 };
	private int[] brazier_spots = new int[] { 8, 16, 26 };
	private int[] others = new int[] { 1, 2, 3, 4, 5, 6, 7, 9, 10, 17, 18, 19, 20, 27, 28, 29, 30, 31, 32, 33, 34 };
	private int drop_state = 0;
	private StaticBody2D outer_boundaries, middle_boundaries;
	/*
	 * Variables for actions
	 */
	public enum SPHERE_STATES
	{
		IDLE,
		BITE_OPEN,
		BITE_CLOSE,
	};
	private SPHERE_STATES sphere_state = SPHERE_STATES.IDLE;
	private const float OPEN_ADJACENT_DELAY = 0.5f;
	private const float BITE_CLOSE_DELAY = 2;
	private float timer = 0;
	private HashSet<int> adjacent_mouths = new HashSet<int>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		controller = GetParent<EyeScreamController>();

		/* Populate region dictionary */
		int ROW_COUNT = (int)(ROOM_BOTTOM - ROOM_TOP) / SPHERE_RADIUS + 1;
		for (int i = 0; i < ROW_COUNT * COL_COUNT; i++)
		{
			ice_regions.Add(i, new List<int>());
		}
		ice_regions.Add(-1, new List<int>());

		/* Aggregate child ice spheres */
		for (int i = 1; i <= SPHERE_COUNT; i++)
		{
			IceSphere sphere = GetNode<IceSphere>("IceSphere" + i.ToString());
			sphere.Initialize(this, sphere.GlobalPosition, i, 0.05f * new Color(0, 0, 0, 1) + 0.95f * colors[GD.Randi() % colors.Length]);
			ice_spheres.Add(i, sphere);

			/* Add to regions */
			foreach (Vector2 displacement in adjacency_vectors)
			{
				int index = region_map(sphere.GlobalPosition + displacement);
				if (ice_regions.ContainsKey(index))
				{
					ice_regions[index].Add(i);
				}
			}
		}

		outer_boundaries = GetNode<StaticBody2D>("OuterBoundaries");
		middle_boundaries = GetNode<StaticBody2D>("MiddleBoundaries");

		/* Fragment */
		fragment_prefab = GD.Load<PackedScene>("res://Gameplay/Particles/Fragment.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		switch (sphere_state)
		{
			case SPHERE_STATES.BITE_OPEN:
				timer -= (float)delta;
				if (timer <= 0)
				{
					sphere_state = SPHERE_STATES.BITE_CLOSE;
					timer = BITE_CLOSE_DELAY;
					foreach (int index in adjacent_mouths)
					{
						ice_spheres[index].Open_Mouth(true);
					}
				}
				break;
			case SPHERE_STATES.BITE_CLOSE:
				timer -= (float)delta;
				if (timer <= 0)
				{
					sphere_state = SPHERE_STATES.IDLE;
					foreach (IceSphere sphere in ice_spheres.Values)
					{
						sphere.Open_Mouth(false);
					}
				}
				break;
		}

		if (Input.IsActionJustPressed("ui_two"))
		{
			Open_Eyes_Region_Close();
		}
	}

	public void Hurt(int damage)
	{
		controller.Hurt(damage);
	}

	public void Open_Eyes_Close()
	{
		if (!active) return;
		/* Select random sphere near player */
		foreach (Player player in GameManager.Instance.Get_Player_Bag().GetActivePlayers())
		{
			if (ice_regions[region_map(player.GlobalPosition)].Count > 0)
			{
				int index = (int)(GD.Randi() % ice_regions[region_map(player.GlobalPosition)].Count);
				index = ice_regions[region_map(player.GlobalPosition)][index];

				/* If under a brazier, open a random one */
				if (brazier_spots.Contains<int>(index))
				{
					Rpc("Open_Eye", others[rand.Next(others.Length)], player.Get_Id());
				}
				else
				{
					Rpc("Open_Eye", index, player.Get_Id());
				}
			}
		}
	}

	public void Open_Eyes_Region_Close()
	{
		if (!active) return;
		/* Select random sphere near player */
		Player cur_player = player_bag.GetAllPlayers()[(int)(GD.Randi() % player_bag.GetAllPlayers().Count)];

		if (ice_regions[region_map(cur_player.GlobalPosition)].Count > 0)
		{
			int region_index = region_map(cur_player.GlobalPosition + adjacency_vectors[(int)(GD.Randi() % (adjacency_vectors.Length - 1)) + 1]);
			/* Try a second time sometimes */
			if (!ice_regions.ContainsKey(region_index)) region_index = region_map(cur_player.GlobalPosition + adjacency_vectors[(int)(GD.Randi() % (adjacency_vectors.Length - 1)) + 1]);
			Rpc("Open_Eye_Region", region_index, cur_player.Get_Id());
		}
	}
	/// <summary>
	/// Opens a given eye 
	/// </summary>
	/// <param name="index"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Open_Eye(int index, int player_id)
	{
		ice_spheres[index].Open_Eye(true);
		ice_spheres[index].Set_Eye_Target(player_bag.GetPlayer(player_id));
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Open_Eye_Region(int region_index, int player_id)
	{
		if (!ice_regions.ContainsKey(region_index)) return;

		int opened = 0;

		foreach (int index in ice_regions[region_index])
		{
			/* Don't open under brazier */
			if (brazier_spots.Contains<int>(index)) continue;
			ice_spheres[index].Open_Eye(true);
			ice_spheres[index].Set_Eye_Target(player_bag.GetPlayer(player_id));

			/* Only open 4 eyes at most */
			opened += 1;
			if (opened >= 4) return;
		}
	}
	public void Open_Bites()
	{
		/* Select a random player */
		int player_count = player_bag.GetActivePlayers().Count;
		if (player_count > 0)
		{
			Player cur_player = player_bag.GetActivePlayers()[rand.Next(player_count)];

			/* Find nearby mouth */
			int bite_index = region_map(cur_player.GlobalPosition);
			var nearby_spheres = ice_regions[bite_index];
			bite_index = nearby_spheres[rand.Next(nearby_spheres.Count)];

			Rpc("Open_Bite", bite_index);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Open_Bite(int bite_index)
	{
		/* Find sphere */
		IceSphere cur_sphere = ice_spheres[bite_index];
		/* Reset set */
		adjacent_mouths.Clear();
		foreach (Vector2 adjacency in adjacency_vectors)
		{
			if (ice_regions.ContainsKey(region_map(cur_sphere.GlobalPosition + adjacency)))
			{
				foreach (int index in ice_regions[region_map(cur_sphere.GlobalPosition + adjacency)])
				{
					adjacent_mouths.Add(index);
				}
			}
		}
		sphere_state = SPHERE_STATES.BITE_OPEN;
		timer = OPEN_ADJACENT_DELAY;
		cur_sphere.Open_Mouth(true);
	}

	/// <summary>
	/// Spawns a fragment corresponding to the ice cream color at a given position
	/// </summary>
	/// <param name="fragment_position"> The position to spawn </param>
	public void Spawn_Fragment(Vector2 fragment_position)
	{
		if (!active) return;
		/* Find corresponding sphere */
		int sphere_index = find_sphere(fragment_position);
		/* If it exists */
		if (sphere_index != -1)
		{
			IceSphere sphere = ice_spheres[sphere_index];
			Color hue = sphere.Modulate;

			/* Create fragment */
			var inst = fragment_prefab.Instantiate<Fragment>();
			this.AddChild(inst);
			inst.Activate(hue, 20 + GD.Randi() % 40, 0, 2.5f, 0.8f + GD.Randf() * 0.4f);
			inst.GlobalPosition = fragment_position + new Vector2(0, GD.Randf() * 80f - 80);
		}
	}

	/// <summary>
	/// Indicates a given sphere has bitten
	/// </summary>
	/// <param name="sphere_index"></param>
	public void Bite(int sphere_index)
	{

		foreach (Player player in GameManager.Instance.Get_Player_Bag().GetActivePlayers())
		{
			/* If the player is on the biting sphere */
			if (sphere_index == find_sphere(player.GlobalPosition))
			{
				if (player.Get_Authority())
				{
					/* Instantly kill them */
					player.Try_Hurt(100);
				}
			}
		}
	}

	public void Drop()
	{
		drop_state += 1;
		Rpc("Handle_Drop", drop_state);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Handle_Drop(int stage)
	{
		drop_state = stage;

		/* Depending on stage */
		if (stage == 1)
		{ /* Outer boundaries */
			foreach (int index in outer_edge)
			{
				ice_spheres[index].Drop();
			}
		}
		else if (stage == 2)
		{ /* Middle boundaries */
			foreach (int index in middle_edge)
			{
				ice_spheres[index].Drop();
			}
		}
	}

	/// <summary>
	/// Moves the boundaries to account for dropped edges
	/// </summary>
	public void Update_Edges()
	{
		if (drop_state == 1)
		{
			outer_boundaries.Position = new Vector2(0, -30);
		}
		else if (drop_state == 2)
		{
			middle_boundaries.Position = new Vector2(0, -30);
		}
	}

	/// <summary>
	/// Returns the int index of the region containing a position
	/// </summary>
	/// <param name="position"> The position to convert</param>
	/// <returns></returns>
	private int region_map(Vector2 position)
	{
		int col = (int)(position.X - ROOM_LEFT) / SPHERE_RADIUS;
		int row = (int)(position.Y - ROOM_TOP) / SPHERE_RADIUS;
		return col + row * COL_COUNT;
	}

	/// <summary>
	/// Returns the ice sphere a given position lies on.
	/// </summary>
	/// <param name="position"> The position to check </param>
	/// <returns> The corresponding ice sphere</returns>
	private int find_sphere(Vector2 position)
	{
		int region_index = region_map(position);
		IceSphere under_sphere = null;
		if (!ice_regions.ContainsKey(region_index)) return -1;
		foreach (int ice_index in ice_regions[region_index])
		{
			IceSphere current_sphere = ice_spheres[ice_index];
			if (current_sphere.center.DistanceSquaredTo(position) < SPHERE_RADIUS2)
			{
				if (under_sphere == null || current_sphere.center.Y > under_sphere.center.Y)
				{
					under_sphere = current_sphere;
				}
			}
		}
		if (under_sphere == null) return -1;
		return under_sphere.id;
	}
}
