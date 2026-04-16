using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection.Metadata;

public partial class Arm : Node2D
{
	private int attack_phase = 0;
	private float slam_progress = 0;
	private bool is_slamming = false;
	private float[] _phases = new float[4];
	private float pause_height = 500f;
	private const float PAUSE_HEIGHT = 500f;
	private Vector2 approach_vector;
	private Vector2 slam_vector;
	private Vector2 _target;
	private DragonBoss owner_boss;
	/// <summary>
	/// Can be 0 or 1, indicating which player can hit it.
	/// </summary>
	private int _color = 0;
	private float vuln_timer = 0f;
	private Random rand;
	private bool is_vuln = false;
	private bool is_heavy_slam = false;
	private PackedScene preloaded_impact;
	private PackedScene preloaded_hurtbox;
	private DragonArmHurtbox _hurtbox;
	private float damage_bonus = 1f;
	private DragonArmHitbox arm_hitbox;
	private Sprite2D _shadow;
	private Vector2 shadow_vector;
	private const float VULN_TIME = .1f;
	[Export]
	public float STICK_TIME = 5f;
	/* Getting world context */
	protected Node WORLD;
	private SoundPlayer sound_player;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_phases[0] = 1f; // approach
		_phases[1] = .3f; // pause
		_phases[2] = .2f; // slam
		_phases[3] = VULN_TIME; // vuln time 
		rand = new Random();

		this.slam_vector = new Vector2(0f, pause_height / _phases[2]);

		// Temporary
		_target = new Vector2(500, 500);

		owner_boss = GetParent<DragonBoss>();
		_shadow = GetNode<Sprite2D>("ArmShadow");
		sound_player = GetNode<SoundPlayer>("SoundPlayer");

		preloaded_impact = GD.Load<PackedScene>("res://Bosses/IsaiahBoss/DragonArm/ArmCrater/ArmCrater.tscn");
		// preloaded_hurtbox = GD.Load<PackedScene>("res://Bosses/IsaiahBoss/DragonArm/DragonArmHurtbox/DragonArmHurtbox.tscn");
		this._hurtbox = GetNode<DragonArmHurtbox>("ArmHurtbox");

