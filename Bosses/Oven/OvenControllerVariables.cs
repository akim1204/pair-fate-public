using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;

public partial class OvenController : BossController
{

	/// <summary> Boundaries of room </summary>
	private const float ROOM_LEFT = 0;
	private const float ROOM_RIGHT = 1920;
	private const float ROOM_BOTTOM = 1280;
	private const float ROOM_TOP = 200;

	private Vector2 ROOM_CENTER = new Vector2(960, 740);

	/// <summary> If this controller is the host. </summary>
	private bool authority;

	/// <summary> If the boss fight has started </summary>
	private bool active = false;

	/// <summary> Bag of all players </summary>
	private PlayerBag player_bag;

	/// <summary> Random number generator </summary>
	private Random rand = new Random();

	/// <summary> Base HP of the boss </summary>
	private const int BOSS_HP_MAX = 4;
	private int Boss_HP = BOSS_HP_MAX;

	/* Cinematic variables */
	private TextureRect original_chocolate;

	private TextureRect full_lava;
	/*
	 * Oven body variables
	 */
	private PackedScene oven_body_prefab;

	private OvenBody oven_body;

	/* 
	 *Oven chocolate variables
	 */
	private const float PLATFORM_PULL_TIME = 3;
	private const float PLATFORM_REST_TIME = 5;
	private int platform_mode = 0;
	private float platform_timer = PLATFORM_REST_TIME;

	/// <summary> Prefab used for the chocolate platforms </summary>
	private PackedScene oven_chocolate_prefab;

	/// <summary> Number of platforms to create </summary>
	private const int PLATFORM_COUNT = 4;

	/// <summary> Dictionary of oven platforms </summary>
	private Dictionary<int, ChocolateColumn> platforms = new Dictionary<int, ChocolateColumn>();

	private Dictionary<int, float> player_ignition = new Dictionary<int, float>();

	/// <summary> How long a player can be in the molten lava </summary>
	private const float IGNITION_CAP = 1.5f;

	/// <summary> How fast ignition decreases. </summary>
	private const float REDUCTION_COEF = 3;

	/// <summary> How fast ignition rises in the center </summary>
	private const float CENTER_COEF = 5;

	/// <summary> Queue of platforms to choose which platform to pull</summary>
	private List<int> platform_choices = new List<int>();

	/// <summary>
	/// The current platform.
	/// </summary>
	private int platform_current = -1;

	/// <summary>
	/// List of vertical boundaries between platforms for tracking 
	/// </summary>
	private int[] platform_boundaries = { 80, 460, 840, 1080, 1460, 1840 };

	/* 
	 * Snake head variables
	 */

	private enum HEAD_STATES
	{
		HIDING, /* Currently not visible */
		SHOWN, /* Currently out */
		SHOWING, /* Appearing from ground */
		INACTIVE, /* Not doing anything */
		DYING, /* In the process of dying */
	};
	/// <summary> The current head mode </summary>
	private HEAD_STATES head_mode = HEAD_STATES.HIDING;
	/// <summary> How long the head is shown </summary>
	private const float HEAD_SHOWN_TIME = 6f;
	private const float HEAD_SLAM_INTERVAL = 0.8f;
	private const float HEAD_SLAM_INITIAL = 1.5f;
	private int slam_count;
	/// <summary> How long it takes for the head to show </summary>
	//private const float HEAD_SHOWING_TIME = 2;
	/// <summary> How much around the head is cleared </summary>
	private const float HEAD_CLEAR_RADIUS = 125;
	/// <summary> How long the head is hidden </summary>
	private const float HEAD_HIDE_TIME = 4;
	/// <summary> Main timer for the head state</summary>
	private float head_timer = 0;
	/// <summary> Secondary timer for internal state actions </summary>
	private float head_secondary_timer = 0;
	/// <summary> Interval between fragment spawns </summary>
	private const float FRAGMENT_INTERVAL = 0.3f;
	/// <summary> Interval between showing reverberations </summary>
	private const float SHOWING_INTERVAL = 0.1f;
	/// <summary> Number of showing reverberations </summary>
	private const int SHOWING_DIVISIONS = 4;
	/// <summary> What reverberation is on </summary>
	private int reverberation_count = 0;
	/// <summary> Intented location of the head </summary>
	private Vector2 head_location = Vector2.Zero;
	/// <summary> Direction the snake head sprays </summary>
	private Vector2 spray_direction = Vector2.Zero;
	private Vector2 spray_position = Vector2.Zero;
	/// <summary> Prefabs </summary>
	private PackedScene snake_head_prefab;
	private PackedScene fragment_prefab;

	private OvenSnakeHead snake_head;

	/*
	 * Oven snake variables
	 */
	public enum SNAKE_STATES
	{
		INACTIVE, /* Not yet in the current state */
		BURROWED, /* Currently not visible */
		CRAWLING, /* Crawling around */
	};
	/// <summary> Timer of the snake </summary>
	//private float snake_timer = 0;
	//private float snake_secondary_timer = 0;
	/// <summary> Prefab for the snake body </summary>
	private PackedScene snake_body_prefab;

	/// <summary> How many segments the snake should have. </summary>
	private const int SNAKE_SEGMENTS = 12;
	private int current_segments = SNAKE_SEGMENTS;

	private const int SNAKE_SEGMENT_HP = 2;
	private const float SNAKE_CRAWL_TIME = 3.5f;
	private const float SNAKE_BURROW_TIME = 9;
	private const float SNAKE_RECALC_DELAY = 0.19f;

	/// <summary> Dictionary mapping of integers to snake bodies
	/// Integer id is assigned based on id of head. </summary>	/// <summary>The corresponding jello.</summary>
	public OvenSnakeBody snake;
	/// <summary> The direction the snake is moving in </summary>
	public Vector2 snake_direction;
	public Vector2 snake_position;
	/// <summary> Target of this snake </summary>
	public Player snake_target;
	public float snake_timer = 0;
	public float snake_secondary_timer = 0;
	public SNAKE_STATES snake_mode = SNAKE_STATES.INACTIVE;

	private Dictionary<int, OvenSnakeBody> snake_bodies = new Dictionary<int, OvenSnakeBody>();
	private Dictionary<int, int> snake_hps = new Dictionary<int, int>();

	/// <summary> How far the snake moves each time </summary>
	private const float SNAKE_MOVE_DISTANCE = 60;
	/// <summary> How much the snake corrects at each segment </summary>
	private const float SNAKE_CORRECTION_ANGLE = Mathf.Pi / 20;

	/// <summary> The bounce ratio of the snake </summary>
	private const float SNAKE_BOUNCE_DIST = 50;
}
