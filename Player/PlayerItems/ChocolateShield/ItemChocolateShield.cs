using Godot;
using System;

public partial class ItemChocolateShield : Item
{
	/*
	 * Constants used for shield placements
	 */
	/// <summary>
	/// Offset of how far behind the shield is dragged.
	/// </summary>
	private const float DRAG_OFFSET = 10f;
	/// <summary>
	///  Offset for hoe distantly the shield is held.
	/// </summary>
	private const float RAISE_OFFSET = 50f;

	/// <summary>
	/// Height of the shield (larger than the visual shield).
	/// </summary>
	private const float SHIELD_HEIGHT = 150f;

	/// <summary>
	///  Widthof the shield (to prevent 'missing' collisions)
	/// </summary>
	private const float SHIELD_WIDTH = 30f;
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
		this.item_type = ItemTypes.Shield;
		this.item_name = "Chocolate Shield";
		this.item_tooltip = "Capable of blocking most small objects, at the cost of movement. Click and hold to use.";

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
		/* Store owner player */
		this.owner_player = owner_player;

		/* Exit throw state */
		in_throw = false;

		/* Lerp towards desired angle */
		this.Rotation = player_facing.Angle() + Mathf.Pi / 2;

		/* Slightly offset */
		this.Position = DRAG_OFFSET * Vector2.FromAngle(this.Rotation + Mathf.Pi / 3);

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
		/* Shield 'drags behind' player */
		drag_angle_dest = player_facing.Angle() + Mathf.Pi / 2;

		/* Lerp towards desired angle */
		this.Rotation = Mathf.LerpAngle(this.Rotation, drag_angle_dest, delta * 5);

		/* Slightly offset */
		this.Position = DRAG_OFFSET * Vector2.FromAngle(this.Rotation);

		/* Place in front of or behind player depending on rotation  TODO: perhaps easier way to calculate this? */
		if (Mathf.Abs((this.Rotation % (Mathf.Pi * 2) + Mathf.Pi * 2) % (Mathf.Pi * 2) - Mathf.Pi / 2) < Mathf.Pi / 2)
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
		this.directed = false;
		return true;
	}

	/// <summary>
	/// Handles the use and animation of the item when acting
	/// </summary>
	/// <param name="delta"> Time elapsed since the previous frame, cast to a float </param>
	/// <param name="move_input_vector"> Player movement vector of current frame </param>
	/// <param name="face_input_vector"> Player facing vector of the current frame. </param> 
	/// <param name="movement_vector"> Player movement vector of current frame </param>
	/// <param name="player_facing"> Direction that the player is currently facing </param>
	/// <param name="action_pressed""> If the action button is currently pressed </param>
	/// <returns>
	/// Vector2 representing new player velocity while acting.
	/// Vector2 representing new player facing direction while acting.
	/// Boolean representing if the player should be returned to free movement.
	/// </returns>
	public override (Vector2, Vector2, bool, Vector2, Vector2) Handle_Action(float delta, Vector2 move_input_vector,
		Vector2 face_input_vector, Vector2 movement_vector, Vector2 player_facing, bool action_pressed)
	{

		/* Track if shield is being used */
		this.shield_raised = action_pressed;


		/* Place in front of or behind player depending on rotation  TODO: perhaps easier way to calculate this? */
		if (Mathf.Abs((this.Rotation % (Mathf.Pi * 2) + Mathf.Pi * 2) % (Mathf.Pi * 2) - Mathf.Pi / 2) < Mathf.Pi / 2)
		{
			ZIndex = 1;
		}
		else
		{
			ZIndex = -1;
		}

		/* If key remains pressed, do not move, but allow reangling */
		if (action_pressed)
		{
			if (face_input_vector != Vector2.Zero)
			{
				/* Incomplete animation stuff */
				this.Rotation = face_input_vector.Angle();
				this.Position = RAISE_OFFSET * Vector2.FromAngle(this.Rotation);
				directed = true;
				return (Vector2.Zero, face_input_vector, false, Vector2.Zero, Vector2.Zero);
			}
			else
			{
				/* If not yet directed, take player direction */
				if (!directed)
				{
					/* Incomplete animation stuff */
					this.Rotation = player_facing.Angle();
					this.Position = RAISE_OFFSET * Vector2.FromAngle(this.Rotation);
				}
				return (Vector2.Zero, face_input_vector, false, Vector2.Zero, Vector2.Zero);
			}
		}
		/* Otherwise, stop action upon key being released */
		else
		{
			return (Vector2.Zero, face_input_vector, true, Vector2.Zero, Vector2.Zero);
		}
	}

	/// <summary>
	/// Returns if the given item is currently being used. Only necessary
	/// for certain types of items
	/// </summary>
	/// <returns> If the current item is being used. </returns>
	public override bool Get_Active()
	{
		return this.shield_raised;
	}

	/// <summary>
	/// Returns vector2 points that define the current item. Only necessary
	/// for certain types of items.
	/// </summary>
	/// <returns>An array of Vector2 representing
	/// 0: top forward
	/// 1: bottom forward
	/// 2: top backward
	/// 3: bottom backward </returns>
	public override Vector2[] Get_Points()
	{
		/* Calculate front ends of shield */
		Vector2 top_forward = RAISE_OFFSET / 2 * Vector2.FromAngle(this.Rotation) +
			SHIELD_HEIGHT / 2 * Vector2.FromAngle(this.Rotation + Mathf.Pi / 2) + this.GlobalPosition;
		Vector2 bottom_forward = RAISE_OFFSET / 2 * Vector2.FromAngle(this.Rotation) +
			SHIELD_HEIGHT / 2 * Vector2.FromAngle(this.Rotation - Mathf.Pi / 2) + this.GlobalPosition;
		Vector2 top_backward = top_forward - SHIELD_WIDTH * Vector2.FromAngle(this.Rotation);
		Vector2 bottom_backward = bottom_forward - SHIELD_WIDTH * Vector2.FromAngle(this.Rotation);
		return new Vector2[] { top_forward, bottom_forward, top_backward, bottom_backward };
	}

	/// <summary>
	/// Plays a specific sound corresponding to this shield
	/// </summary>
	/// <param name="sound"></param>
	public override void Play_Effect(string sound = "")
	{
		sound_player.Play_Effect("Deflect", -15);
	}
}
