using Godot;
using System;

public partial class Tooltip : Control
{
	private Label itemName;
	private Label itemDesc;
	private TextureRect pic;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Hide();
		itemName = GetNode<Label>("nameContainer/ItemName");
		itemDesc = GetNode<Label>("descContainer/ItemDescription");
		pic = GetNode<TextureRect>("TextureRect");


	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Update the item name and item description.
	/// </summary>
	public void Update(string tooltip_name, string tooltip_string, Texture2D tooltip_texture)
	{
		pic.Texture = tooltip_texture;
		itemName.Text = tooltip_name;
		itemDesc.Text = tooltip_string;
		/*
		if (item == "ItemCandyCane"){
			GD.Print("candy cane");
			pic.Texture = (Texture2D)GD.Load("res://Player/PlayerItems/CandyCane/Candy Scythe.png");
			itemName.Text = "CANDY SCYTH";
			itemDesc.Text = "Long and sharp, and the weapon of choice of most gingerbread men. Swings far and wide due to its weight.";
		}
		else if (item == "ItemChocolateShield") {
			GD.Print("shield");
			pic.Texture = (Texture2D)GD.Load("res://Player/PlayerItems/ChocolateShield/Chocolate Shield.png");
			itemName.Text = "CHOCOLATE SHIELD";
			itemDesc.Text = "Crafted and sculpted by the finest of chocolatiers. Capable of blocking most things thrown an it, at the cost of being able to move.";
		}
		else if (item == "ItemWaterBucket") {
			GD.Print("shield");
			pic.Texture = (Texture2D)GD.Load("res://Player/PlayerItems/WaterBucket/placeholder.png");
			itemName.Text = "BUCKET";
			itemDesc.Text = "Useful for a quick drink, and also dousing enemies in milk. Has to be refilled after two uses though.";
		}
		else 
		{
			GD.Print("ADFKLJAF");
			itemName.Text = "FUCK";
		
		*/

	}

	/// <summary>
	/// Shows the tooltip menu.
	/// </summary>
	public void Show_Item()
	{
		if (Visible)
		{
			this.Hide();
		}
		else
		{
			this.Show();
		}
	}
}
