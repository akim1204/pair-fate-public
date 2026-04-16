using Godot;
using System;

public partial class JelloKey : Item
{

	public override void _Ready()
	{
		/* Overriding item type and name */
		this.item_type = ItemTypes.Utility;
		this.item_name = "JelloKey";


		/* Calling base constructor */
		base._Ready();
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
		/* Store owner */
		this.owner_player = owner_player;
		/* Exit throw state */
		in_throw = false;

		/* Centered on hand */
		this.Position = new Vector2(0, 0);
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

		/* Cane 'drags behind' player */
		float angle_dest = player_facing.Angle() - Mathf.Pi / 2;

		/* Lerp towards desired angle */
		this.Rotation = Mathf.LerpAngle(this.Rotation, angle_dest, delta * 15);

		/* Place in front of or behind player depending on rotation  TODO: perhaps easier way to calculate this? */
		if (Mathf.Abs((this.Rotation % (Mathf.Pi * 2) + Mathf.Pi * 2) % (Mathf.Pi * 2) - Mathf.Pi) < Mathf.Pi / 2)
		{
			ZIndex = -1;
		}
		else
		{
			ZIndex = 1;
		}

		/* Calculate hand positions */
		Vector2 hand_left_potential = this.Position + Vector2.FromAngle(this.Rotation - Mathf.Pi / 2) * 3;
		Vector2 hand_right_potential = this.Position + Vector2.FromAngle(this.Rotation - Mathf.Pi / 2) * 18;

		/* No effect on movement */
		return (movement_vector, hand_left_potential, hand_right_potential);
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
		/* Has no action */
		return false;
	}

}
