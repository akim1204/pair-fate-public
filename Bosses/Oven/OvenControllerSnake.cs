using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;

public partial class OvenController : BossController
{

	/// <summary>
	/// Registers a given snake body on the list
	/// </summary>
	public void Register_Body(int body_id, OvenSnakeBody body)
	{
		snake_bodies.Add(body_id, body);
		snake_hps.Add(body_id, SNAKE_SEGMENT_HP);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Hurt_Body(int body_id, int damage)
	{
		/* Check if it exists */
		if (!snake_hps.ContainsKey(body_id))
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Recieved invalid body id");
			return;
		}
		sound_player.Play_Effect("cookie_hurt", -30, (float)rand.NextDouble() / 2 + 0.5f);
		snake_hps[body_id] -= 1;
		if (authority && snake_hps[body_id] <= 0)
		{
			Rpc("Destroy_Body", body_id);
		}
		else
		{
			snake_bodies[body_id].Change_State();
			for (int i = 0; i < 4; i++)
			{
				spawn_cookie_fragment(snake_bodies[body_id].Position + new Vector2(150.0f * (GD.Randf() - 0.5f), -75 + 150.0f * GD.Randf()));
			}
		}
	}

	/// <summary>
	/// Updates which snake body is the head
	/// </summary>
	/// <param name="head_id"></param>
	public void Update_Head(int head_id)
	{
		snake = snake_bodies[head_id];
	}

	/// <summary>
	/// RPC method to destroy a given body
	/// </summary>
	/// <param name="body_id"></param>

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Destroy_Body(int body_id)
	{
		snake_bodies[body_id].Destroy();
		current_segments -= 1;
		canvas_gui.Update_Boss_Health(new float[] { current_segments, SNAKE_SEGMENTS });

		/* Winning */
		if (current_segments <= 0)
		{
			canvas_gui.Is_Boss(false);
			oven_body.Cry();

			/* Disable lava and boss */
			active = false;
			Tween tween = GetTree().CreateTween();
			tween.TweenProperty(original_chocolate, "modulate", new Color(1, 1, 1, 1), 2.5f);
			tween = GetTree().CreateTween();
			tween.TweenProperty(full_lava, "modulate", new Color(1, 1, 1, 0), 2.5f);

			/* Screen shake */
			foreach (PlayerCamera camera in player_bag.GetAllCameras())
			{
				camera.Shake(2.5f, 10, 5);
			}


			var room_detector = GetNode<OvenRoomDetector>("../ExtraEntities/RoomDetector0");
			room_detector.QueueFree();

			foreach (var camera in player_bag.GetAllCameras())
			{
				camera.Bound_Camera(-50, 0, 1970, 2000);
			}
		}
	}

	/// <summary>
	/// Creates the original snake
	/// </summary>
	/// <param name="position"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Create_Snake_Head(Vector2 position)
	{
		/* Remove snake head */
		snake_head.GlobalPosition = new Vector2(0, -200);

		/* Create snake body */
		snake_body_prefab = GD.Load<PackedScene>("res://Bosses/Oven/OvenSnake/OvenSnakeBody.tscn");
		var inst = snake_body_prefab.Instantiate<OvenSnakeBody>();
		int snake_id = inst.Initialize(null, SNAKE_SEGMENTS);
		inst.GlobalPosition = position;
		inst.Set_Controller(this);
		snake = inst;
		snake_bodies.Add(snake_id, inst);
		snake_hps.Add(snake_id, SNAKE_SEGMENT_HP);

		for (int i = SNAKE_SEGMENTS - 1; i > 0; i--)
		{
			inst = snake_body_prefab.Instantiate<OvenSnakeBody>();
			snake_id = inst.Initialize(snake_bodies[i + 1], i);
			inst.GlobalPosition = position;
			inst.Set_Controller(this);
			snake_bodies.Add(snake_id, inst);
			snake_hps.Add(snake_id, SNAKE_SEGMENT_HP);
		}

		head_mode = HEAD_STATES.INACTIVE;
		snake_mode = SNAKE_STATES.BURROWED;
		snake_timer = 4;

		/* Add to world */
		for (int i = SNAKE_SEGMENTS; i > 0; i--)
		{
			GameManager.Instance.Get_World().CallDeferred("add_child", snake_bodies[i]);
		}
		head_location = new Vector2(360 + rand.Next(1200), 300 + rand.Next(840));

		/* Reupdate boss bar */
		canvas_gui.Update_Boss_Health(new float[] { 0, SNAKE_SEGMENTS }, true);
		canvas_gui.Update_Boss_Health(new float[] { SNAKE_SEGMENTS, SNAKE_SEGMENTS });
	}

	/// <summary>
	/// Burrows a snake head
	/// </summary>
	/// <param name="head_id"></param>
	/// <param name="position"></param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void burrow(Vector2 position)
	{
		snake.Burrow(position);
		snake_direction = Vector2.FromAngle(GD.Randf() * 2 * Mathf.Pi);
		snake_position = position;
		sound_player.Play_Effect("crawl", -5, 0.5f);
		choose_player();
	}

	/// <summary>
	/// Sets the eye targets on all players
	/// </summary>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void set_eyes(int head_id, int player_id)
	{
		Player target_player = player_bag.GetPlayer(player_id);
		snake.Set_Eyes(target_player);
	}

