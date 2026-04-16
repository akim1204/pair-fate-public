using Godot;
using System;

public partial class ScreenEffects : CanvasLayer
{
	/// <summary> Screen Effect Materials </summary>///
	Material screen_overlay;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		screen_overlay = GetNode<ColorRect>("ScreenOverlay").Material;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Set_Rotation(float rotation_amount)
	{
		screen_overlay.Set("shader_parameter/rotation", rotation_amount);
	}

	/// <summary>
	/// Sets the screen to be fragmented or not fragmented
	/// </summary>
	/// <param name="state"></param>
	public void Fragment(bool state)
	{
		screen_overlay.Set("shader_parameter/fragmented", state);
	}

	public void Set_Burn(float ratio)
	{
		screen_overlay.Set("shader_parameter/burn", ratio);
	}

	public void Set_Offset(int offset)
	{
		screen_overlay.Set("shader_parameter/div_offset", offset);
	}


	public void Set_Divs(int div1, int div2 = -1, int div_count = 5)
	{
		GD.Print(div1, ", ", div2);
		screen_overlay.Set("shader_parameter/div_burned", div1);
		screen_overlay.Set("shader_parameter/div_burned2", div2);
		screen_overlay.Set("shader_parameter/divisions", div_count);
	}
}
