using Godot;
using System;

public partial class OverlayMenu : Control
{
	/// <summary> Label that displays temporary messages </summary>
	Label message_display;
	Label message_display_centered;

	/// <summary> Timer for how long messages display </summary>
	float message_timer = 0;
	float message_timer_max = 0;

	/// <summary> Sprite used to display images to the player </summary>
	Sprite2D sprite_display;


	/// <summary> Timer for how long sprites display </summary>
	float sprite_timer = 0;
	float sprite_timer_max = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		message_display = GetNode<Label>("MessageGUI");
		message_display_centered = GetNode<Label>("MessageGUI2");
		sprite_display = GetNode<Sprite2D>("SpriteDisplay");
	}

	public override void _Process(double delta)
	{
		/* Decrease message timer */
		message_timer = Mathf.Max(0, message_timer - (float)delta);

		/* Removing message */
		if (message_timer == 0)
		{
			message_display.Text = "";
			message_display_centered.Text = "";
		}
		/* Fade in */
		else if (message_timer > message_timer_max - 0.2)
		{
			Set_Message_Alpha(5 * (message_timer_max - message_timer));
		}
		/* Fade out */
		else if (message_timer < 0.5f)
		{
			Set_Message_Alpha(message_timer * 2);
		}

		/* Decrease sprite timer */
		sprite_timer = Mathf.Max(0, sprite_timer - (float)delta);

		/* Removing message */
		if (sprite_timer == 0)
		{
			if (sprite_display.Texture != null)
			{
				sprite_display.Texture = null;
			}
		}
		/* Fade in */
		else if (sprite_timer > sprite_timer_max - 0.2)
		{
			Set_Sprite_Alpha(Mathf.Min(1, 5 * (sprite_timer_max - sprite_timer)));
		}
		/* Fade out */
		else if (sprite_timer < 0.5f)
		{
			Set_Sprite_Alpha(sprite_timer * 2);
		}
	}


	/// <summary>
	/// Displays a message for a given amount of time.
	/// </summary>
	/// <param name="message"></param>
	/// <param name="time"></param>
	public void Display_Message(string message, float time, bool centered)
	{
		message_timer_max = time;
		if (centered)
		{
			message_display_centered.Text = message;
		}
		else
		{
			message_display.Text = message;
		}
		message_timer = time;
	}

	/// <summary>
	/// Sets transparency of the message overlay.
	/// </summary>
	/// <param name="alpha"></param>
	private void Set_Message_Alpha(float alpha)
	{
		var cur_modulate = message_display.Modulate;
		cur_modulate.A = alpha;
		message_display.Modulate = cur_modulate;
		message_display_centered.Modulate = cur_modulate;
	}

	/// <summary>
	/// Displays a message for a given amount of time.
	/// </summary>
	/// <param name="texture"> The sprite texture to display </param>
	/// <param name="time"> How long to display</param>
	public void Display_Sprite(Texture2D texture, float time, Vector2 position)
	{
		sprite_display.Position = position;
		sprite_display.Texture = texture;
		sprite_timer = time;
		sprite_timer_max = time;
	}
	/// <summary>
	/// Sets transparency of the message overlay.
	/// </summary>
	/// <param name="alpha"></param>
	private void Set_Sprite_Alpha(float alpha)
	{
		var cur_modulate = sprite_display.Modulate;
		cur_modulate.A = alpha;
		sprite_display.Modulate = cur_modulate;
	}
}
