using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

public partial class JelloNetworked : Node2D
{

    /// <summary> Left boundary of the boss room. </summary>
    private const float ROOM_LEFT = 0;
    /// <summary> Right boundary of the boss room. </summary>
    private const float ROOM_RIGHT = 2250;
    /// <summary> Top boundary of the boss room. </summary>
    private const float ROOM_TOP = 150;
    /// <summary> Bottom boundary of the boss room. </summary>
    private const float ROOM_BOTTOM = 1275;

    /// <summary> Controller of this jello </summary>
    private JelloControllerNetworked jello_controller;

    /// <summary> Id that is assigned to this jello. </summary>
    private int jello_id;

    /// <summary> Bag of all players </summary>
    private PlayerBag player_bag;
    /// <summary> Bag of all items </summary>
    private ItemBag item_bag;

    /// <summary> Node representing world space. </summary>
    private Node WORLD;

    /// <summary> Prefab used for the jello eye </summary>
    private PackedScene jello_eye_prefab;

    // List of jello eyes
    private List<JelloEyeNetworked> jello_eyes = new List<JelloEyeNetworked>();

    /// <summary> Dictionary of each object within the jello's relative position to the jello.
    /// Each entry is a Vector2 in the form (angle, height) and a bool (contained) </summary>
    private Dictionary<Node2D, (Vector2, bool)> object_positions = new Dictionary<Node2D, (Vector2, bool)>();

    /// <summary> If this jello contains the key. </summary>
    public bool has_key;
    private Node2D jello_key;


    /// <summary> Object that controls the residue on the ground. </summary>
    private JelloResidue jello_residue;

    /// <summary> Sound effect player </summary>
    private SoundPlayer sound_player;

    /// <summary> Duration that jello remains on the ground. </summary>
    private const float JELLO_RESIDUE_DURATION = 7f;


    /// <summary> Enumeration of available boss states. </summary>
	public enum BossStates
    {
        IDLE, /* Currently not moving */
        SPAWNING, /* The initial spawn */
        SPLITTING, /* Just spawned from splitting */

        SPIT, /* Spitting out */
        SPIT_INDICATE, /* About to enter a spit */
        SLIDE, /* Slidding around the screen */
        SLIDE_INDICATE, /* About to enter a slide */
        MOVE, /* Regular slow movement */
    };

    /// <summary> Current boss state </summary>
    public BossStates boss_state = BossStates.SPLITTING;

    /*
     * Variables used for animation
     */
    /// <summary> Brief flash for the boss when hurt. </summary>
    private float hurt_flash = 0;

    /// <summary> Sprite for the jello boss' body </summary>
    private Sprite2D body_sprite;

    /// <summary> Sprite for the jello shadow. </summary>
    public Sprite2D shadow_sprite;

    /// <summary> Animation State controller for the jello body, should be assined from body_animation_tree. </summary>
    private AnimationNodeStateMachinePlayback body_animation_state;

    /// <summary> Animation Player for Body </summary>
    private AnimationPlayer body_animation_player;

    /// <summary> The number of eyes that the jello currently has. </summary>
    public int eye_count = 1;

    private float body_scale = 1;

    /*
     * Variables used to track rotation of objects within jello.
     */
    /// <summary> Simulated rotation of the jello around its center </summary>
    private float orth_rotation = 0;

    /// <summary> Radius of the jello along the sideways axis. </summary>
    private const float HORIZONTAL_RADIUS = 100;

    /// <summary> Radius of the jello along the front-to-back axis. </summary>
    private const float VERTICAL_RADIUS = 20;

    /*
     * Variables used during idling 
     */
    private Vector2 synced_position;

    private const float SYNC_SPEED = 25f;

    /*
     * Variables used to track SPLITTING
     */

    /// <summary> Location jello moves to after spawn </summary>
    private Vector2 spawn_location;

    private float spawn_distance;

    // The speed at which jellos spawn.
    private const float SPAWN_SPEED = 150f;

    /// <summary> How long it takes for the jello to spawn. </summary>
    private const float SPAWN_DURATION = 2;

