using Godot;
using System;
using System.Net;

/// <summary>
/// Chocolate shield item.
/// </summary>
public partial class ItemWaterBucket : Item
{


	/// <summary>
	/// Prefab for the water bucket splash.
	/// </summary>
	private PackedScene splash_prefab;
	/// <summary>
	/// Maximum fill of the bucket.
	/// </summary>
	private const int MAX_FILL = 3;

	/// <summary>
	/// How many uses the water bucket has left.
	/// </summary>
	private int fill_state = MAX_FILL;

	/// <summary>
	/// How long the player is held in place while splashing.
	/// </summary>
	private const float SPLASH_LOCKOUT = 0.3f;

	/// <summary>
	/// Lockout timer for splashing.
	/// </summary>
	private float lockout_timer = 0;

	public override void _Ready()
	{
		/* Overriding item type and name */
		this.item_type = ItemTypes.Utility;
		this.item_name = "Milk Bucket";
		this.item_tooltip = "Useful for putting out fires and making chocolate. Click to use. Has limited uses and must be refilled.";

		/* Getting splash prefab */
		splash_prefab = GD.Load<PackedScene>("res://Player/PlayerItems/WaterBucket/WaterBucketSplash.tscn");

		/* Calling base constructor */
		base._Ready();
	}


	public override void _Process(double delta)
	{
		// TEMPORARY CODE FOR DISPLAYING AMOUNT and refilling
		QueueRedraw();

		base._Process(delta);
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

		/* Reset angle */
		this.Rotation = 0;

		/* Held above head */
		this.Position = Vector2.Up * 70;

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
		/* Held above head */
		this.Position = Vector2.Up * 70;

		/* TODO: Rotate with movement */

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
		/* Only initiate splash if bucket has water */
		if (this.fill_state >= 1)
		{

			/* Play sound */
			sound_player.Play_Effect("Empty", -5);
			/* Setup lockout timer */
			lockout_timer = SPLASH_LOCKOUT;

			/* Lose water */
			fill_state -= 1;
			Frame = 3 - fill_state;

			/* Set rotation */
			this.Rotation = player_facing.Angle() + Mathf.Pi / 2;
			this.Position = 20 * Vector2.Up + 30 * Vector2.FromAngle(player_facing.Angle());

			/* Create splash */
			var inst = splash_prefab.Instantiate<WaterBucketSplash>();
			WORLD.CallDeferred("add_child", inst);
			inst.GlobalPosition = GlobalPosition + 32 * Vector2.FromAngle(player_facing.Angle());
			inst.Rotation = player_facing.Angle();

			/* Start action */
			return true;
		}
		/* Otherwise, don't start action  and send message*/
		if (owner_player.Get_Authority())
		{
			GameManager.Instance.Display_Message("Refill bucket to use", 2);
		}
		return false;
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
		/* Lockout movement */
		if (lockout_timer >= 0)
		{
			lockout_timer -= delta;
			return (Vector2.Zero, player_facing, false, Vector2.Zero, Vector2.Zero);
		}
		/* Revert rotation */
		this.Rotation = 0;
		return (Vector2.Zero, player_facing, true, Vector2.Zero, Vector2.Zero);
	}

	/// <summary>
	/// Fills the water bucket.
	/// </summary>
	public void Fill()
	{
		if (fill_state < MAX_FILL)
		{
			/* Play sound */
			sound_player.Play_Effect("Fill", -5);
		}
		this.fill_state = MAX_FILL;
		Frame = 3 - fill_state;
	}
}
