using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class AmbientSoundHandler : Node2D
{
	private Random rand;

	private double _orchestral1_timer = 45;
	AudioStreamPlayer _orchestral1;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_orchestral1 = this.GetNode<AudioStreamPlayer>("Orchestral1");
		rand = new Random();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		play_orchestral1(delta);

	}

	private void play_orchestral1(double delta) {
		_orchestral1_timer -= delta;
		if (_orchestral1_timer <= 0) {
			GD.Print("Playing");
			if (!_orchestral1.Playing) {
				GD.Print("Can Play");
			
				_orchestral1.Play();
			}
			_orchestral1_timer = rand.Next(35,65);
		}
	}
}
