using Godot;
using System;
/// <summary>
/// Handles potential inhibitors to the player's movements, such as ice.
/// </summary>
public partial class MovementInhibitor : Node2D
{

    /// <summary>
    /// Places any restrictions on the player movement due to environment.
    /// </summary>
    /// <param name="delta"> The time since the previous frame. </param>
    /// <param name="cur_velocity">The current velocity of the player. </param>
    /// <param name="cur_position">The current position of the player. </param>
    /// <returns>The new velocity of the player. </returns>
    public virtual Vector2 Inhibit_Movement(float delta, Vector2 cur_velocity, Vector2 cur_position)
    {
        Logger.Instance.Log(Logger.LOG_LEVELS.WARN, "Movement Inhibition called on uninitialized class.");
        return cur_velocity;
    }

    /// <summary>
    /// Looks for necessary nodes to calculate movement, should only be called
    /// once seen is initialized.
    /// </summary>
    public virtual void Seek()
    {
        Logger.Instance.Log(Logger.LOG_LEVELS.WARN, "Movement Inhibition seeking called on uninitialized class.");
    }
}