    /// <summary> How far the jello jumps during spawning. </summary>
    private const float SPAWN_HEIGHT = 200;

    /*
     * Variables used to track sliding
     */
    /// <summary> Sliding target that they're looking at </summary>
    private Player slide_target = null;

    /// <summary> Direction the jello is currently sliding. </summary>
    private Vector2 slide_direction = Vector2.Zero;

    /// <summary> Default speed at which the jello slides </summary>
    private const float SLIDE_BASE_SPEED = 2000;

    /// <summary>
    /// Friction applied while sliding.
    /// </summary>
    private const float SLIDE_FRICTION = 300f;

    /// <summary> Speed at which the jello slides. </summary>
    private float slide_speed = 500;

    /// <summary> Radius at which the jello bounces along walls. </summary>
    private const float SLIDE_RADIUS_H = 128;
    private const float SLIDE_RADIUS_V = 32;

    /*
     * Variables used to track spitting.
     */
    private Player spit_target = null;

    /// <summary> Time spent in spit initialization phase. </summary>
    private float SPIT_INIT_TIMER = 3f;

    /// <summary> Timer to track spitting initialization phase. </summary>
    private float spit_init_timer = 0;

    /// <summary> Angle width of the spit </summary>
    float spit_angle = 0.1f;

    /// <summary> current progress of the spit TODO: could be combined into one var? </summary>
    float spit_progress = 0f;

    /// <summary> Current timer of the spit </summary>
    float spit_timer = 0f;

    /// <summary> Pairs of spit angles in which residue flows. </summary>
    Vector2[][] spit_angles;

    /// <summary> Shields that the spit has already encountered, to avoid repeated intersections </summary>
    HashSet<Item> encountered_shields = new HashSet<Item>();


    public override void _Ready()
    {
        /* Loading player bag */
        player_bag = GetNode<PlayerBag>("/root/PlayerBag");

        /* Loading item bag */
        item_bag = GetNode<ItemBag>("/root/ItemBag");

        /* Getting the jello body sprite */
        this.body_sprite = GetNode<Sprite2D>("JelloBodySprite");
        this.shadow_sprite = GetNode<Sprite2D>("JelloBodyShadow");

        /* Getting jello body animator */
        this.body_animation_state = body_sprite.GetNode<AnimationTree>("JelloBodyAnimationTree").Get("parameters/playback").As<AnimationNodeStateMachinePlayback>();

        /* Getting jello body player */
        this.body_animation_player = body_sprite.GetNode<AnimationPlayer>("JelloBodyAnimationPlayer");

        /* Getting the jello residue */
        jello_residue = GetNode<JelloResidue>("../JelloResidue");

        /* Getting jello controller */
        jello_controller = GetParent<JelloControllerNetworked>();

        /* Getting the WORLD */
        WORLD = GameManager.Instance.Get_World();

        /* Getting sound player */
        sound_player = GetNode<SoundPlayer>("SoundPlayer");

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        switch (boss_state)
        {
            case BossStates.SPLITTING:
                handle_splitting((float)delta);
                break;
            case BossStates.SLIDE_INDICATE:
                handle_slide_indicate((float)delta);
                break;
            case BossStates.SLIDE:
                handle_slide((float)delta);
                break;
            case BossStates.SPIT_INDICATE:
                handle_spit_indicate((float)delta);
                break;
            case BossStates.SPIT:
                handle_spit((float)delta);
                break;
            case BossStates.IDLE:
                /* mode towards sync position */
                if ((GlobalPosition - synced_position).Length() < (float)delta * SYNC_SPEED)
                {
                    GlobalPosition = synced_position;
                }
                else
                {
                    GlobalPosition += (synced_position - GlobalPosition).Normalized() * (float)delta * SYNC_SPEED;
                }
                break;
        }

        /* Update object rotations */
        update_rotations((float)delta);

        /* Hurt flash */
        hurt_flash = Mathf.Max(hurt_flash - (float)delta, 0);
        var current_modulate = body_sprite.SelfModulate;
        current_modulate.R = 1 - hurt_flash;
        current_modulate.G = 1 - hurt_flash;
        current_modulate.B = 1 - hurt_flash;

        body_sprite.SelfModulate = current_modulate;

        /* Constantly place jello underneath self TODO: MAKE THIS CIRCULAR */
        if (this.boss_state != BossStates.SPLITTING)
        {
            jello_residue.Update_Grid_Rect(this.GlobalPosition - new Vector2(140, 45) * body_scale,
             this.GlobalPosition + new Vector2(140, 55) * body_scale, JELLO_RESIDUE_DURATION);
        }
    }


