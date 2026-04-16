using Godot;
using System;

public partial class WaterBucketSplash : Sprite2D
{
    /// <summary>
    /// How long the splash lasts.
    /// </summary>
    private float lifetime;

    public override void _Ready()
    {
        lifetime = 1.1f;
    }

    public override void _Process(double delta)
    {
        lifetime -= (float) delta;
        if (lifetime <= 0) {
            QueueFree();
        }
    }
}
