using Godot;
using System;

public partial class OptionsMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_esc") && this.Visible)
		{
			this.Hide();
		}
	}

	public void _on_back_button_button_down()
	{
		this.Hide();
	}
}
