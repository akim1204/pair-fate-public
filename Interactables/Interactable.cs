using Godot;
using System;
using System.Net;

/// <summary>
/// Parent class of all usable player items.
/// </summary>
public partial class Interactable : Highlightable
{


	/// <summary>
	/// Enum representing general item types.
	/// </summary>
	public enum InteractableTypes
	{
		Door,
		Utility,
		Dialogue,
	}

	/// <summary>
	/// Specific name of the item
	/// </summary>
	protected string interactable_name = "Placeholder";

	/// <summary> Id used to track itneractables. </summary>
	protected int interactable_id;

	/// <summary>
	/// Index of type for doors and dialogue to interface with dialogue and door
	/// managers.
	/// </summary>
	protected int type_index;

	/// <summary>
	/// Variable pointed to the item bag containing all items and interactibles
	/// </summary>
	protected ItemBag item_bag;

	/// <summary>
	/// Extra radius of interaction for larger objects;
	/// </summary>
	protected float interact_radius = 0;


	public override void _Ready()
	{
		/* Initially disable outline */
		base._Ready();

		/* Getting interactable id*/
		this.interactable_id = int.Parse(this.Name.ToString().Substring(12));

		/* Getting item bag and updating (DEPENDS ON FINAL ROOM IMPLEMENTATION) */
		item_bag = GetNode<ItemBag>("/root/ItemBag");
		item_bag.AddInteractable(interactable_id, this);

		/* Make interactable pausable */
		ProcessMode = Node.ProcessModeEnum.Pausable;
	}

	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Initialize position of item when it is first picked up
	/// </summary>
	/// <param name="player"> Player who interacts with interactable </param>
	public virtual void Handle_Action(PlayerItem player)
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Pickup Handler was not overridden");
	}

	/// <summary>
	/// Returns the id of this interactable.
	/// </summary>
	/// <returns>Integer representing its id</returns>
	public int Get_Id()
	{
		return this.interactable_id;
	}

	/// <summary>
	/// Returns the specific item name.
	/// </summary>
	/// <returns>Item's name</returns>
	public string Get_Name()
	{
		return this.interactable_name;
	}

	/// <summary>
	/// Returns the general usage category.
	/// </summary>
	/// <returns>Item's type</returns>
	public virtual InteractableTypes Get_Type()
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.WARN, "Interactable did not override Get_Type");
		return InteractableTypes.Utility;
	}

	/// <summary>
	/// Returns if the given item is currently being used. Only necessary
	/// for certain types of items
	/// </summary>
	/// <returns> If the current item is being used. </returns>
	public virtual bool Get_Active()
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Get_Active was not defined");
		return false;
	}

	/// <summary>
	/// Sets the type index of a given interactable.
	/// </summary>
	/// <param name="index"></param>
	public virtual void Set_Type_Index(int index)
	{
		this.type_index = index;
	}

	/// <summary>
	/// Removes an interactable from the list of available interactables.
	/// </summary>
	protected void Remove_Interactable()
	{
		/* Disable highlighting */
		Toggle_Outline(false);

		/* Remove from highlightables */
		item_bag.RemoveInteractable(interactable_id);
	}

	/// <summary>
	/// Destroys the given item and removes it from item bags
	/// </summary>
	public virtual void Destroy()
	{
		/* Remove from item bags */
		item_bag.CallDeferred("RemoveInteractable", this.interactable_id);
		// TODO: Remove if selected?

		/* Delete object */
		QueueFree();
	}

	/// <summary>
	/// Returns the extra radius of this object.
	/// </summary>
	/// <returns></returns>
	public float Get_Radius()
	{
		return interact_radius;
	}
}
