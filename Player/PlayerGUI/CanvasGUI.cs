using Godot;
using System;

public partial class CanvasGUI : CanvasLayer
{
	int hp_main = 0;
	int hp_side = 0;

	string main_color = "";

	PlayerHpGui main_hp;
	PlayerHpGui side_hp;

	BossGUI boss_hp;

	private Tooltip tooltip;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var health_guis = GetNode("PlayerGUI").GetNode("PlayerHealthGUI");
		main_hp = health_guis.GetNode<PlayerHpGui>("MainHealthBar");
		side_hp = health_guis.GetNode<PlayerHpGui>("SideHealthBar");
		tooltip = GetNode<Tooltip>("Tooltip");
	}

	/// <summary>
	/// Sets the boss gui to be visible
	/// </summary>
	public void Is_Boss(bool is_boss_room, BossGUI.BossStyles style = BossGUI.BossStyles.NONE)
	{
		boss_hp = GetNode<BossGUI>("BossGUI");
		boss_hp.Set_Style(style);
		boss_hp.Visible = is_boss_room;
	}

	/// <summary>
	/// Sets the color corresponding to the authority player.
	/// </summary>
	/// <param name="color">The color </param>
	public void Set_Main_Color(string color)
	{
		this.main_color = color;
		if (main_color == "red")
		{
			main_hp.Initiate("red", 3);
			side_hp.Initiate("blue", 3);
		}
		else
		{
			main_hp.Initiate("blue", 3);
			side_hp.Initiate("red", 3);
		}

	}

	/// <summary>
	/// Updates the hp of either the main or side bar.
	/// </summary>
	/// <param name="hp"> Hp Amount</param>
	/// <param name="is_main"> If the calling player is the authority. </param>
	public void Update_Hp(int hp, bool is_main)
	{
		if (is_main)
		{
			main_hp.Update_Hp(hp);
		}
		else
		{
			side_hp.Update_Hp(hp);
		}
	}

	/// <summary>
	/// Updates how full the boss hp is.
	/// </summary>
	public void Update_Boss_Health(float[] values, bool hard = false)
	{
		boss_hp.Update_Health(values, hard);
	}


	public void Show_Tooltip(string tooltip_name, string tooltip_string, Texture2D tooltip_texture)
	{
		tooltip.Update(tooltip_name, tooltip_string, tooltip_texture);
		tooltip.Show_Item();
	}

}
