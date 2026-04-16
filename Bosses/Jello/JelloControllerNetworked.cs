using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Jello_Info
{
	/// <summary>The corresponding jello.</summary>
	public JelloNetworked jello;
	/// <summary> The action timer of the jello, swaps at 0 </summary>
	public float action_timer = 0;
	/// <summary> The current action type of the jello. 
	/// 0 - idle
	/// 1 - sliding
	/// </summary>
	public int action_type = 0;

	/// <summary> The current hp of the jello. </summary>
	public int hp = 30;

	/// <summary> The current target of the jello. </summary>
	public int player_target_id;

}
public partial class JelloControllerNetworked : BossController
{
	/// <summary>
	/// If the fight has not started.
	/// </summary>
	bool started = false;
	/// <summary>
	/// If the boss is still alive.
	/// </summary>
	public bool active = true;
	/// <summary> Prefab for the jello enemy. </summary>
	private PackedScene jello_prefab;

	/// <summary> Prefab used for the jello eye </summary>
	private PackedScene jello_eye_prefab;

	/// <summary>
	/// The fake jello body pre boss-fight.
	/// </summary>
	private PackedScene jello_fakebody_prefab;

	private Node2D jello_fakebody;

	private PackedScene jello_key_prefab;
	private JelloKey jello_key;

	private SoundPlayer sound_player;

	/// <summary> If this controller is the authority and should make decisions. </summary>
	private bool authority = false;

	/// <summary> Initial spawn location of the jello. </summary>
	private Vector2 INITIAL_SPAWN = new Vector2(2000, 1100);

	//private int[] breakpoints = {30, 20, 10, 5};
	private int[] breakpoints = { 20, 12, 6, 2, 0 };
	//private int[] breakpoints = { 8, 6, 4, 2, 0 };

	/// <summary> The number of eyes still alive. </summary>
	private int eye_count = 8;

	/// Timings used for boss ai
	private const float INITIAL_IDLE = 10;
	private const float SLIDE_WAIT = 2.5f;
	private const float SLIDE_RECOVERY = 1.5f;
	private const float SPIT_WAIT = 3.5f;
	private const float SPIT_RECOVERY = 6;
	private const float SPAWN_RECOVERY = 0.5f;

	/// <summary> How far away jellos can land when spawning. </summary>
	private const float SPAWN_DISTANCE = 600;

	/// <summary>
	/// Spawn locations of all the eyes.
	/// </summary>
	private Vector2[] eye_spawns = {
		new Vector2(200, 250),
		new Vector2(500, 600),
		new Vector2(230, 350),
		new Vector2(2000, 600),
		new Vector2(400, 1000),
		new Vector2(800, 700),
		new Vector2(1600, 350),
		new Vector2(1800, 1000),
	};

	/// <summary>
	/// Timer used for cinematic
	/// </summary>
	private float cinematic_timer = 0;
	private const float CINEMATIC_TIME = 5;


	/// <summary> Random number generator </summary>
	Random rand = new Random();

	/// <summary> Bag of all players to reference. </summary>
	private PlayerBag player_bag;

	/// <summary>
	/// Dictionary mapping jello id's to jello infos.
	/// </summary>
	private Dictionary<int, Jello_Info> jello_infos = new Dictionary<int, Jello_Info>();
	private List<int> destroyed_jellos = new List<int>();

	private Dictionary<int, JelloEyeNetworked> jello_eyes = new Dictionary<int, JelloEyeNetworked>();


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Get player bag */
		player_bag = GetNode<PlayerBag>("/root/PlayerBag");

		/* Get Jello Prefab */
		jello_prefab = GD.Load<PackedScene>("res://Bosses/Jello/JelloNetworked.tscn");

		/* Loading jello eye prefab */
		jello_eye_prefab = GD.Load<PackedScene>("res://Bosses/Jello/JelloEyeNetworked.tscn");

		/* Loading fake jello prefab */
		jello_fakebody_prefab = GD.Load<PackedScene>("res://Bosses/Jello/JelloFakebody.tscn");

