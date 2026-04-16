using Godot;
using System;
using System.Collections.Generic;

public partial class OvenSnakeHead : Node2D
{

	private OvenController oven_controller;
	private PackedScene segment_prefab;
	private int state = 0;

	private List<OvenSnakeSprite> segments = new List<OvenSnakeSprite>();
	private OvenSnakeSprite head;

	/// <summary> How many segments the head has </summary>
	private const int SEGMENT_COUNT = 2;

	private enum HEAD_STATES
	{
		RAISING,
		LOWERING,
		HOVERING,
		SLAM,
		IDLE,
	};

	private HEAD_STATES head_state = HEAD_STATES.IDLE;
	/// <summary> Target position of the slam </summary>
	private Vector2 slam_target;
	/// <summary> Direction of head towards slam </summary>
	private Vector2 slam_direction;

	/// <summary> If the slam has made contact </summary>
	private bool slammed;
	private const float SLAM_RAISE_TIME = 0.2f;
	private const float SLAM_SLAM_TIME = 0.15f;
	private const float SLAM_RECOVER_TIME = 0.2f;

	private float head_timer = 0;

	/// <summary> How high the head is raised </summary>
	private const float HEAD_HEIGHT = 200;
	/// <summary> How long it takes to raise the head </summary>
	private const float RAISE_TIME = 0.7f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		oven_controller = GetParent<OvenController>();
		segment_prefab = GD.Load<PackedScene>("res://Bosses/Oven/OvenSnakeHead/OvenSnakeSprite.tscn");

		for (int i = 0; i < SEGMENT_COUNT; i++)
		{
			var inst = segment_prefab.Instantiate<OvenSnakeSprite>();
			segments.Add(inst);
			this.AddChild(inst);
			inst.Scale = new Vector2(3, 3);
		}

		var head_inst = segment_prefab.Instantiate<OvenSnakeSprite>();
		head = head_inst;
		head_inst.Scale = new Vector2(3.5f, 3.5f);
		this.AddChild(head_inst);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		switch (head_state)
		{
			case HEAD_STATES.IDLE:
				break;
			case HEAD_STATES.RAISING:
				head_timer += (float)delta;
				Set_Head(new Vector2(0, -HEAD_HEIGHT * head_timer / RAISE_TIME));
				if (head_timer > RAISE_TIME)
				{
					head_state = HEAD_STATES.HOVERING;
					head_timer = 0;
				}
				break;
			case HEAD_STATES.LOWERING:
				head_timer += (float)delta;
				Set_Head(new Vector2(0, -HEAD_HEIGHT * (1 - head_timer / RAISE_TIME)));
				if (head_timer > RAISE_TIME)
				{
					head_state = HEAD_STATES.IDLE;
					head_timer = 0;
				}
				break;
			case HEAD_STATES.HOVERING:
				head_timer += (float)delta;
				if (head_timer > 2 * Mathf.Pi) head_timer -= 2 * Mathf.Pi;
				Set_Head(new Vector2(0, -HEAD_HEIGHT) + new Vector2(30 * Mathf.Sin(head_timer), -10 * Mathf.Sin(2 * head_timer)));
				break;
			case HEAD_STATES.SLAM:
				handle_slam((float)delta);
				break;
		}

		if (Input.IsActionJustPressed("ui_one"))
		{
			Raise_Head();
		}
		if (Input.IsActionJustPressed("ui_two"))
		{
			Lower_Head();
		}
		if (Input.IsActionJustPressed("ui_three"))
		{
			Set_Slam(GetGlobalMousePosition());
		}
	}

	/// <summary>
	/// Sets the head position to a given location
	/// </summary>
	/// <param name="head_position"></param>
	public void Set_Head(Vector2 head_position)
	{
		head.Position = head_position;

		for (int i = 1; i < SEGMENT_COUNT; i++)
		{
			if (head_position.Y > 0)
			{
				float ratio = 1 - Mathf.Sqrt(1 - (float)i / SEGMENT_COUNT);
				segments[i].Position = new Vector2(ratio * head_position.X,
				(ratio * ratio) * head_position.Y);
			}
			else
			{
				float ratio = 1 - Mathf.Sqrt(1 - (float)i / SEGMENT_COUNT);
				segments[i].Position = new Vector2(ratio * head_position.X,
				((float)i / SEGMENT_COUNT) * head_position.Y);
			}
		}
	}
	public void Set_Slam(Vector2 position)
	{
		head_timer = 0;
		head_state = HEAD_STATES.SLAM;
		slam_target = position - GlobalPosition;
		slam_direction = (slam_target - HEAD_HEIGHT * Vector2.Up).Normalized();
		slammed = false;
	}

	public void Raise_Head()
	{
		head_timer = 0;
		head_state = HEAD_STATES.RAISING;
		head.Play_Animation("Open");
	}

	public void Open_All()
	{
		head.Play_Animation("Open");
		for (int i = 0; i < SEGMENT_COUNT; i++)
		{
			segments[i].Play_Animation("Open");
		}
	}

	public void Lower_Head()
	{
		head_timer = 0;
		head_state = HEAD_STATES.LOWERING;
		head.Play_Animation("Close");
	}

	/// <summary>
	/// Hurts the snake head for a set amount of damage
	/// </summary>
	/// <param name="damage"></param>
	public void Hurt(int damage)
	{
		oven_controller.Rpc("Hurt_Head", damage);
	}

	/// <summary>
	/// Handles the slam of the oven head
	/// </summary>
	/// <param name="delta"></param>
	private void handle_slam(float delta)
	{
		head_timer += delta;
		if (head_timer < SLAM_RAISE_TIME)
		{
			Set_Head(new Vector2(0, -HEAD_HEIGHT) - slam_direction * 250 * head_timer);
		}
		else if (head_timer < SLAM_RAISE_TIME + SLAM_SLAM_TIME)
		{
			float slam_ratio = (head_timer - SLAM_RAISE_TIME) / SLAM_SLAM_TIME;
			Set_Head(new Vector2(0, -HEAD_HEIGHT) * (1 - slam_ratio) + slam_target * (slam_ratio));
		}
		else if (head_timer < SLAM_RAISE_TIME + SLAM_SLAM_TIME + SLAM_RECOVER_TIME)
		{
			float recover_ratio = (head_timer - SLAM_RAISE_TIME - SLAM_SLAM_TIME) / SLAM_RECOVER_TIME;
			Set_Head(new Vector2(0, -HEAD_HEIGHT) * (recover_ratio) + slam_target * (1 - recover_ratio));
			/* Destroy chocolate */
			if (!slammed)
			{
				slammed = true;
				/* Slam area */
				oven_controller.Slam_Area(slam_target + this.GlobalPosition);
			}
		}
		else
		{
			head_timer = 0;
			head_state = HEAD_STATES.HOVERING;
		}
	}

}