    /// <summary>
    /// Initialize the very first jello, which creates the eyes from scratch.
    /// </summary>
    /// <param name="jello_id"> The id assigned to this jello. </param>
    /// <param name="eye_count"> The number of eyes to initialize with. </param>
    public void Initialize_First(int jello_id, List<JelloEyeNetworked> eyes, Vector2 spawn_location, float new_scale = 1.0f, Node2D key = null, bool first = false)
    {
        /* Set this id */
        this.jello_id = jello_id;

        /* Snap to be within area */
        spawn_location.X = Mathf.Min(ROOM_RIGHT - new_scale * SLIDE_RADIUS_H, spawn_location.X);
        spawn_location.X = Mathf.Max(ROOM_LEFT + new_scale * SLIDE_RADIUS_H, spawn_location.X);
        spawn_location.Y = Mathf.Min(ROOM_BOTTOM - new_scale * SLIDE_RADIUS_V, spawn_location.Y);
        spawn_location.Y = Mathf.Max(ROOM_TOP + new_scale * SLIDE_RADIUS_V, spawn_location.Y);
        this.spawn_location = spawn_location;

        /* Getting initial position */
        synced_position = spawn_location;
        spawn_distance = (GlobalPosition - spawn_location).Length();


        /* Snap so its not zero */
        if (spawn_distance < 10)
        {
            spawn_distance = 10;
        }

        /* Set body scale */
        this.body_scale = new_scale;
        /* Getting the jello body sprite */
        this.body_sprite = GetNode<Sprite2D>("JelloBodySprite");

        /* Loading jello eye prefab */
        jello_eye_prefab = GD.Load<PackedScene>("res://Bosses/Jello/JelloEyeNetworked.tscn");

        this.eye_count = eyes.Count;
        /* Creating list of jello eyes */
        for (int i = 0; i < eye_count; i++)
        {
            var inst = eyes[i];
            /* Close all eyes */
            inst.Open_Eye(false);

            /* Add eye as child of jello */
            //inst.CallDeferred("reparent", this);

            /* Add the eye into the list and dictionary */
            jello_eyes.Add(inst);
            object_positions.Add(inst, (new Vector2(i * Mathf.Pi * 2 / eye_count - Mathf.Pi + Mathf.Pi / eye_count, 0.8f), false));
            inst.Set_Parent(this);

        }

        /* Adding key */
        if (key != null)
        {
            key.CallDeferred("reparent", this);
            has_key = true;
            jello_key = key;
            object_positions.Add(key, (new Vector2(Mathf.Pi, 0.25f), true));
        }

        /* Playing animation */
        if (!first)
        {
            GetNode<Sprite2D>("JelloBodySprite").GetNode<AnimationPlayer>("JelloBodyAnimationPlayer").Play("Split" + (GD.Randi() % 2 + 1).ToString());
        }
    }

