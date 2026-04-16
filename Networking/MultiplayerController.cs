using Godot;
using System;

public partial class MultiplayerController : Control
{
    [Export]
    private int port = 1234;
    [Export]
    private string address = "";

    private string START_ROOM = "res://Rooms/TutorialRooms/TutorialRoom1/TutorialRoom1.tscn";
    //private string START_ROOM = "res://Rooms/Wing1/JelloRoom/JelloRoom.tscn";

    /* Different displays */
    private Control main_display;
    private Control host_display;
    private Control join_display;
    private Control load_display;

    /* Elements of main display */
    private LineEdit name_input;

    /* Elements of host display */
    private RichTextLabel IP_display;
    private RichTextLabel host_status;
    private RichTextLabel host_name_display;

    /* Elements of join display */
    private LineEdit IP_input;
    private RichTextLabel join_status;
    private RichTextLabel join_name_display;

    private ENetMultiplayerPeer peer;

    private const ENetConnection.CompressionMode COMPRESSION_TYPE = ENetConnection.CompressionMode.RangeCoder;
    public override void _Ready()
    {
        Multiplayer.PeerConnected += PeerConnected;
        Multiplayer.PeerDisconnected += PeerDisconnected;
        Multiplayer.ConnectedToServer += ConnectedToServer;
        Multiplayer.ConnectionFailed += ConnectionFailed;

        /* Getting displays */
        main_display = GetNode<Control>("MainDisplay");
        host_display = GetNode<Control>("HostDisplay");
        join_display = GetNode<Control>("JoinDisplay");
        load_display = GetNode<Control>("LoadDisplay");

        /* Start on main display */
        main_display.Visible = true;
        host_display.Visible = false;
        join_display.Visible = false;
        load_display.Visible = false;

        /* Getting elements */
        name_input = main_display.GetNode<LineEdit>("NameInput");

        IP_display = host_display.GetNode<RichTextLabel>("IPDisplay");
        host_status = host_display.GetNode<RichTextLabel>("HostStatus");
        host_name_display = host_display.GetNode<RichTextLabel>("NameDisplay");

        IP_input = join_display.GetNode<LineEdit>("IPInput");
        join_status = join_display.GetNode<RichTextLabel>("JoinStatus");
        join_name_display = join_display.GetNode<RichTextLabel>("NameDisplay");


    }

    /*
     *                  ------- GENERAL CONNECTIONS ---------------
     */

    /// <summary>
    /// Runs when client fails to connec, runs on client.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void ConnectionFailed()
    {
        GD.Print("CONNECTION FAILED");
    }

    /// <summary>
    /// Runs when the connection is connected, run on client.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    private void ConnectedToServer()
    {
        GD.Print("Connected To Server");

        /* Send player info */
        RpcId(1, "send_player_information", name_input.Text, Multiplayer.GetUniqueId());
    }

    /// <summary>
    /// Runs when a player disconnects, run on all peers.
    /// </summary>
    /// <param name="id"> Id of the player that disconnected</param>
    /// <exception cref="NotImplementedException"></exception>
    private void PeerDisconnected(long id)
    {
        GD.Print("Player Disconnected: " + id.ToString());
        /* TODO: Actual disconnection stuff */
        remove_player_information(id);
    }


    /// <summary>
    /// Runs when a player connects, runs on all peers.
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void PeerConnected(long id)
    {
        GD.Print("Player Connected: " + id.ToString());
    }


    /// <summary>
    /// Closest connection if open.
    /// </summary>
    public void Close_Connection()
    {
        if (peer != null)
        {
            if (peer.Host != null)
            {

                peer.Host.Destroy();
            }
            peer.Close();
            peer = null;

            /* Clear player information */
            NetworkManager.Network_Player_Infos.Clear();
            join_status.Text = "";
            host_status.Text = "";
        }
    }