	/// <summary>
	/// Choose the target player for a given head
	/// </summary>
	private void choose_player()
	{
		/* Get player bag */
		var ids = player_bag.GetActivePlayerIds();

		if (ids.Count > 0)
		{
			Player target = player_bag.GetPlayer(ids[rand.Next(ids.Count)]);
			snake.Set_Eyes(target);
			snake_target = target;
		}
		else
		{
			snake_target = null;
		}
	}


	private void calculate_next_point()
	{
		Vector2 target_angle;
		/* Make sure theres a target */
		if (snake_target == null)
		{
			target_angle = snake_direction;
		}
		else
		{
			/* Calculate target */
			target_angle = (snake_target.GlobalPosition - snake_position).Normalized();
			//target_angle = (GetGlobalMousePosition() - snake_info.snake_position).Normalized();
		}
		float angle_dif = snake_direction.AngleTo(target_angle);

		/* If close enough */
		if (Mathf.Abs(angle_dif) < SNAKE_CORRECTION_ANGLE)
		{
			snake_direction = target_angle;
		}
		else
		{
			/* Depending on direction */
			if (angle_dif < 0)
			{
				snake_direction = snake_direction.Rotated(-SNAKE_CORRECTION_ANGLE);
			}
			else
			{
				snake_direction = snake_direction.Rotated(SNAKE_CORRECTION_ANGLE);
			}
		}

		/* Calculate new position */
		Vector2 new_position = snake_position + SNAKE_MOVE_DISTANCE * snake_direction;

		/* Bouncing along sudes */
		if (new_position.X < ROOM_LEFT + SNAKE_BOUNCE_DIST || new_position.X > ROOM_RIGHT - SNAKE_BOUNCE_DIST) snake_direction.X *= -1;
		if (new_position.Y < ROOM_TOP + SNAKE_BOUNCE_DIST || new_position.Y > ROOM_BOTTOM - SNAKE_BOUNCE_DIST) snake_direction.Y *= -1;

		snake_position = new_position;

		/* Send to head */
		snake.Rpc("Send_Point", new_position);
	}

	private void calculate_points(Player target_player, int head_id)
	{
		/* Current head 
		if (!snake_heads.ContainsKey(head_id))
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.WARN, "Recieved an invalid head id: " + head_id.ToString());
			return;
		}*/
		OvenSnakeBody head = snake;

		/* Getting snake direction */
		Vector2 snake_direction = target_player.GlobalPosition - head.GlobalPosition;
		snake_direction += (200 + GD.Randf() * 100) * Vector2.FromAngle(GD.Randf() * Mathf.Pi * 2);
		snake_direction = snake_direction.Normalized();

		/* Checking awkward conditions */
		if (snake_direction == Vector2.Zero)
		{
			snake_direction = Vector2.Left;
		}

		Vector2 snake_position = head.GlobalPosition;

		/* Calculate points */
		float snake_dist = SNAKE_MOVE_DISTANCE;
		while (snake_dist > 0)
		{
			/* Calculate collision point */
			float[] w = new float[4];
			if (snake_direction.Y != 0)
			{
				/* Top intersection */
				w[0] = ((ROOM_TOP + SNAKE_BOUNCE_DIST) - snake_position.Y) / snake_direction.Y;
				/* Bottom intersection */
				w[2] = ((ROOM_BOTTOM - SNAKE_BOUNCE_DIST) - snake_position.Y) / snake_direction.Y;
			}
			if (snake_direction.X != 0)
			{
				/* Right intersaction */
				w[1] = ((ROOM_RIGHT - SNAKE_BOUNCE_DIST) - snake_position.X) / snake_direction.X;
				/* Left intersection */
				w[3] = ((ROOM_LEFT + SNAKE_BOUNCE_DIST) - snake_position.X) / snake_direction.X;
			}

			/* Find minimum positive intersection */
			float minW = Mathf.Inf;
			int closest = 0;
			for (int i = 0; i < 4; i++)
			{
				if (w[i] > 0 && w[i] < minW)
				{
					minW = w[i];
					closest = i;
				}
			}

			/* Find intersection */
			Vector2 bounce_point = snake_position + minW * snake_direction;

			/* Partial bounce */
			if (snake_position.DistanceTo(bounce_point) > snake_dist)
			{
				Rpc("Send_Point", head_id, snake_position + snake_dist * snake_direction);
				return;
			}

			/* Update based on closest */
			if (closest == 0)
			{
				bounce_point.Y = ROOM_TOP + SNAKE_BOUNCE_DIST;
				snake_direction.Y *= -1;
			}
			else if (closest == 1)
			{
				bounce_point.X = ROOM_RIGHT - SNAKE_BOUNCE_DIST;
				snake_direction.X *= -1;
			}
			else if (closest == 2)
			{
				bounce_point.Y = ROOM_BOTTOM - SNAKE_BOUNCE_DIST;
				snake_direction.Y *= -1;
			}
			else
			{
				bounce_point.X = ROOM_LEFT + SNAKE_BOUNCE_DIST;
				snake_direction.X *= -1;
			}

			/* Full distance bounce */
			snake_dist -= bounce_point.DistanceTo(snake_position);
			snake_position = bounce_point;
			Rpc("Send_Point", head_id, bounce_point);
		}
	}

	/// <summary>
	/// Sends a point to a given snake
	/// </summary>
	/// <param name="head_id"> Id of the snake </param>
	/// <param name="point"> The point to send </param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	private void Send_Point(Vector2 point)
	{
		snake.Send_Point(point);
	}

}
