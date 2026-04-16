using Godot;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public partial class CandleMissile : Node2D
{
	private float delay_timer;
	private float fade_in_timer;
	private Vector2 _direction;
	private AnimatedSprite2D animator;
	private SoundPlayer sound_player;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Setting transparent
		this.Modulate -= new Color(0f,0f,0f,1f);
		this.animator = GetNode<AnimatedSprite2D>("AnimationPlayer");
		this.animator.Play("off");
		sound_player = GetNode<SoundPlayer>("SoundPlayer");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (this.GlobalPosition.X > 2200 | this.GlobalPosition.X < -768 | this.GlobalPosition.Y > 1792 | this.GlobalPosition.Y < 0) {
			Start_Destroy();
		}

		if (delay_timer > 0) {
			delay_timer -= (float)delta;
		}
		else if (fade_in_timer > 0) {
			if (fade_in_timer < .2f) {
				this.animator.Play("on");
				GetNode<CpuParticles2D>("LightParticles").Emitting = true;
			}
			fade_in_timer -= (float)delta;
			if (fade_in_timer <= 0) {
				sound_player.Play_Effect("whoosh");
			}
			this.Modulate += new Color(0f,0f,0f,1 * (float)delta);
		}
		else {
			this.GlobalPosition += this._direction * (float)delta;
		}
	}
	public void Initialize(Vector2 dir, float ang, Vector2 spawn, float delay)
	{
		/* Fade In */
		this.fade_in_timer = 2f;
		this.GlobalPosition = spawn;
		this._direction = dir;
		this.GlobalRotation = ang;
		this.delay_timer = delay;
	}
	public void Start_Destroy()
	{
		this.QueueFree();
	}
}
