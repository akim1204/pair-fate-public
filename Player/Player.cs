using System;
using System.Collections.Generic;
using Godot;

public partial class Player : CharacterBody2D
{

	/*
	 *		---------Player Input Options---------
	 */
	public int player_id;

	public string input_type = "mouse";

	public string player_color = "";

	private MultiplayerSynchronizer multiplayer_synchronizer;

	private CanvasGUI canvas_gui;

	private PlayerCamera player_camera;

	private SoundPlayer sound_player;
	private bool invulnerable = false;

	/// <summary> Whether there is one button for both picking up and
	/// interacting or two.  </summary>
	public int pickup_type = 2;


	/* 
	 * 		---------Player State Values--------- 
	 */

	/// <summary> Enumeration representing possible player states. </summary>
	public enum PlayerStates
	{
		MOVE, /* Default free movemenet */
		ACT, /* Currently using item */
		THROW, /* Throwing item */
		DEAD, /* Self explanatory */
	};

	/// <summary> Current player state. </summary>
	public PlayerStates player_state = PlayerStates.MOVE;

	/*
	 * 		---------Movement Values---------
	 */

	/// <summary> Constant value of linear acceleration per second. </summary>
	private const float ACCELERATION = 5000.0f;

	/// <summary> Linear friction per second, only applies when there are no inputs. </summary>
	private const float FRICTION = 5000.0f;

	/// <summary> Max speed per second. </summary>
	private const float MAX_SPEED = 350.0f;

	/// <summary>
	/// The object that applies environmental effects to player movement.
	/// </summary>
	private MovementInhibitor movement_inhibitor;
	/* 
	 *		---------Player Health and Death Values---------
	 */

	/// <summary> Current hp of the player. </summary>
	public int player_hp;

	/// <summary> How many revives the player has. </summary>
	public int available_revives = 1;

	/// <summary> Prefab used for player graves </summary>
	private PackedScene grave_prefab;

	/// <summary> Maximum hp of the player. </summary>
	private int PLAYER_MAX_HP = 3;

	/// <summary> How much hp the player revives with. </summary>
	private int PLAYER_REVIVE_HP = 1;

	/// <summary> Timer for how long the player is invulnerable </summary>
	private float hurt_timer;

	/*
	 * 		---------Player Animation Values---------
	 */

	/// <summary> The directional input for where the player is moving. </summary>
	private Vector2 move_input_vector;

	/// <summary> The directional input for where the player is facing. </summary>
	private Vector2 face_input_vector;

	/// <summary> If the act button is currently held down. </summary>
	public bool currently_acting = false;

	/// <summary> Sprite2D node that is the body of the player.</summary>
	private Sprite2D body_sprite;

	// <summary> Sprite2D node that is the shadow of the player. </summary>
	private Sprite2D shadow_sprite;

	/// <summary> Sprite2D node that is the right hand of the player. </summary>
	private Sprite2D hand_left_sprite;

	/// <summary> Sprite2D node that is the left hand of the player. </summary>
	private Sprite2D hand_right_sprite;

	/// <summary> Potential position of the left hand. </summary>
	private Vector2 hand_left_potential;

	/// <summary> Potential position of the right hand. </summary>
	private Vector2 hand_right_potential;

	/// <summary> AnimationPlayer for the player body, should be assigned in _Ready(). </summary>
	private AnimationPlayer body_animator;

	/// <summary> AnimationTree for the player body, should be assigned in _Ready(). </summary>
	private AnimationTree body_animation_tree;

	/// <summary> Animation State controller for the player body, should be assined from body_animation_tree. </summary>
	private AnimationNodeStateMachinePlayback body_animation_state;

	/// <summary> Wobble of the player. </summary>
	private float wobble_timer = 0;

	/// <summary> The rate at which the player wobbles. </summary>
	private const float WOBBLE_RATE = 10;

	/// <summary> Horizontal width of the player body. </summary>
	private const float BODY_WIDTH = 56;

	/// <summary> How far the hand is default offset from the center of the player. </summary>
	private const float HAND_OFFSET = 17;

	/// <summary> Current direction the player is facing. </summary>
	private Vector2 player_facing = new Vector2(1, 0);

	/*
	 * 		---------Player Item Values---------
	 */

