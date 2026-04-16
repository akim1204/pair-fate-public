using Godot;
using System;

public partial class BossGUI : CenterContainer
{
	/// <summary> Enumeration of available boss states. </summary>
	public enum BossStyles
	{
		NONE,
		JELLO,
		OVEN,
		ICE,
		CAKE
	};

	private Sprite2D bar_sprite;

	public BossStyles boss_style = BossStyles.JELLO;


	/* Values for JELLO */
	private Color JELLO_COLOR = new Color(0.8f, 0.38f, 0.4f, 0.6f);
	private Color JELLO_EYE_COLOR = new Color(0.25f, 0.235f, 0.295f, 1);
	/// <summary>
	/// Current percentage of boss hp available.
	/// </summary>
	private float boss_percentage = 1;

	private float[] boss_values = new float[0];
	private float[] potential_values = new float[0];
	private const float BOSS_SHIFT_SPEED = 5;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		bar_sprite = GetNode<Sprite2D>("BossHealth");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		/* If there are any changes */
		bool changed = false;
		for (int i = 0; i < Math.Min(boss_values.Length, potential_values.Length); i++)
		{
			if (boss_values[i] < potential_values[i])
			{
				boss_values[i] = Mathf.Min(potential_values[i], boss_values[i] + (float)delta * BOSS_SHIFT_SPEED);
				changed = true;
			}
			if (boss_values[i] > potential_values[i])
			{
				boss_values[i] = Mathf.Max(potential_values[i], boss_values[i] - (float)delta * BOSS_SHIFT_SPEED);
				changed = true;
			}
		}
		/* Redraw if changed */
		if (changed)
		{
			QueueRedraw();
		}
	}

	/// <summary>
	/// Initializes the style of the health bar
	/// </summary>
	/// <param name="style"></param>
	public void Set_Style(BossStyles style)
	{
		this.boss_style = style;
		/* Initial setup */
		switch (boss_style)
		{
			case BossStyles.JELLO:
				float[] values = new float[23];
				for (int i = 0; i < 23; i++)
				{
					values[i] = 1;
				}
				boss_values = values;
				bar_sprite.Modulate = new Color(1, 1, 1, 1);
				Update_Health(boss_values, false);
				break;
			case BossStyles.OVEN:
				values = new float[2];
				bar_sprite.Modulate = new Color(1, 1, 1, 1);
				break;
			case BossStyles.ICE:
				values = new float[2];
				bar_sprite.Modulate = new Color(1, 1, 1, 0.5f);
				break;
		}

	}

	/// <summary>
	/// Updates the health values for the boss bar
	/// </summary>
	/// <param name="values">Values of health bar </param>
	/// <param name="hard"> Whether to slowly shift or set immediately</param>
	public void Update_Health(float[] values, bool hard)
	{
		potential_values = values;
		if (hard)
		{
			this.boss_values = values;
			QueueRedraw();
		}
	}

	public override void _Draw()
	{
		switch (boss_style)
		{
			case BossStyles.JELLO:
				Draw_Jello();
				break;
			case BossStyles.OVEN:
				Draw_Oven();
				break;
			case BossStyles.ICE:
				Draw_Ice();
				break;
			case BossStyles.CAKE:
				Draw_Cake();
				break;
		}
	}

	private void Draw_Jello()
	{
		/* Error check */
		if (boss_values.Length != 23)
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.WARN, "Boss GUI has invalid values: " + boss_values.ToString());
			return;
		}
		/* Draw background */
		DrawLine(new Vector2(-640, -60), new Vector2(640, -60), Colors.Gray, 80);
		/* Draw eyes */
		for (int i = 0; i < 8; i++)
		{
			if (boss_values[15 + i] > 0.5)
			{
				DrawCircle(new Vector2(-620 + 40 * i, -60), 18, JELLO_EYE_COLOR);
			}
		}
		/* Draw jello hps */
		for (int i = 0; i < 1; i++)
		{ // HP 1
			DrawLine(new Vector2(320, -60), new Vector2(320 + 320 * boss_values[i], -60), JELLO_COLOR, 80);
		}
		for (int i = 1; i < 3; i++)
		{ // HP 2
			DrawLine(new Vector2(160 * (i - 1), -60), new Vector2(160 * (i - 1) + 160 * boss_values[i], -60), JELLO_COLOR, 80);
		}
		for (int i = 3; i < 7; i++)
		{ // HP 3
			DrawLine(new Vector2(-320 + 80 * (i - 3), -60), new Vector2(-320 + 80 * (i - 3) + 80 * boss_values[i], -60), JELLO_COLOR, 80);
		}
		for (int i = 7; i < 15; i++)
		{ // HP 4
			DrawLine(new Vector2(-640 + 40 * (i - 7), -60), new Vector2(-640 + 40 * (i - 7) + 40 * boss_values[i], -60), JELLO_COLOR, 80);
		}
	}

	private void Draw_Oven()
	{
		if (boss_values.Length != 2)
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.WARN, "Boss GUI has invalid values: " + boss_values.ToString());
			return;
		}
		DrawLine(new Vector2(-640, -60), new Vector2(-640 + 1280.0f * boss_values[0] / boss_values[1], -60), Colors.Chocolate, 80);
	}

	private void Draw_Ice()
	{
		if (boss_values.Length != 2)
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.WARN, "Boss GUI has invalid values: " + boss_values.ToString());
			return;
		}
		DrawLine(new Vector2(-640, -60), new Vector2(-640 + 1280.0f * boss_values[0] / boss_values[1], -60), new Color(1, 1, 1, 0.4f), 80);

	}
	private void Draw_Cake()
	{
		if (boss_values.Length != 2)
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.WARN, "Boss GUI has invalid values: " + boss_values.ToString());
			return;
		}
		DrawLine(new Vector2(-640, -60), new Vector2(-640 + 1280.0f * boss_values[0] / boss_values[1], -60), Colors.Indigo, 80);
	}
}
