using Godot;
using System;

public partial class PlayerHurtbox : Area2D
{
	/// <summary>
	/// Node2D corresponding to player.
	/// </summary>
	private Player player;
	public override void _Ready()
	{

		/* Getting owner player */
		player = GetParent<Player>();
	}

	public void Hurt(int damage)
	{
		player.Try_Hurt(damage);
	}

	public Player GetPlayer()
	{
		return player;
	}
}