	/// <summary> Variable pointing to bag containing all players </summary>
	private PlayerBag player_bag;

	/// <summary> PlayerItem node used by this player. </summary>
	private PlayerItem player_item;

	/// <summary> If the player is currently holding an item. </summary>
	private bool item_held;

	/// <summary> Timer used to differentiate wanting to throw or to just drop/pickup an item. </summary>
	private float pickup_timer = 0;

	/// <summary> Cutoff between drop/pickup and throwing. </summary>
	private const float PICKUP_CUTOFF = 0.15f;

	/// <summary> How fast the throw distance grows. </summary>
	private const float THROW_GROW_RATE = 600;

	/// <summary> Max distance item can be thrown. </summary>
	private const float THROW_MAX_DISTANCE = 750;

	/* Called when the Player is first added to the room */
	public override void _Ready()
	{
		/* Get Nodes for Animation */
		this.body_sprite = GetNode<Sprite2D>("PlayerBody");
		this.shadow_sprite = GetNode<Sprite2D>("PlayerShadow");
		this.hand_left_sprite = GetNode<Sprite2D>("PlayerHandLeft");
		this.hand_right_sprite = GetNode<Sprite2D>("PlayerHandRight");
		this.body_animator = GetNode<AnimationPlayer>("PlayerBodyAnimator");
		this.body_animation_tree = GetNode<AnimationTree>("PlayerBodyAnimationTree");
		body_animation_tree.Active = true;
		this.body_animation_state = body_animation_tree.Get("parameters/playback").As<AnimationNodeStateMachinePlayback>();

		/* Multiplayer stuff */
		this.multiplayer_synchronizer = GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer");
		this.multiplayer_synchronizer.SetMultiplayerAuthority(int.Parse(this.Name));

		/* Get sound player */
		sound_player = GetNode<SoundPlayer>("SoundPlayer");

		/* Set audio listener */
		if (Get_Authority())
		{
			GetNode<AudioListener2D>("AudioListener").MakeCurrent();
		}

		/* Set canvas player color */
		if (Get_Authority())
		{
			canvas_gui.Set_Main_Color(player_color);
		}

		/* Get player id */
		player_id = int.Parse(this.Name);

		/* Get player item */
		this.player_item = GetNode<PlayerItem>("PlayerItem");

		/* Add to player bag */
		player_bag = GetNode<PlayerBag>("/root/PlayerBag");
		player_bag.AddPlayer(player_id, this);
		player_bag.ActivatePlayer(player_id);
		if (Get_Authority())
		{
			player_bag.AddCamera(player_id, player_camera);
		}

		/* Reset player hp */
		player_hp = 3;

		/* Get grave prefab */
		grave_prefab = GD.Load<PackedScene>("res://Player/PlayerGraveInteractable.tscn");

		/* Make player pausable */
		ProcessMode = Node.ProcessModeEnum.Pausable;

		/* Get the movement inhibitor */
		movement_inhibitor = GetNodeOrNull<MovementInhibitor>("../MovementInhibitor");

	}


	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_nine"))
		{
			invulnerable = !invulnerable;
			if (invulnerable) GD.Print("Invulnerability Enabled");
			else GD.Print("Invulnerability Disabled");
		}
		/* Get player inputs only if main player */
		if (Get_Authority())
		{
			/* Get movement inputs */
			get_player_inputs();

			/* Check for tooltip click */
			if (Input.IsActionJustPressed("key_tooltip"))
			{
				Show_Tooltip();
			}


			/* Inputs specific to player state */
			switch (this.player_state)
			{
				case PlayerStates.MOVE:
					/* Handle interaction inputs based on input type */
					if (pickup_type == 1)
					{
						handle_interaction_input_one((float)delta);
					}
					else
					{
						handle_interaction_input_two((float)delta);
					}

					/* Handle action */
					if (Input.IsActionJustPressed("key_act"))
					{
						this.player_item.Handle_Action_Begin(move_input_vector, face_input_vector, this.Velocity, player_facing);
					}
					break;
				case PlayerStates.THROW:
					/* Releasing the throw */
					if (Input.IsActionJustReleased("key_pickup"))
					{
						/* Calculate where item should land */
						Vector2 destination = this.GlobalPosition + face_input_vector * Mathf.Min(GetLocalMousePosition().Length(),
							Mathf.Min(THROW_MAX_DISTANCE, (pickup_timer - PICKUP_CUTOFF) * THROW_GROW_RATE));
						this.player_item.Handle_Throw(move_input_vector, face_input_vector, this.Velocity, player_facing, destination);

						/* Clear debug drawing */
						pickup_timer = 0;

						/* Return to movestate */
						Enter_Throw(false);
					}
					break;
			}
		}