		/* Loading jello key prefab */
		jello_key_prefab = GD.Load<PackedScene>("res://Rooms/Wing1/JelloRoom/JelloKey.tscn");

		/* Loading sound player */
		sound_player = GetNode<SoundPlayer>("SoundPlayer");

		/* Figure out if this is authority */
		if (Multiplayer.GetUniqueId() == 1)
		{
			authority = true;
		}

		/* Create eyes */
		for (int i = 0; i < 8; i++)
		{
			JelloEyeNetworked eye_inst = jello_eye_prefab.Instantiate<JelloEyeNetworked>();
			eye_inst.Name = "Eye" + i.ToString();
			eye_inst.Set_Controller(this);
			eye_inst.Set_Id(i);
			CallDeferred("add_child", eye_inst);
			float angle = (float)-i / 4 * Mathf.Pi + 3 * Mathf.Pi / 8;
			//eye_inst.GlobalPosition = INITIAL_SPAWN - new Vector2(Mathf.Cos(angle) * 500, Mathf.Sin(angle) * 300);
			eye_inst.GlobalPosition = eye_spawns[i];
			this.jello_eyes.Add(eye_inst.Get_Id(), eye_inst);
			eye_inst.Active = false;
		}

		/* Create fake body */
		jello_fakebody = jello_fakebody_prefab.Instantiate<Node2D>();
		jello_fakebody.GlobalPosition = INITIAL_SPAWN;
		CallDeferred("add_child", jello_fakebody);

