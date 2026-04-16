using Godot;
using System;

public partial class DeathMenu : Control
{
	string scene_path = "";
	int scene_index = 0;
	float show_timer = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		show_timer = Mathf.Min(1, show_timer + (float)delta / 5);
		var new_mod = Modulate;
		new_mod.A = show_timer;
		Modulate = new_mod;
	}

	/// <summary>
	/// Sets the restart point upon death
	/// </summary>
	public void Set_Restart_Point(string new_path, int new_index)
	{
		scene_path = new_path;
		scene_index = new_index;
	}

	public void _on_restart_button_button_down()
	{
		GameManager.Instance.Load_Level(scene_path, scene_index);
	}

	/// <summary>
	/// Shows the death menu.
	/// </summary>
	public void Show_Death()
	{
		this.Show();
		show_timer = 0;
	}
}
