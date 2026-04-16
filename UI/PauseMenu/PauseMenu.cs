using Godot;
using System;
/// <summary>
/// PauseMenu is what appears whenever you press escape in a scene.
/// </summary>
public partial class PauseMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		/* Toggle Visibility TODO: FUNCTIONS DIFFERENTLY IN LOAD MENUS */
		if (Input.IsActionJustPressed("ui_esc") && GameManager.Instance.In_Game())
		{
			this.Visible = !this.Visible;
		}
	}

	/// <summary>
	/// Helper Function for unpausing the game on continue.
	/// </summary>
	private void _on_continue_button_down()
	{
		// Replace with function body.
		//GameManager.Instance.PauseGame(false);
		this.Hide();
	}


	/// <summary>
	/// Empty fn but will house audio and all that
	/// </summary>
	private void _on_options_button_down()
	{
		GameManager.Instance.Show_Options();
		this.Hide();
	}

	//return back to the MainMenu
	private void _on_exit_button_down()
	{
		// Replace with function body.
		Hide();
		GameManager.Instance.Return_To_Menu();
	}
}



