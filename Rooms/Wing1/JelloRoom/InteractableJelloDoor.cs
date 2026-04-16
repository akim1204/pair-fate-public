using Godot;
using System;

public partial class InteractableJelloDoor : Interactable
{

	/// <summary> Door manager that handles door information. </summary>
	private DoorManager door_manager;

	/// <summary>
	/// Path to scene and scene_index of scene this door leads to.
	/// </summary>
	private string destination_scene;
	private int destination_index;

	private bool activated = false;

	public override void _Ready()
	{
		/* Calling base constructor */
		base._Ready();

		/* Overriding item type and name */
		this.interactable_name = "InteractableDoor";

		door_manager = GameManager.Instance.Get_Door_Manager();

		this.interact_radius = 50;


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

		/* Start the boss the first time around */
		if (!activated)
		{
			/* Get the jello controller */
			JelloControllerNetworked controller = GetNode<JelloControllerNetworked>("../bossController0");
			controller.Start_Fight();
			activated = true;
		}

		/* If no key */
		if (player.Get_Item() == null || player.Get_Item().Get_Name() != "JelloKey")
		{
			GameManager.Instance.Display_Message("This door requires a key", 2);
		}
		/* If boss is not finished */
		else
		{
			JelloControllerNetworked controller = GetNode<JelloControllerNetworked>("../bossController0");
			if (controller.active)
			{
				GameManager.Instance.Display_Message("There is still a threat", 2);
			}
			else
			{
				/* Call a scene load */
				GameManager.Instance.CallDeferred("Load_Level_Synced", destination_scene, destination_index);
			}
		}
	}
}
