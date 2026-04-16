using Godot;
using System;

public partial class KnightEffects : CanvasLayer
{
	private const float ROOM_WIDTH = 1920;
	/// <summary> Shader overlayed over screen </summary>
	ColorRect overlay;

	private float burn = 0;
	private float div_burned = 0, div_burned2 = 0;
	private bool burning = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		overlay = GetNode<ColorRect>("KnightEffectsOverlay");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_one"))
		{
			overlay.Material.Set("shader_parameter/flip_y", true);
		}

		if (Input.IsActionJustPressed("ui_two"))
		{

			overlay.Material.Set("shader_parameter/div_offset", GD.Randi() % 3 + 2);
			overlay.Material.Set("shader_parameter/fragmented", true);
		}

		if (Input.IsActionJustPressed("ui_three"))
		{
			div_burned = GD.Randi() % 5;
			div_burned2 = GD.Randi() % 5;
			overlay.Material.Set("shader_parameter/div_burned", div_burned);
			overlay.Material.Set("shader_parameter/div_burned2", div_burned2);
			overlay.Material.Set("shader_parameter/burn", 0);
			burning = true;
		}

		if (burning)
		{
			/* Increment burn */
			burn = Mathf.Min(burn + (float)delta / 3, 1);
			overlay.Material.Set("shader_parameter/burn", burn);

			/* End of burn */
			if (burn >= 1)
			{
				burning = false;
				burn = 0;
				overlay.Material.Set("shader_parameter/burn", burn);

				/* Hurt players */
				foreach (Player player in GameManager.Instance.Get_Player_Bag().GetAllPlayers())
				{
					/* Check if in burned div */
					if (Mathf.Floor(player.GlobalPosition.X / ROOM_WIDTH * 5) == div_burned
					|| Mathf.Floor(player.GlobalPosition.X / ROOM_WIDTH * 5) == div_burned2)
					{
						player.Try_Hurt(1);
					}
				}
				/* Screen shake */
				foreach (PlayerCamera camera in GameManager.Instance.Get_Player_Bag().GetAllCameras())
				{
					camera.Shake(0.2f, 15, 10);
				}
			}
		}
	}
}
