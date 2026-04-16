using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public partial class DragonBoss : BossController
{
	private Arm right_arm;
	private Arm left_arm;
	private ArmBossHead head;
	public BossStates _state;
	public enum BossStates
	{
		FIRST_PHASE,
		TRANSITION,
		SECOND_PHASE,
		DEAD,
	};

	[Export]
	public float p1_hp = 5;
	[Export]
	public float p2_hp = 12;
	/* Set value to sum of p1 and p2 hp*/
	private const float BOSS_MAX = 17;
	/// <summary>
	/// Delay of the two arms swinging.
	/// </summary>
	private float arm_delay = 1f;
	private int attack_idx = 0;

	private List<Player> player_list;
	private PlayerBag player_bag;
	private Random rand;
	/// <summary>
	/// Node representing world space.
	/// </summary>
	protected Node WORLD;
	private DamageZone zone_blue;
	private DamageZone zone_red;
	private float zone_move = 0;
	private float[] d_phases = new float[4];
	private float[] h_phases = new float[4];
	private float stage_attack_timer;
	private float color_timer = 15f;
	private PackedScene fallobj_prefab;
	private PackedScene candle_prefab;
	private bool started = false;
	private bool paused = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.player_bag = GameManager.Instance.Get_Player_Bag();
		WORLD = GameManager.Instance.Get_World();
		this.fallobj_prefab = GD.Load<PackedScene>("res://Bosses/IsaiahBoss/StagePlatform/Hazards/FallingObject.tscn");
		this.candle_prefab = GD.Load<PackedScene>("res://Bosses/IsaiahBoss/StagePlatform/CandleMissile/CandleMissile.tscn");

		rand = new Random();

		this._state = BossStates.FIRST_PHASE;
		right_arm = GetNode<Arm>("DragonArm2");
		left_arm = GetNode<Arm>("DragonArm");
		head = GetNode<ArmBossHead>("DragonHead");

		zone_blue = GetNode<DamageZone>("DamageZoneBlue");
		zone_red = GetNode<DamageZone>("DamageZoneRed");

		d_phases[0] = .5f; // approach
		d_phases[1] = .8f; // pause
		d_phases[2] = .2f; // slam
		d_phases[3] = .5f; // vuln time 

		h_phases[0] = .5f;
		h_phases[1] = 1f;
		h_phases[2] = .2f;
		h_phases[3] = .5f;
		Start_Fight();


	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_state == BossStates.DEAD)
		{
			return;
		}
		if (arm_delay > 0)
		{
			arm_delay -= (float)delta;
		}
		if (_state == BossStates.FIRST_PHASE && !paused)
		{
			Handle_First_Phase(delta);
		}
		else if (_state == BossStates.SECOND_PHASE)
		{
			Handle_Second_Phase(delta);
		}
		if ((this.head.Get_Weak_Color() == ArmBossHead.WeakColor.RED & zone_red.Get_Active()) |
			(this.head.Get_Weak_Color() == ArmBossHead.WeakColor.BLUE & zone_blue.Get_Active()) |
			(this.head.Get_Weak_Color() == ArmBossHead.WeakColor.PURPLE & zone_blue.Get_Active() & zone_red.Get_Active()))
		{
			// GD.Print("Head color matched, vuln now");
			this.head.Set_Vuln(true);
		}
		else
		{
			this.head.Set_Vuln(false);
		}
		if (color_timer < 0)
		{
			this.head.Switch_Colors();
			color_timer = rand.Next(3, 7);
			// Hurt_Hp(2);
		}
		color_timer -= (float)delta;

		// zone_move += (float)delta;
		// if (zone_move >= Mathf.Pi * 32)
		// {
		// 	zone_move -= Mathf.Pi * 32;
		// }
		// zone_blue.GlobalPosition += new Vector2(400 * Mathf.Cos(zone_move),0) * (float)delta;
		// zone_red.GlobalPosition -= new Vector2(400 * Mathf.Cos(zone_move),0) * (float)delta;

	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Begin_Slam(Vector2 target, float[] phases, bool left_arm)
	{
		if (left_arm)
		{
			this.left_arm.Begin_Slam(target, phases);
		}
		else
		{
			this.right_arm.Begin_Slam(target, phases);
		}

	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Begin_Heavy_Slam(Vector2 target, float[] phases, bool left_arm)
	{
		if (left_arm)
		{
			this.left_arm.Begin_Heavy_Slam(target, phases);
		}
		else
		{
			this.right_arm.Begin_Heavy_Slam(target, phases);
		}
	}

	public Vector2 Get_Rand_Player_Pos()
	{
		this.player_list = player_bag.GetActivePlayers();
		if (player_bag == null | player_list.Count == 0)
		{
			GD.Print("player bag is null or players are dead");
			return new Vector2(500, 500);
		}
		Vector2 pos = player_list[rand.Next(0, player_list.Count)].GlobalPosition;


		return pos - new Vector2(0, 40); // Correct for weird positioning
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Kill_Boss()
	{
		this.p1_hp = 0;
	}

	public void Heal_Hp(int hp)
	{
		GD.Print("I am healed");
		Rpc("RPC_Heal", hp);
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Heal(int hp)
	{
		if (_state == BossStates.FIRST_PHASE)
		{
			p1_hp += hp;
		}
		else
		{
			p2_hp += hp;
		}
		canvas_gui.Update_Boss_Health(new float[] { p2_hp + p2_hp, BOSS_MAX });


	}
	public void Hurt_Hp(int hp)
	{
		// GD.Print("I am hurt");
		Rpc("RPC_Hurt", hp);
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Hurt(int hp)
	{
		foreach (PlayerCamera cam in player_bag.GetAllCameras())
		{
			cam.Shake(.2f, 15f, 15f);
		}
		if (_state == BossStates.FIRST_PHASE)
		{
			p1_hp -= hp;
		}
		else
		{
			p2_hp -= hp;
		}
		canvas_gui.Update_Boss_Health(new float[] { p2_hp + p1_hp, BOSS_MAX });


	}
	public void Begin_Second_Phase()
	{
		Rpc("RPC_Begin_Second_Phase");
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Begin_Second_Phase()
	{
		/* activate attack. */
		this._state = BossStates.SECOND_PHASE;
		// WORLD.GetNode<Node>("interactable0").QueueFree();

	}
	private void Handle_First_Phase(double delta)
	{

		// if (this.right_arm.Get_Is_Vuln() & this.left_arm.Get_Is_Vuln()) {
		// 	this.head.Set_Vuln(true);
		// }
		if (this.p1_hp <= 0)
		{
			Begin_Second_Phase();
		}
		if (!this.left_arm.Get_Slamming() & arm_delay <= 0)
		{
			Vector2 target_player = Get_Rand_Player_Pos();
			if (rand.Next(2) == 1)
			{
				Rpc("RPC_Begin_Slam", target_player, d_phases, true);
			}
			else
			{
				Rpc("RPC_Begin_Heavy_Slam", target_player, d_phases, true);
			}
		}
		if (!this.right_arm.Get_Slamming())
		{
			Vector2 target_player = Get_Rand_Player_Pos();
			if (rand.Next(2) == 1)
			{
				Rpc("RPC_Begin_Slam", target_player, d_phases, false);
			}
			else
			{
				Rpc("RPC_Begin_Heavy_Slam", target_player, d_phases, false);
			}
		}

	}
	private void Handle_Second_Phase(double delta)
	{
		/* For now, delete the boss when dead */
		if (this.p2_hp <= 0)
		{
			Death_Actions();
		}
		/* Continue doing arm slams CONSOLIDATE THIS LATER */
		if (!this.left_arm.Get_Slamming())
		{
			Vector2 target_player = Get_Rand_Player_Pos();
			if (rand.Next(2) == 1)
			{
				Rpc("RPC_Begin_Slam", target_player, d_phases, true);
			}
			else
			{
				Rpc("RPC_Begin_Heavy_Slam", target_player, d_phases, true);
			}
		}
		if (!this.right_arm.Get_Slamming())
		{
			Vector2 target_player = Get_Rand_Player_Pos();
			if (rand.Next(2) == 1)
			{
				Rpc("RPC_Begin_Slam", target_player, d_phases, false);
			}
			else
			{
				Rpc("RPC_Begin_Heavy_Slam", target_player, d_phases, false);
			}
		}

		if (this.stage_attack_timer < 0)
		{
			if (rand.Next(2) == 1)
			{
				for (int j = 0; j < 4; j++)
				{
					float y_level = 864 + 192 * j;
					Vector2[] spawns_list = Generate_Object_Spawns(y_level);
					Rpc("RPC_Drop_Objects", spawns_list);
				}
			}
			else
			{
				float row = 816 + rand.Next(4) * 96f;
				Rpc("RPC_Launch_Candles", row);
			}
			this.stage_attack_timer = rand.Next(6, 13);

		}
		stage_attack_timer -= (float)delta;

	}
	private Vector2[] Generate_Object_Spawns(float y_lev)
	{
		Vector2[] spawn_list = new Vector2[12];
		float x_line = -288f;
		for (int i = 0; i < 12; i++)
		{
			float add_x = rand.Next(32) * (rand.Next(2) - 1);
			float add_y = rand.Next(96) * (rand.Next(2)) - 1;

			spawn_list[i] = new Vector2(x_line + add_x, y_lev + add_y);
			x_line += 192;
		}
		return spawn_list;
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Drop_Objects(Vector2[] spawns)
	{
		this.left_arm.Begin_Sky_Slam(new Vector2(384, 864));
		this.right_arm.Begin_Sky_Slam(new Vector2(1152, 864));
		for (int k = 0; k < spawns.Length; k++)
		{
			FallingObject fall_obj = this.fallobj_prefab.Instantiate<FallingObject>();
			WORLD.CallDeferred("add_child", fall_obj);
			fall_obj.Initialize(new Vector2(spawns[k].X, 0), spawns[k], 3, "cake");
		}
		arm_delay = 4f;
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Launch_Candles(float first_y)
	{
		for (int i = 0; i < 5; i++)
		{
			float y_level = first_y + 96 * i;
			CandleMissile candle = candle_prefab.Instantiate<CandleMissile>();
			WORLD.CallDeferred("add_child", candle);
			candle.Initialize(new Vector2(2000, 0), 0f, new Vector2(-352, y_level), i * .25f);
		}
	}
	public BossStates Get_Boss_State()
	{
		return this._state;
	}
	public void Start_Fight()
	{
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
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void SF_First()
	{

		/* Move players inside */
		var players = player_bag.GetAllPlayers();
		for (int i = 0; i < players.Count; i++)
		{
			Player_Lock(true);
			paused = true;
		}

		/* Bound Camera */
		foreach (var camera in player_bag.GetAllCameras())
		{
			camera.Bound_Camera(-576, 320, 2104, 1632);
			camera.Set_Boss_Pan(head, 6);
		}

		/* Play music */
		GameManager.Instance.Play_Music("res://Sound/BackgroundTracks/Wing1_Boss.wav");


		/* Timer for end of cinematic */
		Timer cinematic_timer = new Timer
		{
			OneShot = true
		};
		cinematic_timer.Timeout += Cinematic_Middle;
		this.AddChild(cinematic_timer);
		cinematic_timer.Start(2);
	}

	public void Cinematic_Middle()
	{

		/* Timer for end of cinematic */
		Timer cinematic_timer = new Timer();
		cinematic_timer.OneShot = true;
		cinematic_timer.Timeout += Cinematic_End;
		this.AddChild(cinematic_timer);
		cinematic_timer.Start(4.4);

		/* Enable boss bar */
		canvas_gui.Is_Boss(true, BossGUI.BossStyles.CAKE);
		canvas_gui.Update_Boss_Health(new float[] { p1_hp + p2_hp, BOSS_MAX }, true);

		/* Display sprite */
		Texture2D boss_label = GD.Load<Texture2D>("res://Bosses/IsaiahBoss/terrormisu_banner.png");
		GameManager.Instance.Display_Sprite(boss_label, 3.5f, new Vector2(0, 750));
	}

	public void Cinematic_End()
	{
		/* Move cameras back */
		foreach (var camera in player_bag.GetAllCameras())
		{
			camera.Release();
			//camera.Bound_Camera(-150, -200, 2400, 1425);
		}

		/* Unlock players */
		Player_Lock(false);
		paused = false;

		/* Enable boss bar */
		canvas_gui.Is_Boss(true, BossGUI.BossStyles.CAKE);
		canvas_gui.Update_Boss_Health(new float[] { p1_hp + p2_hp, BOSS_MAX }, true);

		/* Hide head */
		Timer cinematic_timer = new Timer
		{
			OneShot = true
		};
		this.AddChild(cinematic_timer);
		cinematic_timer.Start(0.5f);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void SF_Fast()
	{
		/* Enable boss bar */
		canvas_gui.Is_Boss(true, BossGUI.BossStyles.CAKE);
		canvas_gui.Update_Boss_Health(new float[] { p1_hp + p2_hp, BOSS_MAX }, true);

	}

	public void Death_Actions()
	{
		this.head.QueueFree();
		this.right_arm.QueueFree();
		this.left_arm.QueueFree();
		GD.Print("Terrormisu killed!");
		this._state = BossStates.DEAD;
		WORLD.GetNode("ExtraEntities/Exit Barrier").QueueFree();
	}

}
