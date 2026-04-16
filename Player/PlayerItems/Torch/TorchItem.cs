using Godot;
using System;

public partial class TorchItem : Item
{
	/*
	 * Constants used for shield placements
	 */
	/// <summary>
	/// Offset of how far behind the shield is dragged.
	/// </summary>
	private const float HOLD_OFFSET = 10f;
	/// <summary>
	/// Destination angle when shield is dragging behind player
	/// </summary>
	private float drag_angle_dest = 0;

	/// <summary>
	/// Awkward boolean to indicate if the shield has been given a direction yet
	/// </summary>
	private bool directed = false;


	/// <summary>
	/// Indicates if the shield is currently held up or not.
	/// </summary>
	private bool shield_raised = false;
	public override void _Ready()
	{
		/* Overriding item type and name */
		this.item_type = ItemTypes.Utility;
		this.item_name = "Torch";
		this.item_tooltip = "Has no use beyond lighting fires.";

		/* Calling base constructor */
		base._Ready();
		this.Rotation = Mathf.Pi / 4;
	}

	/// <summary>
	/// Initialize position of item when it is first picked up
	/// </summary>
	/// <param name="owner_player"> Player that is picking up this item </param>
	/// <param name="move_input_vector"> Player movement vector of current frame </param>
	/// <param name="face_input_vector"> Player facing vector of the current frame. </param> 
	/// <param name="movement_vector"> Player movement vector of current frame </param>
	/// <param name="player_facing"> Direction that the player is currently facing </param>
	public override void Handle_Pickup(Player owner_player, Vector2 move_input_vector,
		Vector2 face_input_vector, Vector2 movement_vector, Vector2 player_facing)
	{
		/* Store owner player */
		this.owner_player = owner_player;

		/* Exit throw state */
		in_throw = false;

		/* Slightly offset */
		this.Position = HOLD_OFFSET * player_facing;
		this.Rotation = -Mathf.Pi / 4;

		Position = Vector2.Zero;
	}

	/// <summary>
	/// Handles the animation of the item when not acting.
	/// </summary>
	/// <param name="move_input_vector"> Player movement vector of current frame </param>
	/// <param name="face_input_vector"> Player facing vector of the current frame. </param> 
	/// <param name="movement_vector"> Player movement vector of current frame </param>
	/// <param name="player_facing"> Direction that the player is currently facing </param>
	/// <param name="delta"> Time elapsed since the previous frame, cast to a float </param>
	/// <returns> Vector2 representing new player velocity. </returns>
	public override (Vector2, Vector2, Vector2) Handle_Animation_Default(float delta, Vector2 move_input_vector,
		Vector2 face_input_vector, Vector2 movement_vector, Vector2 player_facing)
	{
		/* Slightly offset */
		this.Position = HOLD_OFFSET * player_facing;
		/* Place in front of or behind player depending on rotation  TODO: perhaps easier way to calculate this? */
		if (player_facing.Y > 0)
		{
			ZIndex = 1;
		}
		else
		{
			ZIndex = -1;
		}

		/* No effect on movement */
		return (movement_vector, Vector2.Zero, Vector2.Zero);
	}

	/// <summary>
	/// Called when an action is initialized to reset action states and the like.
	/// </summary>
	/// <param name="move_input_vector"> Player movement vector of current frame </param>
	/// <param name="face_input_vector"> Player facing vector of the current frame. </param> 
	/// <param name="movement_vector"> Player movement vector of current frame </param>
	/// <param name="player_facing"> Direction that the player is currently facing </param>
	/// <returns> Returns true if an action is started, false otherwise </returns>
	public override bool Handle_Action_Begin(Vector2 move_input_vector,
		Vector2 face_input_vector, Vector2 movement_vector, Vector2 player_facing)
	{
		return false;
	}


}
