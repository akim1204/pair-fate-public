using Godot;
using System;
/// <summary>
/// Handles potential inhibitors to the player's movements, such as ice.
/// </summary>
public partial class DialogueManager : Node2D
{

    /// <summary>
    /// List of file paths to each dialogue's json stored contents.
    /// </summary>
    [Export]
    public String[] Dialogue_Paths;


    /// <summary>
    /// Returns the path to the dialogue json file.
    /// </summary>
    /// <param name="dialogue_index"> The index of the dialogue (assigned by the world manager) </param>
    public virtual String Get_Dialogue(int dialogue_index)
    {
        if (Dialogue_Paths == null || dialogue_index >= Dialogue_Paths.Length)
        {
            Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Door was not properly assigned a destination");
            return "";
        }
        else
        {
            return Dialogue_Paths[dialogue_index];
        }
    }
}
