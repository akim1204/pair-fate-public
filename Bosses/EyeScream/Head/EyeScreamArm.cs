using Godot;
using System;

public partial class EyeScreamArm : Node2D
{

	/// <summary> Length of arm </summary>
	public float ARM_LENGTH = 240;
	Random rand = new Random();

	/// <summary> Three individual parts of the arm </summary>
	private Node2D Arm1;
	private Node2D Arm2;
	private Node2D Arm3;
	private SoundPlayer sound_player;
	private EyeScreamHand Hand;
	private Sprite2D hand_shadow;
	private Sprite2D sweep_shadow;
	[Export]
	public int orientation = 1;

	public enum ARM_STATES
	{
		IDLE,
		MOVETO,
		SWEEP,
		SLAM,
		STALE,
		DEAD,
	}
	public ARM_STATES arm_state = ARM_STATES.STALE;

	public IceScreamHead controller;
	private float timer = 0;
	private float fragment_timer = 0;
	private const float FRAGMENT_INTERVAL = 0.1f;

	/// <summary> Default position the arm idles at </summary>
	private Vector2 default_center;

	/// <summary> Current location of the handprint </summary>
	public Vector2 current_handprint;

	/// <summary> Goal of the hand's movement </summary>
	private Vector2 move_goal;
	private ARM_STATES move_state = ARM_STATES.MOVETO;

	/// <summary> How fast it moves </summary>
	private const float MOVE_SPEED = 300;

	/// <summary> How long the sweep happens</summary>
	private const float SWEEP_LENGTH = 2f;
	private const float SWEEP_INITIAL = 0.5f;
	private const float SWEEP_LIFT_LENGTH = 0.5f;

	/// <summary> How fast the sweep accelerates </summary>
	private const float SWEEP_ACC = 325;
	private Vector2 slam_goal;

	private const float SLAM_HEIGHT = 200;
	private const float SLAM_LIFT = 100;
	private const float SLAM_LENGTH = 0.2f;
	private const float SLAM_CHARGE_LENGTH = 1f;
	private const int SLAM_COUNT = 3;
	private int slammed_count = 0;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sound_player = GetNode<SoundPlayer>("SoundPlayer");
		Arm1 = GetNode<Node2D>("Arm1");
		Arm2 = Arm1.GetNode<Node2D>("Arm2");
		Arm3 = Arm2.GetNode<Node2D>("Arm3");
		Hand = Arm3.GetNode<EyeScreamHand>("EyeScreamHand");
		hand_shadow = GetNode<Sprite2D>("HandShadow");
		sweep_shadow = GetNode<Sprite2D>("SweepShadow");
		hand_shadow.Hide();
		sweep_shadow.Hide();
		Set_Handprint(200 * Vector2.Up);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		hand_shadow.GlobalPosition = new Vector2(Hand.GlobalPosition.X, slam_goal.Y + 20);
		hand_shadow.GlobalRotation = 0;
		hand_shadow.Scale = new Vector2(1.5f, 1) * Mathf.Max(0, 1 - Mathf.Abs(Hand.GlobalPosition.Y - slam_goal.Y) / 1100);
		hand_shadow.Modulate = new Color(1, 1, 1, Mathf.Max(0, 1 - Mathf.Abs(Hand.GlobalPosition.Y - slam_goal.Y) / 700));

