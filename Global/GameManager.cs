using Godot;
using System;

public partial class GameManager : Node
{


    /// <summary> The shared instance of the game manager. </summary>
	public static GameManager Instance;

    /// <summary> Autoload bag used to contain all items. </summary>
	private ItemBag item_bag;

    /// <summary> Autoload bag used to contain all players. </summary>
	private PlayerBag player_bag;

    /// <summary> Autoloaded scene used to manage background music. </summary>
    private MusicManager music_manager;

    /// <summary> Global script used to manage effect sounds. </summary>
    private SoundManager sound_manager;


    /// <summary> Node layer that contains the UI. </summary>
    private CanvasLayer UI_layer;

    /// <summary> Node layer that contains the world and characters. </summary>
    private Node2D world_layer;

    /// <summary> Object that handles loading scenes. </summary>
    private PackedScene scene_loader_prefab;

    /// <summary> Main menu scene.</summary>
    private MainMenu main_menu;

    /// <summary> Network menu scene. </summary>
    private MultiplayerController network_menu;

    /// <summary> Overlay menu scene. </summary>
    private OverlayMenu overlay_menu;

    /// <summary> Overlay death menu. </summary>
    private DeathMenu death_menu;

    /// <summary> Overlay options menu. </summary>
    private OptionsMenu options_menu;

    /// <summary> Overlay pause menu </summary>
    private PauseMenu pause_menu;

    /// <summary> If the user is currently in game. </summary>
    private bool in_game = false;

    /// <summary> If this instance is the host computer. </summary>
    private bool is_host = false;

    /// <summary> If the screen is rotated </summary>
    private bool rotated = false;

    /// <summary>
    /// Which spawn point the world manager should spawn the players at.
    /// </summary>
    private int spawn_index = 0;

    public override void _Ready()
    {
        this.item_bag = GetNode<ItemBag>("/root/ItemBag");
        this.player_bag = GetNode<PlayerBag>("/root/PlayerBag");
        this.music_manager = GetNode<MusicManager>("/root/MusicManager");
        this.sound_manager = GetNode<SoundManager>("/root/SoundManager");

        this.UI_layer = GetTree().Root.GetNode<Node2D>("GameLayer").GetNode<CanvasLayer>("UILayer");
        this.world_layer = GetTree().Root.GetNode<Node2D>("GameLayer").GetNode<Node2D>("WorldLayer");

        this.main_menu = UI_layer.GetNode<MainMenu>("MainMenu");
        this.network_menu = UI_layer.GetNode<MultiplayerController>("NetworkControl");
        this.overlay_menu = UI_layer.GetNode<OverlayMenu>("OverlayMenu");
        this.death_menu = UI_layer.GetNode<DeathMenu>("DeathMenu");
        this.options_menu = UI_layer.GetNode<OptionsMenu>("OptionsMenu");
        this.pause_menu = UI_layer.GetNode<PauseMenu>("PauseMenu");

        this.scene_loader_prefab = GD.Load<PackedScene>("res://UI/SceneLoading/LoadingScreen.tscn");

        Instance = this;
        Logger.Instance.Log(Logger.LOG_LEVELS.DEBUG, "GameManager initialized");
        is_host = Instance.Multiplayer.GetUniqueId() == 1;


        /* Play initial menu music */
        music_manager.Get_Music_Player();
        Play_Music("res://Sound/BackgroundTracks/MainMenu.mp3");
    }

    /// <summary>
    /// Called to reset the bags before transfering rooms.
    /// </summary>
    public void Reset_Bags()
    {
        if (this.item_bag != null)
        {
            this.item_bag.ClearItems();
            this.item_bag.ClearInteractables();
        }
        if (this.player_bag is PlayerBag)
        {
            this.player_bag.ClearPlayers();
        }
    }

    /// <summary>
    /// Plays background music from the given path.
    /// </summary>
    /// <param name="music_path"> Path to the music file. </param>
    public void Play_Music(string music_path)
    {
        music_manager.Play_Music(music_path);
    }

    public SoundManager Sound_Manager()
    {
        return this.sound_manager;
    }

    /// <summary>
    /// Plays a sound for clicking a button in menu.
    /// </summary>
    public void Play_Menu_Sound()
    {
        AudioStream click_sound = GD.Load<AudioStream>("res://Sound/GeneralEffects/ItemPickup.mp3");
        this.sound_manager.Play_Sound_Static(click_sound, -10);
    }

    /// <summary>
    /// Clears the current world of all scenes and players.
    /// </summary>
    public void Clear_World()
    {
        /* Iterate through all children */
        foreach (Node child in world_layer.GetChildren())
        {
            /* Remove children and free */
            world_layer.RemoveChild(child);
            child.QueueFree();
        }
    }

    /// <summary>
    /// Returns if the given game instance is the host or not.
    /// </summary>
    /// <returns></returns>
    public bool Is_Host()
    {
        return is_host;
    }

    /// <summary>
    /// Initiates loading a specific level. Initiated on one user's screen and then
    /// reflected on both.
    /// </summary>
    /// <param name="scene_path"> Path to the scene being loaded. </param>
    /// <param name="scene_path"> Spawn index to be used in the next room. </param>
    public void Load_Level(string scene_path, int spawn_index)
    {
        Rpc("Load_Level_Synced", scene_path, spawn_index);
    }

