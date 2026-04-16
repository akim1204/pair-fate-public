using Godot;
using System;

public partial class Knight : Interactable
{
	private textbox textbox;
	private Sprite2D text_indicator;

	/// <summary>
	/// Dialogue Manager that handles storage of all the dialogue. 
	/// </summary>
	private DialogueManager dialogue_manager;
	public override void _Ready()
	{
		/* Calling base constructor */
		base._Ready();

		/* Overriding item type and name */
		this.interactable_name = "KnightDialogue";

		textbox = GetNode<textbox>("Textbox");
		text_indicator = GetNode<Sprite2D>("SpeechIndicator");

		/* Getting dialogue files */

		dialogue_manager = GameManager.Instance.Get_Dialogue_Manager();

		/* Check that the doors all work */
		if (dialogue_manager == null)
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Dialogue manager was not properly initialized");
			Remove_Interactable();
		}
		else
		{
			var dialogue_path = dialogue_manager.Get_Dialogue(this.type_index);
			/* Check that it works */
			if (dialogue_path == "")
			{
				Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Door manager did not assign this door");
				Remove_Interactable();
			}
			/* Otherwise, initialize textbox */
			else
			{
				textbox.Parse_Textfile(dialogue_path);
			}
		}
	}

	/// <summary>
	/// Refills water bucket if its carried.
	/// </summary>
	/// <param name="player"> Player who interacts with interactable </param>
	public override void Handle_Action(PlayerItem player)
	{
		if (!textbox.progress_textbox())
		{
			Remove_Interactable();
			text_indicator.QueueFree();
		}
	}
	/// <summary>
	/// Overriding get_type of this interactable type.
	/// </summary>
	public override InteractableTypes Get_Type()
	{
		return InteractableTypes.Dialogue;
	}
}
