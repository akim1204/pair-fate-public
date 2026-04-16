using Godot;
using System;

public partial class PlayerItem : Sprite2D
{

	/// <summary> Whether there is one button for both picking up and
	/// interacting or two.  </summary>
	private int pickup_type = 2;

	/// <summary> The distance from which items can be picked up. </summary>
	private const float PICKUP_DISTANCE = 120f;

	/// <summary> Node representing world space. </summary>
	private Node WORLD;

	/// <summary> Node2D corresponding to player. </summary>
	private Player PLAYER;


	/// <summary> Position of the 'hand' relative to base of player </summary>
	private Vector2 hand_position = new Vector2(0, -20);

	/// <summary> Item currently being carried </summary>
	public Item current_item = null;

	/// <summary> If the current closest items should be highlighted. </summary>
	private bool highlighting = true;

	/// <summary> Closest available highlightable within PICKUP_DISTANCE. </summary>
	private Highlightable closest_overall = null;

	/// <summary> Closest available item within PICKUP_DISTANCE. </summary>
	private Item closest_item = null;

	/// <summary> Closest available interactable within PICKUP_DISTANCE. </summary>
	private Interactable closest_interactable = null;

	/// <summary> Actual type ("item" or "interactable") of the cloest thing</summary>
	private string closest_type = "none";

	/// <summary> Timer to check for a new closest item (so its not every frame) </summary>
	private float closest_check = 0f;

	/// <summary> Autoloaded item bag containing all items </summary>
	protected ItemBag item_bag;

	/// <summary> Autoloaded player bag containing all items </summary>
	protected PlayerBag player_bag;

	public override void _Ready()
	{
		base._Ready();

		/* Getting item bag */
		item_bag = GetNode<ItemBag>("/root/ItemBag");

		/* Getting player bag */
		player_bag = GetNode<PlayerBag>("/root/PlayerBag");

		/* Getting world context */
		WORLD = GameManager.Instance.Get_World();

		/* Getting owner player */
		PLAYER = GetParent<Player>();

		/* Make player item pausable */
		ProcessMode = Node.ProcessModeEnum.Pausable;

		/* Updating hand position */
		this.Position = hand_position;
	}

	public override void _Process(double delta) //TODO: MAKE HIGHLIGHTING PLAYER SPECIFIC WHEN NETWORKING
	{
		/* Highlighting closest item */
		if (highlighting)
		{
			closest_check -= (float)delta;
			if (closest_check <= 0)
			{
				/* Reset check timer */
				closest_check = 0.05f;

				/* Find and highlight closest objects depending on pickup type */
				if (pickup_type == 1)
				{
					highlight_closest_one();
				}
				else
				{
					highlight_closest_two();
				}
			}
		}
	}

	/// <summary>
	/// Returns the player this is attatched to.
	/// </summary>
	/// <returns></returns>
	public Player Get_Player()
	{
		return PLAYER;
	}

	/// <summary>
	/// Disables all highlights.
	/// </summary>
	public void Disable_Highlights()
	{
		/* Dehighlight current items */
		if (closest_interactable != null)
		{
			closest_interactable.Toggle_Outline(false);
			closest_interactable = null;
		}

		if (closest_item != null)
		{
			closest_item.Toggle_Outline(false);
			closest_item = null;
		}

		/* Disable highlighting */
		highlighting = false;
	}

	/// <summary>
	/// Reenables all highlighitng of objects.
	/// </summary>
	public void Enable_Highlights()
	{
		highlighting = true;
	}


	/// <summary>
	/// Returns item currently held by player, null if player is not carrying
	/// an item.
	/// </summary>
	/// <returns> Current player item, null if no item</returns>
	public Item Get_Item()
	{
		return this.current_item;
	}
	/// <summary>
	/// Returns the type ("item" or "interactable") of the closest object
	/// </summary>
	/// <returns> String </returns>
	public string Closest_Type()
	{
		return this.closest_type;
	}

	/// <summary>
	/// Handles interaction (INCOMPLETE)
	/// </summary>
	public void Handle_Interact()
	{
		if (closest_interactable != null)
		{
			closest_interactable.Handle_Action(this);
			/* Call on all other screens */
			Rpc("Handle_Interact_Specific", closest_interactable.Get_Id());
		}
	}

	/// <summary>
	/// Handles an itneraction with a specific interactable.
	/// </summary>
	/// <param name="id">The id of the interactable to interact with. </param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
	public void Handle_Interact_Specific(int id)
	{
		item_bag.GetInteractable(id).Handle_Action(this);
	}