    /// <summary>
    /// Initiates loading a specific level on each user's device.
    /// </summary>
    /// <param name="scene_path"> Path to the scene being loaded. </param>
    /// <param name="scene_path"> Spawn index to be used in the next room. </param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void Load_Level_Synced(string scene_path, int spawn_index)
    {
        // TODO: FIX
        death_menu.Hide();
        Logger.Instance.Log(Logger.LOG_LEVELS.INFO, "Game Manager trying to load game at path " + scene_path);
        /* Initiate scene loading and display on UI*/
        LoadingScreen scene_loader = scene_loader_prefab.Instantiate<LoadingScreen>();
        UI_layer.AddChild(scene_loader);

        /* Clear current world */
        Clear_World(); // TODO: Repeated??

        /* Store spawn index */
        this.spawn_index = spawn_index;

        /* Initiate scene loading process */
        scene_loader.Load_Level(scene_path);

        /* Store most recently loaded */
        death_menu.Set_Restart_Point(scene_path, spawn_index);

        /// TODO: FIX THIS
        in_game = true;
    }

    /// <summary>
    /// Returns the current spawn index.
    /// </summary>
    /// <returns> Integer representing the spawn index. </returns>
    public int Get_Spawn_Index()
    {
        return this.spawn_index;
    }

    /// <summary>
    /// Adds an instantaited node to the world layer, assumes the world
    /// layer has already been cleared.
    /// </summary>
    /// <param name="scene"> The node to add. </param>
    public void Add_To_World_Layer(Node scene)
    {
        world_layer.AddChild(scene);
    }

    /// <summary>
    /// Returns the current world layer objects exist in.
    /// </summary>
    /// <returns> Node2D that is the world layer. </returns>
    public Node Get_World()
    {
        return GetTree().Root.GetNode("GameLayer").GetNode("WorldLayer").GetNode("World");
    }

    /// <summary>
    /// Returns the player bag.
    /// </summary>
    /// <returns></returns>
    public PlayerBag Get_Player_Bag()
    {
        return this.player_bag;
    }

    /// <summary>
    /// Displays a given message for a set amount of time for all users if
    /// initiated on only one.
    /// </summary>
    /// <param name="message"> The message to display. </param>
    /// <param name="duration"> The duration of the message. </param>
    public void Display_Message_All(string message, float duration, bool centered = false)
    {
        Rpc("Display_Message", message, duration, centered);
    }

    /// <summary>
    /// Displays a given message for a set amount of time.
    /// </summary>
    /// <param name="message"> The message to display. </param>
    /// <param name="duration"> The duration of the message. </param>
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void Display_Message(string message, float duration, bool centered = false)
    {
        overlay_menu.Display_Message(message, duration, centered);
    }


    /// <summary>
    /// Displays a given sprite for a set amount of time.
    /// </summary>
    /// <param name="texture"> The sprite texture to display. </param>
    /// <param name="duration"> The duration of the sprite. </param>
    public void Display_Sprite(Texture2D texture, float duration, Vector2 position)
    {
        overlay_menu.Display_Sprite(texture, duration, position);
    }

    /// <summary>
    /// Returns the current door manager.
    /// </summary>
    /// <returns>The door manager, or null if one does not exist. </returns>
    public DoorManager Get_Door_Manager()
    {
        return Get_World().GetNodeOrNull<DoorManager>("DoorManager");
    }

    /// <summary>
    /// Returns the current dialogue manager.
    /// </summary>
    /// <returns>The door manager, or null if one does not exist. </returns>
    public DialogueManager Get_Dialogue_Manager()
    {
        return Get_World().GetNodeOrNull<DialogueManager>("DialogueManager");
    }

    /// <summary>
    /// Shows options menu
    /// </summary>
    public void Show_Options(bool shown = true)
    {
        if (shown)
        {
            options_menu.Show();
        }
        else
        {
            options_menu.Hide();
        }
    }

    /// <summary>
    /// Access the death menu.
    /// </summary>
    /// <returns></returns>
    public DeathMenu Get_Death_Menu()
    {
        return this.death_menu;
    }

    /// <summary>
    /// Shows the death menu;
    /// </summary>
    public void Show_Death()
    {
        death_menu.Show_Death();
    }

    /// <summary>
    /// Currently overly harsh instant return to menu. Doesn't work when networked.
    /// </summary>
    public void Return_To_Menu()
    {
        Rpc("RTM");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
    public void RTM()
    {
        Clear_World();
        main_menu.Show();
        pause_menu.Hide();
        // TODO: FIX THIS!!!
        Display_Message("Game was closed", 2);
        network_menu.CallDeferred("Close_Connection");
        network_menu.Hide();
        in_game = false;
        Play_Music("res://Sound/BackgroundTracks/MainMenu.mp3");
    }

    /// <summary> Show's the network menu. </summary>
    public void Show_Network()
    {
        main_menu.Hide();
        network_menu.Show();
        in_game = false;
    }

    /// <summary>
    /// If currently in game.
    /// </summary>
    /// <returns></returns>
    public bool In_Game()
    {
        return this.in_game;
    }

    /// <summary>
    /// Sets if the screen is rotated or not.
    /// </summary>
    public void Set_Rotated(bool state)
    {
        rotated = state;
    }

    /// <summary>
    /// Returns if the screen is rotated
    /// </summary>
    public bool Get_Rotated()
    {
        return this.rotated;
    }
}