using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class ItemBag : Node
{

	/// <summary>
	/// Dictionary of which items are currently carried. Synced across players.
	/// </summary>
	public Dictionary<int, bool> CarriedItems = new Dictionary<int, bool>();

	/// <summary>
	/// Dictionary mapping ids to items, is not synced across players.
	/// </summary>
	public Dictionary<int, Item> AvailableItems = new Dictionary<int, Item>();

	/// <summary> List of all available interactables in the current screen. </summary>
	public Dictionary<int, Interactable> AvailableInteractables = new Dictionary<int, Interactable>();

	/// <summary>
	/// Returns all items that are currently carried. 
	/// </summary>
	/// <returns> Hashset of carried items. </returns>
	public HashSet<Item> GetCarriedItems()
	{
		HashSet<Item> carried_items = new HashSet<Item>();
		foreach (int item_id in AvailableItems.Keys)
		{
			if (CarriedItems[item_id])
			{
				carried_items.Add(AvailableItems[item_id]);
			}
		}
		return carried_items;
	}

	/// <summary>
	/// Returns all available items, both carried and on the ground.
	/// </summary>
	/// <returns> List of all available items. </returns>
	public List<Item> GetAvailableItems()
	{
		return this.AvailableItems.Values.ToList();
	}

	/// <summary>
	/// Adds a given item to the available items, should be called when the item is first created.
	/// </summary>
	/// <param name="id"> The id of the item to add. </param>
	/// <param name="item">The specific item instance to add.</param>
	public void AddAvailable(int id, Item item)
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Added " + item.Get_Name() + " to item bag.");
		this.AvailableItems.Add(id, item);
		this.CarriedItems.Add(id, false);
	}

	/// <summary>
	/// Removes a given item from the available items, should be called when the item is removed.
	/// </summary>
	/// <param name="id"> The id of the item to add. </param>
	public void RemoveAvailable(int id)
	{
		/* Check that the item exists */
		if (!AvailableItems.Keys.Contains(id))
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Recieved an invalid item id");
		}
		else
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Removed " + AvailableItems[id].Get_Name() + " from item bag.");
			this.AvailableItems.Remove(id);
			this.CarriedItems.Remove(id);
		}
	}

	/// <summary>
	/// Returns the item instance corresponding to a given id.
	/// </summary>
	/// <param name="id">Id of the item to get</param>
	/// <returns>The item object, or null if the id is invalid.</returns>
	public Item Get_Item(int id)
	{
		/* Check that the item exists */
		if (!AvailableItems.Keys.Contains(id))
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Recieved an invalid item id");
			return null;
		}
		else
		{
			return AvailableItems[id];
		}

	}

	/// <summary>
	/// Returns if a given item is able to be picked up.
	/// </summary>
	/// <param name="id">Id of item to be picked up</param>
	/// <returns>Returns if the item is available, returns false if invalid id</returns>
	public bool ItemAvailable(int id)
	{
		/* Check that the item exists */
		if (!AvailableItems.Keys.Contains(id))
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Recieved an invalid item id");
			return false;
		}
		else
		{
			return !CarriedItems[id];
		}

	}


	/// <summary>
	/// Adds a given item to the carried items.
	/// </summary>
	/// <param name="id"> The id of the item to add. </param>
	public void SelectItem(int id)
	{
		/* Check that the item exists */
		if (!AvailableItems.Keys.Contains(id))
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Recieved an invalid item id");
		}
		else
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Added " + AvailableItems[id].Get_Name() + " to selected items.");
			this.CarriedItems[id] = true;
		}
	}

	/// <summary>
	/// Removes a given item from the carried items
	/// </summary>
	/// <param name="id"> The id of the item to add. </param>
	public void DeselectItem(int id)
	{
		/* Check that the item exists */
		if (!AvailableItems.Keys.Contains(id))
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Recieved an invalid item id");
		}
		else
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Removed " + AvailableItems[id].Get_Name() + " from selected items.");
			this.CarriedItems[id] = false;
		}
	}

	/// <summary>
	/// Clears all items.
	/// </summary>
	public void ClearItems()
	{
		this.ClearAvailableItems();
		this.ClearCarriedItems();
	}

	/// <summary>
	/// Clears the list of available items.
	/// </summary>
	public void ClearAvailableItems()
	{
		this.AvailableItems.Clear();
	}

	/// <summary>
	/// Clears the list of carried items.
	/// </summary>
	public void ClearCarriedItems()
	{
		this.CarriedItems.Clear();
	}

	/// <summary>
	/// Adds an interactable to the available interactables.
	/// </summary>
	public void AddInteractable(int id, Interactable interactable)
	{
		this.AvailableInteractables.Add(id, interactable);
	}

	/// <summary>
	/// Returns a list of all available interactables.
	/// </summary>
	/// <returns></returns>
	public List<Interactable> GetAvailableInteractables()
	{
		return this.AvailableInteractables.Values.ToList();
	}

	/// <summary>
	/// Returns an interactable corresponding to a given id.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public Interactable GetInteractable(int id)
	{
		/* Check that the item exists */
		if (!AvailableInteractables.Keys.Contains(id))
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Recieved an invalid interactable id");
			return null;
		}
		else
		{
			return AvailableInteractables[id];
		}
	}

	/// <summary>
	/// Removes a given interactable from the available interactables.
	/// </summary>
	/// <param name="id"> The id of the interactable to remove. </param>
	public void RemoveInteractable(int id)
	{
		/* Check that the item exists */
		if (!AvailableInteractables.Keys.Contains(id))
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Recieved an invalid interactable id");
		}
		else
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Removed " + AvailableInteractables[id].Get_Name() + " from available interactables.");
			this.AvailableInteractables.Remove(id);
		}
	}

	/// <summary> Clear the list of available interactables. </summary>
	public void ClearInteractables()
	{
		this.AvailableInteractables.Clear();
	}

}
