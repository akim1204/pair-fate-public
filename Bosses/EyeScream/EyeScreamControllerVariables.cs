using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public partial class EyeScreamController : BossController
{
    private string soundtrack = "res://Bosses/EyeScream/SoundEffects/OrganBackground.mp3";
    private SoundPlayer sound_player;
    public const float ROOM_LEFT = 0, ROOM_RIGHT = 2560, ROOM_TOP = 0, ROOM_BOTTOM = 1080;
    public const float ROOM_LEFT2 = 320, ROOM_RIGHT2 = 2240;
    private bool active = false;
    private int phase = 0;
    private bool authority;
    private PlayerBag player_bag = GameManager.Instance.Get_Player_Bag();
    private Random rand = new Random();
    private ScreenEffects screen_effects;
    private IceSphereHandler sphere_handler;
    private IceScreamHead head_handler;
    private ColorRect darkening;
    private BackgroundEyes background_eyes;
    private float unhide_timer = 0;

    private List<Brazier> braziers = new List<Brazier>();
    private int ignite_count = 0;

    private const int BOSS_HP_MAX = 18;
    private int breakpoint_1 = BOSS_HP_MAX / 3 * 2;
    private int breakpoint_2 = BOSS_HP_MAX / 3;
    private int boss_hp = BOSS_HP_MAX;

    /// <summary>
    ///
    ///              VARIABLES FOR ARM ATTACKS
    /// 
    /// </summary>


    /// <summary> Interval between arm attacks </summary>
    private const float ARM_INTERVAL = 8;
    /// <summary> Timer for arm attacks </summary>
    private float arm_timer = 0;

    /// <summary> Most recent arm that did an action</summary>
    private int recent_arm = (int)(GD.Randi() % 2);

    /// <summary>
    /// 
    ///             VARIABLES FOR HEAD ATTACKS
    ///
    /// </summary>
    private const float REGULAR_HEAD_HEIGHT = 50;
    private const float HEAD_INTERVAL = 10;

    private float head_timer = HEAD_INTERVAL;
    private int moves_since_rotate = 0;
    private int moves_since_rain = 0;
    private const float BURN_DURATION = 5;
    private float burn_timer = 0;
    private int div_burn1 = -1;
    private int div_burn2 = -1;
    private int burn_count = 0;
    private bool first_burn = true;
    private const float BURN_INTERVAL = 12;
    private float burn_attack_timer = 0;
    private bool rotate_back = false;
    private float rotate_timer = 0;
    /// <summary>
    /// 
    ///             VARIABLES FOR EYES
    ///
    /// </summary>
    private const float EYE_INTERVAL = 6;

    private float eye_timer = 0;
}