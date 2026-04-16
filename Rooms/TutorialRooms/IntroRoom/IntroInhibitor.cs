using Godot;
using System;

public partial class IntroInhibitor : MovementInhibitor
{
	private IntroBoat intro_boat;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	/// <summary>
	/// Places any restrictions on the player movement due to environment.
	/// </summary>
	/// <param name="delta"> The time since the previous frame. </param>
	/// <param name="cur_velocity">The current velocity of the player. </param>
	/// <param name="cur_position">The current position of the player. </param>
	/// <returns>The new velocity of the player. </returns>
	public override Vector2 Inhibit_Movement(float delta, Vector2 cur_velocity, Vector2 cur_position)
	{
		if (intro_boat.moving == true) cur_velocity = Vector2.Zero;
		return cur_velocity;
	}
	/// <summary>
	/// Looks for necessary nodes to calculate movement, should only be called
	/// once seen is initialized.
	/// </summary>
	public override void Seek()
	{
		intro_boat = GetNode<IntroBoat>("../ExtraEntities/IntroBoat");
	}
}
