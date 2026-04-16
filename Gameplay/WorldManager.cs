using Godot;
using System;

public partial class WorldManager : Node2D
{
	private PackedScene player_blue_scene;
	private PackedScene player_red_scene;
	private PackedScene player_camera_scene;
	private PackedScene canvas_GUI;

	private PlayerBag player_bag;

	/// <summary> Path to the background music. If empty, will not replace current music. </summary>
	[Export]
	private string bgm_path = "";

	/// <summary> List of item scenes to be instantiated. </summary>
	[Export]
	private PackedScene[] item_scenes;

	/// <summary> List of interactable scenes to be instantiated. </summary>
	[Export]
	private PackedScene[] interactable_scenes;

	/// <summary> List of boss scenes to be instantiated. </summary>
	[Export]
	private PackedScene[] boss_scenes;

	/// <summary> Boundaries of room </summary>
	[Export]
	private int ROOM_LEFT;
	[Export]
	private int ROOM_RIGHT;
	[Export]
	private int ROOM_TOP;
	[Export]
	private int ROOM_BOTTOM;

	bool players_dead = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Playing background music if available */
		if (bgm_path != null)
		{
			GameManager.Instance.Play_Music(bgm_path);
		}

		/* Get player bag */
		player_bag = GetNode<PlayerBag>("/root/PlayerBag");

		/* Load scenes for red and blue players */
		player_blue_scene = GD.Load<PackedScene>("res://Player/PlayerBlue.tscn");
		player_red_scene = GD.Load<PackedScene>("res://Player/PlayerRed.tscn");
		player_camera_scene = GD.Load<PackedScene>("res://Player/PlayerCamera.tscn");

		/* Loading GUI */
		canvas_GUI = GD.Load<PackedScene>("res://Player/PlayerGUI/CanvasGUI.tscn");

		/* Instantiate GUI */
		var GUI_instance = canvas_GUI.Instantiate<CanvasGUI>();
		AddChild(GUI_instance);

		/* Get the current spawn index */
		int spawn_index = GameManager.Instance.Get_Spawn_Index();

		/* Spawn players at their locations */
		int player_index = 0;
		foreach (var item in NetworkManager.Network_Player_Infos)
		{
			Player player_instance;
			if (item.Player_Style == 0)
			{
				player_instance = player_blue_scene.Instantiate<Player>();
				player_instance.player_color = "blue";
			}
			else
			{
				player_instance = player_red_scene.Instantiate<Player>();
				player_instance.player_color = "red";
			}
			player_instance.Name = item.Id.ToString();
			player_instance.Initiate_GUI(GUI_instance);
			foreach (Node2D spawn_location in GetTree().GetNodesInGroup("player_spawns"))
			{
				if (spawn_location.Name == spawn_index.ToString() + "-" + player_index.ToString())
				{
					player_instance.GlobalPosition = spawn_location.GlobalPosition;
				}
			}
			/* Create camera */
			var new_camera = player_camera_scene.Instantiate<PlayerCamera>();
			player_instance.Initiate_Camera(new_camera, ROOM_LEFT, ROOM_TOP, ROOM_RIGHT, ROOM_BOTTOM, Multiplayer.GetUniqueId());
			AddChild(player_instance);
			AddChild(new_camera);
			player_index += 1;
		}

		/* Spawn items at their locations */
		int item_index = 0;
		if (item_scenes != null)
		{
			foreach (var item_scene in item_scenes)
			{
				Item item_instance = item_scene.Instantiate<Item>();
				item_instance.Name = "item" + item_index.ToString();
				AddChild(item_instance);
				foreach (Node2D spawn_location in GetTree().GetNodesInGroup("item_spawns"))
				{
					if (spawn_location.Name == item_index.ToString())
					{
						item_instance.GlobalPosition = spawn_location.GlobalPosition;
						item_instance.Rotation = spawn_location.Rotation;
					}
				}
				item_index += 1;
			}
		}

		/* Spawn interactables at their locations */
		int interactable_index = 0;
		int dialogue_index = 0; int door_index = 0; /* Indexes for dialogue and door */
		if (interactable_scenes != null)
		{
			foreach (var interactable_scene in interactable_scenes)
			{
				Interactable interactable_instance = interactable_scene.Instantiate<Interactable>();
				interactable_instance.Name = "interactable" + interactable_index.ToString();
				foreach (Node2D spawn_location in GetTree().GetNodesInGroup("interactable_spawns"))
				{
					if (spawn_location.Name == interactable_index.ToString())
					{
						interactable_instance.GlobalPosition = spawn_location.GlobalPosition;
					}
				}

				/* Set its type index if applicable */
				if (interactable_instance.Get_Type() == Interactable.InteractableTypes.Door)
				{
					interactable_instance.Set_Type_Index(door_index);
					door_index += 1;
				}
				if (interactable_instance.Get_Type() == Interactable.InteractableTypes.Dialogue)
				{
					interactable_instance.Set_Type_Index(dialogue_index);
					dialogue_index += 1;
				}

				AddChild(interactable_instance);
				interactable_index += 1;
			}
		}

		/* Spawn bosses at their locations */
		int boss_index = 0;
		if (boss_scenes != null)
		{
			foreach (var boss_scene in boss_scenes)
			{
				/* Initialize boss controller */
				BossController boss_instance = boss_scene.Instantiate<BossController>();
				boss_instance.Name = "bossController" + boss_index.ToString();
				boss_instance.Initiate_GUI(GUI_instance);
				foreach (Node2D spawn_location in GetTree().GetNodesInGroup("boss_spawns"))
				{
					if (spawn_location.Name == boss_index.ToString())
					{
						boss_instance.GlobalPosition = spawn_location.GlobalPosition;
					}
				}
				/* If its the first time (spawning from 0) */
				if (spawn_index == 0)
				{
					boss_instance.Set_First();
				}
				AddChild(boss_instance);
				boss_index += 1;
			}
		}

		/* If there's a boss, hp bar is visible */
		//if (boss_index > 0) {
		//	GUI_instance.Is_Boss(true);
		//}

		/* Setup movement inhibitor */
		MovementInhibitor inhibitor = GetNodeOrNull<MovementInhibitor>("MovementInhibitor");
		if (inhibitor != null)
		{
			inhibitor.Seek();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		/* Restarting ?? */
		if (!players_dead && player_bag.GetActivePlayers().Count == 0)
		{
			// RELOAD SCENE? TODO: BETTER WAY TO STORE THIS
			GameManager.Instance.Show_Death();
			players_dead = true;
		}
	}
}
