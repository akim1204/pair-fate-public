using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerBag : Node
{

	/// <summary>
	/// Dictionary of which players are currently active. Synced across players.
	/// </summary>
	public Dictionary<int, bool> ActivePlayers = new Dictionary<int, bool>();

	/// <summary>
	/// Dictionary mapping ids to players, is not synced across players.
	/// </summary>
	public Dictionary<int, Player> AllPlayers = new Dictionary<int, Player>();

	public Dictionary<int, PlayerCamera> AllPlayerCameras = new Dictionary<int, PlayerCamera>();

	/// <summary>
	/// Returns the hashset of players currently active.
	/// </summary>
	/// <returns> List of active players. </returns>
	public List<Player> GetActivePlayers()
	{
		List<Player> active_players = new List<Player>();
		foreach (int player_id in AllPlayers.Keys)
		{
			if (ActivePlayers[player_id])
			{
				active_players.Add(AllPlayers[player_id]);
			}
		}
		return active_players;
	}

	/// <summary>
	/// Returns the hashset of ids of players currently active.
	/// </summary>
	/// <returns> List of active players. </returns>
	public List<int> GetActivePlayerIds()
	{
		List<int> active_players = new List<int>();
		foreach (int player_id in AllPlayers.Keys)
		{
			if (ActivePlayers[player_id])
			{
				active_players.Add(player_id);
			}
		}
		return active_players;
	}

	/// <summary>
	/// Returns the list of all players, active or inactive.
	/// </summary>
	/// <returns> List of all players. </returns>
	public List<Player> GetAllPlayers()
	{
		return this.AllPlayers.Values.ToList();
	}

	/// <summary>
	/// Returns the player corresponding to a player id.
	/// </summary>
	/// <param name="player_id"></param>
	/// <returns></returns>
	public Player GetPlayer(int player_id)
	{
		/* Check that the player exists */
		if (!AllPlayers.Keys.Contains(player_id))
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Recieved an invalid player id");
			return null;
		}
		return AllPlayers[player_id];
	}

	/// <summary>
	/// Returns a list of all player cameras.
	/// </summary>
	/// <returns></returns>
	public List<PlayerCamera> GetAllCameras()
	{
		return this.AllPlayerCameras.Values.ToList();
	}

	/// <summary>
	/// Adds a given player to the list of players
	/// </summary>
	/// <param name="id">The id of the player to add (1 for host, other number for joinees).</param>
	/// <param name="player">The specific player instance to add.</param>
	public void AddPlayer(int id, Player player)
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Added " + player.Name + " to all players.");
		this.AllPlayers.Add(id, player);
		this.ActivePlayers.Add(id, false);
	}

	/// <summary>
	/// Adds a given camera to the list of cameras.
	/// </summary>
	/// <param name="id"> Id of the player the camera is attatched to.</param>
	/// <param name="player_camera"> The player camera. </param>
	public void AddCamera(int id, PlayerCamera player_camera)
	{
		this.AllPlayerCameras.Add(id, player_camera);
	}

	/// <summary>
	/// Adds a given player to the active players
	/// </summary>
	/// <param name="id"> The id of the player to add. </param>
	public void ActivatePlayer(int id)
	{
		/* Check that the player exists */
		if (!AllPlayers.Keys.Contains(id))
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Recieved an invalid player id");
		}
		else
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Added " + AllPlayers[id].Name + " to active players.");
			this.ActivePlayers[id] = true;
		}
	}

	/// <summary>
	/// Removes a given item from the carried items
	/// </summary>
	/// <param name="id"> The id of the player to be removed. </param>
	public void DeactivatePlayer(int id)
	{
		/* Check that the player exists */
		if (!AllPlayers.Keys.Contains(id))
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Recieved an invalid player id");
		}
		else
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Removed " + AllPlayers[id].Name + " from active players.");
			this.ActivePlayers[id] = false;
		}
	}

	public void RemovePlayer(int id)
	{
		/* Check that the player exists */
		if (!AllPlayers.Keys.Contains(id))
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Recieved an invalid player id");
		}
		else
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Removed " + AllPlayers[id].Name + " from all players.");
			this.ActivePlayers.Remove(id);
			this.AllPlayers.Remove(id);
		}

	}

	/// <summary>
	/// Clears the list of available and active players
	/// </summary>
	public void ClearPlayers()
	{
		this.ActivePlayers.Clear();
		this.AllPlayers.Clear();
		this.AllPlayerCameras.Clear();
	}

}
