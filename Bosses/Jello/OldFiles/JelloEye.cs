using Godot;
using System;
using System.Collections.Generic;

public partial class JelloEye : Node2D
{

    /// <summary> Left boundary of the boss room. </summary>
    private const float ROOM_LEFT = 0;
    /// <summary> Right boundary of the boss room. </summary>
    private const float ROOM_RIGHT = 1920;
    /// <summary> Top boundary of the boss room. </summary>
    private const float ROOM_TOP = 150;
    /// <summary> Bottom boundary of the boss room. </summary>
    private const float ROOM_BOTTOM = 1080;

    /// <summary> Radius of the jello along the sideways axis. </summary>
    private const float HORIZONTAL_RADIUS = 16;

    /// <summary> Radius of the jello along the front-to-back axis. </summary>
    private const float VERTICAL_RADIUS = 8;

    /// <summary>
    /// How much more damage hits to the eyes do.
    /// </summary>
    private const int DAMAGE_MULTIPLIER = 3;

    /// <summary> How much movement is retained upon bouncing. </summary>
    private const float BOUNCE_KINETIC = 0.6f;

	/// <summary> Animation tree for eye </summary>
	private AnimationTree animation_tree;

	/// <summary> Animation state controller for platform, should be assigned from animation_tree. </summary>
	private AnimationNodeStateMachinePlayback animation_state;

    /// <summary> The Jello object that contains this eye.</summary>
    private Jello jello_parent;

    private Sprite2D eye_sprite;

    private Sprite2D shadow_sprite;

    /// <summary>
    /// Whether the jello eye is currently open.
    /// </summary>
    private bool eye_open = false;

    /// <summary> If the eye is currently contained in the jello parent. </summary>
    private bool contained = true;

    /// <summary> If the eye is still on its spit trajectory. </summary>
    private bool traveling = false;

    private float hurt_flash = 0;

    private HashSet<Area2D> interacted_areas = new HashSet<Area2D>();

    /// <summary>
    /// The default travel time of eyes.
    /// </summary>
    private const float BASE_TRAVEL_SPEED = 1500f;

    /// <summary> Friction of the eye </summary>
    private const float TRAVEL_FRICTION = 800f;

    /// <summary>
    /// Traveling velocity of the eye.
    /// </summary>
    private Vector2 travel_velocity = Vector2.Zero;

    /// <summary> The speed at which the eye travels back to the jello. </summary>
    private const float STRUGGLE_SPEED = 240f;

    /// <summary> Delay before the jello eye begins struggling. </summary>
    private const float STRUGGLE_DELAY = 2f;

    private float struggle_delay_timer = 0;

    /// <summary> Tracking the eyeball wobble as they struggle. </summary>
    private float wobble_timer = 0;

    /// <summary> Random number generator. </summary>
    private Random rand = new Random();
    
    /// <summary>
    /// Position of the eye not including wobble changes.
    /// </summary>
    private Vector2 ground_position;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		/* Getting animation parts */
		this.animation_tree = GetNode<AnimationTree>("AnimationTree");
		this.animation_state = animation_tree.Get("parameters/playback").As<AnimationNodeStateMachinePlayback>();

        /* Initialize wobble randomly */
        wobble_timer = (float) rand.NextDouble() * Mathf.Pi * 2;
        
        /* Getting eye sprite */
        eye_sprite = GetNode<Sprite2D>("JelloEyeSprite");

        shadow_sprite = GetNode<Sprite2D>("JelloEyeShadow");

        /* Initialize rotation based on this */
        eye_sprite.Rotation = 1 + 5 * Mathf.Sin(wobble_timer * 8) / 20;

