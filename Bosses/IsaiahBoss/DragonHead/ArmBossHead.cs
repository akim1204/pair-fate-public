using Godot;
using System;
using System.ComponentModel;

public partial class ArmBossHead : Node2D
{
	private float sway_amp = 30f;
	private float sway_speed = 1f;
	private float initial_rotation =0;
	private float sway_timer = 0;
	private float xratio = 2;
	private float yratio = .4f;
	private float spin_timer;
	private bool is_vuln = false;
	private AnimatedSprite2D _sprite;
	private WeakColor weak_color;
	public enum WeakColor {
		RED,
		BLUE,
		PURPLE,
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Rotation = 0f;
		this.weak_color = WeakColor.RED;
		_sprite = GetNode<AnimatedSprite2D>("CakeHeadSprite");
		_sprite.Animation = "default";
		// _sprite.Play("default");
		_sprite.Frame = 2;
		_sprite.AnimationFinished += Set_Eyes;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		Handle_Sway(delta);

		// if (this.weak_color == WeakColor.PURPLE) {
		// 	this.Modulate = new Color(.7f, 0f, .7f, 1f);
		// }
		if (this.weak_color == WeakColor.RED) {
			ShaderMaterial shade_material = _sprite.Material as ShaderMaterial;
			shade_material.SetShaderParameter("line_color", new Color(.67f,0f,0f));
		}
		if (this.weak_color == WeakColor.BLUE) {
			ShaderMaterial shade_material = _sprite.Material as ShaderMaterial;
			shade_material.SetShaderParameter("line_color", new Color(0f,.63f,.9f));
		}


	}
	private void Set_Eyes() {
		_sprite.Stop();
		if (is_vuln) {
			_sprite.Frame = 0;
		}
		else {
			_sprite.Frame = 2;
		}
	}

	private void Handle_Sway(double delta) 
	{
		sway_timer += (float)delta;
		if (sway_timer >= Mathf.Pi * 32)
		{
			sway_timer -= Mathf.Pi * 32;
		}

		Vector2 sway_velocity = new Vector2(sway_amp * xratio * Mathf.Cos(sway_timer * xratio), sway_amp * yratio * Mathf.Sin(sway_timer * yratio));
		this.GlobalPosition += sway_velocity * (float)delta;
	}
	public WeakColor Get_Weak_Color()
	{
		return this.weak_color;
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Set_Weak_Color(int col)
	{
		if (col == 0) {
			this.weak_color = WeakColor.RED;
		}
		if (col == 1) {
			this.weak_color = WeakColor.BLUE;
		}
		if (col == 2) {
			this.weak_color = WeakColor.PURPLE;
		}
	}
	public void Switch_Colors() {
		/* Ignore purple for now. */
		int col = new Random().Next(0,2);
		Rpc("RPC_Set_Weak_Color", col);

		GD.Print("Set Weak Color");
		GD.Print(col);
	}
	public bool Get_Is_Vuln()
	{
		return this.is_vuln;
	}
	public void Set_Vuln(bool status)
	{
		Rpc("RPC_Set_Vuln", status);
	}
	
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Set_Vuln(bool status)
	{
		if (status != this.is_vuln) {
			if (status) {
				_sprite.PlayBackwards("default");
			}
			else {
				_sprite.Play("default");
			}
		}
		this.is_vuln = status;
	}


}

