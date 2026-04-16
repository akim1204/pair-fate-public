using Godot;
using System;

public partial class IntroDoor : Interactable
{

	/// <summary> Door manager that handles door information. </summary>
	private DoorManager door_manager;

	/// <summary>
	/// Path to scene and scene_index of scene this door leads to.
	/// </summary>
	private string destination_scene;
	private int destination_index;

	/// <summary> How close players have to be to the door. </summary>
	private float DOOR_RADIUS = 600;

	/// <summary> Sound player for door </summary>
	private SoundPlayer sound_player;

	public override void _Ready()
	{
		/* Calling base constructor */
		base._Ready();

		this.interact_radius = 200;

		/* Overriding item type and name */
		this.interactable_name = "InteractableDoor";

		door_manager = GameManager.Instance.Get_Door_Manager();

		sound_player = GetNode<SoundPlayer>("SoundPlayer");

		/* Check that the doors all work */
		if (door_manager == null)
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Door manager was not properly initialized");
			Remove_Interactable();
		}
		else
		{
			(destination_scene, destination_index) = door_manager.Get_Door(this.type_index);
			/* Check that it works */
			if (destination_index < 0)
			{
				Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Door manager did not assign this door");
				Remove_Interactable();
			}
		}
	}

	/// <summary>
	/// Overriding get_type of this interactable type.
	/// </summary>
	public override InteractableTypes Get_Type()
	{
		return InteractableTypes.Door;
	}

	/// <summary>
	/// Refills water bucket if its carried.
	/// </summary>
	/// <param name="player"> Player who interacts with interactable </param>
	public override void Handle_Action(PlayerItem player)
	{
		/* Play sound */
		sound_player.Play_Effect("Open", -10);

		/* Check player distances on server side */
		if (GameManager.Instance.Is_Host())
		{
			foreach (Player p in GameManager.Instance.Get_Player_Bag().GetAllPlayers())
			{
				if ((p.GlobalPosition - this.GlobalPosition).Length() > DOOR_RADIUS)
				{
					/* Display Message */
					GameManager.Instance.Display_Message_All("All players must be near door to continue", 2);
					return;
				}
			}
			/* Otherwise, load level */
			GameManager.Instance.CallDeferred("Load_Level_Synced", destination_scene, destination_index);
		}
	}

}
