using Godot;
using System;
using System.Collections.Generic;

/* Controls the different isntances of jellos */
public partial class JelloController : Node2D
{
    
    /// <summary> Hp of the boss. </summary>
    private int DEFAULT_HEALTH = 25;

    // Breakpoints: 5, 10, 15, 25
    private List<int> breakpoints = new List<int>{5, 10, 15, 25};

    private float TOTAL_BAR_SIZE = 900;


    /// <summary>
    ///  List of jello instances.
    /// </summary>
    private Dictionary<Jello, int> jellos = new Dictionary<Jello, int>();

    private PackedScene jello_prefab;

    public override void _Ready()
    {
        /* Get Jello Prefab */
        jello_prefab = GD.Load<PackedScene>("res://Bosses/Jello/Jello.tscn");

        /* Create the initial jello */
        var inst = jello_prefab.Instantiate<Jello>();
        inst.Initialize_Eyes(6);
        inst.Set_Controller(this);
        CallDeferred("add_child", inst);
        jellos.Add(inst, breakpoints[3]);
        inst.GlobalPosition = new Vector2(1500, 500);
    }

    public override void _Process(double delta) {
        /* Draw the healthbar */
        QueueRedraw();

        /* Reset button */
        if (Input.IsActionJustPressed("ui_r")) {
            GetTree().ReloadCurrentScene();
            var player_bag = GetNode<PlayerBag>("/root/PlayerBag");
            var item_bag = GetNode<ItemBag>("/root/ItemBag");
            player_bag.ClearPlayers();
            item_bag.ClearAvailableItems();
            item_bag.ClearInteractables();
            item_bag.ClearCarriedItems();
        }
    }

    public override void _Draw() {
        // Calculate HP Splits
        float total_sum = 0;
        foreach (var health_val in jellos.Values) {
            if (health_val > 15) {
                total_sum += 8;
            }
            else if (health_val > 10) {
                total_sum += 4;
            }
            else if (health_val > 5) {
                total_sum += 2;
            }
            else {
                total_sum += 1;
            }
        }

        // Draw hp bar
        float left_side = 450;
        float width;
        foreach (var health_val in jellos.Values) {
            if (health_val > breakpoints[2]) {
                width = 8 * TOTAL_BAR_SIZE / total_sum;
                DrawLine(new Vector2(left_side, 35), new Vector2(left_side + width * ((float) health_val / breakpoints[3]), 35), Colors.Orange, 70);
                left_side += width;
            }
            else if (health_val > breakpoints[1]) {
                width = 4 * TOTAL_BAR_SIZE / total_sum;
                DrawLine(new Vector2(left_side, 35), new Vector2(left_side + width * ((float) health_val / breakpoints[2]), 35), Colors.Orange, 70);
                left_side += width;
            }
            else if (health_val > breakpoints[0]) {
                width = 2 * TOTAL_BAR_SIZE / total_sum;
                DrawLine(new Vector2(left_side, 35), new Vector2(left_side + width * ((float) health_val / breakpoints[1]), 35), Colors.Orange, 70);
                left_side += width;
            }
            else {
                width = 1 * TOTAL_BAR_SIZE / total_sum;
                DrawLine(new Vector2(left_side, 35), new Vector2(left_side + width * ((float) health_val / breakpoints[0]), 35), Colors.Orange, 70);
                left_side += width;
            }
        }
        /* Draw bounding box */
        DrawRect(new Rect2(new Vector2(445, 5), new Vector2(10 + TOTAL_BAR_SIZE, 70)), Colors.Black, false, 20);
        
    }

    public void Hurt(int damage, Jello inst) {
        /* Only hit if instance still exists */
        if (jellos.ContainsKey(inst)) {
            /* Deal damage */
            jellos[inst] -= damage;

            /* Breakpoints */
            for (int i = 0; i <= 2; i++) {
                var removed = false;
                /* If damage crossed breakpoint*/
                if (jellos[inst] <= breakpoints[i] & jellos[inst] + damage > breakpoints[i]) {
                    removed = true;
                    /* Drop all eyes */
                    var jello_eyes = inst.Drop_Eyes();
                    /* Split into two smaller jellos */
                    for (int j = 0; j < 2; j++) {

                        /* Instantiate and initialize smaller jellos */
                        var inst2 = jello_prefab.Instantiate<Jello>();
                        //inst2.Provide_Eyes(jello_eyes.GetRange((int) Math.Pow(2, i) * j, (int) Math.Pow(2, i)));
                        inst2.Initialize_Eyes((int) Math.Pow(2, i));
                        inst2.Set_Controller(this);
                        CallDeferred("add_child", inst2);
                        jellos.Add(inst2, breakpoints[i]);
                        inst2.Set_Scale(0.4f + 0.15f * i);
                        inst2.GlobalPosition = new Vector2(inst.GlobalPosition.X - 100 + 200 * j, inst.GlobalPosition.Y);

                    }
                    /* Destroy all jellos */
                    inst.QueueFree();
                    jellos.Remove(inst);
                }
                /* Prevent checking multiple breakpoints */
                if (removed) {
                    break;
                }
            }
        }
    }
}
