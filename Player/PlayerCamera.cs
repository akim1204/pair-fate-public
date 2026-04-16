using Godot;
using System;

/// <summary>
/// Camera class for the player camera. Only one should be active at any one time. 
/// </summary>
public partial class PlayerCamera : Camera2D
{
	/// <summary> Possible states the camera can be in </summary>
	public enum Camera_States
	{
		Follow_Player,  // Follow player
		Boss_Pan,       // Pan to boss
	}

	/// <summary> Current state that the camera is in. </summary>
	public Camera_States camera_state = Camera_States.Follow_Player;

	/// <summary> The player this camera is following. </summary>
	Player follow_player;

	/// <summary> The boss that this camera is following. </summary>
	Node2D follow_boss;

	/// <summary> Timer to track how long looking at boss </summary>
	float pan_timer = 0;

	/*
	 * Shake Variables 
	 */
	Random rand = new Random();
	float _duration = 0.0f;

	float _period_in_ms = 0.0f;
	float _amplitude = 0.0f;
	float _timer = 0.0f;
	float _last_shook_timer = 0.0f;
	float _previous_x = 0.0f;
	float _previous_y = 0.0f;
	Vector2 _last_offset = Vector2.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		/* Following player */
		if (camera_state == Camera_States.Follow_Player && follow_player != null)
		{
			this.GlobalPosition = this.GlobalPosition.Lerp(follow_player.GlobalPosition, (float)delta * 3);
		}
		/* Panning to boss */
		if (camera_state == Camera_States.Boss_Pan && follow_boss != null)
		{
			this.GlobalPosition = this.GlobalPosition.Lerp(follow_boss.GlobalPosition, (float)delta * 3);

			/* Track pan timer */
			pan_timer = Mathf.Max(0, pan_timer - (float)delta);
			if (pan_timer <= 0)
			{
				camera_state = Camera_States.Follow_Player;
			}
		}
		Process_Shake(delta);
	}

	/// <summary>
	/// Initiates the camera to be constrained to given bounds.
	/// </summary>
	/// <param name="left_bound"> Left bound of camera. </param>
	/// <param name="top_bound"> Top bound of camera. </param>
	/// <param name="right_bound"> Right bound of camera. </param>
	/// <param name="bottom_bound"> Bottom bound of camera. </param>
	public void Initiate_Camera(Player player, int left_bound, int top_bound, int right_bound, int bottom_bound)
	{
		/* Attach to player */
		this.follow_player = player;

		/* Set camera bounds */
		this.LimitLeft = left_bound;
		this.LimitTop = top_bound;
		this.LimitRight = right_bound;
		this.LimitBottom = bottom_bound;
		this.Enabled = true;

		/* Initially at player position */
		this.GlobalPosition = player.Position;
	}
	/// <summary>
	/// Updates a camera's bounds.
	/// </summary>
	/// <param name="left_bound"> Left bound of camera. </param>
	/// <param name="top_bound"> Top bound of camera. </param>
	/// <param name="right_bound"> Right bound of camera. </param>
	/// <param name="bottom_bound"> Bottom bound of camera. </param>
	public void Bound_Camera(int left_bound, int top_bound, int right_bound, int bottom_bound)
	{
		/* Set camera bounds */
		this.LimitLeft = left_bound;
		this.LimitTop = top_bound;
		this.LimitRight = right_bound;
		this.LimitBottom = bottom_bound;
		this.Enabled = true;
	}



	/// <summary>
	/// Releases the camera to return to following the player.
	/// </summary>
	public void Release()
	{
		camera_state = Camera_States.Follow_Player;
	}

	/// <summary>
	/// Sets the camera to pan to a specfic boss.
	/// </summary>
	/// <param name="boss"> The boss to pan to. </param>
	/// <param name="pan_time"> The time spent focused on the boss. </param>
	public void Set_Boss_Pan(Node2D boss, float pan_time)
	{
		camera_state = Camera_States.Boss_Pan;
		follow_boss = boss;
		pan_timer = pan_time;
	}
	/*		----- Camera Effects -----		*/
	private void Process_Shake(double delta)
	{
		/// Shake when there's shake time remaining.
		if (_timer == 0)
		{
			return;
		}

		/// Only shake on certain frames.
		_last_shook_timer = (float)(_last_shook_timer + delta);

		/// Be mathematically correct in the face of lag; usually only happens once.
		while (_last_shook_timer >= _period_in_ms)
		{
			_last_shook_timer = _last_shook_timer - _period_in_ms;
			/// Lerp between [amplitude] and 0.0 intensity based on remaining shake time.
			float intensity = _amplitude * (1 - ((_duration - _timer) / _duration));
			/// Noise calculation logic from http://jonny.morrill.me/blog/view/14
			float new_x = (float)(1.0 - rand.NextDouble() * 2);
			float x_component = (float)(intensity * (_previous_x + (delta * (new_x - _previous_x))));
			float new_y = (float)(1.0 - rand.NextDouble() * 2);
			float y_component = (float)(intensity * (_previous_y + (delta * (new_y - _previous_y))));
			_previous_x = new_x;
			_previous_y = new_y;
			/// Track how much we've moved the offset, as opposed to other effects.
			Vector2 new_offset = new Vector2(x_component, y_component);
			this.Offset = this.Offset - _last_offset + new_offset;
			_last_offset = new_offset;
		}

		/// Reset the offset when we're done shaking.
		_timer = (float)(_timer - delta);
		if (_timer <= 0)
		{
			_timer = 0f;
			this.Offset = this.Offset - _last_offset;
		}
	}

	public void Shake(float duration, float freq, float ampl)
	{
		/* Only take larger shakes */
		if (duration < this._timer) return;
		this._duration = duration;
		this._timer = duration;
		this._period_in_ms = 1.0f / freq;
		this._amplitude = ampl;

		_previous_x = (float)(1.0 - rand.NextDouble() * 2);
		_previous_y = (float)(1.0 - rand.NextDouble() * 2);
		/// Reset previous offset, if any.
		this.Offset = this.Offset - _last_offset;
		_last_offset = new Vector2(0, 0);
	}

	public void swap_cam(Player player)
	{
		this.follow_player = player;
	}
}
