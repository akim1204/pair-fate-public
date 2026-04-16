using Godot;
using System;
using System.Collections.Generic;

public partial class IceScreamHead : Node2D
{
	public enum HEAD_STATES
	{
		IDLE,
		DEBUG,
		MOVE,
		RAIN,
		SNUFF,
		ROTATE,
		STALE,
	}
	private bool authority = false;
	private SoundPlayer sound_player;
	public ScreenEffects screen_effects;
	private float ROOM_LEFT = EyeScreamController.ROOM_LEFT,
	ROOM_RIGHT = EyeScreamController.ROOM_RIGHT,
	ROOM_TOP = EyeScreamController.ROOM_TOP,
	ROOM_BOTTOM = EyeScreamController.ROOM_BOTTOM;

	public HEAD_STATES head_state = HEAD_STATES.STALE;
	private float timer = 0;

	/// <summary> Head and arms</summary>
	private Sprite2D head;
	private AnimationPlayer head_animator;
	private Sprite2D head_shadow;
	private Sprite2D head_shadow2;
	private Sprite2D head_shadow3;
	private List<Sprite2D> body_segments = new List<Sprite2D>();
	private const int SEGMENT_COUNT = 10;
	private PlayerBag player_bag = GameManager.Instance.Get_Player_Bag();
	private PackedScene body_scene = GD.Load<PackedScene>("res://Bosses/EyeScream/Head/BodySegmentSprite.tscn");
	private PackedScene droplet_scene = GD.Load<PackedScene>("res://Bosses/EyeScream/Head/EyeScreamDroplet.tscn");

	/// <summary> Lowest and height point of segments curve </summary>
	private Vector2 HEAD_SOURCE = new Vector2(0, 1000);
	private float head_source = 1000;
	public float head_height = 950;
	public float dropset = 0;
	private bool dropped = false;
	private float dropline = 700;
	public EyeScreamArm left_arm;
	public EyeScreamArm right_arm;

	private Vector2 head_center = Vector2.Zero;
	private EyeScreamController controller;

	/* Head sweep variables */
	private const float MOVE_SPEED = 400;
	private Vector2 move_goal;
	public HEAD_STATES move_intention;
	private const float RAIN_HEIGHT = 200;
	private const float RAIN_WIDTH = 900;
	private const float RAIN_SPEED = 600;
	private const float RAIN_INTERVAL = 0.15f;
	private const float RAIN_DURATION = 4;
	private float rain_timer = 0;
	private int rain_count = 0;
	private float rain_timer_secondary = 0;
	private int rain_direction = 1;
	private float snuff_timer = 0;
	private int snuff_id = -1;

	/* Screen rotate variables */
	private const float ROTATE_DURATION = 4;
	public bool rotated = false;
	private float rotate_timer = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		controller = GetParent<EyeScreamController>();
		head = GetNode<Sprite2D>("HeadSprite");
		Phase(0);
		sound_player = GetNode<SoundPlayer>("SoundPlayer");
		head_shadow = GetNode<Sprite2D>("CanvasGroup/HeadShadow");
		head_shadow2 = GetNode<Sprite2D>("CanvasGroup/HeadShadow2");
		head_shadow3 = GetNode<Sprite2D>("CanvasGroup/HeadShadow3");
		head_shadow.Hide();
		head_shadow2.Hide();
		head_shadow3.Hide();
		/* Start flipped */
		flip_head(true);

		/* Create segments */
		for (float i = SEGMENT_COUNT - 1; i >= 0; i--)
		{
			var body_inst = body_scene.Instantiate<Sprite2D>();
			this.AddChild(body_inst);
			body_inst.Name = "Body" + i.ToString();
			body_inst.Scale = new Vector2(1, 1) * (9 + i);
			body_inst.ZIndex = Mathf.Max(-30, -21 - (int)i);
			body_inst.Modulate = new Color((1 - i / SEGMENT_COUNT) * 0.7f,
				(1 - i / SEGMENT_COUNT) * 0.7f, (1 - i / SEGMENT_COUNT) * 0.7f);
			body_segments.Add(body_inst);
		}
		head_center = new Vector2(0, head_height);
		place_body();

		left_arm = GetNode<EyeScreamArm>("LeftArm");
		left_arm.Initialize(this, new Vector2(-100, 150));
		right_arm = GetNode<EyeScreamArm>("RightArm");
		right_arm.Initialize(this, new Vector2(100, 150));

