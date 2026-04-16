using Godot;
using System;

public partial class IceSphere : Node2D
{

	public float SPHERE_RADIUS, SPHERE_RADIUS2;

	private SoundPlayer sound_player;
	public Vector2 center;
	public int id;
	public IceSphereHandler handler;

	/// <summary> If the eye is currently open</summary>
	public bool Eye_State = false;
	public Node2D eye_target = null;
	/// <summary> If the mouth is currently open </summary>
	public bool Mouth_State = false;

	private AnimationPlayer sphere_animator;
	private Node2D visibles;

	/// <summary> Pupil of the eye</summary>
	private Sprite2D pupil;
	private Sprite2D core;
	/// <summary> Default scale of the pupil</summary>
	private const float PUPIL_SCALE = 6f;

	/// <summary> How long the eye stays open for </summary>
	private const float EYE_WINDOW = 5;
	private float eye_timer = 0;

	private const float DROP_SHAKE = 4;
	private float drop_timer = 0;
	private float shake_offset;

	/// <summary>
	/// Pupil calculations
	/// </summary>
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sound_player = GetNode<SoundPlayer>("SoundPlayer");
		sphere_animator = GetNode<AnimationPlayer>("SphereAnimator");
		visibles = GetNode<Node2D>("Visibles");
		pupil = visibles.GetNode<Sprite2D>("Pupil");
		core = visibles.GetNode<Sprite2D>("Core");

		/* Store SPHERE_RADIUS */
		this.SPHERE_RADIUS = IceSphereHandler.SPHERE_RADIUS;
		this.SPHERE_RADIUS2 = this.SPHERE_RADIUS * this.SPHERE_RADIUS;

		shake_offset = GD.Randf() * Mathf.Pi;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		/* Aiming eye */
		if (pupil.Visible)
		{
			if (eye_target != null)
			{
				Point_Eye(eye_target.GlobalPosition);
			}
			if (Eye_State == true)
			{
				eye_timer -= (float)delta;
				if (eye_timer <= 0)
				{
					Open_Eye(false);
				}
			}
		}

		/* Dropping eye */
		if (drop_timer > 0)
		{
			drop_timer -= (float)delta;
			visibles.Position = new Vector2(Mathf.Sin(drop_timer * 25 + shake_offset), Mathf.Cos(drop_timer * 25 + shake_offset)) * 5;
			if (drop_timer <= 0)
			{
				handler.Bite(id);
				/* Altert handler */
				handler.Update_Edges();
				Tween tween = GetTree().CreateTween();
				tween.TweenProperty(visibles, "position", new Vector2(0, 2000), 1.75f);
			}
		}
	}

	/// <summary>
	/// Initializes ice sphere and sets its hue and center
	/// </summary>
	/// <param name="center"></param>
	/// <param name="hue"></param>
	public void Initialize(IceSphereHandler handler, Vector2 center, int id, Color hue)
	{
		this.handler = handler;
		this.center = center;
		this.id = id;
		this.GlobalPosition = center;
		this.Modulate = hue;
	}

	/// <summary>
	/// Sets the hue of the ice sphere
	/// </summary>
	/// <param name="hue"> The hue to set it to </param>
	public void Set_Hue(Color hue)
	{
		this.Modulate = hue;
	}

	public void Hurt(int damage)
	{
		/* Only damagable in eye form */
		if (Eye_State == true)
		{
			handler.Hurt(damage);
			Rpc("Flash");
		}
	}

	/// <summary>
	/// Flashes the pupil when it is hurt
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Flash()
	{
		core.SelfModulate = Colors.Red;

		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(core, "self_modulate", new Color(1, 1, 1, 1), 0.5f);

		sound_player.Play_Effect("Hurt", -30, 0.7f);

		if (Eye_State == true)
		{
			sphere_animator.Play("ShellHurt");
		}
	}

	/// <summary>
	/// Causes this sphere to drop away
	/// </summary>
	public void Drop()
	{
		drop_timer = DROP_SHAKE;
	}

	public void Set_Eye_Target(Node2D target)
	{
		eye_target = target;
	}

	public void Set_Eye(Vector2 position)
	{
		float distance = position.LengthSquared();
		if (distance > SPHERE_RADIUS * SPHERE_RADIUS)
		{
			return;
		}
		else
		{
			pupil.Position = position;
			pupil.Rotation = position.Angle() + Mathf.Pi / 2;

			pupil.Scale = PUPIL_SCALE * new Vector2(1, Mathf.Sqrt(SPHERE_RADIUS2 - distance) / SPHERE_RADIUS);
		}
	}

	public void Point_Eye(Vector2 global_position)
	{
		Vector2 position = global_position - this.GlobalPosition;
		float pos_angle = position.Angle();
		Vector2 eye_pos = Vector2.Zero;
		/* Find corresponding rotational angle */
		if (position.Y >= 0)
		{
			/* Disc turned by pi/12 */
			eye_pos.X = Mathf.Cos(pos_angle);
			eye_pos.Y = Mathf.Sin(Mathf.Pi / 12) * Mathf.Sin(pos_angle);
		}
		else
		{
			/* Disc turned by pi/4 */
			eye_pos.X = Mathf.Cos(pos_angle);
			eye_pos.Y = Mathf.Sin(Mathf.Pi / 4) * Mathf.Sin(pos_angle);
		}
		/* Scale to position */
		eye_pos *= Mathf.Min(SPHERE_RADIUS, position.Length()) * 0.9f;
		Set_Eye(eye_pos);
	}

	/// <summary>
	/// Checking animation endings
	/// </summary>
	public void _on_sphere_animator_animation_finished(String anim_name)
	{
		if (anim_name == "MouthClose")
		{
			handler.Bite(id);
		}
		if (anim_name == "ShellClose")
		{
			pupil.Visible = false;
		}
	}

	/// <summary> Sets whether the eye is open or not</summary>
	/// <param name="state"> Desired state of the eye </param>
	public void Open_Eye(bool state)
	{
		/* Only update if different */
		if (state != Eye_State)
		{
			Eye_State = state;
			if (state == true)
			{
				sphere_animator.Play("ShellOpen");
				pupil.Visible = true;
				eye_timer = EYE_WINDOW;
				sound_player.Play_Effect("Open", -30, GD.Randf() / 6 + 0.3f);
			}
			else
			{
				sphere_animator.Play("ShellClose");
			}
		}
	}

	/// <summary> Sets whether the mouth is open or not</summary>
	/// <param name="state"> Desired state of the mouth </param>
	public void Open_Mouth(bool state)
	{
		/* Only do things if eye closed */
		if (Eye_State == true) return;
		/* Only update if different */
		if (state != Mouth_State)
		{
			Mouth_State = state;
			if (state == true)
			{
				sphere_animator.Play("MouthOpen");
			}
			else
			{
				sphere_animator.Play("MouthClose");
			}
		}
	}

}
