using Godot;
using System;

public partial class JelloMini : Sprite2D
{
	
	private Vector2 target_position = Vector2.Zero;

    /// <summary> Enumeration of available enemy states. </summary>
	public enum EnemyStates {
        IDLE, /* Currently not moving */
        MOVE, /* Regular slow movement */
	};

	private EnemyStates state = EnemyStates.IDLE;

	/// <summary> Timer used to track player actions. </summary>

	private float action_timer = 0;

	/// <summary> Base movement speed of jello. </summary>
	private const float TRAVEL_SPEED = 20f;

	/// <summary> Random action generator for actions.  </summary>
	private Random rand = new Random();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print(Multiplayer.GetUniqueId());

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		switch (state) {
			case EnemyStates.IDLE:
			action_timer += (float) delta;
			if (action_timer > 2 && Multiplayer.GetUniqueId() == 1) {
				Vector2 new_position = this.GlobalPosition + Vector2.FromAngle((float) rand.NextDouble() * 2 * Mathf.Pi) * 50f;
				Rpc("Set_Position", new_position);
			}
			break;
			case EnemyStates.MOVE:
			handle_movement((float) delta);
			break;
		}
	}

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Set_Position(Vector2 new_position) {
		state = EnemyStates.MOVE;
		target_position = new_position;
	}

	/// <summary>
	/// Handles movement of the jello.
	/// </summary>
	/// <param name="delta"> Time since last frame in seconds. </param>
	private void handle_movement(float delta) {
		/* If close enough, snap to it */
		if ((GlobalPosition - target_position).LengthSquared() < TRAVEL_SPEED * TRAVEL_SPEED * delta) {
			this.GlobalPosition = target_position;
			state = EnemyStates.IDLE;
			action_timer = 0;
		}
		/* Otherwise, move towards */
		else {
			this.GlobalPosition += (target_position - GlobalPosition).Normalized() 
				* TRAVEL_SPEED * delta;
		}
	}

	public void _on_jello_body_hitbox_area_entered(Area2D area) {
        // If it is a hurtbox
        if (area.GetType().IsAssignableTo(typeof(HitboxParent))) {
			if (((HitboxParent) area).Authority_Hitbox()) {
				GD.Print("Hit registered in ", Multiplayer.GetUniqueId());
			}
        }

	}
}