		/* Hard set index for repeated fights */
		GameManager.Instance.Get_Death_Menu().Set_Restart_Point("res://Rooms/Wing1/JelloRoom/JelloRoom.tscn", 1);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		/* Handle all AI decisions if authority */
		if (authority)
		{
			/* For each jello */
			foreach (int jello_id in jello_infos.Keys)
			{
				handle_jello(jello_id, (float)delta);
			}
		}
	}

	/// <summary>
	/// Starts the jello fight.
	/// </summary>
	public void Start_Fight()
	{
		if (authority && started == false)
		{
			started = true;
			/* Play cinematic the first time */
			if (first_time)
			{
				Rpc("SF_First");
			}
			else
			{
				Rpc("SF_Fast");
			}
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void SF_First()
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.INFO, "Beginning Jello Bossfight");
		if (jello_infos.Count == 0)
		{
			/* Create the initial jello */
			var inst = jello_prefab.Instantiate<JelloNetworked>();
			inst.Name = "Jello0";
			inst.Set_Shadow(false);

			Jello_Info first_jello = new Jello_Info
			{
				jello = inst,
				hp = breakpoints[0],
				action_timer = INITIAL_IDLE
			};

			/* First jello has an id of 0 */
			jello_infos.Add(0, first_jello);

			/* Add this instance to the scene */
			CallDeferred("add_child", inst);
			inst.GlobalPosition = INITIAL_SPAWN;

			/* Activate eyes */
			foreach (JelloEyeNetworked eye in jello_eyes.Values)
			{
				eye.Active = true;
			}

			/* Create key */
			jello_key = jello_key_prefab.Instantiate<JelloKey>();
			jello_key.GlobalPosition = INITIAL_SPAWN;
			jello_key.Name = "item60";
			jello_key.carried = true;
			jello_key.Rotation = 1f;
			AddChild(jello_key);

			/* Initialize Jello */
			inst.Initialize_First(0, jello_eyes.Values.ToList(), INITIAL_SPAWN, 1, jello_key);
			inst.boss_state = JelloNetworked.BossStates.IDLE;

			/* Destroy Fakebody */
			jello_fakebody.CallDeferred("queue_free");

			/* Pan to Jello boss */
			foreach (var camera in player_bag.GetAllCameras())
			{
				camera.Set_Boss_Pan(jello_infos[0].jello, 6);
				camera.Bound_Camera(-150, 0, 2400, 1425);
			}

			/* Move players inside */
			var players = player_bag.GetAllPlayers();
			for (int i = 0; i < players.Count; i++)
			{
				players[i].GlobalPosition = new Vector2(950 + 100 * i, 100);

				/* Lock player movement */
				Player_Lock(true);
			}

			/* Block off door */
			var room_detector = GetNode<JelloRoomDetector>("../ExtraEntities/RoomDetector0");
			room_detector.Enable_Door();

			/* Timer for end of cinematic */
			Timer cinematic_timer = new Timer();
			cinematic_timer.OneShot = true;
			cinematic_timer.Timeout += Cinematic_Middle;
			this.AddChild(cinematic_timer);
			cinematic_timer.Start(4);

			/* Play music */
			GameManager.Instance.Play_Music("res://Sound/BackgroundTracks/Wing1_Boss.wav");

		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void SF_Fast()
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.INFO, "Beginning Jello Bossfight");
		if (jello_infos.Count == 0)
		{
			/* Create the initial jello */
			var inst = jello_prefab.Instantiate<JelloNetworked>();
			inst.Name = "Jello0";

			Jello_Info first_jello = new Jello_Info();
			first_jello.jello = inst;
			first_jello.hp = breakpoints[0];
			first_jello.action_timer = 6;

			/* First jello has an id of 0 */
			jello_infos.Add(0, first_jello);

			/* Add this instance to the scene */
			CallDeferred("add_child", inst);
			inst.GlobalPosition = INITIAL_SPAWN;

			/* Activate eyes */
			foreach (JelloEyeNetworked eye in jello_eyes.Values)
			{
				eye.Active = true;
			}
			/* Create key */
			jello_key = jello_key_prefab.Instantiate<JelloKey>();
			jello_key.GlobalPosition = INITIAL_SPAWN;
			jello_key.Name = "item60";
			jello_key.carried = true;
			jello_key.Rotation = 1f;
			AddChild(jello_key);

			/* Initialize Jello */
			inst.Initialize_First(0, jello_eyes.Values.ToList(), INITIAL_SPAWN, 1, jello_key);
			inst.boss_state = JelloNetworked.BossStates.IDLE;
			/* Destroy Fakebody */
			jello_fakebody.CallDeferred("queue_free");

			/* Block off door */
			var room_detector = GetNode<JelloRoomDetector>("../ExtraEntities/RoomDetector0");
			room_detector.Enable_Door();

			/* Bound Camera */
			foreach (var camera in player_bag.GetAllCameras())
			{
				camera.Bound_Camera(-150, 0, 2400, 1425);
			}

			/* Move players inside */
			var players = player_bag.GetAllPlayers();
			for (int i = 0; i < players.Count; i++)
			{
				players[i].GlobalPosition = new Vector2(200 + 100 * i, 1200);
			}

			/* Enable boss bar */
			canvas_gui.Is_Boss(true, BossGUI.BossStyles.JELLO);
			/* Play music */
			GameManager.Instance.Play_Music("res://Sound/BackgroundTracks/Wing1_Boss.wav");

		}
	}

	public void Cinematic_Middle()
	{

		/* Timer for end of cinematic */
		Timer cinematic_timer = new Timer();
		cinematic_timer.OneShot = true;
		cinematic_timer.Timeout += Cinematic_End;
		this.AddChild(cinematic_timer);
		cinematic_timer.Start(3);

		/* Play sound */
		sound_player.Play_Effect_Static("Roar", -25);

		/* Screen shake */
		foreach (PlayerCamera camera in player_bag.GetAllCameras())
		{
			camera.Shake(4, 15, 15);
		}

		/* Open eyes */
		foreach (JelloEyeNetworked eye in jello_eyes.Values)
		{
			eye.Open_Eye(true);
		}

		/* Display sprite */
		Texture2D boss_label = GD.Load<Texture2D>("res://Bosses/Jello/JelloSprites/JelloTitle.png");
		GameManager.Instance.Display_Sprite(boss_label, 3.5f, new Vector2(0, 500));
	}

	public void Cinematic_End()
	{
		/* Move cameras back */
		foreach (var camera in player_bag.GetAllCameras())
		{
			camera.Release();
			//camera.Bound_Camera(-150, -200, 2400, 1425);
		}

		/* Initiate a slide no matter what TODO: FIX? */
		var cur_jello = jello_infos[0];
		/* Find random player */
		int player_id = player_bag.GetActivePlayerIds()[rand.Next() % player_bag.GetActivePlayerIds().Count];

		Rpc("Slide_Initiation", 0, player_id);
		cur_jello.action_type = 1;
		cur_jello.action_timer = SLIDE_WAIT + 1;

		/* Unlock players */
		Player_Lock(false);

		/* Enable boss bar */
		canvas_gui.Is_Boss(true, BossGUI.BossStyles.JELLO);
	}

	/// <summary>
	/// Called to indicate a jello has finished its action
	/// </summary>
	/// <param name="jello_id"> The id of the jello that idled </param>
	public void Idled(int jello_id)
	{
		if (authority)
		{
			/* Make sure jello exists */
			if (jello_infos.Keys.Contains(jello_id))
			{
				var curr_jello = jello_infos[jello_id];
				Rpc("Sync_Positions", jello_id, curr_jello.jello.GlobalPosition);
				/* If just finished slide */
				if (jello_infos[jello_id].action_type == 1)
				{
					curr_jello.action_timer = SLIDE_RECOVERY;
				}
				else if (jello_infos[jello_id].action_type == 2)
				{
					curr_jello.action_timer = SPIT_RECOVERY;
				}
				else
				{
					curr_jello.action_timer = SPAWN_RECOVERY + (float)rand.NextDouble();
				}
				curr_jello.action_type = 0;
			}
		}
	}

	/// <summary> Sets the synced positions of all jellos </summary>
	/// <param name="sync_position"> The position to sync to</param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Sync_Positions(int jello_id, Vector2 sync_position)
	{
		jello_infos[jello_id].jello.Set_Sync(sync_position);
	}

	/// <summary> Sets slide indications of all jellos </summary>
	/// <param name="player_id"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Slide_Initiation(int jello_id, int player_id)
	{
		jello_infos[jello_id].jello.Set_Slide_Indicate(player_id);
		jello_infos[jello_id].player_target_id = player_id;
	}

	/// <summary> Sets slide target of all jellos </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Slide_Target(int jello_id, Vector2 target_direction)
	{
		/* Set target direction */
		jello_infos[jello_id].jello.Set_Slide(target_direction);
	}

	/// <summary> Sets spit indications of all jellos </summary>
	/// <param name="player_id"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Spit_Initiation(int jello_id, int player_id)
	{
		jello_infos[jello_id].jello.Set_Spit_Indicate(player_id);
		jello_infos[jello_id].player_target_id = player_id;
	}
	/// <summary>
	/// Causes a given jello to spit.
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Spit_Target(int jello_id, Vector2 target_direction, Vector2 sync_position, float orth_snap)
	{
		jello_infos[jello_id].jello.GlobalPosition = sync_position;
		jello_infos[jello_id].jello.Begin_Spit(target_direction, orth_snap);
	}

	/// <summary>
	/// Aligns the jello splitting between the two users.
	/// </summary>
	/// <param name="jello_id"> Id of the jello being split. </param>
	/// <param name="shield_points"> Points of the shield. </param>
	public void Split_Spit(int jello_id, Vector2[] shield_points)
	{
		Rpc("Split_Spit_RPC", jello_id, shield_points);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Split_Spit_RPC(int jello_id, Vector2[] shield_points)
	{
		jello_infos[jello_id].jello.Split_Spit(shield_points);
	}


	/// <summary>
	/// Triggers getting hurt from a source
	/// </summary>
	/// <param name="jello_id"> Jello instance id that took this damage.</param>
	/// <param name="damage">Amount of damage taken</param>
	public void Hurt(int jello_id, int damage)
	{
		Rpc("RPC_Hurt", jello_id, damage);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Hurt(int jello_id, int damage)
	{ //TODO: INCOMPLETE
		if (!jello_infos.ContainsKey(jello_id))
		{
			return;
		}
		Jello_Info curr_jello = jello_infos[jello_id];
		curr_jello.hp = Mathf.Max(curr_jello.hp - damage, 0);
		// Update health
		curr_jello.jello.Set_Hurt_Flash();


		/* If its just done */
		if (curr_jello.hp <= 0)
		{
			if (authority)
			{
				Rpc("Drop_Eye", jello_id);
			}
		}
		else
		{
			/* Calculate breakpoints */
			for (int i = 0; i < breakpoints.Length - 1; i++)
			{
				int breakpoint = breakpoints[i];
				/* If it crosses the threshhold */
				if (curr_jello.hp <= breakpoint && curr_jello.hp + damage > breakpoint)
				{
					if (authority)
					{
						Rpc("Split_Jello", jello_id, curr_jello.jello.Position, i);
					}
					break;
				}
			}
		}

		/* Update health bars */
		update_health();
	}
	/// <summary>
	/// Splits a given jello.
	/// </summary>
	/// <param name="jello_id"> The id of the jello being split</param>
	/// <param name="position"> The position that the jello is at</param>
	/// <param name="breakpoint"> The breakpoint position that this splitting crossed </param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Split_Jello(int jello_id, Vector2 position, int breakpoint)
	{
		/* Destroy the given jello */
		var eyes = jello_infos[jello_id].jello.Destroy();
		destroyed_jellos.Add(jello_id);
		bool has_key = jello_infos[jello_id].jello.has_key;
		jello_infos.Remove(jello_id);

		/* Create smaller jellos */
		for (int i = 0; i < 2; i++)
		{
			/* Calculate id */
			int new_id = jello_id * 2 + 1 + i;

			/* Create the initial jello */
			var inst = jello_prefab.Instantiate<JelloNetworked>();
			inst.Name = "Jello" + new_id.ToString();
			inst.Set_Shadow(true);


			Jello_Info new_jello = new Jello_Info();
			new_jello.jello = inst;
			new_jello.hp = breakpoints[breakpoint];
			new_jello.action_timer = 100;


			/* Give it the corresponding id */
			jello_infos.Add(new_id, new_jello);

			/* Calculate new body scale */
			float body_scale = 1 - 0.15f * breakpoint;

			/* Calculate spawn location */
			float spawn_angle = jello_id * 17 + 23 * i;
			Vector2 spawn_position = position + body_scale * SPAWN_DISTANCE * Vector2.FromAngle(spawn_angle);

			/* Add this instance to the scene */
			CallDeferred("add_child", inst);
			inst.GlobalPosition = position;
			if (i == 0 && has_key)
			{
				inst.Initialize_First(new_id, eyes.GetRange(i * eyes.Count / 2, eyes.Count / 2),
				spawn_position * body_scale, body_scale, jello_key);
			}
			else
			{
				inst.Initialize_First(new_id, eyes.GetRange(i * eyes.Count / 2, eyes.Count / 2),
				spawn_position * body_scale, body_scale);
			}
		}
	}

	/// <summary>
	/// Completely destroys and drops the eye of a jello.
	/// </summary>
	/// <param name="jello_id"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Drop_Eye(int jello_id)
	{
		/* Destroy the given jello */
		var eye = jello_infos[jello_id].jello.Destroy()[0];
		destroyed_jellos.Add(jello_id);
		if (jello_infos[jello_id].jello.has_key)
		{
			jello_key.carried = false;
		}
		eye.Set_Vulnerable();
		jello_infos.Remove(jello_id);
	}

	/// <summary>
	/// If a given eye was destroyed
	/// </summary>
	/// <param name="eye_id"></param>
	public void Eye_Destroyed(int eye_id)
	{
		Rpc("ED", eye_id);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void ED(int eye_id)
	{
		/* Destroy eye if not already destroyed */
		if (jello_eyes.ContainsKey(eye_id))
		{
			var eye = jello_eyes[eye_id];
			jello_eyes.Remove(eye_id);
			eye.Destroy();
			eye_count -= 1;

			// Update health
			update_health();

			/* If done */
			if (eye_count == 0)
			{
				active = false;
				canvas_gui.Is_Boss(false);
			}
		}
	}

	/// <summary>
	/// Adds an eye back for all users
	/// </summary>
	/// <param name="eye_id"> The id of the eye. </param>
	/// <param name="sync_position"> The position to sync the eye position to. </param>
	public void Add_Back_Eye(int eye_id, Vector2 sync_position)
	{
		Rpc("ABE", eye_id, sync_position);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void ABE(int eye_id, Vector2 sync_position)
	{
		jello_eyes[eye_id].Add_Back(sync_position);
	}

	/// <summary>
	/// Handles the ai of a single jello
	/// </summary>
	/// <param name="jello_id"></param>
	/// <param name="delta"></param>
	private void handle_jello(int jello_id, float delta)
	{

		var cur_jello = jello_infos[jello_id];

		/* Progress decision making */
		cur_jello.action_timer -= (float)delta;

		/* Doing next action */
		if (cur_jello.action_timer <= 0)
		{
			/* Beginning new action */
			if (cur_jello.action_type == 0)
			{
				/* Find player target */
				int active_players = player_bag.GetActivePlayers().Count;
				if (active_players > 0)
				{
					/* Find random player */
					int player_id = player_bag.GetActivePlayerIds()[rand.Next() % active_players];

					/* Choosing action randomly */
					int action_choice = rand.Next(8);

					/* Choosing slide */
					if (action_choice < 3)
					{
						Rpc("Slide_Initiation", jello_id, player_id);
						cur_jello.action_type = 1;
						cur_jello.action_timer = SLIDE_WAIT;
					}
					/* Choosing spit */
					else if (action_choice < 5)
					{
						/* If it has at least four eyes */
						if (cur_jello.jello.eye_count > 2)
						{
							Rpc("Spit_Initiation", jello_id, player_id);
							cur_jello.action_type = 2;
							cur_jello.action_timer = SPIT_WAIT;
						}
						/* Otherwise, slide */
						else
						{
							Rpc("Slide_Initiation", jello_id, player_id);
							cur_jello.action_type = 1;
							cur_jello.action_timer = SLIDE_WAIT;
						}
					}
					/* Waiting briefly */
					else
					{
						cur_jello.action_timer += 0.1f;
					}

				}
			}
			/* Beginning slide action */
			else if (cur_jello.action_type == 1)
			{
				Vector2 target_direction = player_bag.GetPlayer(cur_jello.player_target_id).GlobalPosition
					- cur_jello.jello.GlobalPosition;
				Rpc("Slide_Target", jello_id, target_direction);
				cur_jello.action_timer = 100;
			}
			/* Beginning spit action */
			else if (cur_jello.action_type == 2)
			{
				Vector2 target_direction = player_bag.GetPlayer(cur_jello.player_target_id).GlobalPosition
					- cur_jello.jello.GlobalPosition;
				Rpc("Spit_Target", jello_id, target_direction, cur_jello.jello.GlobalPosition, 0);
				cur_jello.action_timer = 100;
			}
		}
	}

	/// <summary>
	/// Updates the health bar of the jello
	/// </summary>
	private void update_health()
	{
		float[] health_values = new float[23];
		/* Jellos that aren't yet there or still alive */
		for (int i = 0; i < 15; i++)
		{
			health_values[i] = 1;
		}
		/* Eyes that aren't there by default */
		for (int i = 15; i < 23; i++)
		{
			health_values[i] = 0;
		}
		/* Objects already dead */
		foreach (int destroyed_id in destroyed_jellos)
		{
			health_values[destroyed_id] = 0;
		}
		/* Jellos that currently exist */
		foreach (int jello_id in jello_infos.Keys)
		{
			Jello_Info curr_jello = jello_infos[jello_id];
			for (int i = 0; i < breakpoints.Length - 1; i++)
			{
				if (curr_jello.hp <= breakpoints[i])
				{
					health_values[jello_id] = (float)(curr_jello.hp - breakpoints[i + 1]) / (float)(breakpoints[i] - breakpoints[i + 1]);
					continue;
				}
			}
		}
		/* Include eyes that still exist */
		foreach (int eye_id in jello_eyes.Keys)
		{
			health_values[15 + eye_id] = 1;
		}
		/* Update it */
		canvas_gui.Update_Boss_Health(health_values);
	}
}