		/* Decrease hurt invulnerability */
		hurt_timer = Mathf.Max(0, hurt_timer - (float)delta);

		/* Handle current player state */
		switch (this.player_state)
		{
			case PlayerStates.MOVE:
				/* While moving, face input defaults to movement direction */
				if (face_input_vector == Vector2.Zero)
				{
					/* Default to input vector */
					if (move_input_vector != Vector2.Zero)
					{
						face_input_vector = move_input_vector;
					}
					else
					{
						face_input_vector = player_facing;
					}
				}

				/* Handle movement */
				this.handle_movement(move_input_vector, (float)delta);

				/* Handle body animation */
				this.handle_animation_move((float)delta, move_input_vector, face_input_vector);

				/* Handle item animation */
				(this.Velocity, hand_left_potential, hand_right_potential) =
					this.player_item.Handle_Animation_Default((float)delta, move_input_vector, face_input_vector,
				this.Velocity, player_facing);

				break;
			case PlayerStates.ACT:
				/* Handle movement */
				this.handle_movement(move_input_vector, (float)delta);

				/* Delegating to item */
				(this.Velocity, this.player_facing, bool finished, hand_left_potential, hand_right_potential) =
					this.player_item.Handle_Action((float)delta, move_input_vector, face_input_vector,
					this.Velocity, player_facing, currently_acting);

				/* Animate the player */
				handle_animation_act((float)delta);

				/* If action is finished return to movement */
				if (finished)
				{
					this.player_state = PlayerStates.MOVE;
				}

				break;
			case PlayerStates.THROW:
				/* Incrementing throw timer */
				this.pickup_timer += (float)delta;

				/* Turn to face throwing direction TODO: INCOMPETE */
				player_facing = face_input_vector;
				body_animation_tree.Set("parameters/Act/blend_position", player_facing);

				break;
			case PlayerStates.DEAD:
				/* Disable velocity TODO: maybe just slow down?*/
				this.Velocity = Vector2.Zero;
				break;

		}
		/* Placement of hands */
		handle_hand_animation();

		/* Apply environmental effects to movement */
		if (movement_inhibitor != null)
		{
			this.Velocity = movement_inhibitor.Inhibit_Movement((float)delta, this.Velocity, this.GlobalPosition);
		}

		/* Apply movement and collision */
		MoveAndSlide();