        this.contained = true;
        enable_shadow(false);

	}

    /// <summary>
    /// Called when the area2d of this object intersects another.
    /// </summary>
    /// <param name="Area"> The area this touches</param>
    public void _on_jello_eye_area_area_entered(Area2D area) {
        
        /* Only interact with each area once over lifetime */
        // If its a shield hitbox
        if (area.GetType().IsAssignableTo(typeof(PlayerShieldHitbox))) {
            var shield_hitbox = (PlayerShieldHitbox) area;
            /* Reflect across shield */
            if (shield_hitbox.Get_Active() && !interacted_areas.Contains(area)) {
                interacted_areas.Add(area);
                /* Bounce if traveling */
                if (traveling) {
                Vector2[] shield_points = shield_hitbox.Get_Points();
                Vector2 orth_vector = (shield_points[0] - shield_points[2]).Normalized();

                travel_velocity = travel_velocity - 2 * orth_vector * (travel_velocity.X * orth_vector.X + travel_velocity.Y + orth_vector.Y);
                travel_velocity *= BOUNCE_KINETIC;
                }
            }
        }

        // If it is a hurtbox
        if (!contained && area.GetType().IsAssignableTo(typeof(PlayerHurtbox))) {
            /* Cast to hurtboxenemyparent */
            ((PlayerHurtbox) area).Hurt(1);
        }

    }

    public override void _Process(double delta)
    {
        /* Flash effect when hurt */
        
        hurt_flash = Mathf.Max(hurt_flash - (float) delta, 0);
        var current_modulate = Modulate;
        current_modulate.R = 1 - hurt_flash;
        current_modulate.G = 1 - hurt_flash;
        current_modulate.B = 1 - hurt_flash;
        
        Modulate = current_modulate;

        /* If not contained, struggle back towards Jello */
        if (!this.contained) {
            /* Traveling while spit */
            if (traveling) {        /* Travel along direction */
                Vector2 intent_position = this.GlobalPosition + travel_velocity * (float) delta;

                /* Bouncing */
                if (intent_position.X > ROOM_RIGHT - HORIZONTAL_RADIUS) {
                    intent_position.X = 2 * (ROOM_RIGHT - HORIZONTAL_RADIUS) - intent_position.X;
                    travel_velocity.X *= -1;
                    travel_velocity *= BOUNCE_KINETIC;
                    //travel_velocity = redirect_slide(travel_velocity, Vector2.Left);
                }
                if (intent_position.X < ROOM_LEFT + HORIZONTAL_RADIUS) {
                    intent_position.X = 2 * (ROOM_LEFT + HORIZONTAL_RADIUS) - intent_position.X;
                    travel_velocity.X *= -1;
                    travel_velocity *= BOUNCE_KINETIC;
                    //travel_velocity = redirect_slide(travel_velocity, Vector2.Right);
                }
                if (intent_position.Y > ROOM_BOTTOM - VERTICAL_RADIUS) {
                    intent_position.Y = 2 * (ROOM_BOTTOM - VERTICAL_RADIUS) - intent_position.Y;
                    travel_velocity.Y *= -1;
                    travel_velocity *= BOUNCE_KINETIC;
                    //travel_velocity = redirect_slide(travel_velocity, Vector2.Up);
                }
                if (intent_position.Y < ROOM_TOP + VERTICAL_RADIUS) {
                    intent_position.Y = 2 * (ROOM_TOP + VERTICAL_RADIUS) - intent_position.Y;
                    travel_velocity.Y *= -1;
                    travel_velocity *= BOUNCE_KINETIC;
                    //travel_velocity = redirect_slide(travel_velocity, Vector2.Down);
                }

                this.GlobalPosition = intent_position;
                if (travel_velocity.Length() < TRAVEL_FRICTION * (float) delta) {
                    travel_velocity = Vector2.Zero;
                    traveling = false;
                    struggle_delay_timer = STRUGGLE_DELAY * (0.9f + (float) rand.NextDouble() / 5);
                }
                else {
                    travel_velocity -= TRAVEL_FRICTION * (float) delta * travel_velocity.Normalized();
                }
            }
            /* Otherwise, struggle back to jello body */
            else {
                /* Track struggle delay*/
                struggle_delay_timer = Mathf.Max(0, struggle_delay_timer - (float) delta);
                if (struggle_delay_timer <= 0) {
                    /* Track wobble */
                    wobble_timer += (float) delta;
                    if (wobble_timer > Mathf.Pi * 2) {
                        wobble_timer -= Mathf.Pi * 2;
                    }

                    eye_sprite.Rotation = 1 + 5 * Mathf.Sin(wobble_timer * 8) / 20;
                    eye_sprite.Position = 10 * Mathf.Abs(Mathf.Sin(wobble_timer * 8)) * Vector2.Up;

                    /* If close enough, enter jello */
                    if ((this.GlobalPosition - jello_parent.GlobalPosition).LengthSquared() < 4096) {
                        jello_parent.Add_Back(this);
                        this.contained = true;
                        enable_shadow(false);
                        eye_sprite.Position = Vector2.Zero;
                    }
                    /* Otherwise, move towards */
                    else {
                        this.GlobalPosition += (jello_parent.GlobalPosition - this.GlobalPosition).Normalized()
                            * (float) delta * STRUGGLE_SPEED;
                    }
                }
            }
        }    
    }

    /// <summary>
    /// Hurts the eye, which does increased damage to the jello.
    /// </summary>
    /// <param name="damage">Amount of damage to be dealt.</param>
    public void Hurt(int damage) {
        /* Only hurtable if not currently in jello */
        if (!contained) {
            jello_parent.Hurt(damage * DAMAGE_MULTIPLIER);
            hurt_flash = 0.3f;
            /* Play animation */
            if (eye_open) {
                Animation_Travel("OpenHurt");
            }
            else {
                Animation_Travel("ClosedHurt");
            }
        }
    }

    /// <summary>
    /// Spits out a given eye.
    /// </summary>
    /// <param name="new_position"> The position the eye ends up at </param>
    public void Spit(Vector2 new_position) {
        this.contained = false;
        enable_shadow(true);
        this.traveling = true;
        travel_velocity = (new_position - this.GlobalPosition).Normalized() * (BASE_TRAVEL_SPEED * (0.8f + (float) rand.NextDouble() * 0.4f));
        interacted_areas.Clear();
    }

    /// <summary>
    /// Sets the jello parent of this eye
    /// </summary>
    /// <param name="parent">The jello object that is this eyes parent</param>
    public void Set_Parent(Jello parent) {
        this.jello_parent = parent;
    }

    /// <summary>
    /// Uncontains the eye.
    /// </summary>
    public void Drop() {
        this.contained = false;
        enable_shadow(true);
        this.traveling = false;
    }

    /// <summary>
    /// Travels to a given animation state
    /// </summary>
    /// <param name="state">Name of the state to travel to</param>
    public void Animation_Travel(string state) {
        animation_state.Travel(state);
    }

    /// <summary>
    /// Opens or closes the eye.
    /// </summary>
    /// <param name="state">Whether to open (true) or close (false)</param>
    public void Open_Eye(bool state) {
        if (state) {
            if (!eye_open) {
                eye_open = true;
                Animation_Travel("Opening");
            } 
        }
        if (!state) {
            if (eye_open) {
                eye_open = false;
                Animation_Travel("Closing");
            }
        }
    }

    /// <summary>
    /// Enables or disables the eye shadows.
    /// </summary>
    /// <param name="enabled"> If the shadow should be enabled or disabled. </param>
    private void enable_shadow(bool enabled) {
        var current_modulate = shadow_sprite.Modulate;
        if (enabled) {
            current_modulate.A = 1;
        }
        else {
            current_modulate.A = 0;
        }
        shadow_sprite.Modulate = current_modulate;
    }
}