	/// <summary>
	/// Drops the current item.
	/// </summary>
	/// <returns> Returns true if an item was dropped, false otherwise. </returns>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public bool Drop_Item()
	{
		if (current_item != null)
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.INFO, "Dropping item " + current_item.Get_Name());
			/* Remove item from selected items */
			item_bag.DeselectItem(current_item.Get_Id());

			/* Play sound */
			PLAYER.Play_Drop();

			/* Drop item */
			current_item.CallDeferred("reparent", WORLD);
			current_item.Handle_Drop();
			current_item.carried = false;
			current_item = null;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Attempts to pick up the closest item if there is one nearby.
	/// </summary>
	/// <param name="move_input_vector"> Player input vector of current frame </param>
	/// <param name="face_input_vector"> Player facing vector of current frame </param>
	/// <param name="movement_vector"> Player movement vector of current frame </param>
	/// <param name="player_facing"> Direction that the player is currently facing </param>
	/// <returns> Returns true if an item was picked up, false otherwise </returns>
	/// 
	public bool Handle_Pickup(Vector2 move_input_vector, Vector2 face_input_vector,
		Vector2 movement_vector, Vector2 player_facing)
	{

		/* Remove current item if one is carried */
		Drop_Item();

		/* Only pickup if there is a closest item */
		if (closest_item != null)
		{
			/* Move closest item to self */
			Logger.Instance.Log(Logger.LOG_LEVELS.INFO, "Picking up item " + closest_item.Get_Name());

			/* Add item to selected items */
			item_bag.SelectItem(closest_item.Get_Id());

			/* Pickup item */
			closest_item.Reparent(this);
			closest_item.carried = true;    // (TODO: move this into a single function)
			closest_item.Toggle_Outline(false); /* Disable item outline */
			closest_item.Handle_Pickup(this.PLAYER, move_input_vector, face_input_vector, movement_vector, player_facing);
			current_item = closest_item;
			closest_item = null;

			/* Indicate item was picked up */
			return true;
		}

		/* No item was picked up */
		return false;
	}

	/// <summary>
	/// Initiates that a given player is attempting to pick up an item.
	/// </summary>
	/// <param name="player_id"> The id of the player. </param>
	public void Initiate_Pickup_Request(int player_id)
	{
		/* Initiate drop if item is carried */
		if (current_item != null)
		{
			/* No race condition on dropping */
			Rpc("Drop_Item");
		}
		/* If there is a closest item, attempt to pick it up */
		if (closest_item != null)
		{
			/* Check with the host to avoid races */
			RpcId(1, "Check_Host_Bag", closest_item.Get_Id(), player_id);
		}
	}

	/// <summary>
	/// Checks with the host item_bag to make sure an item is available.
	/// </summary>
	/// <param name="item"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Check_Host_Bag(int item_id, int player_id)
	{
		/* Check if the item bag still has the item available */
		if (item_bag.ItemAvailable(item_id))
		{
			/* Allow player to do pickup */
			Rpc("Handle_Pickup_Specific", item_id, player_id);

			/* Add item to item bag carried  TODO: maybe redundant?*/
			item_bag.SelectItem(item_id);
		}
		else
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Desync between player items.");
		}
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Handle_Pickup_Specific(int item_id, int player_id)
	{
		/* If this is the player item initiating pickup */
		if (PLAYER.Get_Id() == player_id)
		{
			item_bag.SelectItem(item_id);
			/* Get the item being picked up */
			Item target_item = item_bag.Get_Item(item_id);

			Logger.Instance.Log(Logger.LOG_LEVELS.DEBUG, "Picking up item:" + target_item.Get_Name());

			/* Pickup item */
			target_item.Reparent(this);
			target_item.carried = true; // (TODO: move this into a single function)
			target_item.Toggle_Outline(false); /* Disable item outline */
			if (closest_item != null) closest_item.Toggle_Outline(false); // TODO? Maybe not necessary, but safe > sorry
			(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4) = PLAYER.Get_Orientation();
			target_item.Handle_Pickup(this.PLAYER, v1, v2, v3, v4);
			current_item = target_item;
			closest_item = null;

			/* Play pickup sound effect */
			PLAYER.Play_Pickup();
		}
	}


	/// <summary>
	/// Handles the default animation of the item when the player is moving without acting.
	/// </summary>
	/// <param name="move_input_vector"> Player input vector of current frame </param>
	/// <param name="face_input_vector"> Player facing vector of current frame </param>
	/// <param name="player_facing"> Direction that the player is currently facing </param>
	/// <param name="movement_vector"> Player movement vector of current frame </param>
	/// <param name="delta"> Time elapsed since the previous frame, cast to a float </param>
	/// <returns> Vector2 representing new movement velocity, placement of left hand, 
	/// placement of right hand </returns>
	public (Vector2, Vector2, Vector2) Handle_Animation_Default(float delta, Vector2 move_input_vector,
		Vector2 face_input_vector, Vector2 movement_vector, Vector2 player_facing)
	{
		/* If an item is currently held, delegate to the item */
		if (current_item != null)
		{
			Vector2 movement_new, left_new, right_new;
			(movement_new, left_new, right_new) = current_item.Handle_Animation_Default(
				delta, move_input_vector, face_input_vector, movement_vector, player_facing);
			return (movement_new, left_new + hand_position, right_new + hand_position);
		}
		else
		{
			/* Do nothing */
			return (movement_vector, hand_position, hand_position);
		}
	}