    /// <summary>
    /// Handles the SPLITTING actions of the jello.
    /// </summary>
    /// <param name="delta"> Time since the last frame. </param>
    public void handle_splitting(float delta)
    {
        float dist = (GlobalPosition - spawn_location).Length();
        /* Move towards SPLITTING location */
        if (dist < (float)delta * spawn_distance / SPAWN_DURATION)
        {
            GlobalPosition = spawn_location;
            return_to_idle(0);
            /* Set scale */
            body_sprite.Scale = 2.5f * new Vector2(body_scale, body_scale);
            body_sprite.Position = Vector2.Zero;
            body_sprite.Rotation = 0;
            sound_player.Play_Effect("Bounce3", -30);
            shadow_sprite.Visible = false;
        }
        else
        {
            GlobalPosition += (spawn_location - GlobalPosition).Normalized() * (float)delta * spawn_distance / SPAWN_DURATION;
            /* Shrink while SPLITTING */
            float temp_scale = body_scale + 0.15f * dist / spawn_distance;
            body_sprite.Scale = 2.5f * new Vector2(temp_scale, temp_scale);
            shadow_sprite.Scale = 4f * new Vector2(temp_scale + Mathf.Abs(dist / spawn_distance - 0.5f) / 2 - 0.25f, temp_scale + Mathf.Abs(dist / spawn_distance - 0.5f) / 2 - 0.25f);

            body_sprite.Position = new Vector2(0, -SPAWN_HEIGHT *
            (1 - 4 * Mathf.Pow(Mathf.Abs(dist / spawn_distance - 0.5f), 2)));

            body_sprite.Rotation = 4 * Mathf.Min(1, dist / spawn_distance + 0.1f);
        }
    }

    /// <summary>
    /// Adds an eye back into the jello.
    /// </summary>
    /// <param name="eye"></param>
    public void Add_Back(JelloEyeNetworked eye)
    {
        /* Check to make sure its already there */
        if (object_positions.ContainsKey(eye))
        {
            eye.CallDeferred("reparent", this);
            (Vector2 relative_pos, bool contained) = object_positions[eye];
            object_positions[eye] = (relative_pos, true);
        }
        else
        {
            Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Attempted to call Add_Back() in Jello.cs on a non-included object.");
        }
    }

    /// <summary>
    /// Returns if the jello sprite is close to the ground.
    /// </summary>
    /// <returns></returns>
    public bool Near_Ground()
    {
        return (this.body_sprite.Position.LengthSquared() < 1024);
    }

    public void Hurt(int damage)
    {
        if (jello_controller != null && boss_state != BossStates.SPLITTING)
        {
            jello_controller.Hurt(jello_id, damage);
            if (damage == 1)
            {
                sound_player.Play_Effect("Hit", -40);
            }
        }
    }

    public void Set_Hurt_Flash()
    {

        hurt_flash = 0.3f;
    }

    /// <summary>
    /// Destroys this eye and provides pointers to all of its eyes.
    /// </summary>
    public List<JelloEyeNetworked> Destroy()
    {
        /* Drop all jellos */
        foreach (JelloEyeNetworked eye in jello_eyes)
        {
            /*
            eye.Set_Parent(null);
            eye.CallDeferred("reparent", WORLD);
            eye.CallDeferred("Drop");
            */
            eye.Set_Parent(null);
            eye.CallDeferred("reparent", WORLD);
            eye.Drop();
        }
        if (has_key)
        {
            jello_key.CallDeferred("reparent", WORLD);
        }

        /* Play sound */
        sound_player.Play_Effect("Death", -40);
        /* Destroy jello */
        QueueFree();


        /* Return list of eyes */
        return jello_eyes;
    }

    /// <summary>
    /// Damaging players on collision.
    /// </summary>
    /// <param name="area"></param>
    public void _on_jello_body_hitbox_area_entered(Area2D area)
    {
        // If it is a hurtbox
        if (boss_state != BossStates.SPLITTING && area.GetType().IsAssignableTo(typeof(PlayerHurtbox)))
        {
            /* Cast to hurtboxenemyparent */
            ((PlayerHurtbox)area).Hurt(1);
        }


    }
    /// <summary>
    /// Returns to the idle state.
    /// </summary>
    /// <param name="cooldown"> Time before next action. </param>
    private void return_to_idle(float cooldown)
    {
        this.boss_state = BossStates.IDLE;
        body_animation_state.Travel("Idle");

        /* Inform controller */
        jello_controller.Idled(this.jello_id);
    }


