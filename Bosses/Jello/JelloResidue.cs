using Godot;
using System;
using System.Text.Json.Serialization.Metadata;

public partial class JelloResidue : Node2D
{

    /*
     * Constants for the size of the boss room
     */
    private const int ROOM_LEFT = 0;
    private const int ROOM_RIGHT = 2250;
    private const int ROOM_TOP = 150;
    private const int ROOM_BOTTOM = 1275;
    /// <summary>
    /// Witdh of each grid element
    /// </summary>
    public const int GRID_SIZE = 25;

    private bool debug = false;

    /// <summary>
    /// Grid used to track residues
    /// </summary>
    private float[,] residue_grid;
    private int residue_width;
    private int residue_height;

    /// <summary>
    /// Color of the residue
    /// </summary>
    private Color RESIDUE_COLOR = new Color(255, 0, 0, 0.4f);

    /// <summary>
    ///  Whether there are undisplayed updates to the grid
    /// </summary>
    private bool updated = false;

    public override void _Ready()
    {
        this.residue_width = (ROOM_RIGHT - ROOM_LEFT) / GRID_SIZE + 1;
        this.residue_height = (ROOM_BOTTOM - ROOM_TOP) / (GRID_SIZE * 2 / 5) + 1;
        this.residue_grid = new float[residue_height, residue_width];
    }

    public override void _Process(double delta)
    {
        /* Only redraw if something changed */
        if (this.updated)
        {
            QueueRedraw();
        }

        /*
        if (Input.IsActionJustPressed("ui_three")) {
            this.residue_grid = new float[residue_height, residue_width];
        }
        */

        /* Reduce residue grid */
        for (int i = 0; i < residue_width; i++)
        {
            for (int j = 0; j < residue_height; j++)
            {
                residue_grid[j, i] = Mathf.Max(residue_grid[j, i] - (float)delta, 0);
            }
        }
    }

    public override void _Draw()
    {
        if (debug)
        {
            /* Drawing grid */
            for (int i = ROOM_TOP; i <= ROOM_BOTTOM; i += GRID_SIZE * 2 / 5)
            {
                DrawLine(new Vector2(ROOM_LEFT, i), new Vector2(ROOM_RIGHT, i), Colors.Black, 1.0f);
            }
            for (int j = ROOM_LEFT; j <= ROOM_RIGHT; j += GRID_SIZE)
            {
                DrawLine(new Vector2(j, ROOM_TOP), new Vector2(j, ROOM_BOTTOM), Colors.Black, 1.0f);
            }
        }

        /* Displaying slime (TODO: somehow have this only update textures that change, not the entire grid?*/
        for (int i = 0; i < residue_height; i++)
        {
            for (int j = 0; j < residue_width; j++)
            {
                if (residue_grid[i, j] > 0)
                {
                    DrawRect(new Rect2(j * GRID_SIZE + ROOM_LEFT, i * GRID_SIZE + ROOM_TOP, GRID_SIZE, GRID_SIZE), new Color(RESIDUE_COLOR.R, RESIDUE_COLOR.G, RESIDUE_COLOR.B,
                    Mathf.Min(0.5f, residue_grid[i, j] / 5)));
                }
            }
        }
    }

    /// <summary>
    /// Updates the given position on the grid to have or not have residue
    /// </summary>
    /// <param name="x"> Position's X val </param>
    /// <param name="y"> Position's Y val </param>
    /// <param name="status"> Whether to add or remove residue </param>
    public void Update_Grid(float x, float y, float status)
    {
        /* Check within bounds */
        if (x < ROOM_LEFT || x >= ROOM_RIGHT || y < ROOM_TOP || y >= ROOM_BOTTOM)
        {
            //Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Invalid call to Update_Grid in JelloResidue.cs");
            return;
        }

        int y_pos = (int)((y - ROOM_TOP) / (GRID_SIZE));
        int x_pos = (int)((x - ROOM_LEFT) / GRID_SIZE);

        residue_grid[y_pos, x_pos] = status;
        /* Indicate grid must be redrawn */
        updated = true;
    }

