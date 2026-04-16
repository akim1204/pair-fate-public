using Godot;
using System;
using System.Security;

public partial class ChocolateColumn : Node2D
{
	[Export]
	/// <summary> Size of each chocolate column </summary>
	private int WIDTH = 360;
	[Export]
	private int HEIGHT = 1160;

	/// <summary> Size of each chocolate square </summary>
	private const int GRID_SIZE = 40;

	private const float ORTH_RATIO = 0.5f;

	private Texture2D chocolate_texture;

	/// <summary> Player bag containing all the players </summary>
	private PlayerBag player_bag;

	/// <summary>
	/// Grid used to track chocolate squares
	/// </summary>
	private float[,] chocolate_grid;
	private int grid_width;
	private int grid_height;

	/// <summary> </summary>
	private int vertical_offset = 0;
	private int horizontal_offset = 0;
	private float vertical_push = 0;
	private float horizontal_push = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Create and populate chocolate grid */
		this.grid_width = (WIDTH + GRID_SIZE - 1) / GRID_SIZE;
		this.grid_height = (int)(HEIGHT + GRID_SIZE * ORTH_RATIO - 1) / (int)(GRID_SIZE * ORTH_RATIO);
		this.chocolate_grid = new float[grid_height, grid_width];

		/* Get player bag */
		player_bag = GameManager.Instance.Get_Player_Bag();

		/* Initially chocolate is almost entirely full */
		for (int i = 0; i < grid_width; i++)
		{
			for (int j = 0; j < grid_height; j++)
			{
				chocolate_grid[j, i] = 1;
			}
		}

		/* Load texture */
		chocolate_texture = GD.Load<Texture2D>("res://Bosses/Oven/OvenChocolate/ChocolateSquare.png");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Draw()
	{
		/* Draw chocolate squares */
		for (int j = 0; j < grid_height; j++)
		{
			for (int i = 0; i < grid_width; i++)
			{
				if (chocolate_grid[j, i] == 1)
				{
					DrawTextureRect(chocolate_texture, new Rect2(new Vector2((i + horizontal_offset) % grid_width * GRID_SIZE + horizontal_push,
					(j + vertical_offset) % grid_height * GRID_SIZE * ORTH_RATIO + vertical_push),
									new Vector2(GRID_SIZE, GRID_SIZE * ORTH_RATIO)), false);
				}
			}
		}
	}

	public void Solidify(Vector2[] points)
	{
		float dist = points[1].Length();
		for (float i = 0; i < 1; i += GRID_SIZE / 20 / dist)
		{
			Update_Line(points[0] + i * (points[1] + points[2]), points[0] + i * (points[1] + points[3]));
		}
		QueueRedraw();
	}

	public void Clear()
	{

		for (int i = 0; i < grid_width; i++)
		{
			for (int j = 0; j < grid_height; j++)
			{
				chocolate_grid[j, i] = 0;
			}
		}
	}

	public void Push_Vertical(float distance, bool push_players = false)
	{
		/* Push it vertically */
		vertical_push += distance;

		/* Wrap around push */
		if (vertical_push >= GRID_SIZE * ORTH_RATIO)
		{
			/* Clear row */
			for (int j = 0; j < grid_width; j++)
			{
				chocolate_grid[grid_height - vertical_offset - 1, j] = 0;
			}

			/* Update offset */
			vertical_offset += (int)(vertical_push / (GRID_SIZE * ORTH_RATIO));

			vertical_push %= GRID_SIZE * ORTH_RATIO;
		}

		if (vertical_push < 0)
		{
			/* Clear row */
			for (int j = 0; j < grid_width; j++)
			{
				chocolate_grid[(grid_height - vertical_offset) % grid_height, j] = 0;
			}

			/* Update offset */
			vertical_push += GRID_SIZE * ORTH_RATIO;

			vertical_offset -= 1;
		}

		/* Wrap around offsets */
		if (vertical_offset > grid_height)
		{
			vertical_offset = vertical_offset % grid_height;
		}

		if (vertical_offset < 0)
		{
			vertical_offset += grid_height;
		}
		QueueRedraw();

		/* Move players if necessary */
		if (push_players)
		{
			foreach (Player player in player_bag.GetActivePlayers())
			{
				if (Check_Position(player.GlobalPosition))
				{
					player.GlobalPosition += distance * Vector2.Down;
				}
			}
		}
	}

	public void Push_Horizontal(float distance, bool push_players = false)
	{
		/* Push it horizontally */
		horizontal_push += distance;

		/* Wrap around push */
		if (horizontal_push >= GRID_SIZE)
		{
			for (int j = 0; j < grid_height; j++)
			{
				chocolate_grid[j, grid_width - horizontal_offset - 1] = 0;
			}

			horizontal_offset += (int)(horizontal_push / GRID_SIZE);
			horizontal_push %= GRID_SIZE;
		}

		if (horizontal_push < 0)
		{           /* Clear row */
			for (int j = 0; j < grid_height; j++)
			{
				chocolate_grid[j, (grid_width - horizontal_offset) % grid_width] = 0;
			}
			horizontal_offset -= 1;
			horizontal_push += GRID_SIZE;
		}

		/* Wrap around offsets */
		if (horizontal_offset >= grid_width)
		{
			horizontal_offset = horizontal_offset % grid_width;
		}

		if (horizontal_offset < 0)
		{
			horizontal_offset += grid_width;
		}
		QueueRedraw();

		/* Move players if necessary */
		if (push_players)
		{
			foreach (Player player in player_bag.GetActivePlayers())
			{
				if (Check_Position(player.GlobalPosition))
				{
					player.GlobalPosition += distance * Vector2.Right;
				}
			}
		}
	}