    /// <summary>
    /// Updates the location of objects rotating within the jello.
    /// </summary>
    /// <param name="delta"> Time since the last frame. </param>
    private void update_rotations(float delta)
    {
        Node2D obj;
        Vector2 relative_pos;
        bool contained;
        float true_rotation;
        Vector2 true_position;
        foreach (var pair in object_positions)
        {
            /* Get object and position */
            obj = pair.Key;
            (relative_pos, contained) = pair.Value;

            /* Only move object if contained */
            if (contained)
            {
                /* TODO: THIS FIX IS INADEQUATE */

                /* If spit indicating */
                if (boss_state == BossStates.SPIT_INDICATE)
                {
                    true_rotation = relative_pos.X / 2 + orth_rotation - Mathf.Pi / 2;
                }
                else
                {
                    /* If normal rotation */
                    true_rotation = relative_pos.X + orth_rotation;
                }
                true_position = body_sprite.GlobalPosition + new Vector2(Mathf.Sin(true_rotation) * HORIZONTAL_RADIUS, Mathf.Cos(true_rotation) * VERTICAL_RADIUS - 20) * body_scale * relative_pos.Y;

                /* Lerp towards desired location */
                obj.GlobalPosition = obj.GlobalPosition.Lerp(true_position, delta * 2);

                if (boss_state == BossStates.SPLITTING)
                {
                    obj.GlobalPosition = true_position;
                }
            }
        }

        /* Additionally rotate body sprite */
        if (boss_state != BossStates.SPLITTING)
        {
            if (((orth_rotation % (Mathf.Pi / 2)) + (Mathf.Pi / 2)) % (Mathf.Pi / 2) < Mathf.Pi / 4)
            {
                body_animation_player.Play("Rot1");
            }
            else
            {
                body_animation_player.Play("Rot2");
            }
        }
    }

    /// <summary>
    /// Sets the visibility of the shadow
    /// </summary>
    /// <param name="state"></param>
    public void Set_Shadow(bool state)
    {
        shadow_sprite = GetNode<Sprite2D>("JelloBodyShadow");
        shadow_sprite.Visible = state;
    }

    /* Sets the synced position of the Jellos */
    public void Set_Sync(Vector2 sync_position)
    {
        synced_position = sync_position;
        //Logger.Instance.Log(Logger.LOG_LEVELS.INFO, "Sync position set to:" + sync_position);
    }

    /// <summary> Set the player the slide is targeting. </summary>
    /// <param name="target"> Player id of target to look at. </param>
    public void Set_Slide_Indicate(int target)
    {
        boss_state = BossStates.SLIDE_INDICATE;
        player_bag = GetNode<PlayerBag>("/root/PlayerBag");
        slide_target = player_bag.GetPlayer(target);

        /* Close all eyes */

        for (int i = 0; i < jello_eyes.Count; i++)
        {
            jello_eyes[i].Open_Eye(false);
        }
        /* Open eyes one and two */
        for (int i = 0; i < Mathf.Min(2, jello_eyes.Count); i++)
        {
            jello_eyes[jello_eyes.Count / 2 - i].Open_Eye(true);
        }
    }

    /// <summary>
    /// Initiates the jello slide with a given direction.
    /// </summary>
    /// <param name="target_direction"> The target direction to travel to. </param>
    public void Set_Slide(Vector2 target_direction)
    {
        /* Enter boss slide state */
        this.boss_state = BossStates.SLIDE;

        /* Calculate slide speed */
        slide_speed = SLIDE_BASE_SPEED * body_scale;

        /* Set slide direction */
        slide_direction = target_direction.Normalized();

        /* Close eyes */
        foreach (JelloEyeNetworked eye in jello_eyes)
        {
            eye.Open_Eye(false);
        }
    }

    /// <summary> Handles looking at the slide target </summary>
    /// <param name="delta"> Time since last frame.  </param>
    private void handle_slide_indicate(float delta)
    {
        if (slide_target != null)
        {
            /* Angle towards player */
            float desired_angle = -(slide_target.GlobalPosition - GlobalPosition).Angle() + Mathf.Pi / 2;

            /* Account for eye count */
            //desired_angle += Mathf.Pi / eye_count;

            orth_rotation = Mathf.LerpAngle(orth_rotation, desired_angle, delta * 3);
        }
    }

