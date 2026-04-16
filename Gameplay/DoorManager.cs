using Godot;
using System;
/// <summary>
/// Handles potential inhibitors to the player's movements, such as ice.
/// </summary>
public partial class DoorManager : Node2D
{

    /// <summary>
    /// List of scene paths each door corresponds to.
    /// </summary>
    [Export]
    public String[] Door_Destination_Strings;

    /// <summary>
    /// List of scene indices each door corresponds to.
    /// </summary>
    [Export]
    public int[] Door_Destination_Indices;

    /// <summary>
    /// Returns the path and spawn index corresponding to a given door interactable.
    /// </summary>
    /// <param name="door_index"> The index of the door (assigned by the world manager) </param>
    public virtual (String, int) Get_Door(int door_index)
    {
        if (Door_Destination_Strings == null || Door_Destination_Indices == null ||
            door_index >= Door_Destination_Strings.Length || door_index >= Door_Destination_Indices.Length)
        {
            Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Door was not properly assigned a destination");
            return (null, -1);
        }
        else
        {
            return (Door_Destination_Strings[door_index], Door_Destination_Indices[door_index]);
        }
    }
}
