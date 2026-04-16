using Godot;
using System;

public partial class IntroBoat : Sprite2D
{
	private Vector2 DESTINATION = new Vector2(1030, 1925);

	private const float travel_speed = 200;

	public bool moving = true;

	private PlayerBag player_bag;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.player_bag = GetNode<PlayerBag>("/root/PlayerBag");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		/* Calculate movement */
		Vector2 displacement;
		if (this.GlobalPosition.DistanceTo(DESTINATION) < (float)delta * travel_speed)
		{
			displacement = DESTINATION - GlobalPosition;

			/* Set moving and display tutorial message */
			moving = false;

			GameManager.Instance.Display_Message("Use WASD to move.", 2);

			this.ProcessMode = ProcessModeEnum.Disabled;
		}
		else
		{
			displacement = (DESTINATION - GlobalPosition).Normalized() * (float)delta * travel_speed;
		}

		/* Move */
		this.GlobalPosition += displacement;
		foreach (Player player in player_bag.GetActivePlayers())
		{
			player.GlobalPosition += displacement;
		}
	}
}