		/* Draw ui elements */
		QueueRedraw();
	}

	public override void _Draw()
	{
		/* Drawing throw line */
		if (Get_Authority() && this.player_state == PlayerStates.THROW && pickup_timer >= PICKUP_CUTOFF)
		{
			DrawLine(Vector2.Zero, face_input_vector.Normalized() * Mathf.Min(GetLocalMousePosition().Length(),
				Math.Min(THROW_MAX_DISTANCE, (pickup_timer - PICKUP_CUTOFF) * THROW_GROW_RATE)), Colors.Green, 7.0f);
		}

		/* Invincibility effect */
		var CurrentModulate = Modulate;
		if (hurt_timer > 0 && (int)(hurt_timer * 5) % 2 == 0)
		{
			CurrentModulate.A = 0.5f;
		}
		else
		{
			CurrentModulate.A = 1f;
		}

		Modulate = CurrentModulate;
	}

	/// <summary>
	/// Returns a dictionary of savable player information (INCOMPLETE)
	/// </summary>
	/// <returns></returns>
	public Dictionary<string, string> Save()
	{
		return new Dictionary<string, string>(){
			{"name", GetPath()},
			{"position", GD.VarToStr(GlobalPosition)},
		};
	}

	/// <summary>
	/// Sets the gui element of the player.
	/// </summary>
	/// <param name="gui"> The already initialized canvas gui </param>
	public void Initiate_GUI(CanvasGUI gui)
	{
		this.canvas_gui = gui;
	}

	/// <summary>
	/// Initiates the camera to be constrained to given bounds.
	/// </summary>
	/// <param name="left_bound"> Left bound of camera. </param>
	/// <param name="top_bound"> Top bound of camera. </param>
	/// <param name="right_bound"> Right bound of camera. </param>
	/// <param name="bottom_bound"> Bottom bound of camera. </param>
	public void Initiate_Camera(PlayerCamera new_camera, int left_bound, int top_bound, int right_bound, int bottom_bound, int multiplayer_id)
	{
		/* Get player camera */
		player_camera = new_camera;

		/* If this is the authority */
		if (int.Parse(this.Name) == multiplayer_id)
		{
			player_camera.Initiate_Camera(this, left_bound, top_bound, right_bound, bottom_bound);
		}
	}

	/// <summary>
	/// Returns the id of the current player.
	/// </summary>
	/// <returns> Integer representing player id. </returns>
	public int Get_Id()
	{
		return this.player_id;
	}

	/// <summary>
	/// Returns if the current user is the multiplayer authority of this player.
	/// </summary>
	/// <returns></returns>
	public bool Get_Authority()
	{
		return multiplayer_synchronizer.GetMultiplayerAuthority() == Multiplayer.GetUniqueId();
	}

	/// <summary>
	/// Returns item currently held by player, null if player is not carrying
	/// an item.
	/// </summary>
	/// <returns> Current player item, null if no item</returns>
	public Item Get_Item()
	{
		return this.player_item.Get_Item();
	}

	public void Show_Tooltip()
	{
		{
			if (this.player_item.Get_Item() != null)
			{
				string tooltip_string = this.player_item.Get_Item().Get_Tooltip();
				Texture2D tooltip_texture = this.player_item.Get_Item().Get_Texture();

				canvas_gui.Show_Tooltip(player_item.Get_Item().Get_Name(), tooltip_string, tooltip_texture);
			}
		}
	}

	/// <summary>
	/// Returns the major orientation values of the player.
	/// </summary>
	/// <returns>A series of Vector2s including
	/// move_input_vector, face_input_vector, Velocity, and player_facing </returns>
	public (Vector2, Vector2, Vector2, Vector2) Get_Orientation()
	{
		return (move_input_vector, face_input_vector, Velocity, player_facing);
	}

	/// <summary>
	/// Enters or leaves the acting state
	/// </summary>
	/// <param name="entering">Whether to enter or leave the act state</param>
	public void Enter_Act(bool entering)
	{
		if (entering)
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Entering ACT State");
			this.player_state = PlayerStates.ACT;
		}
		else
		{
			this.player_state = PlayerStates.MOVE;
		}
	}

	/// <summary>
	/// Enters or leaves the throwing state
	/// </summary>
	/// <param name="entering">Whether to enter or leave the throw state</param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
	public void Enter_Throw(bool entering)
	{
		if (entering)
		{
			this.player_state = PlayerStates.THROW;

			/* Stop movement */
			this.Velocity = Vector2.Zero;

			/* Update animation TODO: INCOMPLETE*/
			body_animation_state.Travel("Act");
		}
		else
		{
			this.player_state = PlayerStates.MOVE;
		}
		/* Additionally update on all other screens */
		if (Get_Authority())
		{
			Rpc("Enter_Throw", entering);
		}
	}

	/// <summary>
	/// Attempts to apply damage to a player, defers to the player's understanding.
	/// </summary>
	/// <param name="damage">Amount of damage to deal</param>
	public void Try_Hurt(int damage)
	{
		/* Defer to the authority player for being hurt */
		if (Get_Authority())
		{
			/* Hurting is only tracked while not invulnerable */
			if (hurt_timer <= 0)
			{
				/* Apply hurt damage to all platers */
				Rpc("Hurt", damage);
			}
		}
		/* Otherwise, do nothing */
	}

	/// <summary>
	/// Deals a certain amount of damage to the player.
	/// </summary>
	/// <param name="damage"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Hurt(int damage)
	{
		if (invulnerable) return;
		/* Play sound */
		if (player_hp > 0)
		{
			sound_player.Play_Effect("Hurt", -20);
		}

		/* Update hp */
		player_hp = Mathf.Max(0, player_hp - damage);
		canvas_gui.Update_Hp(player_hp, Get_Authority());


		/* Check for death */
		if (player_hp <= 0)
		{
			Kill();
		}
		/* Otherwise, enter hurt state */
		else
		{
			hurt_timer = 1;
		}
	}

	/// <summary>
	/// Kills the player, regardless of their hp.
	/// </summary>
	public void Kill()
	{
		if (invulnerable) return;
		/* Avoiding redundant kills */
		if (player_state != PlayerStates.DEAD)
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.INFO, "Player was killed");
			/* Force hp to 0 */
			player_hp = 0;
			/* Set player state */
			player_state = PlayerStates.DEAD;

			/* Drop any items */
			player_item.Drop_Item();
			player_item.Disable_Highlights();
			/* Remove player from player_bag */
			player_bag.DeactivatePlayer(this.player_id);
			body_animation_state.Travel("Die");

			/* TODO: Disable HItbox */
			//this.GetNode<Area2D>("PlayerHurtbox").Monitorable = false;

			/* Disables shadow */
			this.shadow_sprite.Visible = false;

			/* Spawn grave TODO: Delay this?*/
			var grave_inst = grave_prefab.Instantiate<PlayerGraveInteractable>();
			grave_inst.GlobalPosition = this.GlobalPosition;
			grave_inst.Set_Id(this.player_id);
			GameManager.Instance.Get_World().CallDeferred("add_child", grave_inst);

			/* Force hp to zero */
			player_hp = 0;
			canvas_gui.Update_Hp(player_hp, Get_Authority());

			/* swap cam to alive player */
			List<int> players = player_bag.GetActivePlayerIds();
			if (players.Count != 0)
			{
				int other_player_id = players[0];
				Player other = player_bag.GetPlayer(other_player_id);
				player_camera.swap_cam(other);
			}
		}
	}

	public void Revive()
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.INFO, "Player was revived");
		/* Set player state */
		player_state = PlayerStates.MOVE;

		/* Add player back to player_bag */
		player_bag.ActivatePlayer(this.player_id);
		player_item.Enable_Highlights();

		/* Enable Hitbox */
		//this.GetNode<Area2D>("PlayerHurtbox").Monitorable = false;

		/* Bring hp back up */
		player_hp = 1;
		canvas_gui.Update_Hp(player_hp, Get_Authority());
		player_camera.swap_cam(player_bag.GetPlayer(this.player_id));

		/* Re-enable shadow */
		this.shadow_sprite.Visible = true;

	}


	/// <summary>
	/// Gets the inputs of the current player.
	/// </summary>
	private void get_player_inputs()
	{

		move_input_vector = Vector2.Zero;
		move_input_vector.X = Input.GetActionRawStrength("key_right") - Input.GetActionRawStrength("key_left");
		move_input_vector.Y = Input.GetActionRawStrength("key_down") - Input.GetActionRawStrength("key_up");

		if (input_type == "controller")
		{
			// TODO: ARITIFIAL DEADZONE
			if (Mathf.Abs(move_input_vector.X) < 0.2)
			{
				move_input_vector.X = 0;
			}
			if (Mathf.Abs(move_input_vector.Y) < 0.2)
			{
				move_input_vector.Y = 0;
			}
		}

		/* Get facing inputs */
		face_input_vector = Vector2.Zero;
		/* Mouse and keyboard */
		if (input_type == "mouse")
		{
			face_input_vector = GetLocalMousePosition().Normalized();

			/* Handling rotation */
			if (GameManager.Instance.Get_Rotated())
			{
				/* Relative position of mouse */
				Vector2 mouse_pos = player_camera.GetScreenCenterPosition() - GetGlobalMousePosition();
				/* Rotated player pos */
				Vector2 player_pos = GlobalPosition - player_camera.GetScreenCenterPosition();
				face_input_vector = (mouse_pos - player_pos).Normalized();
			}
		}
		else if (input_type == "controller")
		{
		}

		/* Get if act key currently down */
		currently_acting = Input.IsActionPressed("key_act");

	}

	/// <summary>
	/// Handles the default movement of the player body, does not actually apply movement.
	/// </summary>
	/// <param name="input_vector"> Player input of the current frame </param>
	/// <param name="delta"> Time elapsed since the previous frame, cast to a float </param>
	private void handle_movement(Vector2 input_vector, float delta)
	{
		/* Get normalized input */
		Vector2 input_normalized = input_vector;
		if (input_normalized.Length() > 1)
		{
			input_normalized = input_normalized.Normalized();
		}
		/* If input is nonzero */
		if (input_normalized != Vector2.Zero)
		{
			/* Accelerate to max speed */
			this.Velocity = this.Velocity.MoveToward(input_normalized * MAX_SPEED, ACCELERATION * delta);
		}
		/* Otherwise, deccelerate */
		else
		{
			/* Deccelerate to 0 */
			this.Velocity = Velocity.MoveToward(Vector2.Zero, FRICTION * delta);
		}
	}

	/// <summary>
	/// Handles the animation of the player body during MOVE state.
	/// </summary>
	/// <param name="delta"> Time in seconds since last frame </param> 
	/// <param name="move_input_vector"> Player movement input of the current frame </param>
	/// <param name="face_input_vector"> Player directional input of the current frame </param>
	private void handle_animation_move(float delta, Vector2 move_input_vector, Vector2 face_input_vector)
	{

		/* Set animation direction to face towards mouse */
		body_animation_tree.Set("parameters/Idle/blend_position", face_input_vector);
		body_animation_tree.Set("parameters/Run/blend_position", face_input_vector);

		/* Update player facing */
		player_facing = face_input_vector;

		/* If movement is nonzero, enter runstate */
		if (move_input_vector != Vector2.Zero)
		{
			/* Set animation state */
			body_animation_state.Travel("Run");

			/* Additionally track wobble */
			wobble_timer += delta * WOBBLE_RATE;
			if (wobble_timer > Mathf.Pi * 2)
			{
				wobble_timer -= Mathf.Pi * 2;
			}

			/* Play Step Sounds */
			if (wobble_timer % Mathf.Pi < (wobble_timer - delta * WOBBLE_RATE + Mathf.Pi) % Mathf.Pi)
			{
				sound_player.Play_Effect("Step", -15, (float)GD.RandRange(.8, 1.3));
			}
		}
		/* Otherwise, enter idlestate */
		else
		{
			/* Set animation state */
			body_animation_state.Travel("Idle");

			/* Return wobble state to 0*/
			if (Mathf.Abs(wobble_timer - Mathf.Pi) < Mathf.Pi / 2)
			{ /* Closer to Pi */
				if (wobble_timer > Mathf.Pi)
				{
					wobble_timer = Mathf.Max(Mathf.Pi, wobble_timer - delta * WOBBLE_RATE);
				}
				else if (wobble_timer < Mathf.Pi)
				{
					wobble_timer = Mathf.Min(Mathf.Pi, wobble_timer + delta * WOBBLE_RATE);
				}
			}
			else
			{/* Closer to 0 */
				if (wobble_timer > Mathf.Pi)
				{
					wobble_timer = Mathf.Min(Mathf.Pi * 2, wobble_timer + delta * WOBBLE_RATE);
				}
				else
				{
					wobble_timer = Mathf.Max(0, wobble_timer - delta * WOBBLE_RATE);
				}
			}
		}

		/* Animate wobble */
		body_sprite.Position = new Vector2(0, -10 * Mathf.Abs(Mathf.Sin(wobble_timer)));
		body_sprite.Rotation = Mathf.Sin(wobble_timer) / 15;
	}

	/// <summary>
	/// Handles the animation of the player body during Act tate.
	/// </summary>
	private void handle_animation_act(float delta)
	{

		/* Update direction player is facing */
		body_animation_tree.Set("parameters/Act/blend_position", player_facing);

		/* Update animation */
		body_animation_state.Travel("Act");


		/* Return wobble state to zero*/
		if (Mathf.Abs(wobble_timer - Mathf.Pi) < Mathf.Pi / 2)
		{ /* Closer to Pi */
			if (wobble_timer > Mathf.Pi)
			{
				wobble_timer = Mathf.Max(Mathf.Pi, wobble_timer - delta * WOBBLE_RATE);
			}
			else if (wobble_timer < Mathf.Pi)
			{
				wobble_timer = Mathf.Min(Mathf.Pi, wobble_timer + delta * WOBBLE_RATE);
			}
		}
		else
		{ /* Closer to 0 */
			if (wobble_timer > Mathf.Pi)
			{
				wobble_timer = Mathf.Min(Mathf.Pi * 2, wobble_timer + delta * WOBBLE_RATE);
			}
			else
			{
				wobble_timer = Mathf.Max(0, wobble_timer - delta * WOBBLE_RATE);
			}
		}

		/* Animate wobble */
		body_sprite.Position = new Vector2(0, -10 * Mathf.Abs(Mathf.Sin(wobble_timer)));
		body_sprite.Rotation = Mathf.Sin(wobble_timer) / 15;
	}

	/// <summary>
	/// Handles the placement of the hands.
	/// </summary>
	private void handle_hand_animation()
	{
		/* Placement of hands if player is holding item */
		if (player_item.Has_Item())
		{
			hand_left_sprite.Position = hand_left_potential;
			hand_right_sprite.Position = hand_right_potential;
		}
		/* Placement of hands if the player has no item held */
		else
		{
			//TODO: INCOMPLETE
			hand_left_sprite.Position = new Vector2(-HAND_OFFSET * Mathf.Sin(player_facing.Angle()), -20);

			hand_right_sprite.Position = new Vector2(HAND_OFFSET * Mathf.Sin(player_facing.Angle()), -20);
		}
	}

	/// <summary>
	/// Handles interaction inputs when pickup and interact share a button.
	/// </summary>
	private void handle_interaction_input_one(float delta)
	{

		/* Handle picking up items */
		if (Input.IsActionJustPressed("key_pickup"))
		{
			/* Reset pickup timer */
			this.pickup_timer = 0;
		}

		/* Tracking how long the player presses down the interact button */
		if (Input.IsActionPressed("key_pickup"))
		{
			this.pickup_timer += (float)delta;

			/* If held for long enough and holding an item, enter throw state */
			if (player_item.Get_Item() != null && this.pickup_timer > PICKUP_CUTOFF)
			{
				Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Beginning Throw");
				this.player_state = PlayerStates.THROW;

				/* Stop movement */
				this.Velocity = Vector2.Zero;


				/* Update animation TODO: INCOMPLETE*/
				body_animation_state.Travel("Idle");
			}
		}

		/* Handle actually interacting with an object */
		if (Input.IsActionJustReleased("key_pickup"))
		{

			/* Handle pickup action if near item or nothing */
			if (player_item.Closest_Type() == "item" || player_item.Closest_Type() == "none")
			{
				Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Picking up Object");
				//Rpc("Handle_Pickup_Rpc");
				//player_item.Handle_Pickup(move_input_vector, face_input_vector, this.Velocity, player_facing);
			}

			/* Enter interact state if near interactable */
			if (player_item.Closest_Type() == "interactable")
			{
				Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Interacting with Object");
				player_item.Handle_Interact();
			}

		}
	}

	/// <summary>
	/// Handles interaction inputs when pickup and interact have different buttons.
	/// </summary>
	private void handle_interaction_input_two(float delta)
	{

		/* Handle picking up items */
		if (Input.IsActionJustPressed("key_pickup"))
		{
			/* Reset pickup timer */
			this.pickup_timer = 0;
		}

		/* Tracking how long the player presses down the interact button */
		if (Input.IsActionPressed("key_pickup"))
		{
			this.pickup_timer += (float)delta;

			/* If held for long enough and holding an item, enter throw state */
			if (player_item.Get_Item() != null && this.pickup_timer > PICKUP_CUTOFF)
			{
				Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Beginning Throw");
				Enter_Throw(true);
			}
		}

		/* Handle actually pickuping up an item */
		if (Input.IsActionJustReleased("key_pickup"))
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Picking up Object");
			/* Handle item pickups through multiplayer */
			player_item.Initiate_Pickup_Request(player_id);
			//player_item.Handle_Pickup(move_input_vector, face_input_vector, this.Velocity, player_facing);

		}

		/* Handle interaction */
		if (Input.IsActionJustPressed("key_interact"))
		{

			Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Interacting with Object");
			player_item.Handle_Interact();
		}

	}

	/// <summary>
	/// Plays the pickup sound effect.
	/// </summary>
	public void Play_Pickup()
	{
		sound_player.Play_Effect("Pickup", 0);
	}
	/// <summary>
	/// Plays the drop sound effect.
	/// </summary>
	public void Play_Drop()
	{
		sound_player.Play_Effect("Drop", 0);
	}


}
