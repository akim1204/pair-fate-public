using Godot;
using System;
using System.Collections.Generic;

public partial class EyeScreamController : BossController
{

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Hard set index for repeated fights */
		GameManager.Instance.Get_Death_Menu().Set_Restart_Point("res://Bosses/EyeScream/EyeScreamTest.tscn", 1);


		sound_player = GetNode<SoundPlayer>("SoundPlayer");
		sphere_handler = GetNode<IceSphereHandler>("IceSphereHandler");
		head_handler = GetNode<IceScreamHead>("EyeScreamHead");
		darkening = GetNode<ColorRect>("Darkening");
		screen_effects = GetNode<ScreenEffects>("ScreenEffects");
		background_eyes = GetNode<BackgroundEyes>("BackgroundEyes");

		head_handler.screen_effects = screen_effects;
		/* Getting braziers */
		for (int i = 0; i < 3; i++)
		{
			braziers.Add(GameManager.Instance.Get_World().GetNode<Brazier>("ExtraEntities/interactable10" + i.ToString()));
			braziers[i].Controller = this;
			braziers[i].id = i;
		}

		authority = Multiplayer.GetUniqueId() == 1;

		/* Initializing arm attacks */
		arm_timer = ARM_INTERVAL;


		darkening.Material.Set("shader_parameter/ratio", 0);
		screen_effects.Set_Rotation(0);
		screen_effects.Set_Burn(0);
		screen_effects.Fragment(false);

		/* Debug stuff 
		phase = 1;
		Lockin();
		sphere_handler.Drop(); sphere_handler.Drop();
		head_handler.Rotate_Head();
		*/

		/* If its not the first time */
		if (!first_time)
		{
			head_handler.head_height = REGULAR_HEAD_HEIGHT;
			head_handler.place_body();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		/* Boss AI */
		if (active && authority)
		{
			if (phase == 0)
			{
				handle_arms((float)delta);
				handle_head((float)delta);
				handle_eye((float)delta);
			}
			if (phase >= 1)
			{
				/* Only do arm attacks when the head isn't rotating */
				if (head_handler.head_state != IceScreamHead.HEAD_STATES.ROTATE && head_handler.move_intention != IceScreamHead.HEAD_STATES.ROTATE)
				{
					handle_arms((float)delta);
					handle_head2((float)delta);
					handle_eye((float)delta);
					handle_burn_attack((float)delta);
				}
			}
		}

		if (rotate_back)
		{
			rotate_timer = Mathf.Min(1, rotate_timer + (float)delta);
			screen_effects.Set_Rotation(rotate_timer * Mathf.Pi + Mathf.Pi);
		}
		/* Handling burn */
		handle_burn((float)delta);

		if (Input.IsActionJustPressed("ui_one"))
		{
			Hurt(1);
		}
		if (Input.IsActionJustPressed("ui_two"))
		{
			Die();
		}

		if (unhide_timer > 0)
		{
			unhide_timer = Mathf.Max(unhide_timer - (float)delta / 4, 0);
			darkening.Material.Set("shader_parameter/ratio", 1 - unhide_timer);

			/* Forcing it to front */
			if (unhide_timer == 0)
			{
			}
		}
		if (unhide_timer < 0)
		{
			unhide_timer = Mathf.Min(unhide_timer + (float)delta / 1, 0);
			darkening.Material.Set("shader_parameter/ratio", 0 - unhide_timer);
		}
	}

	public void Spawn_Fragment(Vector2 fragment_position)
	{
		sphere_handler.Spawn_Fragment(fragment_position);
	}

	/// <summary>
	/// Changes scene to a locked in view
	/// </summary>
	public void Lockin()
	{
		foreach (PlayerCamera camera in player_bag.GetAllCameras())
		{
			camera.Bound_Camera(320, 0, 2240, 1080);
		}
		head_handler.Lockin();
	}

