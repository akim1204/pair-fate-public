using Godot;
using System;
using System.Collections.Generic;

public partial class InteractableDoorPrevented : Interactable
{


	/// <summary>
	/// Path to scene and scene_index of scene this door leads to.
	/// </summary>
	[Export]
	private string destination_scene;
	[Export]
	private int destination_index;

	/// <summary> How close players have to be to the door. </summary>
	private float DOOR_RADIUS = 400;

	/// <summary> Sound player for door </summary>
	private SoundPlayer sound_player;

	/// <summary>
	/// Nodes that must be gone before the door can open
	/// </summary>
	[Export]
	private String[] preventors;

	public override void _Ready()
	{
		/* Calling base constructor */
		base._Ready();

		/* Overriding item type and name */
		this.interactable_name = "InteractableDoor";


		sound_player = GetNode<SoundPlayer>("SoundPlayer");
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
		sound_player.Play_Effect_Static("Open", -15); //TODO: PLAY THIS EVERYWHERE

		/* Check player distances on server side */
		if (GameManager.Instance.Is_Host())
		{
			/* Check preventors are cleared */
			foreach (String path in preventors)
			{
				if (GetNodeOrNull(path) != null)
				{
					/* Display Message */
					GameManager.Instance.Display_Message_All("Something is preventing this door from being used", 2);
					return;

				}
			}
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
