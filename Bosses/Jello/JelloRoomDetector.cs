using Godot;
using System;

public partial class JelloRoomDetector : Area2D
{

	/// <summary>
	/// The door to the boss room.
	/// </summary>
	private Node2D room_door;

	/// <summary>
	/// If the detector has been activated
	/// </summary>
	private bool activated = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Get and disable the door */
		room_door = GetNode<Node2D>("RoomDoor");
		this.RemoveChild(room_door);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_area_entered(Area2D area)
	{
		/* If a player character enters this area and its the second time*/
		if (GameManager.Instance.Get_Spawn_Index() == 1 && !activated && area.GetType().IsAssignableTo(typeof(PlayerHurtbox)))
		{
			/* Get the jello controller */
			JelloControllerNetworked controller = GetNode<JelloControllerNetworked>("../../bossController0");
			controller.Start_Fight();
			activated = true;
		}
	}

	public void Enable_Door()
	{
		CallDeferred("add_child", room_door);
	}
}
