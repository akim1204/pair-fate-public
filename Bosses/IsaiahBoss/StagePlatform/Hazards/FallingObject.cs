using Godot;
using System;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

public partial class FallingObject : Node2D
{
	private AnimatedSprite2D _sprite;
	private float crash_timer = 0f;
	private float sprite_offset;
	private const float FALLTIME = .5f;
	private Vector2 _end_pos;
	private float pause_time = 2.5f;
	private Vector2 move_vector;
	private bool on_ground = false;
	private Sprite2D _shadow;
	private SoundPlayer sound_player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this._sprite = this.GetNode<AnimatedSprite2D>("ObjectSprite");
		sound_player = GetNode<SoundPlayer>("SoundPlayer");
		_shadow = GetNode<Sprite2D>("ObjectShadow");
		_shadow.Hide();
		_sprite.Play("cake");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (pause_time > 0) 
		{
			if (pause_time < 1.5f) {
				_shadow.Show();
			}
			pause_time -= (float)delta;
		}

		else {
			if (!on_ground) {
				if ((this._sprite.GlobalPosition += this.move_vector * (float)delta).Y > _end_pos.Y) {
					this._sprite.GlobalPosition = this._end_pos;
					if (!on_ground) {
						sound_player.Play_Effect("thump",-5);
					}
					this.on_ground = true;
					this.crash_timer = .1f;
				}
			}
			else {
				if (this.crash_timer < 0) {
					Start_Destroy();
				}
				if (this.crash_timer < .05f) {
					_sprite.Play("smash");
				}
				this.crash_timer -= (float)delta;
			}
		}

	}
	public void Initialize(Vector2 start_pos, Vector2 end_pos, float size, string skin)
	{
		this._sprite = this.GetNode<AnimatedSprite2D>("ObjectSprite");
		this.crash_timer = 0;

		this.Scale *= size;
		this.GlobalPosition = end_pos;
		this._end_pos = end_pos;
		/* Switch sprite to the correct skin */
		// this.sprite.Play(skin);

		this.move_vector = new Vector2(0, (end_pos.Y - start_pos.Y)/FALLTIME);
		this._sprite.GlobalPosition = start_pos;

	}
	public void Set_Crash_Timer(float dur)
	{
		this.crash_timer = dur;
	}
	public bool Get_Crashing()
	{
		return crash_timer > 0;
	}
	public void Start_Destroy()
	{
		this.QueueFree();
	}
}
