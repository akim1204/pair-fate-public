using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class OvenSnakeBody : Node2D
{

	/// <summary> Oven boss controller </summary>
	private OvenController oven_controller;
	Random rand = new Random();

	/// <summary> Prefab for each body segment. </summary>
	private PackedScene body_segment_prefab;
	private PackedScene death_prefab;

	/// <summary> Speed at which the head of the snake travels </summary>
	private const float TRAVEL_SPEED = 450;

	/// <summary> Global position the head of the snake is moving towards. </summary>
	private Vector2 travel_goal;

	/// <summary> Queue of points the object is traveling to. </summary>
	private Queue<Vector2> travel_points = new Queue<Vector2>();

	/*
	 *  Tracking if this is a head or a body segment
	 */
	/// <summary> If this body segment is a head </summary>
	private bool is_head = true;

	/// <summary> Body segment this segment is following, null if this is a head </summary>
	private OvenSnakeBody parent = null;
	/// <summary> Body segment that is following this segment, null if this is a head </summary>
	private OvenSnakeBody child = null;

	/// <summary> The distance at which the body segments follow the head. </summary>
	private float FOLLOW_DISTANCE = 120;

	/// <summary> How many body segments there are </summary>
	private int SEGMENT_COUNT = 0;

	/// <summary> Id of this snake segment </summary>
	private int id;

	/// <summary> The right leg </summary>
	private OvenSnakeLeg leg_right;

	/// <summary> The left leg </summary>
	private OvenSnakeLeg leg_left;
	private bool leg_flipped = false;

	/// <summary> How far from center the footprints are</summary>
	private float LEG_OFFSET = 150;
	private float LEG_LEAD = 125;

	/*
	 *  Tracking and animating eyes
	 */


	/// <summary> Sprite for the head </summary>
	private OvenSnakeSprite head_sprite;

	/// <summary> Offset of the eyes along the body segments. </summary>
	private const float EYE_OFFSET = 30;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Get prefab for body */
		body_segment_prefab = GD.Load<PackedScene>("res://Bosses/Oven/OvenSnake/OvenSnakeBody.tscn");
		death_prefab = GD.Load<PackedScene>("res://Bosses/Oven/OvenSnakeHead/OvenSnakeSpriteDeath.tscn");

		/* Set names */
		this.Name = "OvenSnakeSegment" + SEGMENT_COUNT.ToString();
		this.id = SEGMENT_COUNT;

		/* Get the legs */
		leg_right = GetNode<OvenSnakeLeg>("OvenSnakeLegRight");
		leg_left = GetNode<OvenSnakeLeg>("OvenSnakeLegLeft");

		/* Register to controller */
		//oven_controller.Register_Body(this.id, this);

		this.Visible = false;

		head_sprite = GetNode<OvenSnakeSprite>("OvenSnakeSprite");
		head_sprite.Play_Animation("Open");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		/* Initiate movement from the head */
		if (is_head)
		{
			handle_head((float)delta);
		}
	}

	/// <summary>
	/// Initializes a body segment.
	/// </summary>
	/// <param name="parent"> The parent of the body segment</param>
	/// <param name="segment_count"> How many segments are after this body part.</param>
	/// <returns> Id of this oven snake <>
	public int Initialize(OvenSnakeBody parent, int segment_count)
	{
		if (parent != null)
		{
			/* Set parent */
			this.parent = parent;
			this.parent.child = this;
			/* This is not a head */
			is_head = false;
		}

		SEGMENT_COUNT = segment_count;

		return segment_count;
	}

	/// <summary>
	/// Returns the id of this snake head
	/// </summary>
	/// <returns> The id of the snake </returns>
	public int Get_Id()
	{
		return this.id;
	}

	/// <summary>
	/// Hurts a given snake body 
	/// </summary>
	/// <param name="damage"></param>
	public void Hurt(int damage)
	{
		oven_controller.Rpc("Hurt_Body", this.id, damage);
	}

	public void Change_State()
	{
		head_sprite.Play_Animation("Damage");
	}

	/// <summary>
	/// Sets the controller of this snake body.
	/// </summary>
	/// <param name="controller"> The oven controller </param>
	public void Set_Controller(OvenController controller)
	{
		oven_controller = controller;
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	public void Destroy()
	{

		/* Delete from parent */
		if (parent != null)
		{
			parent.child = this.child;
		}

		/* Activate rest of body if exists */
		if (child != null)
		{
			child.parent = this.parent;
		}

		/* Destroy heads */
		if (is_head)
		{
			//oven_controller.Remove_Head(id);
			if (this.child != null)
			{
				child.Activate_Head();
			}
		}

		/* Create death anim */
		var inst = death_prefab.Instantiate<Node2D>();
		GameManager.Instance.Get_World().CallDeferred("add_child", inst);
		inst.GlobalPosition = this.GlobalPosition;
		QueueFree();
	}

	/// <summary>
	/// Activates this segment as a head.
	/// </summary>
	public void Activate_Head()
	{
		/* Activate head and inform controller */
		is_head = true;
		oven_controller.Update_Head(id);
		parent = null;

		/* Clear queue 
		Clear_Queue();
		Send_Point(this.GlobalPosition);
		*/
	}

	/// <summary>
	/// Sends a goal point down the line of body segments.
	/// </summary>
	/// <param name="point"> The position to add to the travel queue. </param>
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void Send_Point(Vector2 point)
	{
		/* Add to personal queue */
		this.travel_points.Enqueue(point);

		/* Send to child */
		if (this.child != null)
		{
			child.Send_Point(point);
		}
	}

	/// <summary>
	/// Clears a segments queue alongside all of its following segments.
	/// </summary>
	public void Clear_Queue()
	{
		this.travel_points.Clear();
		travel_goal = Vector2.Zero;

		if (this.child != null)
		{
			child.Clear_Queue();
		}
	}

	/// <summary>
	/// Burrows the snake head at a given location
	/// </summary>
	/// <param name="position"></param>
	public void Burrow(Vector2 position)
	{
		/* Place body */
		this.GlobalPosition = position;

		/* Clear queue */
		this.travel_points.Clear();
		travel_goal = Vector2.Zero;

		/* Update legs */
		leg_left.Set_Footprint(GlobalPosition);
		leg_right.Set_Footprint(GlobalPosition);

		/* Propegate */
		if (this.child != null)
		{
			this.child.Burrow(position);
		}
	}

	/// <summary>
	/// Tells a given node and its children nodes to initiate travel.
	/// </summary>
	/// <param name="parent_position"> Position of parent node. </param>
	/// <param name="parent_queue_size"> Size of the parent's queue </param>
	public void Child_Travel(Vector2 parent_position, int parent_queue_size, float delta)
	{
		// TODO: ERROR CHECK THIS
		/* Calculate distance to parent */
		float distance = 0;
		Vector2 walkerPosition = this.GlobalPosition;
		if (travel_points.Count - parent_queue_size > 0)
		{
			distance += walkerPosition.DistanceTo(travel_goal);
			walkerPosition = travel_goal;
		}
		for (int i = 0; i < travel_points.Count - parent_queue_size - 1; i++)
		{
			distance += walkerPosition.DistanceTo(travel_points.ElementAt(i));
			walkerPosition = travel_points.ElementAt(i);
		}
		distance += walkerPosition.DistanceTo(parent_position);

		/* Travel if necessary */
		if (distance > FOLLOW_DISTANCE)
		{
			travel(Mathf.Min(distance - FOLLOW_DISTANCE, 1.25f * delta * TRAVEL_SPEED));
		}
		if (child != null)
		{
			child.Child_Travel(GlobalPosition, travel_points.Count, delta);
		}
	}

	/// <summary>
	/// Sets the target of the eyes to a specific player.
	/// </summary>
	/// <param name="eye_target"> The target of the eyes </param>
	public void Set_Eyes(Player eye_target)
	{
		/* Propagate to children */
		if (this.child != null)
		{
			child.Set_Eyes(eye_target);
		}
	}

	/// <summary>
	/// Handles the pathfinding of the head at each moment.
	/// </summary>
	/// <param name="delta"> The time since the last frame </param>
	private void handle_head(float delta)
	{
		/* Travel */
		if (travel(delta * TRAVEL_SPEED))
		{
			if (child != null)
			{
				child.Child_Travel(GlobalPosition, travel_points.Count, delta);
			}
		}
		else
		{
			if (child != null)
			{
				child.handle_head(delta);
			}
		}
	}

	/// <summary>
	/// Handles moving along the sequence of path points
	/// </summary>
	/// <param name="distance"> The distance to travel along the path </param>
	private bool travel(float distance)
	{
		/* If it has a goal */
		if (travel_goal != Vector2.Zero)
		{
			if (!Visible)
			{
				Visible = true;
				oven_controller.head_reverberation(GlobalPosition, 0, 75);
				Vector2 directional = (travel_goal - GlobalPosition).Normalized();
				leg_left.Set_Footprint(this.GlobalPosition + (GD.Randi() % 25 + LEG_LEAD) * directional
					+ LEG_OFFSET * new Vector2(-directional.Y, directional.X) * (leg_flipped ? -1 : 1));
				leg_right.Set_Footprint(this.GlobalPosition + (GD.Randi() % 25 + LEG_LEAD) * directional
					+ LEG_OFFSET * new Vector2(directional.Y, -directional.X) * (leg_flipped ? -1 : 1));
			}

			/* Update positions of legs */
			if (!leg_left.In_Range)
			{
				Vector2 directional = (travel_goal - GlobalPosition).Normalized();
				leg_left.Set_Footprint(this.GlobalPosition + (GD.Randi() % 25 + LEG_LEAD) * directional
				 + LEG_OFFSET * new Vector2(-directional.Y, directional.X) * (leg_flipped ? -1 : 1));
			}
			if (!leg_right.In_Range)
			{
				Vector2 directional = (travel_goal - GlobalPosition).Normalized();
				leg_right.Set_Footprint(this.GlobalPosition + (GD.Randi() % 25 + LEG_LEAD) * directional
				 + LEG_OFFSET * new Vector2(directional.Y, -directional.X) * (leg_flipped ? -1 : 1));
			}

			/* Move towards target */
			if (GlobalPosition.DistanceTo(travel_goal) < distance) /* If turning */
			{
				/* Calculate 'leftover' distance TODO: repeated calculatiosn? */
				float leftover = distance - GlobalPosition.DistanceTo(travel_goal);

				/* Move to position */
				GlobalPosition = travel_goal;

				/* Get next point in queue */
				if (travel_points.Count > 0)
				{
					next_goal();
				}
				/* Otherwise, stop travel */
				else
				{
					travel_goal = Vector2.Zero;
					Visible = false;
					/* Clear area */
					oven_controller.head_reverberation(GlobalPosition, 0, 75);
					oven_controller.spawn_fragment(GlobalPosition + new Vector2(-100 + rand.Next(200), -100 + rand.Next(200)));
				}

				/* Use up 'leftover' distance TODO: ISSUE WITH CLOSE POINTS */
				GlobalPosition += (travel_goal - GlobalPosition).Normalized() * leftover;
			}
			else /* Straight line travel */
			{
				GlobalPosition += (travel_goal - GlobalPosition).Normalized() * distance;
			}

			return true;
		}
		/* Otherwise, pull next value from queue */
		else
		{
			if (travel_points.Count > 0)
			{
				next_goal();

			}
			return false;
		}
	}

	/// <summary>
	/// Pulls the next travel goal from the list of travel points and repositions
	/// the arms.
	/// </summary>
	private void next_goal()
	{
		if (travel_points.Count == 0)
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.WARN, "Oven Snake Body attempted to pull from empty queue");
			return;
		}
		travel_goal = travel_points.Dequeue();

		/* Reposition arms */
		Vector2 directional = (travel_goal - GlobalPosition).Normalized();
		if (directional.X < 0)
		{
			leg_flipped = true;
			directional *= -1;
			leg_left.Orientation = 1;
			leg_right.Orientation = -1;
		}
		else
		{
			leg_flipped = false;
			leg_left.Orientation = -1;
			leg_right.Orientation = 1;
		}
		leg_right.Position = 30 * new Vector2(directional.Y, -directional.X);
		leg_left.Position = 30 * new Vector2(-directional.Y, directional.X);

		/* Choose first leg points */

		directional = (travel_goal - GlobalPosition).Normalized();
	}
	public void _on_snake_body_hitbox_area_entered(Area2D area)
	{
		// If it is a hurtbox
		if (this.Visible && area.GetType().IsAssignableTo(typeof(PlayerHurtbox)))
		{
			/* Cast to hurtboxenemyparent */
			((PlayerHurtbox)area).Hurt(1);
		}
	}
}