		arm_hitbox = GetNode<DragonArmHitbox>("ArmHitbox");
		WORLD = GetTree().Root;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (this.is_slamming)
		{
			Handle_Slam(delta);
		}
		Handle_Vuln(delta);

	}

	private void Handle_Slam(double delta)
	{
		/* Approach */
		if (slam_progress <= _phases[0])
		{
			this.Modulate = new Color(1f, 1f, 1f, 1f);
			this.GlobalPosition += approach_vector * (float)delta;
			this.is_vuln = false;
			arm_hitbox.Set_Active(false);

			this._shadow.Offset = new Vector2(0, pause_height * slam_progress / _phases[0]);

		}
		/* Pause */
		else if (slam_progress <= _phases[1] + _phases[0])
		{
			this.is_vuln = false;
			if (this.is_heavy_slam)
			{
				this.Modulate = new Color(.6f, .2f, .8f, 1f);
			}
			/* If shadow is hidden, show before slamming  */
			if (_phases[0] + _phases[1] + _phases[2] - slam_progress < .2f)
			{
				_shadow.Show();
			}
			/* Animate */
		}
		/* Slam down */
		else if (slam_progress < _phases[2] + _phases[1] + _phases[0])
		{
			this.GlobalPosition += this.slam_vector * (float)delta;
			/* Remember to show shadow!*/
			_shadow.Show();
			if (Math.Abs((float)(slam_progress - (_phases[2] + _phases[1] + _phases[0]))) < .02f)
			{
				foreach (PlayerCamera camera in GameManager.Instance.Get_Player_Bag().GetAllCameras())
				{
					camera.Shake(0.5f, 20, 10);
				}

				if (this.is_heavy_slam)
				{
					/* Avoid spawning multiple craters */
					if (arm_hitbox.Get_Active())
					{
						Add_Crater();
					}
				}
				Play_Land_Sound();
				arm_hitbox.Set_Active(true);

			}
			this._shadow.Offset -= this.slam_vector * (float)delta;
			this.is_vuln = false;
		}
		else
		{
			if (!this.is_vuln)
			{
				Begin_Vuln(VULN_TIME);
			}
			/* Handle vuln */
		}

		/* Slam finished */
		if (slam_progress >= _phases[0] + _phases[1] + _phases[2] + _phases[3])
		{
			slam_progress = 0;
			this.is_slamming = false;
			this.is_vuln = false;
			/* Reset pause height if it was modified */
			this.pause_height = PAUSE_HEIGHT;
		}
		slam_progress += (float)delta;

	}

	public void Add_Crater()
	{
		Rpc("RPC_Add_Crater", this._target);
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Add_Crater(Vector2 target)
	{
		if (WORLD == null)
		{
			GD.Print("World is null");
			return;
		}
		var crater = preloaded_impact.Instantiate<ArmCrater>();
		WORLD.CallDeferred("add_child", crater);
		crater.Initialize(target);
	}
	public void Begin_Heavy_Slam(Vector2 target, float[] phase_times)
	{
		Rpc("RPC_Begin_Heavy_Slam", target, phase_times);
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Begin_Heavy_Slam(Vector2 target, float[] phase_times)
	{
		damage_bonus = 1;
		this.slam_vector = new Vector2(0f, pause_height / _phases[2]);
		this.is_heavy_slam = true;
		for (int i = 0; i < 4; i++)
		{
			if (i == 1)
			{
				_phases[i] = 1f; // Extend pause for longer 
				continue;
			}
			_phases[i] = phase_times[i];
		}
		this._target = target;

		this.slam_progress = 0;

		Vector2 move_vector = new Vector2(target.X, target.Y - pause_height) - this.GlobalPosition;
		/* Move to the approach spot within the duration of _phase[0] */
		this.approach_vector = move_vector /= _phases[0];

		this.is_slamming = true;
	}
	public void Begin_Slam(Vector2 target, float[] phase_times)
	{
		Rpc("RPC_Begin_Slam", target, phase_times);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Begin_Slam(Vector2 target, float[] phase_times)
	{
		damage_bonus = 1;
		this.slam_vector = new Vector2(0f, pause_height / _phases[2]);
		this.is_heavy_slam = false;
		for (int i = 0; i < 4; i++)
		{
			_phases[i] = phase_times[i];
		}
		this._target = target;

		this.slam_progress = 0;

		Vector2 move_vector = new Vector2(target.X, target.Y - pause_height) - this.GlobalPosition;
		/* Move to the approach spot within the duration of _phase[0] */
		this.approach_vector = move_vector /= _phases[0];


		this.is_slamming = true;
	}
	public void Handle_Vuln(double delta)
	{
		// if (!this.is_vuln & this._hurtbox != null) {
		// 	this._hurtbox.QueueFree();
		// 	this._hurtbox = null;
		// }
		this._hurtbox.Set_Active(this.is_vuln);
	}
	public void Begin_Vuln(float vuln_time)
	{
		Rpc("RPC_Begin_Vuln", vuln_time);
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Begin_Vuln(float vulnerable_time)
	{
		_phases[3] = vulnerable_time;
		// var hurtbox = this.preloaded_hurtbox.Instantiate<DragonArmHurtbox>();
		// AddChild(hurtbox);
		// this._hurtbox = hurtbox;
		this.is_vuln = true;

	}
	/// <summary>
	/// Handles the actions of getting hit by a player-created hitbox.
	/// </summary>
	/// <param name="player">Owner of the hitbox that hit the arm</param>
	public void Register_Hit(Player player)
	{
		// player_list = GameManager.Instance.Get_Player_Bag().GetActivePlayers();

		// if (player == player_list[_color]) {
		// 	/* With each consecutive hit, damage the boss more */
		// 	owner_boss.Hurt_Hp((int) damage_bonus);
		// 	_color = rand.Next(player_list.Count);

		// 	/* Increase multiplier every 2 hits */
		// 	this.damage_bonus += .25f;

		// 	if (this.is_slamming) { // If you catch it, increase vuln time;
		// 		_phases[3] = 2f;
		// 	}
		// }
		// else {
		// 	owner_boss.Heal_Hp(1);
		// 	/* Let the arm immediately rise back up, remember to delete hitbox */
		// 	_phases[3] = VULN_TIME;
		// 	this.damage_bonus = 1;
		// 	Begin_Slam(_target, _phases);
		// }
		/* Arms cannot be damaged. */
		return;

	}
	public void Stick_Arm()
	{
		Rpc("RPC_Stick_Arm");
	}
	/// <summary>
	/// Delays the time spent on the ground, for when it gets stuck.
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Stick_Arm()
	{
		if (this.is_slamming)
		{
			_phases[3] = STICK_TIME; // Stick the arm for 5 seconds
		}
	}
	public void Begin_Sky_Slam(Vector2 target)
	{
		this.pause_height = 800f;
		_shadow.Hide();
		float[] new_times = new float[4];
		new_times[0] = 1f; new_times[1] = 4f; new_times[2] = .2f; new_times[3] = .8f;
		Begin_Slam(target, new_times);
	}
	private void Play_Land_Sound()
	{
		string stomp_pick = "Stomp" + rand.Next(1, 5).ToString();
		sound_player.Play_Effect(stomp_pick, -4, .9f);
	}
	private void Play_Hurt_Animation()
	{
		return;
	}
	public int Get_Color()
	{
		return _color;
	}
	public bool Get_Slamming()
	{
		return this.is_slamming;
	}
	public bool Get_Is_Vuln()
	{
		return this.is_vuln;
	}
	public void Start_Destroy()
	{
		this.QueueFree();
	}
}
