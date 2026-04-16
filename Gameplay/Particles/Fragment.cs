using Godot;
using System;

public partial class Fragment : Node2D
{

	private Polygon2D fragment_shape;
	private const int SHAPE_COUNT = 4;

	/// <summary>
	/// If this fragment is activated
	/// </summary>
	private bool activated = false;

	/// <summary> How many times the fragment bounces </summary>
	private int bounces = 0;

	/// <summary> How much velocity is retained after each bounce </summary>
	private const float BOUNCE_COEF = 0.4f;

	/// <summary> How high the fragment rises </summary>
	private float fragment_height = 0;

	private float timer = 0;
	private float horizontal_velocity = 0;
	private float timer_duration;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (activated)
		{
			timer += (float)delta;
			fragment_shape.Position = new Vector2(fragment_shape.Position.X + (float)delta * horizontal_velocity, -fragment_height + fragment_height * Mathf.Pow(2 * Mathf.Abs(timer / timer_duration - 0.5f), 2));
			if (timer >= timer_duration)
			{
				timer -= timer_duration;

				/* Update bounces */
				bounces -= 1;
				fragment_height *= BOUNCE_COEF;

				if (bounces < 0)
				{
					QueueFree();
				}
			}
		}
	}

	public void Activate(Color fragment_color, float fragment_height, int bounces = 0, float fragment_scale = 1, float timer_duration = 1, float horizontal_velocity = 0)
	{
		/* Set bounces */
		this.bounces = bounces;

		/* Set scale */
		fragment_shape = GetNode<Polygon2D>("Shape" + (GD.Randi() % SHAPE_COUNT + 1).ToString());
		fragment_shape.Visible = true;
		fragment_shape.Scale = new Vector2(fragment_scale, fragment_scale);

		fragment_shape.Color = fragment_color;
		this.fragment_height = fragment_height;
		this.timer_duration = timer_duration;
		this.horizontal_velocity = horizontal_velocity;

		/* Activate */
		activated = true;
	}
}