	/// <summary>
	/// Check a given position if its on the chocolate
	/// </summary>
	/// <param name="position"> Position to check </param>
	public bool Check_Position(Vector2 position)
	{
		/* Find offset */
		float v_offset = position.Y - GlobalPosition.Y - vertical_push;
		float h_offset = position.X - GlobalPosition.X - horizontal_push;

		int v_position = Mathf.FloorToInt(v_offset / (GRID_SIZE * ORTH_RATIO));
		int h_position = Mathf.FloorToInt(h_offset / GRID_SIZE);

		if (v_position >= grid_height || h_position >= grid_width || v_position < 0 || h_position < 0)
		{
			return false;
		}

		/* Track around */
		v_position -= vertical_offset;
		v_position = v_position < 0 ? v_position + grid_height : v_position;
		h_position -= horizontal_offset;
		h_position = h_position < 0 ? h_position + grid_width : h_position;
		/* Return position */
		return chocolate_grid[v_position, h_position] > 0;
	}

	/// <summary>
	/// Updates a specific point within the grid
	/// </summary>
	/// <param name=""></param>
	/// <param name=""></param>
	private void update(float x0, float y0, bool add)
	{
		y0 += vertical_offset / GRID_SIZE;
		if (x0 >= 0 && x0 < grid_width && y0 >= 0 && y0 < grid_height)
		{
			if (add)
			{
				chocolate_grid[Mathf.FloorToInt(y0 + grid_height - vertical_offset) % grid_height, Mathf.FloorToInt(x0 + grid_width - horizontal_offset) % grid_width] = 1;
			}
			else
			{

				chocolate_grid[Mathf.FloorToInt(y0 + grid_height - vertical_offset) % grid_height, Mathf.FloorToInt(x0 + grid_width - horizontal_offset) % grid_width] = 0;
			}
		}
	}

	/* Bresenham's algorithm */
	private void line_low(float x0, float y0, float x1, float y1, bool add)
	{
		/* Deltas */
		float dx = x1 - x0;
		float dy = y1 - y0;
		float yi = Mathf.Sign(dy);
		dy = Mathf.Abs(dy);

		/* Initial Values */
		float D = (2 * dy) - dx;
		float y = y0;

		/* Fill in line */
		for (float x = x0; x <= x1; x++)
		{
			update(x, y, add);
			if (D > 0)
			{
				y += yi;
				D += 2 * (dy - dx);
			}
			else
			{
				D += 2 * dy;
			}
		}
	}

	private void line_high(float x0, float y0, float x1, float y1, bool add)
	{
		/* Deltas */
		float dx = x1 - x0;
		float dy = y1 - y0;
		float xi = 1;
		if (dx < 0)
		{
			xi = -1;
		}
		dx = Mathf.Abs(dx);

		/* Initial Values */
		float D = (2 * dx) - dy;
		float x = x0;

		/* Fill in line */
		for (float y = y0; y <= y1; y++)
		{
			update(x, y, add);
			if (D > 0)
			{
				x += xi;
				D += 2 * (dx - dy);
			}
			else
			{
				D += 2 * dx;
			}
		}
	}

	/// <summary>
	/// Updates a given line along the chocolate platform
	/// </summary>
	/// <param name="begin"> Beginning of line </param>
	/// <param name="end"> End of line </param>
	/// <param name="add"> Whether it is adding or removing chocolate </param>
	public void Update_Line(Vector2 begin, Vector2 end, bool add = true)
	{
		/* Map begin and end to be within the struct */
		begin -= this.GlobalPosition;
		end -= this.GlobalPosition;
		begin.Y /= ORTH_RATIO;
		end.Y /= ORTH_RATIO;

		end /= GRID_SIZE;
		begin /= GRID_SIZE;

		if (Mathf.Abs(end.Y - begin.Y) < Mathf.Abs(end.X - begin.X))
		{
			if (begin.X > end.X)
			{
				line_low(end.X, end.Y, begin.X, begin.Y, add);
			}
			else
			{
				line_low(begin.X, begin.Y, end.X, end.Y, add);

			}
		}
		else
		{
			if (begin.Y > end.Y)
			{
				line_high(end.X, end.Y, begin.X, begin.Y, add);
			}
			else
			{
				line_high(begin.X, begin.Y, end.X, end.Y, add);
			}
		}
		QueueRedraw();
	}

	/// <summary>
	/// Clears a given line of the chocolate column
	/// </summary>
	/// <param name="line">Line, not accounting for rotation</param>
	public void Clear_Line(int line, int end_line = -1)
	{
		for (int i = 0; i < grid_width; i++)
		{
			if (end_line >= 0)
			{
				for (int j = line; j < end_line; j++)
				{
					chocolate_grid[(j + grid_height - vertical_offset) % grid_height, i] = 0;
				}
			}
			else
			{
				chocolate_grid[(line + grid_height - vertical_offset) % grid_height, i] = 0;
			}
		}
		QueueRedraw();
	}
}
