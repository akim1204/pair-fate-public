using Godot;
using System;

public partial class LoadButton : Button
{
	[Export]
	private string load_level;

	private MultiplayerController multiplayer_controller;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		multiplayer_controller = GetParent().GetParent<MultiplayerController>();
	}

	public void _on_button_down()
	{
		/* Check that a file exists */
		if (FileAccess.FileExists(load_level))
		{
			/* Load level */
			multiplayer_controller.Rpc("start_game", load_level);
			GameManager.Instance.Play_Menu_Sound();
		}
	}
}
