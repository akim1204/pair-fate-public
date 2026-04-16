using Godot;
using System;
using System.Collections.Generic;
public partial class preChocoController2 : Node2D
{

	private Dictionary<int, float> player_ignition = new Dictionary<int, float>();

	/// <summary> How long a player can be in the molten lava </summary>
	private const float IGNITION_CAP = 1f;

	/// <summary> How fast ignition decreases. </summary>
	private const float REDUCTION_COEF = 3;

	// Col1 represents right flowing river
	private ChocolateColumn col1;
	// player_bag is needed for ignition
	private PlayerBag player_bag;
	// Col2 represents left flowing river
	private ChocolateColumn col2;
	private ChocolateColumn col3;
	private TextureRect lava1;
	private float lava1_offset;
	private TextureRect lava2;
	private float lava2_offset;
	private TextureRect lava3;
	private float lava3_offset;

	private bool authority;

	public override void _Ready()
	{
		authority = Multiplayer.GetUniqueId() == 1;
		//get players for ignition
		player_bag = GameManager.Instance.Get_Player_Bag();
		//get columns for river
		col1 = GetNode<ChocolateColumn>("ChocolateCol1");
		lava1 = GetNode<TextureRect>("Lava1");
		col2 = GetNode<ChocolateColumn>("ChocolateCol2");
		lava2 = GetNode<TextureRect>("Lava2");
		col3 = GetNode<ChocolateColumn>("ChocolateCol3");
		lava3 = GetNode<TextureRect>("Lava3");
		//empty both rivers
		col1.Clear();
		col2.Clear();
		col3.Clear();

		/* Initialize player ignition */
		foreach (int pId in player_bag.GetActivePlayerIds())
		{
			player_ignition.Add(pId, 0);
		}

	}
	//ignition etc
	public override void _Process(double delta)
	{
		col1.Push_Vertical(-75 * (float)delta, true);
		lava1_offset = (lava1_offset - 75 * (float)delta) % 48;
		lava1.Position = new Vector2(580, -100 + lava1_offset);
		col2.Push_Horizontal(-75 * (float)delta, true);
		lava2_offset = (lava2_offset + 75 * (float)delta) % 96;
		lava2.Position = new Vector2(1000 + lava2_offset, -470);
		lava3_offset = (lava3_offset - 75 * (float)delta) % 96;
		lava3.Position = new Vector2(1000 - lava2_offset, -1056);
		col3.Push_Horizontal(75 * (float)delta, true);
		check_ignition((float)delta);
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

			bool safe = false;
			/* Platforms */
			if (yPos < -1056 && xPos < 1728)
			{
				safe = true;
			}
			else if (yPos > -752 && yPos <= -470 && xPos > 2500)
			{
				safe = true;
			}
			else if (yPos >= -180 && yPos <= 294 && xPos >= 1151 && xPos <= 1727)
			{
				safe = true;
			}
			else if (xPos <= 570 && yPos >= 866)
			{
				safe = true;
			}
			else if (col1.Check_Position(player.GlobalPosition) || col2.Check_Position(player.GlobalPosition) || col3.Check_Position(player.GlobalPosition))
			{
				safe = true;
			}
			//add checks for the 2 platforms
			//if xPos within the range and Y pos is within the range check Col1
			if (safe)
			{
				player_ignition[pId] = Mathf.Max(0, player_ignition[pId] - delta * REDUCTION_COEF);
			}
			else
			{
				player_ignition[pId] += (float)delta;
			}
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