    /// <summary> Handles sliding of the jello. </summary>
    /// <param name="delta"> Time since last frame. </param>
    private void handle_slide(float delta)
    {
        /* Rotate while sliding */
        orth_rotation += delta;
        if (orth_rotation > Mathf.Pi * 2)
        {
            orth_rotation -= Mathf.Pi * 2;
        }

        /* Reduce slide speed */
        slide_speed -= delta * SLIDE_FRICTION * body_scale;
        if (slide_speed <= 0)
        {
            return_to_idle(1);
        }

        /* Travel along direction */
        Vector2 intent_position = this.GlobalPosition + slide_direction * Mathf.Min(slide_speed, 800) * (float)delta;

        /* Bouncing */
        if (intent_position.X > ROOM_RIGHT - SLIDE_RADIUS_H * body_scale)
        {
            intent_position.X = 2 * (ROOM_RIGHT - SLIDE_RADIUS_H * body_scale) - intent_position.X;
            slide_direction.X *= -1;
            sound_player.Play_Effect("Bounce1", -20);
            //slide_direction = redirect_slide(slide_direction, Vector2.Left);
        }
        if (intent_position.X < ROOM_LEFT + SLIDE_RADIUS_H * body_scale)
        {
            intent_position.X = 2 * (ROOM_LEFT + SLIDE_RADIUS_H * body_scale) - intent_position.X;
            slide_direction.X *= -1;
            sound_player.Play_Effect("Bounce1", -20);
            //slide_direction = redirect_slide(slide_direction, Vector2.Right);
        }
        if (intent_position.Y > ROOM_BOTTOM - SLIDE_RADIUS_V * body_scale)
        {
            intent_position.Y = 2 * (ROOM_BOTTOM - SLIDE_RADIUS_V * body_scale) - intent_position.Y;
            slide_direction.Y *= -1;
            sound_player.Play_Effect("Bounce2", -20);
            //slide_direction = redirect_slide(slide_direction, Vector2.Up);
        }
        if (intent_position.Y < ROOM_TOP + SLIDE_RADIUS_V * body_scale)
        {
            intent_position.Y = 2 * (ROOM_TOP + SLIDE_RADIUS_V * body_scale) - intent_position.Y;
            slide_direction.Y *= -1;
            sound_player.Play_Effect("Bounce2", -20);
            //slide_direction = redirect_slide(slide_direction, Vector2.Down);
        }

        this.GlobalPosition = intent_position;
    }

    /// <summary> Set the player the slide is targeting. </summary>
    /// <param name="target"> Player id of target to look at. </param>
    public void Set_Spit_Indicate(int target)
    {
        boss_state = BossStates.SPIT_INDICATE;
        spit_target = player_bag.GetPlayer(target);

        /* Open all eyes */
        for (int i = 0; i < jello_eyes.Count; i++)
        {
            jello_eyes[i].Open_Eye(true);
        }
    }

    /// <summary>
    /// Indicates the jello spit's target
    /// </summary>
    /// <param name="delta"> Time since the previous frame. </param>
    private void handle_spit_indicate(float delta)
    {

        if (spit_target != null)
        {
            /* Angle towards player */
            float desired_angle = -(spit_target.GlobalPosition - GlobalPosition).Angle() + Mathf.Pi;

            /* Account for eye count */
            //desired_angle -= Mathf.Pi / eye_count;

            orth_rotation = Mathf.LerpAngle(orth_rotation, desired_angle, delta * 3);
        }
    }

