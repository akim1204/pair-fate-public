using Godot;
using System;

public partial class LoadingScreen : Control
{
	/// <summary> Path to the scene currently being loaded. </summary>
	private string path;
	/// <summary> If the scene is currently loading. </summary>
	private bool loading = false;

	/// <summary> Whether to wait for input before loading in. </summary>
	private bool wait_for_input = true;

	private Button continue_button;


	int loaded_players = 0;
	[Export]
	Godot.Collections.Array Tips;

	/// <summary> Progress bar instance to display progress bar. </summary>
	ProgressBar progress_bar;

	Label loading_label;

	private int index;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		progress_bar = GetNode<ProgressBar>("ProgressBar");
		loading_label = GetNode("Control").GetNode("VBoxContainer").GetNode<Label>("LoadingLabel");
		continue_button = GetNode<Button>("ContinueButton");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (loading)
		{
			/* Getting current progress */
			var progress = new Godot.Collections.Array();
			var status = ResourceLoader.LoadThreadedGetStatus(path, progress);
			if (status == ResourceLoader.ThreadLoadStatus.InProgress)
			{
				/* Update progress bar */
				progress_bar.Value = (double)progress[0] * 100;
			}
			else if (status == ResourceLoader.ThreadLoadStatus.Loaded)
			{
				/* Indicate loading is complete */
				SetProcess(false);
				progress_bar.Value = 100;

				/* Indicate that one player has finished loading */
				Rpc("Player_Loaded");
				Logger.Instance.Log(Logger.LOG_LEVELS.DEBUG, "One player has finished loading scene");
			}
			else if (status == ResourceLoader.ThreadLoadStatus.InvalidResource)
			{
				ResourceLoader.LoadThreadedRequest(path, "PackedScene", false);
			}
		}
	}

	public void _on_continue_button_button_down()
	{
		/* TODO: SOME REASON ITS CALLING MULTIPLE TIMES ??? WHEN NOT INSTANTIATES ?*/
		if (loaded_players > 0 && loaded_players == NetworkManager.Network_Player_Infos.Count)
		{
			Rpc("Change_Scene");
		}
	}

	/// <summary> Indicates that a player has finished loading the scene. </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Player_Loaded()
	{
		loaded_players += 1;
		/* If all players have loaded */
		if (loaded_players == NetworkManager.Network_Player_Infos.Count)
		{
			loading_label.Text = "All players have loaded in, press the button to continue.";
			continue_button.Show();
		}
	}

	/// <summary>
	/// Changes the current world scene to a different one.
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Change_Scene()
	{
		/* Get loaded scene */
		PackedScene resource = ResourceLoader.LoadThreadedGet(path) as PackedScene;
		/* Reset currently world scene to create a new one */
		GameManager.Instance.Reset_Bags();
		GameManager.Instance.Clear_World();

		Node currentNode = resource.Instantiate();
		GameManager.Instance.Add_To_World_Layer(currentNode);

		/*
		if(!GameManager.Instance.LoadingFromSave)
		{
			GD.Print("Not Loading from a save");
			GameManager.Instance.CheckForPlayer();
			GD.Print("Looking for spawn index at " + index);
			GameManager.Instance.MovePlayer(index);
		}
		GameManager.Instance.LoadingFromSave = false;
		GameManager.Instance.Paused = false;
		*/
		QueueFree();


	}

	/// <summary>
	/// Begins loading a specific loading path and displays information */
	/// </summary>
	/// <param name="path"> The path to load </param>
	public void Load_Level(string path)
	{
		this.path = path;
		//Show();
		if (Tips != null)
		{
			if (Tips.Count != 0)
			{
				Random rnd = new Random();
				GetNode<Label>("Control/VBoxContainer2/TipValue").Text = (string)Tips[rnd.Next(0, Tips.Count - 1)];
			}
		}
		string[] levelNameParts = path.Split('/');
		string[] levelListWithExtension = levelNameParts[levelNameParts.Length - 1].Split(".");

		GetNode<Label>("Control/VBoxContainer/LevelName").Text = levelListWithExtension[0];

		/* Reset number of loaded players */
		loaded_players = 0;

		/* Begin loading */
		ResourceLoader.LoadThreadedRequest(path, "PackedScene", false);
		loading = true;
	}
}
