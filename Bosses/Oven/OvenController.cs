using Godot;
using System;
using System.Collections.Generic;

public partial class OvenController : BossController
{

	private SoundPlayer sound_player;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sound_player = GetNode<SoundPlayer>("SoundPlayer");

		/* Checking authority */
		authority = Multiplayer.GetUniqueId() == 1;

		/* Hard set index for repeated fights */
		GameManager.Instance.Get_Death_Menu().Set_Restart_Point("res://Bosses/Oven/OvenTest.tscn", 1);

		/* Getting player bag */
		player_bag = GameManager.Instance.Get_Player_Bag();

		/* Create oven body */
		oven_body_prefab = GD.Load<PackedScene>("res://Bosses/Oven/OvenBody/OvenBody.tscn");
		oven_body = oven_body_prefab.Instantiate<OvenBody>();
		oven_body.Set_Controller(this);
		this.AddChild(oven_body);

		/* Cinematic values */
		original_chocolate = GetNode<TextureRect>("OriginalChocolate");
		full_lava = GetNode<TextureRect>("FullLava");
		full_lava.Modulate = new Color(1, 1, 1, 0);
		if (!first_time)
		{
			original_chocolate.Modulate = new Color(1, 1, 1, 0);
			oven_body.Activate();
			full_lava.Modulate = new Color(1, 1, 1, 1);
		}

		/* Create Snake Head */
		snake_head_prefab = GD.Load<PackedScene>("res://Bosses/Oven/OvenSnakeHead/OvenSnakeHead.tscn");
		fragment_prefab = GD.Load<PackedScene>("res://Gameplay/Particles/Fragment.tscn");
		snake_head = snake_head_prefab.Instantiate<OvenSnakeHead>();
		snake_head.GlobalPosition = new Vector2(0, -200);
		this.AddChild(snake_head);

		/* Create chocolate platforms */
		oven_chocolate_prefab = GD.Load<PackedScene>("res://Bosses/Oven/OvenChocolate/ChocolateColumn.tscn");
		for (int i = 0; i < PLATFORM_COUNT; i++)
		{
			var choc_inst = oven_chocolate_prefab.Instantiate<ChocolateColumn>();
			platforms.Add(i, choc_inst);
			choc_inst.Name = "ChocolatePlatform" + i.ToString();
			choc_inst.GlobalPosition = new Vector2((i / 2) * 990 + (i % 2) * 380 + 90, 120);
			this.AddChild(choc_inst);
			//choc_inst.Clear_Line(0, 4);
		}

		/* Create and shuffle initial list */
		List<int> fake_list = new List<int>(0);
		for (int i = 0; i < PLATFORM_COUNT; i++)
		{
			fake_list.Add(i);
		}
		for (int i = PLATFORM_COUNT - 1; i >= 0; i--)
		{
			int index = rand.Next(i);
			platform_choices.Add(fake_list[i]);
			fake_list.RemoveAt(i);
		}

		/* Initialize player ignition */
		foreach (int pId in player_bag.GetActivePlayerIds())
		{
			player_ignition.Add(pId, 0);
		}


