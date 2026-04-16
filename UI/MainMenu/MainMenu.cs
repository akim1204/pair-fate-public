using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class MainMenu : Control
{
	[Export]
	public string NetworkPath = "res://UI/Networking/NetworkJoin.tscn";

	[Export]
	public PackedScene LoadMenu;

	/// <summary>
	/// Layer that contains all the ui.
	/// </summary>
	private CanvasLayer UI_layer;

	private OptionsMenu options_menu;

	/* Selection options */
	private Sprite2D selection_indicator;
	private Button play_indicator;
	private Button quit_indicator;
	private Button options_indicator;
	private int current_selection = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Get the ui layer */
		UI_layer = GetParent<CanvasLayer>();
		options_menu = GetNode<OptionsMenu>("OptionsDisplay");
		/* Get selection things */
		selection_indicator = GetNode<Sprite2D>("SelectionIndicator");
		var indicator_node = GetNode<Node2D>("TextButtons");
		play_indicator = indicator_node.GetNode<Button>("PlayIndicator");
		play_indicator.GrabFocus();
		options_indicator = indicator_node.GetNode<Button>("OptionsIndicator");
		quit_indicator = indicator_node.GetNode<Button>("QuitIndicator");
	}
	private void OnLoadingLevel()
	{
		Hide();
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("key_up"))
		{
			current_selection -= 1;
			current_selection += 3;
			current_selection %= 3;
			selection_indicator.Position = new Vector2(350, 400 + current_selection * 100);
			switch (current_selection)
			{
				case 0:
					play_indicator.GrabFocus();
					break;
				case 1:
					options_indicator.GrabFocus();
					break;
				case 2:
					quit_indicator.GrabFocus();
					break;
			}
		}
		if (Input.IsActionJustPressed("key_down"))
		{
			current_selection += 1;
			current_selection %= 3;
			selection_indicator.Position = new Vector2(350, 400 + current_selection * 100);
			switch (current_selection)
			{
				case 0:
					play_indicator.GrabFocus();
					break;
				case 1:
					options_indicator.GrabFocus();
					break;
				case 2:
					quit_indicator.GrabFocus();
					break;
			}
		}

		if (Input.IsActionJustPressed("menu_select"))
		{
			switch (current_selection)
			{
				case 0:
					start_game();
					break;
				case 1:
					show_options();
					break;
				case 2:
					quit_game();
					break;
			}
		}
	}

	private void start_game()
	{
		/* Switch to network scene */
		GameManager.Instance.Show_Network();
		GameManager.Instance.Play_Menu_Sound();
	}
	private void _on_play_indicator_button_down()
	{
		start_game();
	}
	private void _on_options_indicator_button_down()
	{
		show_options();
	}
	private void _on_quit_indicator_button_down()
	{
		quit_game();
	}

	/// <summary>
	/// Called when the start game button is pressed
	/// </summary>
	private void _on_start_game_button_down()
	{
		start_game();
	}

	private void quit_game()
	{
		GetTree().Quit();
	}
	private void _on_quit_game_button_down()
	{
		quit_game();
	}

	private void show_options()
	{
		options_menu.Show();
		GameManager.Instance.Play_Menu_Sound();
	}

	private void _on_options_menu_button_down()
	{
		show_options();
	}
}

