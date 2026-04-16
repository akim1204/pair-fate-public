using Godot;
using System;

public partial class ProximityMessage : Area2D
{

	/// <summary>
	/// The message to display.
	/// </summary>
	[Export]
	private string display_message = "";

	/// <summary>
	/// How long the message will be displayed.
	/// </summary>
	[Export]
	private float display_duration = 2;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_area_entered(Area2D area)
	{
		/* Check if player */
		if (area.GetType().IsAssignableTo(typeof(PlayerHurtbox)))
		{
			/* Dispaly message and destroy */
			GameManager.Instance.Display_Message_All(display_message, 2);
			Rpc("destroy");
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void destroy()
	{
		CallDeferred("disable");
	}
	public void disable()
	{
		this.ProcessMode = ProcessModeEnum.Disabled;
	}
}
