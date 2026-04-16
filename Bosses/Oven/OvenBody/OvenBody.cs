using Godot;
using System;
using System.Collections.Generic;

public partial class OvenBody : Node2D
{
	private OvenController oven_controller;

	private Sprite2D oven_left;
	private Sprite2D oven_right;

	private OvenBodyFire fire_left_left;
	private OvenBodyFire fire_left_right;
	private OvenBodyFire fire_right_left;
	private OvenBodyFire fire_right_right;
	private AnimationPlayer oven_animator;

	private List<OvenBodyFire> oven_fires = new List<OvenBodyFire>();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		oven_left = GetNode<Sprite2D>("OvenLeft");
		oven_right = GetNode<Sprite2D>("OvenRight");
		oven_animator = GetNode<AnimationPlayer>("OvenAnimator");

		fire_left_left = oven_left.GetNode<OvenBodyFire>("OvenLeftLeftFire");
		oven_fires.Add(fire_left_left);
		fire_left_right = oven_left.GetNode<OvenBodyFire>("OvenLeftRightFire");
		oven_fires.Add(fire_left_right);
		fire_right_left = oven_right.GetNode<OvenBodyFire>("OvenRightLeftFire");
		oven_fires.Add(fire_right_left);
		fire_right_right = oven_right.GetNode<OvenBodyFire>("OvenRightRightFire");
		oven_fires.Add(fire_right_right);

		/* Initialize of controllers */
		for (int i = 0; i < 4; i++)
		{
			oven_fires[i].Set_Controller(oven_controller);
		}
	}

	public void Activate()
	{

		for (int i = 0; i < 4; i++)
		{
			oven_fires[i].Activate();
		}
	}

	public void Set_Controller(OvenController controller)
	{
		oven_controller = controller;
	}

	/// <summary>
	/// Ignites a given oven fire
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Ignite(int fire)
	{
		/* Extinguish all other fires */
		Extinguish();
		/* Ignite fires */
		oven_fires[fire].Ignite();
		if (fire < 2)
		{
			oven_animator.Play("LeftIgnite");
		}
		else
		{
			oven_animator.Play("RightIgnite");
		}
	}

	/// <summary>
	/// Extinguishes all fires
	/// </summary>
	public void Extinguish()
	{
		/* Extinguish all other fires */
		for (int i = 0; i < 4; i++)
		{
			oven_fires[i].Extinguish();
		}
		oven_animator.Play("Idle");
	}

	/// <summary>
	/// Fully ignites the oven
	/// </summary>
	public void Full_Ignite()
	{
		oven_animator.Play("FullIgnite");
		for (int i = 0; i < 4; i++)
		{
			oven_fires[i].Fake_Ignite();
		}
	}

	public void Cry()
	{

		/* Extinguish all other fires */
		for (int i = 0; i < 4; i++)
		{
			oven_fires[i].Extinguish();
		}
		oven_animator.Play("Cry");
	}
}
