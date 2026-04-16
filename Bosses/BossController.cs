using Godot;
using System;

public partial class BossController : Node2D
{
    /// <summary>
    /// GUI that displays information to player
    /// </summary>
    protected CanvasGUI canvas_gui;

    /// <summary>
    /// If the player is locked in place, such as during a cutscene.
    /// </summary>
    protected bool player_locked;

    /// <summary>
    /// If it is the players first time fighting the boss.
    /// </summary>
    protected bool first_time;

    /// <summary>
    /// Provides canvas gui to boss controller.
    /// </summary>
    /// <param name="gui"> Gui to provide</param>
    public void Initiate_GUI(CanvasGUI gui)
    {
        this.canvas_gui = gui;
    }

    /// <summary>
    /// Indicates that its the first time the players fought this boss.
    /// </summary>
    public void Set_First()
    {
        this.first_time = true;
    }

    /// <summary>
    /// Locks the player in place
    /// </summary>
    public void Player_Lock(bool state)
    {
        player_locked = state;
    }

    /// <summary>
    /// Returns if the player is locked.
    /// </summary>
    /// <returns></returns>
    public bool Get_Player_Lock()
    {
        return player_locked;
    }
}
