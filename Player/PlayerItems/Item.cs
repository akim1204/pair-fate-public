using Godot;
using System;
using System.Net;

/// <summary>
/// Parent class of all usable player items.
/// </summary>
public partial class Item : Highlightable
{

	/// <summary> Whether the current item is carried by a player. </summary>
	public bool carried = false;

	/// <summary> Player that is currently carrying this item. </summary>
	protected Player owner_player = null;

	/// <summary> Sound player for this object, can be null. </summary>
	protected SoundPlayer sound_player;

	/// <summary> Enum representing general item types. </summary>
	public enum ItemTypes
	{
		Weapon,
		Shield,
		Utility,
	}

	/// <summary> Overarching usage category of the item.</summary>
	protected ItemTypes item_type = ItemTypes.Utility;

	/// <summary> Id of the item used to trackw ithin the item bag. </summary>
	protected int item_id;

	/// <summary> Specific name of the item </summary>
	protected string item_name = "Placeholder";

	/// <summary> The tooltip string for the item </summary>
	protected string item_tooltip = "Placeholder";

	/// <summary> Variable pointed to the item bag containing all items </summary>
	protected ItemBag item_bag;

	/// <summary> Whether the item is currently in the throw state. </summary>
	protected bool in_throw;

	/// <summary> Destination of throw when thrown. </summary>
	protected Vector2 throw_destination;

	/// <summary> Ground position of the item not counting the throw arc. </summary>
	protected Vector2 throw_ground_position;

	/// <summary> Progress through the throw in seconds. </summary>
	protected float throw_progress;

	/// <summary> How long the throw will take in seconds. </summary>
	protected float throw_duration;

	/// <summary>
	/// Gravity of the throw arc.
	/// </summary>
	protected const float THROW_GRAVITY = 500f;

	/// <summary>
	/// Speed the item travels in the air while thrown.
	/// </summary>
	protected const float THROW_SPEED = 250f;

	/// <summary>
	/// Node representing world space.
	/// </summary>
	protected Node WORLD;


	public override void _Ready()
	{
		/* Initially disable outline */
		base._Ready();

		/* Getting item id */
		this.item_id = int.Parse(this.Name.ToString().Substring(4));

		/* Getting item bag and updating (DEPENDS ON FINAL ROOM IMPLEMENTATION) */
		item_bag = GetNode<ItemBag>("/root/ItemBag");
		item_bag.AddAvailable(item_id, this);

		/* Get sound node */
		sound_player = GetNodeOrNull<SoundPlayer>("SoundPlayer");

		/* Place item behind players */
		this.ZIndex = -1;

		/* Get world space */
		WORLD = GameManager.Instance.Get_World();
	}

	public override void _Process(double delta)
	{
		/* If in throw state, move towards throw destination */
		if (in_throw)
		{
			throw_progress += (float)delta;
			/* Snap to position if reached */
			if ((throw_destination - this.throw_ground_position).Length() < THROW_SPEED * delta)
			{
				this.GlobalPosition = throw_destination;
				this.throw_ground_position = throw_destination;
				in_throw = false;
			}
			/* Otherwise, move towards destinatino */
			else
			{
				this.throw_ground_position += (throw_destination - throw_ground_position).Normalized() * THROW_SPEED * (float)delta;
			}

			/* Calculate corresponding visual position */
			this.GlobalPosition = this.throw_ground_position + THROW_GRAVITY * Vector2.Up *
				(Mathf.Pow(throw_duration, 2) / 8 - Mathf.Pow(Mathf.Abs(throw_duration / 2 - throw_progress), 2) / 2);
		}

		// TODO: SHOULD THIS BE HERE?
		if (carried == false)
		{
			this.ZIndex = -1;
		}
	}

	/// <summary>
	/// Initialize position of item when it is first picked up
	/// </summary>
	/// <param name="owner_player"> Player that is picking up this item </param>
	/// <param name="move_input_vector"> Player movement vector of current frame </param>
	/// <param name="face_input_vector"> Player facing vector of the current frame. </param> 
	/// <param name="movement_vector"> Player movement vector of current frame </param>
	/// <param name="player_facing"> Direction that the player is currently facing </param>
	public virtual void Handle_Pickup(Player owner_player, Vector2 move_input_vector,
		Vector2 face_input_vector, Vector2 movement_vector, Vector2 player_facing)
	{
		this.owner_player = owner_player;
		/* Exit throw state */
		in_throw = false;
		Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Pickup Handler was not overridden");
	}

	/// <summary>
	/// Called when the item is dropped.
	/// </summary>
	public virtual void Handle_Drop()
	{
		/* Place item behind players */
		this.ZIndex = -1;
	}

