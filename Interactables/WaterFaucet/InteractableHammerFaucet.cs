using System;
using Godot;

/// <summary>
/// Candy Cane item.
/// </summary>
public partial class InteractableHammerFaucet : Interactable
{

	public override void _Ready()
	{
		/* Overriding item type and name */
		this.interactable_name = "HammerFaucet";

		/* Calling base constructor */
		base._Ready();
	}

	/// <summary>
	/// Initialize position of item when it is first picked up
	/// </summary>
	/// <param name="player"> Player who interacts with interactable </param>
	public override void Handle_Action(PlayerItem player)
	{
		if (player.Get_Item() != null)
		{
			if (player.Get_Item().Get_Name() == "Bottle Hammer")
			{
				Logger.Instance.Log(Logger.LOG_LEVELS.INFO, "Filling Hammer Bucket");
				ItemHammer hammer = (ItemHammer)player.Get_Item();
				hammer.Fill();
			}
		}
	}

	/// <summary>
	/// Overriding get_type of this interactable type.
	/// </summary>
	public override InteractableTypes Get_Type()
	{
		return InteractableTypes.Utility;
	}

}