    private void handle_spit(float delta)
    {
        spit_timer += (float)delta * 1800;
        while (spit_timer > JelloResidue.GRID_SIZE)
        {
            spit_timer -= JelloResidue.GRID_SIZE;
            spit_progress += JelloResidue.GRID_SIZE;
            // Draw each residue line
            foreach (Vector2[] spit_pair in spit_angles)
            {
                jello_residue.Update_Grid_Line(this.GlobalPosition + spit_pair[0] * spit_progress,
                    this.GlobalPosition + spit_pair[1] * spit_progress, 10);
                // Check for intersections with carried shields
                foreach (Item item in item_bag.GetCarriedItems())
                {
                    /* Only check shields that are currently being used */
                    if (item.Get_Type() == Item.ItemTypes.Shield && !encountered_shields.Contains(item) && item.Get_Active() && item.Get_Owner().Get_Authority())
                    {
                        /* Get coordinates of shield */
                        Vector2[] shield_points = item.Get_Points();

                        /* Check if spit progress intersects with shield */
                        if (segment_intersect(this.GlobalPosition + spit_pair[0] * spit_progress,
                            this.GlobalPosition + spit_pair[1] * spit_progress, shield_points[0], shield_points[2]) ||
                            segment_intersect(this.GlobalPosition + spit_pair[0] * spit_progress,
                            this.GlobalPosition + spit_pair[1] * spit_progress, shield_points[1], shield_points[3]))
                        {

                            encountered_shields.Add(item);

                            /* Tell jello controller to split the jello */
                            jello_controller.Split_Spit(this.jello_id, shield_points);
                        }
                    }
                }
            }
        }
        /* Duration of spit and reverting to idle state */
        if (spit_progress >= 3000)
        {
            return_to_idle(2);
        }
    }

    /// <summary>
    /// Splits the jello spit around a shield.
    /// </summary>
    /// <param name="shield_points"> The points of the shield. </param>
    public void Split_Spit(Vector2[] shield_points)
    {

        /* Split spit around shield */
        this.spit_progress -= JelloResidue.GRID_SIZE;
        shield_split(shield_points);
    }

    /// <summary>
    /// Shoots all the eyes in the jello.
    /// </summary>
    public void Begin_Spit(Vector2 spit_direction, float orth_snap)
    {
        /* Snap orthogonal rotation */
        orth_rotation = orth_snap;

        /* Calculate angles */
        float base_angle = spit_direction.Angle();

        /* Spit each individual eye */
        float true_rotation;
        bool eye_cont;
        Vector2 eye_rot, true_position, new_direction;
        for (int i = 0; i < jello_eyes.Count; i++)
        {
            JelloEyeNetworked eye = jello_eyes[i];
            (eye_rot, eye_cont) = object_positions[eye];
            eye.Open_Eye(false);
            if (eye_cont)
            {
                new_direction = Vector2.FromAngle(base_angle + ((float)(2 * i - jello_eyes.Count) / 2) * Mathf.Pi / 30);

                true_rotation = eye_rot.X / 2 + orth_rotation - Mathf.Pi / 2;

                true_position = this.GlobalPosition + new Vector2(Mathf.Sin(true_rotation) * HORIZONTAL_RADIUS, Mathf.Cos(true_rotation) * VERTICAL_RADIUS - 20) * body_scale;

                eye.Reparent(WORLD);
                eye.Spit(true_position, new_direction);
                object_positions[eye] = (eye_rot, false);
            }
        }

        /* Initialize the spit progress */
        float spit_center_angle = spit_direction.Angle();
        Vector2 spit_angle_top = Vector2.FromAngle(spit_center_angle - Mathf.Pi * spit_angle).Normalized();
        Vector2 spit_angle_bottom = Vector2.FromAngle(spit_center_angle + Mathf.Pi * spit_angle).Normalized();
        spit_angles = new Vector2[][] { new Vector2[] { spit_angle_top, spit_angle_bottom } };

        /* Reset Progress */
        spit_progress = 0f;
        spit_timer = 0f;
        encountered_shields.Clear();

        /* Enter spit state */
        boss_state = BossStates.SPIT;

        /* Play effect */
        sound_player.Play_Effect("Spit", -20);
    }

    /// <summary>
    /// Returns if two line segments intersect with each other
    /// TODO: some issues when segments are collinear
    /// </summary>
    /// <param name="P1">Point 1 of segment 1</param>
    /// <param name="P2">Point 2 of segment 1</param>
    /// <param name="Q1">Point 1 of segment 2</param>
    /// <param name="Q2">Point 2 of segment 2</param>
    /// <returns></returns>
    private bool segment_intersect(Vector2 P1, Vector2 P2, Vector2 Q1, Vector2 Q2)
    {
        return orientation(P1, Q1, Q2) != orientation(P2, Q1, Q2) && orientation(P1, P2, Q1) != orientation(P1, P2, Q2);
    }
    private bool orientation(Vector2 A, Vector2 B, Vector2 C)
    {
        return (C.Y - A.Y) * (B.X - A.X) > (B.Y - A.Y) * (C.X - A.X);
    }