		sweep_shadow.GlobalPosition = new Vector2(Hand.GlobalPosition.X + -10 * orientation, slam_goal.Y - 10);
		switch (arm_state)
		{
			case ARM_STATES.DEAD:
				if (current_handprint.Y > -200)
				{
					Set_Handprint(current_handprint + (float)delta * Vector2.Up * 350);
				}
				break;
			case ARM_STATES.STALE:
				break;
			case ARM_STATES.IDLE:
				timer += (float)delta;
				Set_Handprint(default_center + idle_displacement(timer));
				break;
			case ARM_STATES.MOVETO:
				if (current_handprint.DistanceTo(move_goal) < MOVE_SPEED * delta)
				{
					Set_Handprint(move_goal);
					arm_state = move_state;
					timer = 0;

					if (arm_state != ARM_STATES.SLAM)
					{
						hand_shadow.Hide();
					}
					if (arm_state == ARM_STATES.SWEEP)
					{
						sound_player.Play_Effect_Static("SweepTouch", -35, 0.6f);
					}
				}
				else
				{
					Set_Handprint(current_handprint +
					(move_goal - current_handprint).Normalized() * MOVE_SPEED * (float)delta);
				}
				if (move_state == ARM_STATES.SWEEP)
				{
					sweep_shadow.Scale = new Vector2(3.2f * orientation, 3.2f) * Mathf.Max(0, 1 - Mathf.Abs(Hand.GlobalPosition.Y - slam_goal.Y) / 700);
					sweep_shadow.Modulate = new Color(1, 1, 1, Mathf.Max(0, 1 - Mathf.Abs(Hand.GlobalPosition.Y - slam_goal.Y) / 500));
				}
				break;
			case ARM_STATES.SWEEP:
				handle_sweep((float)delta);
				break;
			case ARM_STATES.SLAM:
				timer += (float)delta;
				handle_slam((float)delta);
				break;
		}
	}

	public void Initialize(IceScreamHead controller, Vector2 default_position)
	{
		this.controller = controller;
		this.default_center = default_position;
	}


	public void Stale()
	{
		this.arm_state = ARM_STATES.DEAD;
		Hand.Stale();
		hand_shadow.Hide();
		sweep_shadow.Hide();
	}
	public void Set_Move_Goal(Vector2 position, ARM_STATES intention)
	{
		/* Wrapping to size */
		if (current_handprint.Length() > 3 * ARM_LENGTH)
		{
			current_handprint = current_handprint.Normalized() * 3 * ARM_LENGTH;
		}
		move_goal = ToLocal(position);
		arm_state = ARM_STATES.MOVETO;
		move_state = intention;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Begin_Sweep(Vector2 position, Vector2 handprint)
	{
		Set_Handprint(handprint);
		Set_Move_Goal(position, ARM_STATES.SWEEP);

		sweep_shadow.Show();
		slam_goal = position;
		Hand.Sweep();
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Begin_Slam(Vector2 position, Vector2 handprint)
	{
		slam_goal = position;
		Set_Handprint(handprint);
		Set_Move_Goal(position + SLAM_LIFT * 2.5f * Vector2.Up, ARM_STATES.SLAM);
		slammed_count = 0;

		Hand.Fist();
		hand_shadow.Show();
	}

	public void Return_Idle()
	{
		timer = 0;
		Set_Move_Goal(ToGlobal(default_center + idle_displacement(0)), ARM_STATES.IDLE);
		Hand.Idle();
		sweep_shadow.Modulate = new Color(1, 1, 1, 0);
	}

	public void Set_Handprint(Vector2 position)
	{
		current_handprint = position;
		/* Scale to arm length of one */
		position /= ARM_LENGTH;

		/* Total offset of hand */
		float offset = position.Length();

		/* Case when too long */
		if (offset > 3)
		{
			/* Set angles */
			Arm2.Rotation = 0;
			Arm3.Rotation = 0;
			Arm1.Rotation = position.Angle();
			return;
		}
		/* Calculate individual angles */
		float remainder = offset - 1;
		/* First angle */
		float theta = Mathf.Acos(remainder / 2);

		/* Set angles */
		Arm2.Rotation = -theta * orientation;
		Arm3.Rotation = -theta * orientation;

		/* Final angle */
		Arm1.Rotation = position.Angle() + theta * orientation;
	}

	/// <summary>
	/// Colloding with another area2d
	/// </summary>
	/// <param name="area"></param>
	public void _on_area_2d_area_entered(Area2D area)
	{

		// If it is a player and this is sweeping
		if (area.GetType().IsAssignableTo(typeof(PlayerHurtbox)))
		{
			if (arm_state == ARM_STATES.SWEEP && timer < SWEEP_LENGTH + SWEEP_INITIAL &&
			timer > SWEEP_INITIAL)
			{
				/* Cast to hurtboxenemyparent */
				((PlayerHurtbox)area).Hurt(1);
			}
		}
	}

	private Vector2 idle_displacement(float time)
	{
		return new Vector2(40 * Mathf.Cos(time * 2 + 3), 20 + 40 * Mathf.Sin(time / 2 + 3));
	}

	private void handle_slam(float delta)
	{
		if (timer > SLAM_LENGTH + SLAM_CHARGE_LENGTH)
		{
			/* Play sound effect */
			sound_player.Play_Effect_Static("Slam", -35, 0.7f + (float)rand.NextDouble() / 3);

			if (slammed_count == SLAM_COUNT)
			{
				timer = 0;
				Set_Move_Goal(ToGlobal(default_center + idle_displacement(0)), ARM_STATES.IDLE);
				Hand.Idle();
			}
			else
			{
				slammed_count += 1;
				timer = SLAM_CHARGE_LENGTH - 2 * SLAM_LENGTH;
			}
			/* Screen shake */
			foreach (PlayerCamera camera in GameManager.Instance.Get_Player_Bag().GetAllCameras())
			{
				camera.Shake(0.5f, 15, 10);
			}

			/* Check damage on players */
			foreach (Player player in GameManager.Instance.Get_Player_Bag().GetAllPlayers())
			{
				if (player.GlobalPosition.DistanceTo(ToGlobal(current_handprint) + 25 * Vector2.Right * orientation + 10 * Vector2.Down) < 100)
				{
					player.Try_Hurt(1);
				}
			}

			/* Spawn fragments */
			for (int i = 0; i < 8; i++)
			{
				controller.Spawn_Fragment(ToGlobal(current_handprint) + GD.Randf() * 60f * Vector2.FromAngle(GD.Randf() * Mathf.Pi * 2));
			}
		}
		else if (timer > SLAM_CHARGE_LENGTH)
		{
			Set_Handprint(current_handprint + Vector2.Down * (float)delta
			 * (SLAM_HEIGHT + SLAM_LIFT) / SLAM_LENGTH);
		}
		else
		{
			if (slammed_count != 0)
			{
				Set_Handprint(current_handprint + Vector2.Up * (float)delta
				  * (SLAM_HEIGHT + SLAM_LIFT) / SLAM_LENGTH / 2
				  + Vector2.Right * orientation * (float)delta *
					  100);
			}
			else
			{
				Set_Handprint(current_handprint + Vector2.Up * (float)delta
				 * SLAM_HEIGHT / SLAM_CHARGE_LENGTH);
			}
		}
	}

	private void handle_sweep(float delta)
	{
		timer += (float)delta;
		if (timer > SWEEP_LENGTH + SWEEP_LIFT_LENGTH + SWEEP_INITIAL)
		{
			timer = 0;
			Set_Move_Goal(ToGlobal(default_center + idle_displacement(0)), ARM_STATES.IDLE);
			Hand.Idle();
			sweep_shadow.Hide();
		}
		else if (timer > SWEEP_LENGTH + SWEEP_INITIAL)
		{
			Set_Handprint(current_handprint + (float)delta * (150 * Vector2.Up
			+ SWEEP_LENGTH * SWEEP_ACC * Vector2.Right * orientation));
			sweep_shadow.Scale = new Vector2(3.2f * orientation, 3.2f) * Mathf.Max(0, 1 - Mathf.Abs(Hand.GlobalPosition.Y - slam_goal.Y) / 400);
			sweep_shadow.Modulate = new Color(1, 1, 1, Mathf.Min(Mathf.Max(0, 2 - Mathf.Abs(Hand.GlobalPosition.Y - slam_goal.Y) / 150), 1));
		}
		else if (timer > SWEEP_INITIAL)
		{
			if (timer - (float)delta <= SWEEP_INITIAL)
			{
				sound_player.Play_Effect_Static("SweepContinue", -35, 0.6f);
			}
			Set_Handprint(current_handprint + Vector2.Right *
			((timer - SWEEP_INITIAL) * SWEEP_ACC + SWEEP_INITIAL * SWEEP_ACC / 5) * (float)delta * orientation);

			fragment_timer += (float)delta * 5;
			if (fragment_timer > FRAGMENT_INTERVAL)
			{
				fragment_timer -= FRAGMENT_INTERVAL;
				controller.Spawn_Fragment(ToGlobal(current_handprint) + (200 - rand.Next(400)) * Vector2.Up);
			}
		}
		else
		{
			Set_Handprint(current_handprint + Vector2.Right * timer * SWEEP_ACC / 5 * (float)delta * orientation);

			fragment_timer += (float)delta;
			if (fragment_timer > FRAGMENT_INTERVAL)
			{
				fragment_timer -= FRAGMENT_INTERVAL;
				controller.Spawn_Fragment(ToGlobal(current_handprint) + (200 - rand.Next(400)) * Vector2.Up);
			}
		}
	}
}
