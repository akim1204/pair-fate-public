using Godot;
using System;

public partial class PlayerHpGui : HBoxContainer
{
	private Texture2D blue_heart_sprite;
	private Texture2D red_heart_sprite;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	/// <summary>
	/// Updates the hp to match the given hp.
	/// </summary>
	/// <param name="hp">Amount of hp to set</param>
	public void Update_Hp(int hp)
	{
		for (int i = 0; i < GetChildCount(); i++) {
			GetChild<Sprite2D>(i).Visible = hp > i;
		}
	}

	/// <summary>
	/// Initializes the health bar gui to display the right value
	/// </summary>
	/// <param name="main_color">What the current authority player color is</param>
	/// <param name="hp">Current hp</param> 
	public void Initiate(string main_color, int hp) {
		blue_heart_sprite = GD.Load<Texture2D>("res://Player/PlayerSprites/PlayerBlueHeartSprite.png");
		red_heart_sprite = GD.Load<Texture2D>("res://Player/PlayerSprites/PlayerRedHeartSprite.png");
		if (main_color == "red") {
			for (int i = 0; i < GetChildCount(); i++) {
				GetChild<Sprite2D>(i).Texture = red_heart_sprite;
			}
		}
		else {
			for (int i = 0; i < GetChildCount(); i++) {
				GetChild<Sprite2D>(i).Texture = blue_heart_sprite;
			}
			
		}
	} 
}