	/// <summary>
	/// Called when an action is initialized to reset action states and the like.
	/// </summary>
	/// <param name="move_input_vector"> Player movement vector of current frame </param>
	/// <param name="face_input_vector"> Player facing vector of the current frame. </param> 
	/// <param name="movement_vector"> Player movement vector of current frame </param>
	/// <param name="player_facing"> Direction that the player is currently facing </param>
	/// <returns> Returns true if an action is started, false otherwise </returns>
	public void Handle_Action_Begin(Vector2 move_input_vector, Vector2 face_input_vector, Vector2 movement_vector, Vector2 player_facing)
	{
		/* Begin action if item is held */
		if (current_item != null)
		{
			/* If action begins */
			if (current_item.Handle_Action_Begin(move_input_vector, face_input_vector, movement_vector, player_facing))
			{
				PLAYER.Enter_Act(true);
				// Start actions on all other screens
				Rpc("Start_Actions", move_input_vector, face_input_vector, movement_vector, player_facing);
			}
		}
		/* Otherwise, do nothing if there's no item */
	}

	/// <summary>
	/// Starts the action on all other players' screens
	/// </summary>
	/// <param name="move_input_vector"></param>
	/// <param name="face_input_vector"></param>
	/// <param name="movement_vector"></param>
	/// <param name="player_facing"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
	public void Start_Actions(Vector2 move_input_vector, Vector2 face_input_vector, Vector2 movement_vector, Vector2 player_facing)
	{
		current_item.Handle_Action_Begin(move_input_vector, face_input_vector, movement_vector, player_facing);
		PLAYER.Enter_Act(true);
		PLAYER.currently_acting = true; // TODO: FIX THIS:
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
	/// Vector2 representing new left hand position.
	/// ./// Vector2 representing new right hand position
	/// </returns>
	public (Vector2, Vector2, bool, Vector2, Vector2) Handle_Action(float delta, Vector2 move_input_vector, Vector2 face_input_vector, Vector2 movement_vector, Vector2 player_facing, bool action_pressed)
	{
		/* If an item is currently held, delegate to the item */
		if (current_item != null)
		{
			Vector2 velocity_new, facing_new, left_hand_potential, right_hand_potential;
			bool end_action;
			(velocity_new, facing_new, end_action, left_hand_potential, right_hand_potential) =
				current_item.Handle_Action(delta, move_input_vector, face_input_vector, movement_vector, player_facing, action_pressed);
			return (velocity_new, facing_new, end_action, left_hand_potential + hand_position, right_hand_potential + hand_position);
		}
		/* Otherwise, immediately end action */
		else
		{
			return (movement_vector, player_facing, true, hand_position, hand_position);
		}
	}

	/// <summary>
	/// Returns if the player is currently holding an item.
	/// </summary>
	/// <returns> Boolean representing if there is an item </returns>
	public bool Has_Item()
	{
		return this.current_item != null;
	}

	/// <summary>
	/// Handles throwing the current item
	/// </summary>
	/// <param name="move_input_vector"> Player movement vector of current frame </param>
	/// <param name="face_input_vector"> Player facing vector of the current frame. </param> 
	/// <param name="movement_vector"> Player movement vector of current frame </param>
	/// <param name="player_facing"> Direction that the player is currently facing </param>
	/// <param name="destination"> Player input vector of current frame </param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
	public void Handle_Throw(Vector2 move_input_vector, Vector2 face_input_vector, Vector2 movement_vector, Vector2 player_facing, Vector2 destination)
	{
		/* If holding an item, throw it */
		if (this.current_item != null)
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.INFO, "Throwing item " + current_item.Get_Name());
			/* Remove item from selected items */
			item_bag.DeselectItem(current_item.Get_Id());
			/* Drop item */
			current_item.Reparent(WORLD);
			current_item.Handle_Throw(destination);
			current_item.carried = false;
			current_item = null;

			/* Additionally call on all screens */
			if (PLAYER.Get_Authority())
			{
				Rpc("Handle_Throw", move_input_vector, face_input_vector, movement_vector, player_facing, destination);
			}
		}
	}

	/// <summary>
	/// This function only highlights the single closest item/interactable overall.
	/// </summary>
	private void highlight_closest_one()
	{
		/* Find the closest item */
		Item closest_cand_item = null;
		float min_item_dist = PICKUP_DISTANCE * PICKUP_DISTANCE;
		float cand_dist;
		foreach (Item item in item_bag.GetAvailableItems())
		{
			/* Only track items that are not carried */
			if (!item.carried)
			{
				/* Calculate distance */
				cand_dist = PLAYER.GlobalPosition.DistanceSquaredTo(item.GlobalPosition);
				/* If within pickup distance and is the closest */
				if (cand_dist < min_item_dist)
				{
					min_item_dist = cand_dist;
					closest_cand_item = item;
				}
			}
		}

		/* Find the closest interactable */
		Interactable closest_cand_inter = null;
		float min_inter_dist = PICKUP_DISTANCE * PICKUP_DISTANCE;
		foreach (Interactable interactable in item_bag.GetAvailableInteractables())
		{
			cand_dist = PLAYER.GlobalPosition.DistanceSquaredTo(interactable.GlobalPosition);
			/* If within pickup distance and is the closest */
			if (cand_dist < min_inter_dist)
			{
				min_inter_dist = cand_dist;
				closest_cand_inter = interactable;
			}
		}
		/* Replace closest tracked item and interactable */
		if (closest_cand_item != closest_item)
		{
			closest_item = closest_cand_item;
		}
		if (closest_cand_inter != closest_interactable)
		{
			closest_interactable = closest_cand_inter;
		}

		/* Find actual closest thing */
		Highlightable closest_cand = null;
		if (closest_cand_item != null && min_item_dist < min_inter_dist)
		{
			closest_cand = closest_cand_item;
			closest_type = "item";
		}
		if (closest_cand_inter != null && min_inter_dist < min_item_dist)
		{
			closest_cand = closest_cand_inter;
			closest_type = "interactable";
		}
		if (closest_cand_item == null && closest_cand_inter == null)
		{
			closest_type = "none";
		}
		/* If there is a different closest item than the current closest */
		if (closest_cand != closest_overall && PLAYER.Get_Authority())
		{
			/* If the old closest exists, disable its outline */
			if (closest_overall != null)
			{
				closest_overall.Toggle_Outline(false);
			}
			closest_overall = closest_cand;
			/* if the new closest exists, enbale its outline */
			if (closest_overall != null)
			{
				closest_overall.Toggle_Outline(true);
			}
		}
	}

	/// <summary>
	/// Highlights both the closest item as well as the closest interactable.
	/// </summary>
	private void highlight_closest_two()
	{
		/* Find the closest item */
		Item closest_cand_item = null;
		float min_item_dist = PICKUP_DISTANCE * PICKUP_DISTANCE;
		float cand_dist;
		foreach (Item item in item_bag.GetAvailableItems())
		{
			/* Only track items that are not carried */
			if (!item.carried)
			{
				/* Calculate distance */
				cand_dist = PLAYER.GlobalPosition.DistanceSquaredTo(item.GlobalPosition);
				/* If within pickup distance and is the closest */
				if (cand_dist < min_item_dist)
				{
					min_item_dist = cand_dist;
					closest_cand_item = item;
				}
			}
		}

		/* Find the closest interactable */
		Interactable closest_cand_inter = null;
		float min_inter_dist = PICKUP_DISTANCE;
		foreach (Interactable interactable in item_bag.GetAvailableInteractables())
		{
			cand_dist = PLAYER.GlobalPosition.DistanceTo(interactable.GlobalPosition) - interactable.Get_Radius();
			/* If within pickup distance and is the closest */
			if (cand_dist < min_inter_dist)
			{
				min_inter_dist = cand_dist;
				closest_cand_inter = interactable;
			}
		}

		/* Replace and highlight closest thing */
		if (closest_cand_item != closest_item)
		{
			/* Only update highlights if authority */
			if (PLAYER.Get_Authority())
			{
				if (closest_cand_item != null)
				{
					closest_cand_item.Toggle_Outline(true);
				}
				if (closest_item != null)
				{
					closest_item.Toggle_Outline(false);
				}
			}
			closest_item = closest_cand_item;
		}
		if (closest_cand_inter != closest_interactable)
		{
			/* Only update highlights if authority */
			if (PLAYER.Get_Authority())
			{
				if (closest_cand_inter != null)
				{
					closest_cand_inter.Toggle_Outline(true);
				}
				if (closest_interactable != null)
				{
					closest_interactable.Toggle_Outline(false);
				}
			}
			closest_interactable = closest_cand_inter;
		}
	}
}
