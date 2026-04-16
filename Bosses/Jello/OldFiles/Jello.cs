using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Jello : Node2D
{

    /// <summary> Left boundary of the boss room. </summary>
    private const float ROOM_LEFT = 0;
    /// <summary> Right boundary of the boss room. </summary>
    private const float ROOM_RIGHT = 1920;
    /// <summary> Top boundary of the boss room. </summary>
    private const float ROOM_TOP = 150;
    /// <summary> Bottom boundary of the boss room. </summary>
    private const float ROOM_BOTTOM = 1080;

    /// <summary> Enumeration of available boss states. </summary>
	public enum BossStates {
        IDLE, /* Currently not moving */

        SPIT, /* Spitting out */
        SPIT_INIT, /* About to enter a spit */
        SLIDE, /* Slidding around the screen */
        SLIDE_INIT, /* About to enter a slide */
        MOVE, /* Regular slow movement */
	};

    /// <summary> Current boss state </summary>
    private BossStates boss_state = BossStates.IDLE;

    private JelloController jello_controller;

    /// <summary> Brief flash for the boss when hurt. </summary>
    private float hurt_flash = 0;

    /// <summary> Sprite for the jello boss' body </summary>
    private Sprite2D body_sprite;

	/// <summary> Animation State controller for the jello body, should be assined from body_animation_tree. </summary>
	private AnimationNodeStateMachinePlayback body_animation_state;

    /// <summary> Access to the bag of all players. </summary>
    private PlayerBag player_bag;

    /// <summary> Access to the bag of all items. </summary>
    private ItemBag item_bag;

	/// <summary> Node representing world space. </summary>
	private Node WORLD;

    /// <summary> Random number generator for slime. </summary>
    private Random rand = new Random();

	/// <summary> Object that controls the residue on the ground. </summary>
	private JelloResidue jello_residue;

    /// <summary> Duration that jello remains on the ground. </summary>
    private const float JELLO_RESIDUE_DURATION = 7f;

    /// <summary> Prefab used for the jello eye </summary>
    private PackedScene jello_eye_prefab;

    // List of jello eyes
    private List<JelloEye> jello_eyes = new List<JelloEye>();

    /// <summary> The number of eyes that the jello currently has. </summary>
    private int eye_count = 0;

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


    /// <summary>
    /// Dictionary of each object within the jello's relative position to the jello.
    /// Each entry is a Vector2 in the form (angle, height) and a bool (contained)
    /// </summary>
    private Dictionary<Node2D, (Vector2, bool)> object_positions = new Dictionary<Node2D, (Vector2, bool)>();

    /*
     * Variables used to track movement.
     */
    private const float MOVE_BASE_SPEED = 50;

    private float idle_timer = 0;

    private float move_timer = 0;

    /*
     * Variables used to track sliding
     */

    /// <summary> Time spent in slide initialization phase. </summary>
    private float SLIDE_INIT_TIMER = 1.5f;

    /// <summary> Timer to track sliding stuff. </summary>
    private float slide_timer = 0;
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
    private const float SLIDE_RADIUS_H = 64;
    private const float SLIDE_RADIUS_V = 32;

    /// <summary> Only allow shimmying up to a certain angle. </summary>
    private const float SHIMMY_ANGLE = Mathf.Pi / 3;

    /*
     * Variables used to track spitting.
     */

    /// <summary> Time spent in spit initialization phase. </summary>
    private float SPIT_INIT_TIMER = 3f;

    /// <summary> Timer to track spitting initialization phase. </summary>
    private float spit_init_timer = 0;

	/// <summary> Angle width of the spit </summary>
	float spit_angle = 0.1f;

    /// <summary> Center angle of spit </summary>
    private Player spit_target;

	/// <summary> current progress of the spit TODO: could be combined into one var? </summary>
	float spit_progress = 0f;

	/// <summary> Current timer of the spit </summary>
	float spit_timer = 0f;

	/// <summary> Pairs of spit angles in which residue flows. </summary>
	Vector2[][] spit_angles;

	/// <summary> Shields that the spit has already encountered, to avoid repeated intersections </summary>
	HashSet<Item> encountered_shields = new HashSet<Item>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		/* Loading jello eye prefab */
        jello_eye_prefab = GD.Load<PackedScene>("res://Bosses/Jello/JelloEye.tscn");

        /* Getting jello body animator */
        this.body_animation_state = GetNode<AnimationTree>("JelloBodyAnimationTree").Get("parameters/playback").As<AnimationNodeStateMachinePlayback>();

        /* Getting the jello body sprite */
        this.body_sprite = GetNode<Sprite2D>("JelloBodySprite");

        /* Loading player bag */
        player_bag = GetNode<PlayerBag>("/root/PlayerBag");

        /* Loading item bag */
        item_bag = GetNode<ItemBag>("/root/ItemBag");

		/* Getting world context */
		WORLD = GetTree().Root.GetNode("World");

        /* Getting the jello residue */
		jello_residue = GetNode<JelloResidue>("../JelloResidue");

        /* Initial Idle */
        idle_timer = (float) rand.NextDouble() + 1.5f;
	}

    public override void _Process(double delta)
    {

        /* Debug, return to idle */
        if (Input.IsActionJustPressed("ui_zero")) {
            /* Return to idle state */
            return_to_idle(1);
        }

        /* Debug, enter slide */
        if (Input.IsActionJustPressed("ui_one")) {
            initiate_slide();
        }

        /* Debug, initiate spit */
        if (Input.IsActionJustPressed("ui_two")) {
            initiate_spit();
        }

        if (Input.IsActionJustPressed("ui_three")) {
            body_animation_state.Travel("Moving");
            boss_state = BossStates.MOVE;
        }

        /* Hurt flash */
        hurt_flash = Mathf.Max(hurt_flash - (float) delta, 0);
        var current_modulate = body_sprite.SelfModulate;
        current_modulate.R = 1 - hurt_flash;
        current_modulate.G = 1 - hurt_flash;
        current_modulate.B = 1 - hurt_flash;
        
        body_sprite.SelfModulate = current_modulate;

        /* Act depending on current state */
        switch (this.boss_state) {
            case BossStates.IDLE:
                idle_timer -= (float) delta;
                if (idle_timer <= 0) {
                    boss_state = BossStates.MOVE;
                    body_animation_state.Travel("Moving");
                    move_timer = 0;
                }
                break;
            case BossStates.SLIDE_INIT:
                slide_timer -= (float) delta;
                if (slide_timer <= 0) {
                    /* Enter slide state */
                    boss_state = BossStates.SLIDE;
                    body_animation_state.Travel("Sliding");
                    /* Additionally close all eyes */
                    foreach (JelloEye eye in jello_eyes) {
                        eye.Open_Eye(false);
                    }
                }

                break;
            case BossStates.SLIDE:
                handle_slide((float) delta);
                break;
            case BossStates.SPIT_INIT:
                handle_spit_init((float) delta);
                break;
            case BossStates.SPIT:
				handle_spit((float) delta);
			    break;
            case BossStates.MOVE:
                handle_move((float) delta);
                break;
        }
        
        /* Handle rotation of things within the eye */
        rotate_objects((float) delta);

        /* Constantly place jello underneath self TODO: MAKE THIS CIRCULAR */
        jello_residue.Update_Grid_Rect(this.GlobalPosition - new Vector2(140, 45) * body_scale,
         this.GlobalPosition + new Vector2(140, 55) * body_scale, JELLO_RESIDUE_DURATION);

        /* Update health bar */
        QueueRedraw();
    }
    public override void _Draw()
    {

    }

    /// <summary>
    /// Sets the controller of this jello object.
    /// </summary>
    /// <param name="controller"></param>
    public void Set_Controller(JelloController controller) {
        this.jello_controller = controller;
    }

    /// <summary>
    /// Initialize the eyes of the jello.
    /// </summary>
    /// <param name="eye_count"> The eyes to initialize with. </param>
    public void Initialize_Eyes(int eye_count) {
		/* Loading jello eye prefab */
        jello_eye_prefab = GD.Load<PackedScene>("res://Bosses/Jello/JelloEye.tscn");

        this.eye_count = eye_count;
        /* Creating list of jello eyes */
        for (int i = 0; i < eye_count; i++) {
            var inst = jello_eye_prefab.Instantiate<JelloEye>();

            /* Add eye as child of jello */
            CallDeferred("add_child", inst);

            /* Add the eye into the list and dictionary */
            jello_eyes.Add(inst);
            object_positions.Add(inst, (new Vector2(i * Mathf.Pi * 2 / eye_count - 4 * Mathf.Pi / eye_count, 0), true));
            inst.Set_Parent(this);

            /* Hard set their starting positions TODO: may be unecessary once there are spawn animations */
            float true_rotation = i * Mathf.Pi * 2 / eye_count + orth_rotation;
            inst.Position = new Vector2(Mathf.Sin(true_rotation) * HORIZONTAL_RADIUS * body_scale, Mathf.Cos(true_rotation) * VERTICAL_RADIUS * body_scale);
        }
    }

    public void Provide_Eyes(List<JelloEye> eyes) {
        this.eye_count = eyes.Count;
        /* Creating list of jello eyes */
        for (int i = 0; i < eye_count; i++) {
            var eye = eyes[i];
            /* Add the eye into the list and dictionary */
            jello_eyes.Add(eye);
            object_positions.Add(eye, (new Vector2(i * Mathf.Pi * 2 / eye_count - 4 * Mathf.Pi / eye_count, 0), true));
            eye.Set_Parent(this);
        }
    }

    /// <summary>
    /// Sets the scale of the jello body.
    /// </summary>
    /// <param name="scale"> The scale to use. </param>
    public void Set_Scale(float scale) {
        body_scale = scale;
        this.body_sprite = GetNode<Sprite2D>("JelloBodySprite");
        body_sprite.Scale = 5 * new Vector2(scale, scale);
    }

    public void _on_jello_body_hitbox_area_entered(Area2D area) {
        
        // If it is a hurtbox
        if (area.GetType().IsAssignableTo(typeof(PlayerHurtbox))) {
            /* Cast to hurtboxenemyparent */
            ((PlayerHurtbox) area).Hurt(1);
        }

    }

    /// <summary>
    /// Drops all the eyes of a slime.
    /// </summary>
    public List<JelloEye> Drop_Eyes() {
        foreach (JelloEye eye in jello_eyes) {
            //eye.Drop();
            //eye.Reparent(WORLD);
            eye.QueueFree();
        }
        return jello_eyes;
    }

    /// <summary>
    /// Deals damage to the boss.
    /// </summary>
    /// <param name="damage"> Amount of hp that the boss should lose. </param>
    public void Hurt(int damage) {
        jello_controller.Hurt(damage, this);
        hurt_flash = 0.3f;
    }

    /// <summary>
    /// Add a given object back into the jello object. The given object should
    /// already be within the object_positions dictionary.
    /// </summary>
    /// <param name="obj"> The object to add back in. </param>
    public void Add_Back(Node2D obj) {
        /* Check to make sure its already there */
        if (object_positions.ContainsKey(obj)) {
            obj.Reparent(this);
            (Vector2 relative_pos, bool contained) = object_positions[obj];
            object_positions[obj] = (relative_pos, true);
        }
        else {
            Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Attempted to call Add_Back() in Jello.cs on a non-included object.");
        }
    }

    /// <summary>
    /// Returns to the idle state.
    /// </summary>
    /// <param name="cooldown"> Time before next action. </param>
    private void return_to_idle(float cooldown) {
        this.boss_state = BossStates.IDLE;
        body_animation_state.Travel("Idle");
        idle_timer = cooldown;
    }

    /// <summary>
    /// Handles internal rotation of objects within jello.
    /// </summary>
    /// <param name="delta"> Time since last frame in seconds. </param>
    private void rotate_objects(float delta) {
        
        Node2D obj;
        Vector2 relative_pos;
        bool contained;
        float true_rotation;
        Vector2 true_position;
        foreach (var pair in object_positions) {
            /* Get object and position */
            obj = pair.Key;
            (relative_pos, contained) = pair.Value;

            /* Only move object if contained */
            if (contained) {
                /* Staring someone down */
                if (boss_state == BossStates.SPIT_INIT & spit_target != null) {
                    true_rotation = relative_pos.X / 2 - (this.GlobalPosition - spit_target.GlobalPosition).Angle() - Mathf.Pi / 2;
                }
                /* During normal movement */
                else {
                    true_rotation = relative_pos.X + orth_rotation;
                }

                true_position = new Vector2(Mathf.Sin(true_rotation) * HORIZONTAL_RADIUS, Mathf.Cos(true_rotation) * VERTICAL_RADIUS - 20) * body_scale;

                /* Lerp towards desired location */
                obj.Position = obj.Position.Lerp(true_position, delta * 2);
            }
        }
    }

    /*
     * Handles the movement of the boss.
     */
    private void handle_move(float delta) {
        /* Track closest player */
        Player closest_player = null;
        float closest_dist = Mathf.Inf;
        foreach (Player player in player_bag.GetActivePlayers()) {
            if ((this.GlobalPosition - player.GlobalPosition).LengthSquared() < closest_dist) {
                closest_dist = (this.GlobalPosition - player.GlobalPosition).LengthSquared();
                closest_player = player;
            }
        }
        if (closest_player != null) {
            this.GlobalPosition += (closest_player.GlobalPosition - this.GlobalPosition).Normalized()
                * MOVE_BASE_SPEED * (float) delta;
            orth_rotation += (float) delta / 5;
        }

        /* Choose randomly to initiate different moves */
        move_timer += delta;
        if (move_timer > 2) {
            var act_val = rand.Next() % 10;
            if (act_val < 4) {
                initiate_slide();
            }
            else if (act_val < 7) {
                /* Only spit if 4 or more eyes */
                if (eye_count > 2) {
                    initiate_spit();
                }
                else {
                    initiate_slide();
                }
            }
            else {
                move_timer -= 0.25f;
            }
        }
    }

    /// <summary>
    /// Initiates the spit attack.
    /// </summary>
    private void initiate_spit() {
        /* Finding player target */
        var players = player_bag.GetActivePlayers();
		if (players.Count > 0) {
            spit_target = players
                .ElementAt<Player>(rand.Next(player_bag.GetActivePlayers().Count));
		}
        else {
            spit_target = null;
        }

        /* Set boss state */
		this.boss_state = BossStates.SPIT_INIT;

        spit_init_timer = SPIT_INIT_TIMER;

        /* Open all eyes */
        foreach (JelloEye eye in jello_eyes) {
            eye.Open_Eye(true);
        }
    }

    /// <summary>
    /// Handles the spit initialization phase of the jello.
    /// </summary>
    /// <param name="delta"> Time since the last frame. </param>
    private void handle_spit_init(float delta) {
        spit_init_timer -= (float) delta;
        if (spit_init_timer <= 0) {
            /* Enter spit state */
            boss_state = BossStates.SPIT;

            /* Calculate initial angle */
            float spit_center_angle;
            if (spit_target != null) {
                spit_center_angle =  (spit_target.GlobalPosition - this.GlobalPosition).Angle();
            }
            else {
                spit_center_angle = (float) rand.NextDouble() * Mathf.Pi * 2;
            }

            Vector2 spit_angle_top = Vector2.FromAngle(spit_center_angle - Mathf.Pi * spit_angle).Normalized();
            Vector2 spit_angle_bottom = Vector2.FromAngle(spit_center_angle + Mathf.Pi * spit_angle).Normalized();
            spit_angles = new Vector2[][]{new Vector2[]{spit_angle_top, spit_angle_bottom}};

            /* Reset Progress */
            spit_progress = 0f;
            spit_timer = 0f;
            encountered_shields.Clear();
            /* Also spit out eyes */
            Node2D obj;
            Vector2 relative_pos;
            bool contained;
            foreach (var pair in object_positions) {
                /* Get object and position */
                obj = pair.Key;
                (relative_pos, contained) = pair.Value;

                /* Set to be uncontained */
                object_positions[obj] = (relative_pos, false);

                /* Only spit object if contained */
                if (contained) {
                    obj.Reparent(WORLD);
                    Vector2 new_position = this.GlobalPosition + 400 * Vector2.FromAngle(spit_center_angle - 0.3f + (float) rand.NextDouble() * 0.6f); 
                    if (obj is JelloEye) {
                        ((JelloEye) obj).Spit(new_position);
                        ((JelloEye) obj).Open_Eye(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handles the spit calculations.
    /// </summary>
    /// <param name="delta"> Time sine the last frame. </param>
    private void handle_spit(float delta) {
        spit_timer += (float) delta * 1800;
				while (spit_timer > JelloResidue.GRID_SIZE) {
					spit_timer -= JelloResidue.GRID_SIZE;
					spit_progress += JelloResidue.GRID_SIZE;
					// Draw each residue line
					foreach (Vector2[] spit_pair in spit_angles) {
						jello_residue.Update_Grid_Line(this.GlobalPosition + spit_pair[0] * spit_progress,
							this.GlobalPosition + spit_pair[1] * spit_progress, 10);
						// Check for intersections with carried shields
						foreach (Item item in item_bag.GetCarriedItems()) {
							/* Only check shields that are currently being used */
							if (item.Get_Type() == Item.ItemTypes.Shield && !encountered_shields.Contains(item) && item.Get_Active()) {
								/* Get coordinates of shield */
								Vector2[] shield_points = item.Get_Points();

								/* Check if spit progress intersects with shield */
								if (segment_intersect(this.GlobalPosition + spit_pair[0] * spit_progress,
									this.GlobalPosition + spit_pair[1] * spit_progress, shield_points[0], shield_points[2]) ||
									segment_intersect(this.GlobalPosition + spit_pair[0] * spit_progress,
									this.GlobalPosition + spit_pair[1] * spit_progress, shield_points[1], shield_points[3])) {

									/* Split spit around shield */
									this.spit_progress -= JelloResidue.GRID_SIZE;
									encountered_shields.Add(item);
									shield_split(shield_points);
								}
							}
						}
					}
				}
				/* Duration of spit and reverting to idle state */
				if (spit_progress >= 3000) {
                    return_to_idle(2);
				}
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
	private bool segment_intersect(Vector2 P1, Vector2 P2, Vector2 Q1, Vector2 Q2) {
		return orientation(P1, Q1, Q2) != orientation(P2, Q1, Q2) && orientation(P1, P2, Q1) != orientation(P1, P2, Q2);
	}
	private bool orientation(Vector2 A, Vector2 B, Vector2 C) {
		return (C.Y - A.Y) * (B.X - A.X) > (B.Y - A.Y) * (C.X - A.X);
	}

	/// <summary>
	/// Updates the spit pairs to wrap around a given shield..
	/// Should only be called one spit_angles are initialized and should only be
	/// called once per shield.
	/// </summary>
	/// <param name="shield"></param>
	private void shield_split(Vector2[] shield) {
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
		if (top_angle0 < 2 * Mathf.Pi * spit_angle && bottom_angle0 < 2 * Mathf.Pi * spit_angle) {
			contained0 = true;
		}
		/* Check if the second shield vector lies within the spit angle */
		Vector2 shield_vector1 = (shield[1] - this.GlobalPosition).Normalized();
		float top_angle1 = Mathf.Abs(spit_pair[0].AngleTo(shield_vector1));
		float bottom_angle1 = Mathf.Abs(spit_pair[1].AngleTo(shield_vector1));
		bool contained1 = false;
		if (top_angle1 < 2 * Mathf.Pi * spit_angle && bottom_angle1 < 2 * Mathf.Pi * spit_angle) {
			contained1 = true;
		}

		/* Shield is contained entirely within spit */
		if (contained0 && contained1) {
			/* Top angle closer to first end of shield */
			if (top_angle0 < top_angle1) {
				spit_angles = new Vector2[][]{new Vector2[]{spit_pair[0], vec_proj(spit_pair[0], shield_vector0)},
					new Vector2[]{vec_proj(spit_pair[1], shield_vector1), spit_pair[1]}};
			}
			/* Top angle closer to second end of shield */
			else {
				spit_angles = new Vector2[][]{new Vector2[]{spit_pair[0], vec_proj(spit_pair[0], shield_vector1)},
					new Vector2[]{vec_proj(spit_pair[1], shield_vector0), spit_pair[1]}};
			}
		}
		/* Shield is partially contained in spit */
		else if (contained0 && !contained1) {
			/* If top angle lines is along shield */
			if (top_angle1 < bottom_angle1) {
				spit_angles = new Vector2[][]{new Vector2[]{spit_pair[1], vec_proj(spit_pair[1], shield_vector0)}};
			}
			/* Bottom angle line is along shield */
			else {
				spit_angles = new Vector2[][]{new Vector2[]{spit_pair[0], vec_proj(spit_pair[0], shield_vector0)}};
			}
		}
		else if (contained1 && !contained0) {
			/* If top angle lines is along shield */
			if (top_angle0 < bottom_angle0) {
				spit_angles = new Vector2[][]{new Vector2[]{spit_pair[1], vec_proj(spit_pair[1], shield_vector1)}};
			}
			/* Bottom angle line is along shield */
			else {
				spit_angles = new Vector2[][]{new Vector2[]{spit_pair[0], vec_proj(spit_pair[0], shield_vector1)}};
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
	private Vector2 vec_proj(Vector2 u, Vector2 v) {
		return u.Dot(v) / v.Length() * v;
	}

    /// <summary>
    /// Finds a target for and initiates the slide.
    /// </summary>
    private void initiate_slide() {
        
        /* Finding player target */
        Node2D player_target = player_bag.GetActivePlayers()
            .ElementAt<Node2D>(rand.Next(player_bag.GetActivePlayers().Count));

        /* Generating slide direction */
        slide_direction = (player_target.GlobalPosition - this.GlobalPosition).Normalized();

        /* Enter slide initiialization state */
        this.boss_state = BossStates.SLIDE_INIT;
        slide_timer = SLIDE_INIT_TIMER;

        /* Calculate slide speed */
        slide_speed = SLIDE_BASE_SPEED * body_scale;

        /* Open the two closest eyes to the player */
        float closest = Mathf.Inf;
        JelloEye closest_eye = null;
        float second_closest = Mathf.Inf;
        JelloEye second_closest_eye = null;
        float player_dist;
        foreach (JelloEye eye in jello_eyes) {
            /* Calculate distance */
            player_dist = (eye.GlobalPosition - player_target.GlobalPosition).LengthSquared();
            /* If its the new closest */
            if (player_dist < closest) {
                second_closest_eye = closest_eye;
                second_closest = closest;
                closest = player_dist;
                closest_eye = eye;
            }
            /* If its the second closest */
            else if (player_dist < second_closest) {
                second_closest_eye = eye;
                second_closest = player_dist;
            }
        }
        /* Open those two eyes if possible */
        if (closest_eye != null) {
            closest_eye.Open_Eye(true);
        }
        if (second_closest_eye != null) {
            second_closest_eye.Open_Eye(true);
        }
    }

    /// <summary>
    /// Handle the sliding actions of the jello.
    /// </summary>
    /// <param name="delta"> Time in seconds since the previous frame. </param>
    private void handle_slide(float delta) {
        /* Rotate while sliding */
        orth_rotation += delta;
        if (orth_rotation > Mathf.Pi * 2) {
            orth_rotation -= Mathf.Pi * 2;
        }

        /* Reduce slide speed */
        slide_speed -= delta * SLIDE_FRICTION * body_scale;
        if (slide_speed <= 0) {
            return_to_idle(1);
        }

        /* Travel along direction */
        Vector2 intent_position = this.GlobalPosition + slide_direction * Mathf.Min(slide_speed, 800) * (float) delta;

        /* Bouncing */
        if (intent_position.X > ROOM_RIGHT - SLIDE_RADIUS_H * body_scale) {
            intent_position.X = 2 * (ROOM_RIGHT - SLIDE_RADIUS_H * body_scale) - intent_position.X;
            slide_direction.X *= -1;
            //slide_direction = redirect_slide(slide_direction, Vector2.Left);
        }
        if (intent_position.X < ROOM_LEFT + SLIDE_RADIUS_H * body_scale) {
            intent_position.X = 2 * (ROOM_LEFT + SLIDE_RADIUS_H * body_scale) - intent_position.X;
            slide_direction.X *= -1;
            //slide_direction = redirect_slide(slide_direction, Vector2.Right);
        }
        if (intent_position.Y > ROOM_BOTTOM - SLIDE_RADIUS_V * body_scale) {
            intent_position.Y = 2 * (ROOM_BOTTOM - SLIDE_RADIUS_V * body_scale) - intent_position.Y;
            slide_direction.Y *= -1;
            //slide_direction = redirect_slide(slide_direction, Vector2.Up);
        }
        if (intent_position.Y < ROOM_TOP + SLIDE_RADIUS_V * body_scale) {
            intent_position.Y = 2 * (ROOM_TOP + SLIDE_RADIUS_V * body_scale) - intent_position.Y;
            slide_direction.Y *= -1;
            //slide_direction = redirect_slide(slide_direction, Vector2.Down);
        }

        this.GlobalPosition = intent_position;
    }

    /// <summary>
    ///  Redirects the jello upon bouncing to head towards characters within a certain angle.
    /// </summary>
    /// <param name="cur_direction"> Curent direction the slime is traveling </param>
    /// <param name="orth_direction"> Angle orthogonal to the wall, to limit shimmying. </param>
    /// <returns></returns>
    private Vector2 redirect_slide(Vector2 cur_direction, Vector2 orth_direction) {
        float smallest_angle = Mathf.Inf;
        float cand_angle;
        Node2D potential_player = null;
        foreach (Node2D player in player_bag.GetActivePlayers()) {
            cand_angle = cur_direction.AngleTo(player.GlobalPosition - this.GlobalPosition);
            if (Mathf.Abs(cand_angle) < Mathf.Abs(smallest_angle)) {
                smallest_angle = cand_angle;
                potential_player = player;
            }
        }

        /* Redirecting */
        if (potential_player != null) {
            /* Shrinking angle */
            if (Mathf.Abs(smallest_angle) > SHIMMY_ANGLE) {
                smallest_angle = SHIMMY_ANGLE * Mathf.Sign(smallest_angle);
            }

            /* Calculating vector */
            Vector2 ret_vector = Vector2.FromAngle(cur_direction.Angle() + smallest_angle);

            /* Making sure angle is not backwards */
            float orth_angle = orth_direction.AngleTo(ret_vector);
            if (orth_angle > Mathf.Pi * 4 / 9) {
                ret_vector = Vector2.FromAngle(orth_direction.Angle() + Mathf.Pi * 4 / 9 * Mathf.Sign(orth_angle));
            }
            return ret_vector;
        }
    
        /* Otherwise, just return */
        return cur_direction;
    }
}