    /// <summary>
    /// Updates the spit pairs to wrap around a given shield..
    /// Should only be called one spit_angles are initialized and should only be
    /// called once per shield.
    /// </summary>
    /// <param name="shield"></param>
    private void shield_split(Vector2[] shield)
    {
        Logger.Instance.Log(Logger.LOG_LEVELS.DEBUG, "Jello Spit encountered Shield");

        /* This is very inefficient and poorly written */
        Vector2[] spit_pair = spit_angles[0];

        /* Check each end of the shield for its containment  TODO:check nonzero*/
        // Check if the first shield vector lies within the spit angle */
        Vector2 shield_vector0 = (shield[0] - this.GlobalPosition).Normalized();
        float top_angle0 = Mathf.Abs(spit_pair[0].AngleTo(shield_vector0));
        float bottom_angle0 = Mathf.Abs(spit_pair[1].AngleTo(shield_vector0));
        bool contained0 = false;
        /* Angle to shield must be 'close' to both the top and bottom angles to be within
		 the spit angle, assumes spit angle < pi/2 */
        if (top_angle0 < 2 * Mathf.Pi * spit_angle && bottom_angle0 < 2 * Mathf.Pi * spit_angle)
        {
            contained0 = true;
        }
        /* Check if the second shield vector lies within the spit angle */
        Vector2 shield_vector1 = (shield[1] - this.GlobalPosition).Normalized();
        float top_angle1 = Mathf.Abs(spit_pair[0].AngleTo(shield_vector1));
        float bottom_angle1 = Mathf.Abs(spit_pair[1].AngleTo(shield_vector1));
        bool contained1 = false;
        if (top_angle1 < 2 * Mathf.Pi * spit_angle && bottom_angle1 < 2 * Mathf.Pi * spit_angle)
        {
            contained1 = true;
        }

        /* Shield is contained entirely within spit */
        if (contained0 && contained1)
        {
            /* Top angle closer to first end of shield */
            if (top_angle0 < top_angle1)
            {
                spit_angles = new Vector2[][]{new Vector2[]{spit_pair[0], vec_proj(spit_pair[0], shield_vector0)},
                    new Vector2[]{vec_proj(spit_pair[1], shield_vector1), spit_pair[1]}};
            }
            /* Top angle closer to second end of shield */
            else
            {
                spit_angles = new Vector2[][]{new Vector2[]{spit_pair[0], vec_proj(spit_pair[0], shield_vector1)},
                    new Vector2[]{vec_proj(spit_pair[1], shield_vector0), spit_pair[1]}};
            }
        }
        /* Shield is partially contained in spit */
        else if (contained0 && !contained1)
        {
            /* If top angle lines is along shield */
            if (top_angle1 < bottom_angle1)
            {
                spit_angles = new Vector2[][] { new Vector2[] { spit_pair[1], vec_proj(spit_pair[1], shield_vector0) } };
            }
            /* Bottom angle line is along shield */
            else
            {
                spit_angles = new Vector2[][] { new Vector2[] { spit_pair[0], vec_proj(spit_pair[0], shield_vector0) } };
            }
        }
        else if (contained1 && !contained0)
        {
            /* If top angle lines is along shield */
            if (top_angle0 < bottom_angle0)
            {
                spit_angles = new Vector2[][] { new Vector2[] { spit_pair[1], vec_proj(spit_pair[1], shield_vector1) } };
            }
            /* Bottom angle line is along shield */
            else
            {
                spit_angles = new Vector2[][] { new Vector2[] { spit_pair[0], vec_proj(spit_pair[0], shield_vector1) } };
            }
        }
    }

    /// <summary>
    /// Calculates the projection of vector u onto v, result will be in the
    /// direction of v.
    /// </summary>
    /// <param name="u">A nonzero vector</param>
    /// <param name="v">A nonzero vector</param>
    /// <returns></returns>
    private Vector2 vec_proj(Vector2 u, Vector2 v)
    {
        return u.Dot(v) / v.Length() * v;
    }

}
