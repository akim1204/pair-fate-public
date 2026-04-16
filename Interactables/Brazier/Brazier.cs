using Godot;
using System;

public partial class Brazier : Interactable
{
	public EyeScreamController Controller;
	private GpuParticles2D emitter;
	private SoundPlayer sound_player;
	public bool Ignited = false;
	public int id;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Calling base constructor */
		base._Ready();

		/* Overriding item type and name */
		this.interactable_name = "Brazier";

		sound_player = GetNode<SoundPlayer>("SoundPlayer");

		emitter = GetNode<GpuParticles2D>("FlameEmitter");
		emitter.Emitting = false;
	}


	public override void _Process(double delta)
	{
		base._Process(delta);
	}

	/// <summary>
	/// Initialize position of item when it is first picked up
	/// </summary>
	/// <param name="player"> Player who interacts with interactable </param>
	public override void Handle_Action(PlayerItem player)
	{
		if (player.Get_Item() != null && player.Get_Item().Get_Name() == "Torch")
		{
			Ignite(true);
		}
		else
		{
			GameManager.Instance.Display_Message("You are missing the tool for this", 4);
		}
	}

	public void Ignite(bool state)
	{
		if (Ignited != state)
		{
			/* Play sound */
			if (state == true)
			{
				sound_player.Play_Effect("Light", -15, GD.Randf() / 5 + 0.6f);
			}
			/* Inform controller */
			Controller.Inform_Ignite(state);
		}
		Ignited = state;
		emitter.Emitting = state;
	}

	/// <summary>
	/// Overriding get_type of this interactable type.
	/// </summary>
	public override InteractableTypes Get_Type()
	{
		return InteractableTypes.Utility;
	}
}
