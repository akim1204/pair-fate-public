using Godot;
using System;

public partial class PlayerGraveInteractable : Interactable
{
	private int player_id = 0;

	private PlayerBag player_bag;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Hard set name */
		this.Name = "Interactable100" + (player_id % 5).ToString();

		/* Overriding item type and name */
		this.interactable_name = "PlayerGrave";

		/* Get player bag */
		player_bag = GetNode<PlayerBag>("/root/PlayerBag");

		/* Play animation */
		this.GetNode<AnimationPlayer>("AnimationPlayer").Play("Spawn");


		base._Ready();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Sets player this corresponds to
	/// </summary>
	public void Set_Id(int player_id)
	{
		this.player_id = player_id;
	}

	/// <summary>
	/// Revives the corresponding player
	/// </summary>
	/// <param name="player"> Player who interacts with interactable </param>
	public override void Handle_Action(PlayerItem player)
	{
		/* Check that valid */
		if (player_bag.GetPlayer(player_id) == null)
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Grave does not have linked player");
			Destroy();
			return;
		}
		/* Check that player still has revive */
		if (player.Get_Player().available_revives >= 1)
		{
			player.Get_Player().available_revives -= 1;
			player_bag.GetPlayer(player_id).Revive();
			/* Destroy this */
			Destroy();
		}
		else
		{
			if (player.Get_Player().Get_Authority())
			{
				GameManager.Instance.Display_Message("You are out of revives", 2);
			}
		}
	}

}