	/// <summary>
	/// Handles the animation of the item when not acting.
	/// </summary>
	/// <param name="delta"> Time elapsed since the previous frame, cast to a float </param>
	/// <param name="move_input_vector"> Player movement vector of current frame </param>
	/// <param name="face_input_vector"> Player facing vector of the current frame. </param> 
	/// <param name="movement_vector"> Player movement vector of current frame </param>
	/// <param name="player_facing"> Direction that the player is currently facing </param>
	/// <returns> Vector2 representing new player velocity. </returns>
	/// <returns> Vector2 representing the player's left hand. </returns>
	/// <returns> Vector2 representing the player's right hand. </returns>
	public virtual (Vector2, Vector2, Vector2) Handle_Animation_Default(float delta, Vector2 move_input_vector,
		Vector2 face_input_vector, Vector2 movement_vector, Vector2 player_facing)
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Animation Handler was not overridden");
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
	public virtual bool Handle_Action_Begin(Vector2 move_input_vector,
		Vector2 face_input_vector, Vector2 movement_vector, Vector2 player_facing)
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Action Begin Handler was not overridden");
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
	/// Vector2 representing left hand position.
	/// Vector2 representing right hand position.
	/// Boolean representing if the player should be returned to free movement.
	/// </returns>
	public virtual (Vector2, Vector2, bool, Vector2, Vector2) Handle_Action(float delta, Vector2 move_input_vector,
		Vector2 face_input_vector, Vector2 movement_vector, Vector2 player_facing, bool action_pressed)
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Action Handler was not overridden");
		return (movement_vector, player_facing, true, Vector2.Zero, Vector2.Zero);
	}

	/// <summary>
	/// Returns the specific item name.
	/// </summary>
	/// <returns>Item's name</returns>
	public string Get_Name()
	{
		return this.item_name;
	}

	/// <summary>
	/// Returns the item id of this item.
	/// </summary>
	/// <returns></returns>
	public int Get_Id()
	{
		return this.item_id;
	}

	/// <summary>
	/// Returns the general usage category.
	/// </summary>
	/// <returns>Item's type</returns>
	public ItemTypes Get_Type()
	{
		return this.item_type;
	}

	/// <summary>
	/// Returns if the given item is currently being used. Only necessary
	/// for certain types of items
	/// </summary>
	/// <returns> If the current item is being used. </returns>
	public virtual bool Get_Active()
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Get_Active was not defined in " + this.Get_Name());
		return false;
	}

	/// <summary>
	/// Returns if a given item is currently in the air during a throw.
	/// </summary>
	/// <returns> If the item is in the air, true, false otherwise. </returns>
	public virtual bool Get_Thrown()
	{
		return in_throw;
	}

	/// <summary>
	/// Returns vector2 points that define the current item. Only necessary
	/// for certain types of items.
	/// </summary>
	/// <returns>An array of Vector2 representing the points.</returns>
	public virtual Vector2[] Get_Points()
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Get_Points was not defined in " + this.Get_Name());
		return new Vector2[] { };
	}

	/// <summary>
	/// Plays a specific effect of the item
	/// </summary>
	public virtual void Play_Effect(string sound = "")
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Play_Effect was not defined in " + this.Get_Name());
	}

	/// <summary>
	/// Initiates a throw for the item
	/// </summary>
	/// <param name="destination"> Where the item should land </param>
	public virtual void Handle_Throw(Vector2 destination)
	{
		/* Enter throw state */
		in_throw = true;
		throw_destination = destination;
		throw_ground_position = this.GlobalPosition;

		/* Calculate how long throw is expected to take */
		throw_duration = (throw_destination - throw_ground_position).Length() / THROW_SPEED;
		throw_progress = 0;
	}

	/// <summary>
	/// Destroys the given item and removes it from item bags
	/// </summary>
	public virtual void Destroy()
	{
		/* Remove from item bags */
		item_bag.CallDeferred("RemoveAvailable", this);
		// TODO: Remove if selected?

		/* Delete object */
		QueueFree();
	}

	/// <summary>
	/// Returns the owner of this item (current player holding it)
	/// </summary>
	/// <returns></returns>
	public virtual Player Get_Owner()
	{
		return owner_player;
	}

	/// <summary>
	/// Returns the tooltip string of this item
	/// </summary>
	/// <returns></returns>
	public virtual string Get_Tooltip()
	{
		return this.item_tooltip;
	}

	/// <summary>
	/// Returns the texture of this item
	/// </summary>
	/// <returns></returns>
	public virtual Texture2D Get_Texture()
	{
		return this.Texture;
	}
}