		/* Initial state for head */
		head_mode = HEAD_STATES.HIDING;
		head_timer = 6;
		/* Generate new head location */
		head_location = new Vector2(360 + rand.Next(1200), 300 + rand.Next(840));

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		/* Handle pulling of platforms */
		if (active)
		{
			if (authority)
			{
				handle_platforms((float)delta);

				handle_snake((float)delta);

				handle_snake_b((float)delta);
			}


			/* Check ignition of players */
			check_ignition((float)delta);

			/* Redraw things */
			QueueRedraw();

		}
	}

	public override void _Draw()
	{
		foreach (Player player in player_bag.GetActivePlayers())
		{
			/* Draw ignition bar above user player */
			if (player.Get_Authority())
			{
				DrawLine(player.GlobalPosition - new Vector2(50, 100),
				player.GlobalPosition - new Vector2(50 - 100 * Mathf.Min(player_ignition[player.Get_Id()] / IGNITION_CAP, 1), 100),
				Colors.Red, 15);
			}
		}
	}

	public void Start_Fight()
	{

		if (authority && active == false)
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
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void SF_First()
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.INFO, "Beginning Oven Bossfight");

		/* Move players inside */
		var players = player_bag.GetAllPlayers();
		for (int i = 0; i < players.Count; i++)
		{
			players[i].GlobalPosition = new Vector2(300 + 1320 * i, 1060);
			/* Lock player movement */
			Player_Lock(true);
		}

		/* Block off door */
		var room_detector = GetNode<OvenRoomDetector>("../ExtraEntities/RoomDetector0");
		room_detector.Enable_Door();

		/* Bound Camera */
		foreach (var camera in player_bag.GetAllCameras())
		{
			camera.Bound_Camera(-50, 0, 1970, 1400);
			camera.Set_Boss_Pan(oven_body, 6);
		}

		/* Play music */
		GameManager.Instance.Play_Music("res://Sound/BackgroundTracks/ovenMusic.wav");


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
		sound_player.Play_Effect_Static("oven_roar", -25, 0.45f);

		/* Timer for end of cinematic */
		Timer cinematic_timer = new Timer();
		cinematic_timer.OneShot = true;
		cinematic_timer.Timeout += Cinematic_End;
		this.AddChild(cinematic_timer);
		cinematic_timer.Start(4.4);

		/* Enable boss bar */
		canvas_gui.Is_Boss(true, BossGUI.BossStyles.OVEN);
		canvas_gui.Update_Boss_Health(new float[] { BOSS_HP_MAX, BOSS_HP_MAX }, true);

		/* Hide chocolate and ignite oven*/
		oven_body.Activate();
		oven_body.Full_Ignite();
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(original_chocolate, "modulate", new Color(1, 1, 1, 0), 2.0f);
		tween = GetTree().CreateTween();
		tween.TweenProperty(full_lava, "modulate", new Color(1, 1, 1, 1), 2.0f);

		/* Screen shake */
		foreach (PlayerCamera camera in player_bag.GetAllCameras())
		{
			camera.Shake(2, 10, 5);
		}

		/* Get head to pop up */
		reveal_head(new Vector2(960, 480));

		/* Display sprite */
		Texture2D boss_label = GD.Load<Texture2D>("res://Bosses/Oven/OvenSprites/Banner.png");
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

		/* Enable boss bar */
		canvas_gui.Is_Boss(true, BossGUI.BossStyles.OVEN);
		canvas_gui.Update_Boss_Health(new float[] { BOSS_HP_MAX, BOSS_HP_MAX }, true);

		/* Hide head */
		snake_head.Lower_Head();
		Timer cinematic_timer = new Timer
		{
			OneShot = true
		};
		cinematic_timer.Timeout += hide_head;
		this.AddChild(cinematic_timer);
		cinematic_timer.Start(0.5f);

		/* Activate oven */
		oven_body.Extinguish();
		/* Generate new head location */
		head_location = new Vector2(360 + rand.Next(1200), 300 + rand.Next(840));
		active = true;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void SF_Fast()
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.INFO, "Beginning Oven Bossfight");

		/* Move players inside */
		var players = player_bag.GetAllPlayers();
		for (int i = 0; i < players.Count; i++)
		{
			players[i].GlobalPosition = new Vector2(300 + 1320 * i, 1200);
			GD.Print(players[i].GlobalPosition);
		}

		/* Block off door */
		var room_detector = GetNode<OvenRoomDetector>("../ExtraEntities/RoomDetector0");
		room_detector.Enable_Door();

		/* Bound Camera */
		foreach (var camera in player_bag.GetAllCameras())
		{
			camera.Bound_Camera(-50, 0, 1970, 1400);
		}

		/* Enable boss bar */
		canvas_gui.Is_Boss(true, BossGUI.BossStyles.OVEN);
		canvas_gui.Update_Boss_Health(new float[] { BOSS_HP_MAX, BOSS_HP_MAX }, true);

		/* Play music */
		GameManager.Instance.Play_Music("res://Sound/BackgroundTracks/ovenMusic.wav");

		/* Activate oven */
		active = true;
	}
	/*
	 *			---------- MODE FUNCTIONS -----------
	 */
	/// <summary>
	/// Handles calculations for platforms
	/// </summary>
	private void handle_platforms(float delta)
	{
		platform_timer -= delta;
		if (platform_timer < 0)
		{
			/* Turning off oven */
			if (platform_mode == 1)
			{
				platform_mode = 0;
				platform_timer += PLATFORM_REST_TIME;
				/* Enraged */

				/* Enrage timer */
				if (player_bag.GetActivePlayers().Count == 1)
				{
					platform_timer = 0;
				}
				Rpc("stop_platform");
			}
			else
			{
				platform_mode = 1;
				float new_timer = platform_timer + PLATFORM_PULL_TIME * ((float)rand.NextDouble() / 3 + 1);
				Rpc("set_platform_timer", new_timer);
				choose_platform();
			}
		}
	}

	/// <summary>
	/// Handles calculations for snake
	/// </summary>
	private void handle_snake(float delta)
	{
		if (head_mode != HEAD_STATES.INACTIVE)
		{
			head_timer -= delta;
			/* Constant timings */
			if (head_mode == HEAD_STATES.HIDING)
			{
				/* Spawning fragments */
				if (head_timer < 3)
				{
					if (head_timer + delta >= 3)
					{
						Rpc("play_static", "rumble", -20, 0.8f);
					}
					head_secondary_timer -= delta;
					if (head_secondary_timer < 0)
					{
						Rpc("spawn_fragment", head_location + new Vector2(-100 + rand.Next(200), -100 + rand.Next(200)));
						head_secondary_timer += FRAGMENT_INTERVAL;
					}
				}
			}
			if (head_mode == HEAD_STATES.SHOWING)
			{
				/* Spawning reverberations */
				head_secondary_timer -= delta;
				if (head_secondary_timer < 0)
				{
					if (reverberation_count < SHOWING_DIVISIONS)
					{
						float radius_part = HEAD_CLEAR_RADIUS / SHOWING_DIVISIONS;
						Rpc("head_reverberation", head_location,
							radius_part * reverberation_count,
							radius_part * (reverberation_count + 1));
						reverberation_count += 1;
						head_secondary_timer += SHOWING_INTERVAL;
					}
				}

				/* Head spawning */
				snake_head.Set_Head(head_location + (200 - head_timer * 200 / (SHOWING_DIVISIONS * SHOWING_INTERVAL)) * Vector2.Up);
			}
			if (head_mode == HEAD_STATES.SHOWN)
			{
				head_secondary_timer -= delta;
				if (head_secondary_timer < 0)
				{
					if (slam_count > 0)
					{
						head_secondary_timer += HEAD_SLAM_INTERVAL;
						Rpc("slam_head", head_location +
							125 * Vector2.FromAngle(slam_count * Mathf.Pi / 2 + Mathf.Pi / 4));
						slam_count -= 1;
					}
				}
				if (head_timer < 0.6f && head_secondary_timer < 0)
				{
					Rpc("hide_head_anim");
					head_secondary_timer = 50;
				}
			}
			if (head_mode == HEAD_STATES.DYING)
			{
				head_secondary_timer -= delta;
				if (head_secondary_timer < 0)
				{
					Rpc("hide_head_anim");
					head_secondary_timer = 20;
				}
			}
			if (head_timer < 0)
			{
				/* Indicating spawn */
				if (head_mode == HEAD_STATES.SHOWN)
				{
					head_mode = HEAD_STATES.HIDING;
					head_timer += HEAD_HIDE_TIME;
					head_secondary_timer = FRAGMENT_INTERVAL;
					Rpc("hide_head");
					/* Generate new head location */
					head_location = new Vector2(360 + rand.Next(1200), 300 + rand.Next(840));
				}
				else if (head_mode == HEAD_STATES.SHOWING)
				{
					head_mode = HEAD_STATES.SHOWN;
					head_timer += HEAD_SHOWN_TIME;
					head_secondary_timer = HEAD_SLAM_INITIAL;
					slam_count = 4;
				}
				else if (head_mode == HEAD_STATES.HIDING)
				{
					head_mode = HEAD_STATES.SHOWING;
					head_secondary_timer = SHOWING_INTERVAL + head_timer;
					head_timer += SHOWING_DIVISIONS * SHOWING_INTERVAL + 0.2f;
					reverberation_count = 0;
					Rpc("reveal_head", head_location);
				}
				else if (head_mode == HEAD_STATES.DYING)
				{
					head_mode = HEAD_STATES.INACTIVE;
					Rpc("Create_Snake_Head", snake_head.GlobalPosition);
				}
			}
		}
	}

	/// <summary>
	/// Handle body of the snake
	/// </summary>
	/// <param name="delta"></param>
	private void handle_snake_b(float delta)
	{
		if (snake_mode != SNAKE_STATES.INACTIVE)
		{
			snake_timer -= delta;
			if (snake_mode == SNAKE_STATES.BURROWED)
			{
				if (snake_timer < 2)
				{
					snake_secondary_timer -= delta;
					if (snake_timer + (float)delta >= 2)
					{
						Rpc("play_static", "rumble", -20, 0.8f);
					}
					if (snake_secondary_timer < 0)
					{
						Rpc("spawn_fragment", head_location + new Vector2(-100 + rand.Next(200), -100 + rand.Next(200)));
						snake_secondary_timer += FRAGMENT_INTERVAL;
					}
				}
			}
			else if (snake_mode == SNAKE_STATES.CRAWLING)
			{
				snake_secondary_timer -= delta;
				if (snake_secondary_timer < 0)
				{
					//choose_player(SNAKE_SEGMENTS);
					for (int i = 0; i < 2; i++)
						calculate_next_point();
					snake_secondary_timer += SNAKE_RECALC_DELAY;
				}
			}

			/* Transitions */
			if (snake_timer < 0)
			{
				if (snake_mode == SNAKE_STATES.BURROWED)
				{
					snake_mode = SNAKE_STATES.CRAWLING;
					Rpc("burrow", head_location);
					snake_timer = SNAKE_CRAWL_TIME;
					snake_secondary_timer = SNAKE_RECALC_DELAY;
					for (int i = 0; i < 2; i++)
						calculate_next_point();

					/* Clear area */
					Rpc("head_reverberation", head_location, 0, 75);
				}
				else if (snake_mode == SNAKE_STATES.CRAWLING)
				{
					snake_mode = SNAKE_STATES.BURROWED;

					Rpc("play_static", "burrow", -20, 0.6f);
					snake_timer = SNAKE_BURROW_TIME;
					/* Generate new head location */
					head_location = new Vector2(360 + rand.Next(1200), 300 + rand.Next(840));
				}
			}
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void play_static(string name, float volume, float pitch)
	{
		sound_player.Play_Effect_Static(name, volume, pitch);
	}

	/*
	 *			---------- HEAD FUNCTIONS ----------------
	 */
	/// <summary>
	/// Hurts the boss based on head damage.
	/// </summary>
	/// <param name="damage"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Hurt_Head(int damage)
	{
		Boss_HP -= damage;
		sound_player.Play_Effect("cookie_hurt", -20);
		/* Fragments */
		for (int i = 0; i < 8; i++)
		{
			spawn_cookie_fragment(head_location + new Vector2(120.0f * (GD.Randf() - 0.5f), -50.0f + 120.0f * GD.Randf()));
		}
		/* Head is dead */
		if (Boss_HP <= 0)
		{
			Boss_HP = 0;
			/* Destroy head */
			if (authority)
			{
				//Rpc("Create_Snake_Head", snake_head.GlobalPosition);
				Rpc("death_knell");
			}
			//hide_head();
		}
		else
		{
			canvas_gui.Update_Boss_Health(new float[] { Boss_HP, BOSS_HP_MAX });
		}
	}
	/// <summary>
	/// Reveals the head at a location.
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void reveal_head(Vector2 head_position)
	{
		snake_head.GlobalPosition = head_position;
		snake_head.Raise_Head();
		sound_player.Play_Effect_Static("burrow", -15, 0.8f);
	}

	/// <summary>
	/// Slams a given location
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void slam_head(Vector2 slam_position)
	{
		snake_head.Set_Slam(slam_position);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void death_knell()
	{
		canvas_gui.Update_Boss_Health(new float[] { 0, BOSS_HP_MAX });
		snake_head.Open_All();
		head_mode = HEAD_STATES.DYING;
		head_timer = 4;
		head_secondary_timer = 3.6f;
		foreach (PlayerCamera camera in player_bag.GetAllCameras())
		{
			camera.Shake(5, 10, 10);
		}
		sound_player.Play_Effect_Static("oven_roar", -15, 0.8f);
	}

	/// <summary>
	/// Clears a ring around the head
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void head_reverberation(Vector2 center, float radius_inner, float radius_outer)
	{
		/* Clear area around the head */
		for (int i = 0; i < 4; i++)
		{
			for (float j = radius_inner; j <= radius_outer; j += 10)
			{
				/* Top */
				platforms[i].Update_Line(new Vector2(center.X - radius_outer, center.Y - j),
				new Vector2(center.X + radius_outer, center.Y - j), false);
				/* Bottom */
				platforms[i].Update_Line(new Vector2(center.X - radius_outer, center.Y + j),
				new Vector2(center.X + radius_outer, center.Y + j), false);
				/* Left */
				platforms[i].Update_Line(new Vector2(center.X - j, center.Y - radius_outer),
				new Vector2(center.X - j, center.Y + radius_outer), false);
				/* Right */
				platforms[i].Update_Line(new Vector2(center.X + j, center.Y - radius_outer),
				new Vector2(center.X + j, center.Y + radius_outer), false);
			}
		}

	}

	/// <summary>
	/// Slams a given location with the oven.
	/// </summary>
	/// <param name="slam_position"></param>
	public void Slam_Area(Vector2 slam_position)
	{
		/* Spawn fragments */
		for (int i = 0; i < 6; i++)
		{
			spawn_fragment(slam_position + new Vector2(-100 + rand.Next(200), -100 + rand.Next(200)));
		}
		/* Destroy area */
		head_reverberation(slam_position, 0, 125);
		/* Play sound */
		sound_player.Play_Effect("pull", -20, 0.8f);
		/* Damage players nearby */
		foreach (Player player in player_bag.GetActivePlayers())
		{
			/* Check player is authority */
			if (player.Get_Authority())
			{
				if (player.GlobalPosition.DistanceTo(slam_position) < 175)
				{
					player.Try_Hurt(1);
				}
			}
		}
		foreach (PlayerCamera camera in player_bag.GetAllCameras())
		{
			camera.Shake(0.3f, 10, 10);
		}
	}

	/// <summary>
	/// Spawns a fragment at a given location
	/// </summary>
	/// <param name="fragment_position"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void spawn_fragment(Vector2 fragment_position)
	{
		var inst = fragment_prefab.Instantiate<Fragment>();
		this.AddChild(inst);
		/* Choose color depending on where it is */
		Color fragment_color = Colors.Firebrick;
		/* Checking if it will spawn on a platform */
		int[] breakpoint_mappings = new int[] { 1, 2, 4, 5 };
		for (int i = 0; i < 4; i++)
		{
			if (fragment_position.X < platform_boundaries[breakpoint_mappings[i]])
			{
				if (platforms[i].Check_Position(fragment_position))
				{
					fragment_color = Colors.SaddleBrown;
				}
				break;
			}
		}
		inst.Activate(fragment_color, 80 + rand.Next(40), 0, 2.5f, 0.8f + (float)rand.NextDouble() * 0.4f, 150 * (GD.Randf() - 0.5f));
		inst.GlobalPosition = fragment_position;
	}

	public void spawn_cookie_fragment(Vector2 fragment_position)
	{
		var inst = fragment_prefab.Instantiate<Fragment>();
		this.AddChild(inst);
		/* Choose color depending on where it is */
		Color fragment_color = Colors.Bisque;
		inst.Activate(fragment_color, 80 + rand.Next(40), 0, 2.5f, 0.8f + (float)rand.NextDouble() * 0.4f, 300 * (GD.Randf() - 0.5f));
		inst.GlobalPosition = fragment_position;
	}

	/// <summary>
	/// Hides the head.
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void hide_head()
	{
		/* hide the head position */
		snake_head.GlobalPosition = new Vector2(0, -200);
	}

	/// <summary>
	/// Head hide animation
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void hide_head_anim()
	{
		sound_player.Play_Effect("burrow", -10, 0.8f);
		snake_head.Lower_Head();
	}

	/*
	 *			---------- PLATFORM FUNCTIONS ---------------------
	 */
	/// <summary>
	/// Pulls the currently chosen platform
	/// </summary>
	public void Pull_Platform(float delta)
	{
		if (platform_current == -1) return;

		int pull_rate = 200;
		/* Enrage timer */
		if (player_bag.GetActivePlayers().Count == 1)
		{
			pull_rate = 300;
		}
		platforms[platform_current].Push_Vertical(-pull_rate * delta, false);
		/* Enraged */

		/* Pull players accordingly */
		foreach (Player player in player_bag.GetActivePlayers())
		{
			/* Pushed along platforms */
			if (platforms[platform_current].Check_Position(player.GlobalPosition))
			{
				player.GlobalPosition += pull_rate * delta * Vector2.Up;
			}
			/* Pulled by lava */
			else
			{
				if (player.GlobalPosition.X > platforms[platform_current].GlobalPosition.X - 20 &&
				player.GlobalPosition.X < platforms[platform_current].GlobalPosition.X + 400)
				{
					player.GlobalPosition += pull_rate / 2 * delta * Vector2.Up;
				}
			}
		}
	}

	/// <summary>
	/// Eats given platform location
	/// </summary>
	public void Eat_Platform()
	{
		if (platform_current == -1)
		{
			for (int i = 0; i < 4; i++)
			{
				/* Spawn fragments */
				Vector2 platforms_center = platforms[i].GlobalPosition + new Vector2(180, 0);
				for (int j = 0; j < 1; j++)
				{
					spawn_fragment(platforms_center + new Vector2(-200 + rand.Next(400), 50 + rand.Next(50)));
				}

				platforms[i].Clear_Line(0, 4);
			}
			return;
		}
		/* Spawn fragments */
		Vector2 platform_center = platforms[platform_current].GlobalPosition + new Vector2(180, 0);
		for (int i = 0; i < 5; i++)
		{
			spawn_fragment(platform_center + new Vector2(-200 + rand.Next(400), 50 + rand.Next(50)));
		}
		platforms[platform_current].Clear_Line(0, 5);

		/* Kill any players inside the oven */
		foreach (Player player in player_bag.GetActivePlayers())
		{
			if (player.Get_Authority())
			{
				/* Check within the oven */
				if (player.GlobalPosition.Y < 200 &&
				player.GlobalPosition.X > platform_center.X - 250 &&
				player.GlobalPosition.X < platform_center.X + 250)
				{
					sound_player.Play_Effect("crunch", 0);
					GameManager.Instance.Display_Message("You've been eaten.", 2.5f);
					Rpc("Ignite_Player", player.Get_Id());
				}
			}
		}
	}

	/// <summary>
	/// Sets the platform timer
	/// </summary>
	/// <param name="timer"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void set_platform_timer(float timer)
	{
		platform_timer = timer;
	}

	/// <summary>
	/// Updates platform_current to a random platform, prioritizing older platforms
	/// </summary>
	private void choose_platform()
	{
		/* Linear importance */
		int index = rand.Next(PLATFORM_COUNT * (PLATFORM_COUNT + 1) / 2);

		for (int i = 0; i < PLATFORM_COUNT; i++)
		{
			/* Locate the given choise */
			index -= PLATFORM_COUNT - i;
			if (index < 0)
			{
				/* Cycle that choice to the back */
				Rpc("set_platform", platform_choices[i]);
				Logger.Instance.Log(Logger.LOG_LEVELS.INFO, "Chose Platform " + platform_current);
				platform_choices.Add(platform_choices[i]);
				platform_choices.RemoveAt(i);
				/* Ignite platforms on all screens */
				oven_body.Rpc("Ignite", platform_current);

				/* Enrage action */
				if (player_bag.GetActivePlayers().Count == 1)
				{

					oven_body.Full_Ignite();
				}
				return;
			}
		}
	}

	/// <summary>
	/// Set the platform choice on all screens
	/// </summary>
	/// <param name="choice"> The integer choice of platform </param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void set_platform(int choice)
	{
		platform_current = choice;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void stop_platform()
	{
		oven_body.Extinguish();

		/* Enrage timer */
		if (player_bag.GetActivePlayers().Count == 1)
		{
			oven_body.Full_Ignite();
		}
	}

	/// <summary>
	/// Check if any players have to be ignited.
	/// </summary>
	private void check_ignition(float delta)
	{
		foreach (Player player in player_bag.GetActivePlayers())
		{
			if (!player.Get_Authority()) continue;
			float xPos = player.GlobalPosition.X;
			int pId = player.Get_Id();
			/* Pulled into oven */
			if (player.GlobalPosition.Y < 200)
			{
				player_ignition[pId] += delta * 2;
				if (player_ignition[pId] > IGNITION_CAP)
				{
					sound_player.Play_Effect("oven_roar", -20, 0.9f);
					GameManager.Instance.Display_Message("You've been overcooked.", 2.5f);
					Rpc("Ignite_Player", pId);
				}
			}
			/* Platform 0 */
			else if (xPos < platform_boundaries[1])
			{
				check_platform_ignition(0, player, delta);
			}
			/* Platform 1 */
			else if (xPos < platform_boundaries[2])
			{
				check_platform_ignition(1, player, delta);
			}
			/* Lava Center */
			else if (xPos < platform_boundaries[3])
			{
				player_ignition[pId] += delta * CENTER_COEF;
				if (player_ignition[pId] > IGNITION_CAP)
				{
					GameManager.Instance.Display_Message("You've been overcooked.", 2.5f);
					Rpc("Ignite_Player", pId);
				}
			}
			/* Platform 3 */
			else if (xPos < platform_boundaries[4])
			{
				check_platform_ignition(2, player, delta);
			}
			/* Platform 4 */
			else
			{
				check_platform_ignition(3, player, delta);
			}
		}
	}

	/// <summary>
	/// Checks the ignition of a player within a specific platform
	/// </summary>
	/// <param name="platform_id">The id of the platform to check</param>
	/// <param name="player"> The player </param>
	/// <param name="delta"> The time since the last frame</param>
	private void check_platform_ignition(int platform_id, Player player, float delta)
	{
		/* Quick error check */
		if (platform_id > PLATFORM_COUNT)
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "check_platform_ignition recieved invalid platform id");
			return;
		}
		/* Check inclusion in the platform */
		int pId = player.Get_Id();
		if (platforms[platform_id].Check_Position(player.GlobalPosition))
		{
			player_ignition[pId] = Mathf.Max(0, player_ignition[pId] - delta * REDUCTION_COEF);
		}
		else
		{
			player_ignition[pId] += delta;
			if (player_ignition[pId] > IGNITION_CAP)
			{
				/* Kill if this is the authority */
				if (player.Get_Authority())
				{
					GameManager.Instance.Display_Message("You've been overcooked.", 2.5f);
					Rpc("Ignite_Player", pId);
				}
			}
		}
	}

	/// <summary>
	/// Ignites and kills a player on all screens.
	/// </summary>
	/// <param name="player_id"> The id of the player being killed </param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Ignite_Player(int player_id)
	{
		//GD.Print("Player Ignited, not killed");
		player_bag.GetPlayer(player_id).Kill();
	}
}