    private void remove_player_information(long id)
    {
        /* Remove given player info */
        foreach (var item in NetworkManager.Network_Player_Infos)
        {
            if (item.Id == id)
            {
                NetworkManager.Network_Player_Infos.Remove(item);
                break;
            }
        }

        /* Update displays */
        update_displays();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void send_player_information(string name, int id)
    {
        /* If host, blue, otherwise red */
        int player_style = 0;
        if (id == 1)
        {
            player_style = 1;
        }

        /* Create player info */
        NetworkPlayerInfo playerInfo = new NetworkPlayerInfo()
        {
            Name = name,
            Id = id,
            Player_Style = player_style,
        };

        /* Add to network manager */
        if (!NetworkManager.Network_Player_Infos.Contains(playerInfo))
        {
            NetworkManager.Network_Player_Infos.Add(playerInfo);
        }

        /* Agregate to all from host */
        if (Multiplayer.IsServer())
        {
            foreach (var item in NetworkManager.Network_Player_Infos)
            {
                Rpc("send_player_information", item.Name, item.Id);
            }
        }

        /* Update display */
        update_displays();
    }

    /// <summary>
    /// Updates all player displays based on joined players.
    /// </summary>
    private void update_displays()
    {
        String joined_players = "Joined Players:\n";
        foreach (var item in NetworkManager.Network_Player_Infos)
        {
            joined_players += item.Name + "\n";
        }
        host_status.Text = joined_players;
        join_status.Text = joined_players;
    }


    /*
     *              ---------- MAIN DISPLAY -----------------
     */

    public void _on_host_button_down()
    {
        GameManager.Instance.Play_Menu_Sound();
        if (name_input.Text == "")
        {
            GameManager.Instance.Display_Message("Name cannot be empty", 2);
            return;
        }

        /* Create a new multiplayer peer */
        peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(port, 8);
        if (error != Error.Ok)
        {
            GD.Print("Unable to create host: ", error.ToString());
            return;
        }

        /* Set compression */
        peer.Host.Compress(COMPRESSION_TYPE);

        Multiplayer.MultiplayerPeer = peer;
        GD.Print("Waiting For Players!");

        /* Sending player info of host */
        send_player_information(name_input.Text, 1);

        /* Display IP */
        IP_display.Text = "Your IP is: " + get_ip();

        /* Display host display */
        display_host();
    }


    public void _on_join_button_down()
    {
        GameManager.Instance.Play_Menu_Sound();
        if (name_input.Text == "")
        {
            GameManager.Instance.Display_Message("Name cannot be empty", 2);
            return;
        }
        /* Go to join display */
        display_join();
    }

    /// <summary>
    /// Displays the main screen.
    /// </summary>
    private void display_main()
    {
        host_display.Hide();
        main_display.Show();
        join_display.Hide();
        load_display.Hide();
    }

    /// <summary>
    /// Displays the host screen.
    /// </summary>
    private void display_host()
    {
        host_display.Show();
        host_name_display.Text = name_input.Text;
        main_display.Hide();
        join_display.Hide();
        load_display.Hide();
    }

    /// <summary>
    /// Displays the join screen.
    /// </summary>
    private void display_join()
    {
        host_display.Hide();
        main_display.Hide();
        join_display.Show();
        load_display.Hide();
        join_name_display.Text = name_input.Text;
    }

    private void display_load()
    {
        host_display.Hide();
        main_display.Hide();
        join_display.Hide();
        load_display.Show();
    }

    /*
     *                 ------ HOST DISPLAY ---------------
     */

    /// <summary>
    /// Shows the load menu.
    /// </summary>
    public void _on_start_button_down()
    {
        GameManager.Instance.Play_Menu_Sound();
        display_load();
    }

    /// <summary>
    ///  TODO: THIS NEEDS TO BE FIXED< CLOSING PORTS???
    /// </summary>
    private void _on_host_back_button_down()
    {
        GameManager.Instance.Play_Menu_Sound();
        /* Close peer TODO: FIX THIS*/
        peer.Host.Destroy();
        peer = null;

        /* Clear player information */
        NetworkManager.Network_Player_Infos.Clear();
        join_status.Text = "";
        host_status.Text = "";

        /* Return to main */
        display_main();
    }

    /// <summary>
    /// Gets the ip of the current player 
    /// </summary>
    private string get_ip()
    {
        string ip_address = "";
        /* Windows OS */
        if (OS.HasFeature("windows"))
        {
            ip_address = IP.ResolveHostname(OS.GetEnvironment("COMPUTERNAME"), IP.Type.Ipv4);
        }
        /* x11 OS */
        if (OS.HasFeature("x11"))
        {
            ip_address = IP.ResolveHostname(OS.GetEnvironment("HOSTNAME"), IP.Type.Ipv4);
        }
        /* OSX OS */
        if (OS.HasFeature("OSX"))
        {
            ip_address = IP.ResolveHostname(OS.GetEnvironment("HOSTNAME"), IP.Type.Ipv4);
        }
        return ip_address;
    }

    /*
     *                    -------- JOIN DISPLAY ----------
     */
    private void _on_join_back_button_down()
    {
        GameManager.Instance.Play_Menu_Sound();
        /* Close peer */
        Close_Connection();
        /* Return to main */
        display_main();
    }

    private void _on_join_ip_button_down()
    {
        GameManager.Instance.Play_Menu_Sound();
        /* Get IP from input */
        address = IP_input.Text.Trim();

        /* Join a given host */
        peer = new ENetMultiplayerPeer();
        var error = peer.CreateClient(address, port);

        if (error != Error.Ok || address == "")
        {
            GD.Print("Unable to create host: ", error.ToString());
            join_status.Text = "Unable to join, please check IP or connection.";
            return;
        }

        /* Set compression */
        peer.Host.Compress(COMPRESSION_TYPE);
        Multiplayer.MultiplayerPeer = peer;
        GD.Print("Joining Game !");
        join_status.Text = "Joining Game!";
    }

    /*
     *                  ---------- LOAD DISPLAY -------
     */

    private void _on_load_back_button_down()
    {
        GameManager.Instance.Play_Menu_Sound();
        display_host();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void start_game(string level_path)
    {
        /* Only start game if there is at least one player */
        if (NetworkManager.Network_Player_Infos.Count == 0)
        {
            Logger.Instance.Log(Logger.LOG_LEVELS.WARN, "No players are joined.");
            return;
        }
        /* Print out all connected players */
        foreach (var item in NetworkManager.Network_Player_Infos)
        {
            Logger.Instance.Log(Logger.LOG_LEVELS.INFO, item.Name + "'s info known by " + Multiplayer.GetUniqueId());
        }

        /* Maybe hide instead of freeing ?*/
        GameManager.Instance.Load_Level_Synced(level_path, 0);
        /* Also revert to main display */
        display_main();
        Hide();
    }
}
