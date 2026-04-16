using Godot;
using System;
using System.Collections.Generic;
public partial class preChocoController : Node2D
{

	private Dictionary<int, float> player_ignition = new Dictionary<int, float>();

	/// <summary> How long a player can be in the molten lava </summary>
	private const float IGNITION_CAP = 1.5f;

	/// <summary> How fast ignition decreases. </summary>
	private const float REDUCTION_COEF = 3;

	// Col1 represents right flowing river
	private ChocolateColumn col1;
	// player_bag is needed for ignition
	private PlayerBag player_bag;
	// Col2 represents left flowing river
	private ChocolateColumn col2;
	private bool authority;

	public override void _Ready()
	{
		authority = Multiplayer.GetUniqueId() == 1;
		//get players for ignition
		player_bag = GameManager.Instance.Get_Player_Bag();
		//get columns for river
		col1 = GetNode<ChocolateColumn>("ChocolateCol1");
		col2 = GetNode<ChocolateColumn>("ChocolateCol2");
		//empty both rivers
		col1.Clear();
		col2.Clear();


		/* Initialize player ignition */
		foreach (int pId in player_bag.GetActivePlayerIds())
		{
			player_ignition.Add(pId, 0);
		}

	}
	//ignition etc
	public override void _Process(double delta)
	{
		check_ignition((float)delta);
		//check redraw
		QueueRedraw();
	}

	/// <summary>
	/// Checks the ignition of a player within a specific platform
	/// </summary>
	/// <param name="platform_id">The id of the platform to check</param>
	/// <param name="player"> The player </param>
	/// <param name="delta"> The time since the last frame</param>
	private void check_platform_ignition(ChocolateColumn platform_id, Player player, float delta)
	{
		/* Check inclusion in the platform */
		int pId = player.Get_Id();
		if (platform_id.Check_Position(player.GlobalPosition))
		{
			player_ignition[pId] = Mathf.Max(0, player_ignition[pId] - delta * REDUCTION_COEF);
		}
		else
		{
			player_ignition[pId] += delta;
			if (player_ignition[pId] > IGNITION_CAP)
			{
				/* Kill if this is the authority */
				if (player.Get_Authority())
				{
					GameManager.Instance.Display_Message("You've been overcooked.", 2.5f);
					Rpc("Ignite_Player", pId);
				}
			}
		}
	}


	/// <summary>
	/// Check if any players have to be ignited.
	/// </summary>
	private void check_ignition(float delta)
	{
		foreach (Player player in player_bag.GetActivePlayers())
		{
			/* Getting id */
			int pId = player.Get_Id();
			/* Adding if new */
			if (!player_ignition.ContainsKey(pId))
			{
				player_ignition.Add(pId, 0);
			}

			float yPos = player.GlobalPosition.Y;
			float xPos = player.GlobalPosition.X;
			//add checks for the 2 platforms
			//if xPos within the range and Y pos is within the range check Col1
			if ((yPos >= 864 || yPos <= 192) && (xPos >= 1152 || xPos <= 768))
			{
				player_ignition[pId] = Mathf.Max(0, player_ignition[pId] - delta * REDUCTION_COEF);
			}
			else if (xPos < 760)
			{
				check_platform_ignition(col1, player, delta);
				return;
			}
			else if (xPos > 1160)
			{
				check_platform_ignition(col2, player, delta);
				return;
			}
			else if (xPos > 768 && xPos < 1152 && yPos > 192)
			{
				player_ignition[pId] += (float)delta * 5;
				if (player_ignition[pId] > IGNITION_CAP)
				{
					/* Kill if this is the authority */
					if (player.Get_Authority())
					{
						GameManager.Instance.Display_Message("You've been overcooked.", 2.5f);
						Rpc("Ignite_Player", pId);
					}
				}
			}
		}

	}

	public override void _Draw()
	{
		foreach (Player player in player_bag.GetActivePlayers())
		{
			if (!player_ignition.ContainsKey(player.Get_Id()))
			{
				player_ignition.Add(player.Get_Id(), 0);
			}

			/* Draw ignition bar above user player */
			if (player.Get_Authority())
			{
				DrawLine(player.GlobalPosition - new Vector2(50, 100),
				player.GlobalPosition - new Vector2(50 - 100 * Mathf.Min(player_ignition[player.Get_Id()] / IGNITION_CAP, 1), 100),
				Colors.Red, 15);
			}
		}
	}

	/// <summary>
	/// Ignites and kills a player on all screens.
	/// </summary>
	/// <param name="player_id"> The id of the player being killed </param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Ignite_Player(int player_id)
	{
		player_bag.GetPlayer(player_id).Kill();
	}


}
