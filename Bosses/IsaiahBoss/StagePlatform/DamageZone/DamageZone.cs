using Godot;
using System;

public partial class DamageZone : Area2D
{
	private bool _activated = false;
	private Vector2 center;
	[Export]
	public bool is_red = true;
	private AnimatedSprite2D sprite_player;
	private SoundPlayer sound_player;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sound_player = GetNode<SoundPlayer>("SoundPlayer");
		sprite_player = this.GetNode<AnimatedSprite2D>("Sprite2D");
		center = this.GlobalPosition;
		if (is_red) {
			sprite_player.Play("red");
		}
		else {
			sprite_player.Play("blue");
		}
		sprite_player.AnimationChanged += Play_Sounds;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_activated = false;
		if (this.HasOverlappingAreas()) {
			for (int i=0; i<this.GetOverlappingAreas().Count;i++) {
				Area2D area = this.GetOverlappingAreas()[i];
				if (area.GetType().IsAssignableTo(typeof(PlayerHurtbox))) {
					/* Cast to hurtboxenemyparent */
					Handle_Player_Area((PlayerHurtbox)area);
				}
			}
		}
		if (_activated) {
			if (is_red) {
				sprite_player.Play("red_active");
			}
			else {
				sprite_player.Play("blue_active");
			}
		}
		else {
			if (is_red) {
				sprite_player.Play("red");
			}
			else {
				sprite_player.Play("blue");
			}
		}
	}
	private void Handle_Player_Area(PlayerHurtbox area)
	{

		if ((area.GetPlayer() == GameManager.Instance.Get_Player_Bag().GetAllPlayers()[0]
			& this.is_red) | (area.GetPlayer() != GameManager.Instance.Get_Player_Bag().GetAllPlayers()[0]
			& !this.is_red)) {

			_activated = true;
		}
	}
	private void Play_Sounds() {
		if (sprite_player.Animation == "red_active" | sprite_player.Animation == "blue_active") {
			sound_player.Play_Effect("click_down",-4);
		}
		else {
			sound_player.Play_Effect("click_up",-4);
		}
	}
	public bool Get_Active()
	{
		return _activated;
	}
}