	/// <summary>
	/// Sets the visibility of the background, state if visible, false otherwise
	/// </summary>
	/// <param name="state"></param>
	public void Unhide(bool state)
	{
		sphere_handler.active = state;

		/* If completely one type */
		if (unhide_timer == 0)
		{
			unhide_timer = state ? 1 : -1;
		}
		/* Otherwise, if already mid transition */
		else
		{
			unhide_timer = (state ? 1 : -1) * Mathf.Abs(unhide_timer);
		}
	}

	/* Start fight quickly on subsequent fights */
	public void Start_Fight_Fast()
	{

		/* Display messages */
		GameManager.Instance.Display_Message("Around and around again", 4);
		active = true;
		head_handler.Release();
		head_handler.First_Flip();
		Unhide(true);
		/* Delay for pushing forward */
		Timer cinematic_timer = new Timer();
		cinematic_timer.OneShot = true;
		cinematic_timer.Timeout += head_handler.Push_Forward;
		this.AddChild(cinematic_timer);
		cinematic_timer.Start(3);

		GameManager.Instance.Play_Music(soundtrack);
		/* Initializing gui */
		canvas_gui.Is_Boss(true, BossGUI.BossStyles.ICE);
		canvas_gui.Update_Boss_Health(new float[] { BOSS_HP_MAX, BOSS_HP_MAX }, true);
	}