		authority = Multiplayer.GetUniqueId() == 1;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (head_state != HEAD_STATES.STALE)
		{
			timer += (float)delta;
		}
		if (timer > Mathf.Pi * 4) timer -= Mathf.Pi * 4;
		switch (head_state)
		{
			case HEAD_STATES.IDLE:
				head_center = new Vector2(0, head_height);
				place_body();
				break;
			case HEAD_STATES.STALE:
				break;
			case HEAD_STATES.ROTATE:
				rotate_timer = Mathf.Min(ROTATE_DURATION, rotate_timer + (float)delta);
				screen_effects.Set_Rotation(rotate_timer / ROTATE_DURATION * Mathf.Pi + (rotated ? Mathf.Pi : 0));
				head.Rotation = rotate_timer / ROTATE_DURATION * Mathf.Pi + (rotated ? Mathf.Pi : 0);
				if (rotate_timer == ROTATE_DURATION)
				{
					move_goal = new Vector2(0, head_height);
					move_intention = HEAD_STATES.IDLE;
					head_state = HEAD_STATES.MOVE;
					rotated = !rotated;
					GameManager.Instance.Set_Rotated(rotated);
				}
				place_body();
				break;
			case HEAD_STATES.MOVE:
				if (head_center.DistanceTo(move_goal) <= (float)delta * MOVE_SPEED)
				{
					head_center = move_goal;
					head_state = move_intention;

					/* Depending on move intention, do things */
					if (move_intention == HEAD_STATES.RAIN)
					{
						flip_head(true ^ rotated);
					}
					/* Snuff out candle and return to idle */
					if (move_intention == HEAD_STATES.SNUFF)
					{
						controller.Snuff(snuff_id);

						move_goal = new Vector2(0, head_height);
						move_intention = HEAD_STATES.IDLE;
						head_state = HEAD_STATES.MOVE;
					}
					if (move_intention == HEAD_STATES.ROTATE)
					{
						sound_player.Play_Effect_Static("RotateLong", -30, 1.2f);
					}
				}
				else
				{
					head_center += (move_goal - head_center).Normalized() * (float)delta * MOVE_SPEED;
				}
				place_body();
				break;
			case HEAD_STATES.RAIN:
				rain_timer += (float)delta;
				if (rain_timer > RAIN_DURATION)
				{

					move_goal = new Vector2(0, head_height);
					move_intention = HEAD_STATES.IDLE;
					head_state = HEAD_STATES.MOVE;

					if (rotated)
					{
						flip_head(true);
					}
					else
					{
						flip_head(false);
					}
				}
				if (rain_timer > 1)
				{
					head_center += rain_direction * RAIN_SPEED * Vector2.Right * (float)delta;
					/* Spawning rain */
					rain_timer_secondary += (float)delta;
					if (rain_timer_secondary >= RAIN_INTERVAL)
					{
						rain_timer_secondary -= RAIN_INTERVAL;
						if (rotated)
						{
							var inst = droplet_scene.Instantiate<EyeScreamDroplet>();
							GameManager.Instance.Get_World().CallDeferred("add_child", inst);
							inst.GlobalPosition = ToGlobal(head_center) + 100 * Vector2.Right
							+ Vector2.FromAngle(GD.Randf() * Mathf.Pi * 2) * 30 + 60 * Vector2.Down;
							inst = droplet_scene.Instantiate<EyeScreamDroplet>();
							GameManager.Instance.Get_World().CallDeferred("add_child", inst);
							inst.GlobalPosition = ToGlobal(head_center) + 100 * Vector2.Left
							+ Vector2.FromAngle(GD.Randf() * Mathf.Pi * 2) * 30 + 60 * Vector2.Down;
						}
						else
						{
							var inst = droplet_scene.Instantiate<EyeScreamDroplet>();
							GameManager.Instance.Get_World().CallDeferred("add_child", inst);
							inst.GlobalPosition = ToGlobal(head_center) + 100 * Vector2.Right
							+ Vector2.FromAngle(GD.Randf() * Mathf.Pi * 2) * 30;
							inst = droplet_scene.Instantiate<EyeScreamDroplet>();
							GameManager.Instance.Get_World().CallDeferred("add_child", inst);
							inst.GlobalPosition = ToGlobal(head_center) + 100 * Vector2.Left
							+ Vector2.FromAngle(GD.Randf() * Mathf.Pi * 2) * 30;
						}
						sound_player.Play_Effect("Droplet", -28, GD.Randf() / 2 + 0.5f);
					}
				}
				place_body();
				break;
		}

	}

	public void Return_Idle()
	{

		move_goal = new Vector2(0, head_height);
		move_intention = HEAD_STATES.IDLE;
		head_state = HEAD_STATES.MOVE;
	}

	/// <summary>
	/// Updates scene to be in a locked in view
	/// </summary>
	public void Lockin()
	{
		ROOM_LEFT = 320;
		ROOM_RIGHT = 2240;
	}

	public void Phase(int phase)
	{
		head.Frame = phase;
	}

	/// <summary>
	/// Begins movement of head and arms
	/// </summary>
	public void Release()
	{
		Rpc("Rpc_Release");
	}
	public void First_Flip()
	{

		flip_head(false, true);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Rpc_Release()
	{
		head_state = HEAD_STATES.IDLE;
		left_arm.Return_Idle();
		right_arm.Return_Idle();
	}

	/// <summary>
	/// Pushes head and arms to be in front of darkening
	/// </summary>
	public void Push_Forward()
	{
		for (int i = 0; i < SEGMENT_COUNT; i++)
		{
			body_segments[SEGMENT_COUNT - i - 1].ZIndex = Mathf.Max(-11, -8 - i);
		}
		head.ZIndex = 0;
		left_arm.ZIndex = 0;
		right_arm.ZIndex = 0;
		head_shadow.Show();
		head_shadow2.Show();
		head_shadow3.Show();
	}

	public void Push_Backward()
	{
		for (int i = 0; i < SEGMENT_COUNT; i++)
		{
			body_segments[SEGMENT_COUNT - i - 1].ZIndex = -15;
		}
		//head_shadow.Hide();
		head_shadow2.Hide();
		head_shadow3.Hide();

	}

	/// <summary>
	/// Starts a slam at a given location
	/// </summary>
	/// <param name="arm"></param>
	/// <param name="position"></param>
	public void Start_Slam(int arm, Vector2 position)
	{
		if (arm == 0)
		{
			left_arm.Rpc("Begin_Slam", position, left_arm.current_handprint);
		}
		else
		{
			right_arm.Rpc("Begin_Slam", position, right_arm.current_handprint);
		}
	}

	public void Start_Sweep(int arm, Vector2 position)
	{
		if (arm == 0)
		{
			left_arm.Rpc("Begin_Sweep", position, left_arm.current_handprint);
		}
		else
		{
			right_arm.Rpc("Begin_Sweep", position, right_arm.current_handprint);
		}
	}

	/// <summary>
	/// Begins a rain, following the given player id
	/// </summary>
	public void Start_Rain(int player_id)
	{

		/* Swap direction */
		rain_direction *= -1;
		Player cur_player = player_bag.GetPlayer(player_id);

		Vector2 start_position;
		if (rain_direction == 1)
		{
			if (ROOM_LEFT == 0)
			{
				start_position = ToLocal(new Vector2(Mathf.Max(ROOM_LEFT + 50, cur_player.GlobalPosition.X - RAIN_WIDTH), RAIN_HEIGHT));
			}
			else
			{
				start_position = ToLocal(new Vector2(ROOM_LEFT + 20, RAIN_HEIGHT));
			}
		}
		else
		{
			if (ROOM_RIGHT == 2560)
			{
				start_position = ToLocal(new Vector2(Mathf.Min(ROOM_RIGHT - 50, cur_player.GlobalPosition.X + RAIN_WIDTH), RAIN_HEIGHT));
			}
			else
			{
				start_position = ToLocal(new Vector2(ROOM_RIGHT - 20, RAIN_HEIGHT));
			}
		}

		/* Send Rpc*/
		Rpc("Align_Rain", head_center, start_position, rain_direction);
	}

	/// <summary>
	/// RPC that aligns rain attacks on both screens
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Align_Rain(Vector2 align_position, Vector2 start_position, int rain_direction)
	{
		/* Align positions */
		head_center = align_position;
		this.rain_direction = rain_direction;

		/* Set goal position */
		move_goal = start_position;

		/* Enter corresponding state and reset timers */
		move_intention = HEAD_STATES.RAIN;
		rain_timer = 0;
		rain_count = 0;
		rain_timer_secondary = 0;

		head_state = HEAD_STATES.MOVE;
	}

	/// <summary>
	/// Spawns a fragment at a given location
	/// </summary>
	/// <param name="fragment_position"></param>
	public void Spawn_Fragment(Vector2 fragment_position)
	{
		controller.Spawn_Fragment(fragment_position);
	}

	/// <summary>
	/// Initiates an attack to snuff out a brazier, called on a single screen
	/// </summary>
	public void Snuff_Brazier(Brazier brazier)
	{
		Vector2 start_position;

		start_position = ToLocal(brazier.GlobalPosition);

		/* Send Rpc*/
		Rpc("Align_Snuff", head_center, start_position, brazier.id);
	}

	/// <summary>
	/// RPC that aligns snuff attacks on both screens
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Align_Snuff(Vector2 align_position, Vector2 start_position, int brazier_id)
	{
		/* Align positions */
		head_center = align_position;

		/* Set goal position */
		move_goal = start_position;

		/* Enter corresponding state and reset timers */
		move_intention = HEAD_STATES.SNUFF;
		snuff_timer = 0;
		snuff_id = brazier_id;

		head_state = HEAD_STATES.MOVE;
	}

	public void Rotate_Head()
	{
		Rpc("Align_Rotate", head_center);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Align_Rotate(Vector2 align_position)
	{
		/* Align positions */
		head_center = align_position;

		move_goal = ToLocal(new Vector2((ROOM_RIGHT + ROOM_LEFT) / 2, (ROOM_TOP + ROOM_BOTTOM) / 2));


		move_intention = HEAD_STATES.ROTATE;
		rotate_timer = 0;

		head_state = HEAD_STATES.MOVE;

		/* Also return arms to idle state */
		left_arm.Return_Idle();
		right_arm.Return_Idle();
	}

	/// <summary>
	/// Situates the body in comparison with the head
	/// </summary>
	public void place_body()
	{
		head.Position = head_center + new Vector2(50 * Mathf.Cos(timer), 25 + 25 * Mathf.Sin(timer / 2));
		if (head.Position.Y < head_height || head.Position.Y > head_source) return;
		float start_point = -Mathf.Sqrt((head.Position.Y - head_height) / head_source);
		for (int i = 0; i < SEGMENT_COUNT; i++)
		{
			float position = (1 - start_point) / SEGMENT_COUNT * (i + 0.5f);
			float ratio = -Mathf.Pow(start_point + position, 2) + 1;
			body_segments[SEGMENT_COUNT - i - 1].Position = new Vector2(head.Position.X * (1 - (float)(i + 1) / SEGMENT_COUNT),
				head_height * ratio + head_source * (1 - ratio));
			body_segments[SEGMENT_COUNT - i - 1].Position += new Vector2(
				25 * (i + 1) * Mathf.Cos(timer + i), 5 * i * Mathf.Sin(timer / 2 + i)
			);
			body_segments[SEGMENT_COUNT - i - 1].Position += new Vector2(
				0, Mathf.Max(0, dropset - (SEGMENT_COUNT - i) * 200)
			);

		}
		if (head.Position.Y + Mathf.Max(0, dropset - (SEGMENT_COUNT + 1.5f) * 200) < dropline)
		{
			head.Position += Vector2.Down * Mathf.Max(0, dropset - (SEGMENT_COUNT + 1.5f) * 200);
		}
		else
		{
			head.Position = new Vector2(head.Position.X, Mathf.Max(head.Position.Y, dropline));
		}
		if (!dropped && dropset > 0 && head.Position.Y >= dropline)
		{
			/* First time dropping */
			dropped = true;
			head_state = HEAD_STATES.STALE;
			foreach (PlayerCamera camera in player_bag.GetAllCameras())
			{
				camera.Shake(1.5f, 30, 15);
			}
		}

		/* Place shadows */
		head_shadow.Position = head.Position + 180 * Vector2.Down;
		head_shadow2.Position = body_segments[SEGMENT_COUNT - 1].Position + 120 * Vector2.Down;
		head_shadow3.Position = body_segments[SEGMENT_COUNT - 2].Position + 90 * Vector2.Down;
	}

	/// <summary>
	/// Flips the head up or down
	/// </summary>
	/// <param name="state">True to flip head over, false otherwise </param>
	private void flip_head(bool state, bool slow = false)
	{
		if (state)
		{
			Tween tween = GetTree().CreateTween();
			tween.TweenProperty(head, "rotation", Mathf.Pi, 1.25f).SetTrans(Tween.TransitionType.Sine);
			if (head_state == HEAD_STATES.STALE) return;
			sound_player.Play_Effect_Static("RotateShort", -25);
		}
		else
		{
			if (slow)
			{
				sound_player.Play_Effect_Static("RotateLong", -30);
				Tween tween = GetTree().CreateTween();
				tween.TweenProperty(head, "rotation", 0, 5f).SetTrans(Tween.TransitionType.Sine);
			}
			else
			{
				Tween tween = GetTree().CreateTween();
				tween.TweenProperty(head, "rotation", 0, 1.25f).SetTrans(Tween.TransitionType.Sine);
				sound_player.Play_Effect_Static("RotateShort", -25);
			}
		}
	}
}
