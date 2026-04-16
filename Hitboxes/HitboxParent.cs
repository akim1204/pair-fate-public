using Godot;
using System;

public partial class HitboxParent : Area2D
{
    /// <summary>
    /// Type of hitbox, used by accepting hurtbox
    /// </summary>
    public virtual string hitbox_type { get { return "None"; } }

    /// <summary>
    /// The player that spawned this hitbox.
    /// </summary>
    protected Player owner_player;

    /// <summary>
    /// Returns a set of vectors representing the hitbox to use for calculations.
    /// Actual vectors vary depending on hitbox type.
    /// </summary>
    /// <returns>A set of vectors representing the hitbox.</returns>
    public virtual Vector2[] Get_Points()
    {
        return new Vector2[] { };
    }

    /// <summary>
    /// Initiates the hitbox with its owner player.
    /// </summary>
    /// <param name="owner_player">Player that owns this hitbox</param>
    public void Initialize(Player owner_player)
    {
        this.owner_player = owner_player;
    }

    /// <summary>
    /// Returns if the owner of this player is the authority and thus whether the
    /// hit should be registered.
    /// </summary>
    /// <returns>True if the hitbox owner is a authority, false otherwise.</returns>
    public bool Authority_Hitbox()
    {
        if (owner_player != null)
        {
            return owner_player.Get_Authority();
        }
        else
        {
            Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Hitbox registered without an owner");
            return false;
        }
    }
    public Player Get_Player()
    {
        return this.owner_player;
    }
}