	public void Start_Fight()
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.INFO, "Beginning Eye Scream Boss Fight");

		/* Display messages */
		GameManager.Instance.Display_Message("it's bright. much too bright.", 4);
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(head_handler, "head_height", 800, 2f).SetTrans(Tween.TransitionType.Sine);
		tween.TweenProperty(head_handler, "head_height", 900, 2f).SetTrans(Tween.TransitionType.Sine);
		head_handler.head_state = IceScreamHead.HEAD_STATES.IDLE;

		sound_player.Play_Effect_Static("Rumble", -50);

		/* Delay for middle of cinematic */
		Timer cinematic_timer = new Timer();
		cinematic_timer.OneShot = true;
		cinematic_timer.Timeout += Cinematic_Middle;
		this.AddChild(cinematic_timer);
		cinematic_timer.Start(4);


	}
	public void Cinematic_Middle()
	{
		/* Raise head */
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(head_handler, "head_height", REGULAR_HEAD_HEIGHT, 4f).SetTrans(Tween.TransitionType.Sine);
		head_handler.head_state = IceScreamHead.HEAD_STATES.IDLE;
		/* Delay for end of cinematic */
		Timer cinematic_timer = new Timer();
		cinematic_timer.OneShot = true;
		cinematic_timer.Timeout += Cinematic_Middle2;
		this.AddChild(cinematic_timer);
		cinematic_timer.Start(4);

		sound_player.Play_Effect_Static("Rumble", -40, 0.7f);
	}

	public void Cinematic_Middle2()
	{
		/* playing music */
		GameManager.Instance.Play_Music(soundtrack);
		/* Display messages */
		GameManager.Instance.Display_Message("so I will put you out.", 4, true);
		/* Delay for end of cinematic */
		Timer cinematic_timer = new Timer();
		cinematic_timer.OneShot = true;
		cinematic_timer.Timeout += Cinematic_End;
		this.AddChild(cinematic_timer);
		cinematic_timer.Start(4);
	}
	public void Cinematic_End()
	{
		active = true;

		/* Flip head */
		head_handler.First_Flip();

		/* Display boss title */
		Texture2D boss_label = GD.Load<Texture2D>("res://Bosses/EyeScream/Banner.png");
		GameManager.Instance.Display_Sprite(boss_label, 3.5f, new Vector2(0, 625));

		eye_timer = 2;

		/* Initializing gui */
		canvas_gui.Is_Boss(true, BossGUI.BossStyles.ICE);
		canvas_gui.Update_Boss_Health(new float[] { BOSS_HP_MAX, BOSS_HP_MAX }, true);

		head_handler.Release();
		head_handler.Push_Forward();
	}

	public void Die()
	{
		if (head_handler.rotated)
		{
			rotate_back = true;
		}
		active = false;
		head_handler.Return_Idle();
		head_handler.left_arm.Stale();
		head_handler.right_arm.Stale();
		head_handler.Push_Backward();
		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(head_handler, "dropset", 4000, 5f);
		tween = GetTree().CreateTween();
		tween.TweenProperty(background_eyes, "modulate", new Color(1, 1, 1, 0), 3f);
		/* Disable bossbar */
		canvas_gui.Is_Boss(false);
		GameManager.Instance.Play_Music("");

		/* Disable burn */

		screen_effects.Set_Burn(0);
		screen_effects.Fragment(false);
		burn_timer = 0;



	}
	/// <summary>
	/// Called when a brazier is lit (true) or extinguished (false)
	/// </summary>
	/// <param name="state"></param>
	public void Inform_Ignite(bool state)
	{
		if (state)
		{
			/* Update ignited count */
			ignite_count += 1;
			/* If all are ignited, now visible */
			if (ignite_count == braziers.Count)
			{
				Unhide(true);
				if (!active)
				{
					/* First cinematic */
					if (first_time)
					{
						Start_Fight();
					}
					/* Othewrise, quickstart*/
					else
					{
						Start_Fight_Fast();
					}
				}
			}
		}
		else
		{
			ignite_count -= 1;
			/* If just unignited */
			if (ignite_count == braziers.Count - 1)
			{
				Unhide(false);
			}
		}
	}

	/// <summary>
	/// Deactivates a given brazier
	/// </summary>
	public void Snuff(int brazier_id)
	{
		braziers[brazier_id].Ignite(false);
		sound_player.Play_Effect_Static("Extinguish", -30, 0.6f);
	}

	public void Hurt(int damage)
	{
		Rpc("RPC_Hurt");
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void RPC_Hurt()
	{
		boss_hp = Mathf.Max(0, boss_hp - 1);

		/* Checking breakpoints */
		if (phase == 0)
		{
			canvas_gui.Update_Boss_Health(new float[] { boss_hp, BOSS_HP_MAX });
			if (boss_hp == breakpoint_1)
			{
				sphere_handler.Drop();
			}
			if (boss_hp == breakpoint_2)
			{
				sphere_handler.Drop();
			}
			if (boss_hp == 0)
			{
				boss_hp = BOSS_HP_MAX * 2;
				GD.Print("Entering phase 1");
				phase = 1;
				head_handler.Phase(1);
				Lockin();
				canvas_gui.Update_Boss_Health(new float[] { 0, BOSS_HP_MAX * 2 }, true);
				canvas_gui.Update_Boss_Health(new float[] { boss_hp, BOSS_HP_MAX * 2 });

				/* Immediately rotate */
				head_handler.Rotate_Head();

				/* Reveal eyes */
				Tween tween = GetTree().CreateTween();
				background_eyes.Show();
				tween.TweenProperty(background_eyes, "modulate", new Color(0, 0, 0, 1), 4f);
				tween.TweenProperty(background_eyes, "modulate", new Color(1, 1, 1, 1), 1.5f);
			}
		}

		/* Phase 2 */
		else
		{
			canvas_gui.Update_Boss_Health(new float[] { boss_hp, BOSS_HP_MAX * 2 });
			if (boss_hp == BOSS_HP_MAX * 3 / 2)
			{
				phase = 2;
				head_handler.Phase(2);
				GD.Print("Entering phase 2");
				/* Rotate back to normal */
				if (head_handler.rotated)
				{
					head_handler.Rotate_Head();
				}
				burn_attack_timer = 4;
			}
			if (boss_hp == BOSS_HP_MAX)
			{
				GD.Print("Entering phase 3");
				phase = 3;
				head_handler.Phase(3);
			}
			if (boss_hp == BOSS_HP_MAX / 2)
			{
				GD.Print("Entering phase 4");
				phase = 4;
				head_handler.Phase(4);
			}
			if (boss_hp == 0)
			{
				Die();
			}
		}
	}


	/// <summary>
	/// Handles internal calculations of arms
	/// </summary>
	/// <param name="delta"> Time since the last frame </param>
	private void handle_arms(float delta)
	{
		/* Decrement timer */
		arm_timer -= delta;

		/* Attacking */
		if (arm_timer <= 0)
		{
			arm_timer += ARM_INTERVAL;

			/* Find a player */
			var cur_players = player_bag.GetActivePlayers();
			if (cur_players.Count > 0)
			{
				Player cur_player = cur_players[rand.Next(cur_players.Count)];

				/* Attack type */
				if (rand.Next(2) == 0)
				{ /* Sweep */
					if (cur_player.GlobalPosition.X < (ROOM_RIGHT - ROOM_LEFT) / 2)
					{
						head_handler.Start_Sweep(0, new Vector2(
							Mathf.Max(ROOM_LEFT + 25 + 320 * phase, cur_player.GlobalPosition.X - 400),
							cur_player.GlobalPosition.Y
						));
					}
					else
					{
						head_handler.Start_Sweep(1, new Vector2(
							Mathf.Min(ROOM_RIGHT - 25 - 320 * phase, cur_player.GlobalPosition.X + 400),
							cur_player.GlobalPosition.Y
						));
					}
				}
				else
				{ /* Slam */
					if (cur_player.GlobalPosition.X < (ROOM_RIGHT - ROOM_LEFT) / 2)
					{
						head_handler.Start_Slam(0, cur_player.GlobalPosition);
					}
					else
					{
						head_handler.Start_Slam(1, cur_player.GlobalPosition);
					}
				}

			}
		}
	}

	private void handle_head(float delta)
	{
		head_timer -= delta;
		if (head_timer <= 0)
		{
			head_timer += HEAD_INTERVAL;

			if (rand.Next(3) == 1)
			{ /* Rain attack */
				int player_id = player_bag.GetActivePlayerIds()[rand.Next(player_bag.GetActivePlayerIds().Count)];
				head_handler.Start_Rain(player_id);
			}
			/* Snuff if braziers are lit */
			else
			{
				/* Choose brazier at random */
				Brazier target_brazier = braziers[rand.Next(braziers.Count)];
				if (target_brazier.Ignited) head_handler.Snuff_Brazier(target_brazier);
			}
		}
	}
	private void handle_burn_attack(float delta)
	{
		burn_attack_timer -= delta;
		if (burn_attack_timer <= 0)
		{
			burn_attack_timer += BURN_INTERVAL;
			if (phase == 2)
			{
				/* Phase 1, single divisions */
				generate_random_divisions();
				if (first_burn)
				{
					first_burn = false;
					Rpc("Start_Burns", div_burn1, -1, false, burn_count);
				}
				else
				{
					Rpc("Start_Burns", div_burn1, -1, true, burn_count);
				}
			}
			else if (phase == 3)
			{/* Multi divisions */

				generate_random_divisions();

				burn_timer = BURN_DURATION;
				if (first_burn)
				{
					first_burn = false;
					Rpc("Start_Burns", div_burn1, div_burn2, false, burn_count);
				}
				else
				{
					Rpc("Start_Burns", div_burn1, div_burn2, true, burn_count);
				}

			}
			else if (phase == 4)
			{
				generate_random_divisions();

				burn_timer = BURN_DURATION;
				Rpc("Start_Burns", div_burn1, div_burn2, true, burn_count);
			}
		}
	}
	private void handle_head2(float delta)
	{
		head_timer -= delta;
		if (head_timer <= 0)
		{
			head_timer += HEAD_INTERVAL * 4 / 5;
			moves_since_rotate += 1;
			moves_since_rain += 1;

			/* Choose attack */
			int attack_index = rand.Next(4);
			if (attack_index == 0 && moves_since_rain > 1)
			{ /* Rain attack */
				moves_since_rain = 0;
				if (player_bag.GetActivePlayerIds().Count == 0) return;
				int player_id = player_bag.GetActivePlayerIds()[rand.Next(player_bag.GetActivePlayerIds().Count)];
				head_handler.Start_Rain(player_id);
			}
			else if (attack_index == 1 && moves_since_rotate > 2)
			{
				if (phase == 1)
				{ /* Phase 1, rotates */
					//head_handler.Rotate_Head();
					//moves_since_rotate = 0;
				}
				else if (phase == 2)
				{
					if (head_handler.rotated)
					{
						head_handler.Rotate_Head();
					}
				}
				else if (phase == 3 || phase == 4)
				{
					head_handler.Rotate_Head();
					moves_since_rotate = 1;
				}
			}
			/* Snuff if braziers are lit */
			else
			{
				/* Choose brazier at random */
				Brazier target_brazier = braziers[rand.Next(braziers.Count)];
				if (target_brazier.Ignited) head_handler.Snuff_Brazier(target_brazier);
			}
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Start_Burns(int div1, int div2, bool fragment, int burn_count)
	{
		this.burn_count = burn_count;
		div_burn1 = div1;
		div_burn2 = div2;
		burn_timer = BURN_DURATION;
		int offset1;
		switch (burn_count)
		{
			case 2:
				offset1 = 1;
				div_burn2 = -1;
				break;
			case 3:
				offset1 = 2;
				div_burn2 = -1;
				break;
			case 4:
				offset1 = 3;
				break;
			default:
				offset1 = rand.Next(3) + 2;
				break;
		}
		screen_effects.Set_Offset(offset1);
		screen_effects.Fragment(fragment);
		screen_effects.Set_Divs(div_burn1, div_burn2, Mathf.Max(2, burn_count));

		/* Sound effect */
		sound_player.Play_Effect_Static("BurnStart", -30, 0.2f);

		/* Screen shake on fragmentation */
		if (fragment)
		{
			/* Screen shake */
			foreach (PlayerCamera camera in GameManager.Instance.Get_Player_Bag().GetAllCameras())
			{
				camera.Shake(0.4f, 15, 20);
			}
		}
	}

	/// <summary>
	/// Generates random divisions
	/// </summary>
	private void generate_random_divisions()
	{
		burn_count = Mathf.Min(5, burn_count + 1);
		div_burn1 = rand.Next(burn_count);
		div_burn2 = rand.Next(burn_count);
		while (burn_count != 1 && div_burn2 == div_burn1)
		{
			div_burn2 = rand.Next(burn_count);
		}
	}

	private void handle_eye(float delta)
	{
		eye_timer -= delta;
		if (eye_timer <= 0)
		{
			eye_timer += EYE_INTERVAL;

			if (rand.Next(4) == 1)
			{
				sphere_handler.Open_Eyes_Region_Close();
			}
			else
			{
				sphere_handler.Open_Eyes_Close();
			}
		}
	}

	private void handle_burn(float delta)
	{
		if (burn_timer > 0)
		{
			burn_timer = Mathf.Max(burn_timer - (float)delta, 0);
			screen_effects.Set_Burn(1 - (burn_timer / BURN_DURATION));


			/* Finishing burn */
			if (burn_timer == 0)
			{
				screen_effects.Set_Burn(0);
				screen_effects.Fragment(false);
				/* Hurt players */
				foreach (Player player in GameManager.Instance.Get_Player_Bag().GetAllPlayers())
				{
					/* Check if in burned div */
					if (Mathf.Floor((player.GlobalPosition.X - ROOM_LEFT2) / (ROOM_RIGHT2 - ROOM_LEFT2) * 5) == div_burn1
					|| Mathf.Floor((player.GlobalPosition.X - ROOM_LEFT2) / (ROOM_RIGHT2 - ROOM_LEFT2) * 5) == div_burn2)
					{
						player.Try_Hurt(1);
					}
					/* Screen shake */
					foreach (PlayerCamera camera in GameManager.Instance.Get_Player_Bag().GetAllCameras())
					{
						camera.Shake(0.4f, 15, 20);
					}
					/* Sound effect */
					sound_player.Play_Effect_Static("BurnFinish", -20, 0.5f);

				}
			}
		}
	}
}