    /// <summary>
    /// Returns the residue of a given position.
    /// </summary>
    /// <param name="position">The position to check. </param>
    public float Get_Residue(Vector2 position)
    {
        /* Check within bounds */
        if (position.X < ROOM_LEFT || position.X >= ROOM_RIGHT || position.Y < ROOM_TOP || position.Y >= ROOM_BOTTOM)
        {
            //Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Invalid call to Update_Grid in JelloResidue.cs");
            return 0;
        }

        /* Calculate position*/
        int y_pos = (int)((position.Y - ROOM_TOP) / (GRID_SIZE));
        int x_pos = (int)((position.X - ROOM_LEFT) / GRID_SIZE);

        /* Return residue */
        return residue_grid[y_pos, x_pos];

    }

    /// <summary>
    /// Updates all the grid positions intersecting a given rectangle parallel to the axes.
    /// </summary>
    /// <param name="top_left">Top left corner of the rectangle.</param>
    /// <param name="bottom_right"> Bottom right corner of the rectangle</param>
    /// <param name="status"> Whether to add or remove residue</param>
    public void Update_Grid_Rect(Vector2 top_left, Vector2 bottom_right, float status)
    {
        /* Iterate through each point within the rectangle */
        for (float i = top_left.X; i <= bottom_right.X; i += GRID_SIZE)
        {
            for (float j = top_left.Y; j <= bottom_right.Y; j += GRID_SIZE * 2 / 5)
            {
                Update_Grid(i, j, status);
            }
        }

        /* Iterate through far edges */
        for (float i = top_left.X; i <= bottom_right.X; i += GRID_SIZE)
        {
            Update_Grid(i, bottom_right.Y, status);
        }

        for (float j = top_left.Y; j <= bottom_right.Y; j += GRID_SIZE * 2 / 5)
        {
            Update_Grid(bottom_right.X, j, status);
        }

        /* Update far corner */
        Update_Grid(bottom_right.X, bottom_right.Y, status);
    }

    /// <summary>
    /// Updates all the grid positions intersecting with a given line
    /// Implementation of Bresenham's
    /// </summary>
    /// <param name="start"> Start of line </param>
    /// <param name="end"> End of line </param>
    /// <param name="status"> Whether to add or remove residue </param>
    public void Update_Grid_Line(Vector2 start, Vector2 end, float status)
    {
        if (Mathf.Abs(end.Y - start.Y) < Mathf.Abs(end.X - start.X))
        {
            if (start.X > end.X)
            {
                update_grid_line_low(end, start, status);
            }
            else
            {
                update_grid_line_low(start, end, status);
            }
        }
        else
        {
            if (start.Y > end.Y)
            {
                update_grid_line_high(end, start, status);
            }
            else
            {
                update_grid_line_high(start, end, status);
            }
        }
    }

    private void update_grid_line_low(Vector2 start, Vector2 end, float status)
    {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float yi = GRID_SIZE / 4;
        if (dy < 0)
        {
            yi = -GRID_SIZE / 4;
            dy = -dy;
        }

        float D = (2 * dy) - dx;
        float y = start.Y;

        for (float x = start.X; x <= end.X; x += GRID_SIZE / 4)
        {
            Update_Grid(x, y, status);
            if (D > 0)
            {
                y = y + yi;
                D = D + (2 * (dy - dx));
            }
            else
            {
                D = D + 2 * dy;
            }
        }
    }

    private void update_grid_line_high(Vector2 start, Vector2 end, float status)
    {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;
        float xi = GRID_SIZE / 4;
        if (dx < 0)
        {
            xi = -GRID_SIZE / 4;
            dx = -dx;
        }

        float D = (2 * dx) - dy;
        float x = start.X;

        for (float y = start.Y; y <= end.Y; y += GRID_SIZE / 4)
        {
            Update_Grid(x, y, status);
            if (D > 0)
            {
                x = x + xi;
                D = D + (2 * (dx - dy));
            }
            else
            {
                D = D + 2 * dx;
            }
        }
    }

}
