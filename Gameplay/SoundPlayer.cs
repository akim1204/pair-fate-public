using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

public partial class SoundPlayer : Node2D
{
	/// <summary> List of names corresponding to effect files. </summary>
	[Export]
	private string[] effect_names;

	/// <summary> List of paths to effect files to load. </summary>
	[Export]
	private string[] effect_files;

	/// <summary> Dictioanry of names to streams </summary>
	private Dictionary<string, AudioStream> effect_streams = new Dictionary<string, AudioStream>();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Loading sound files */
		for (int i = 0; i < effect_files.Length; i++)
		{
			AudioStream stream = GD.Load<AudioStream>(effect_files[i]);

			/* Add to dictioanry */
			if (i < effect_names.Length)
			{
				effect_streams.Add(effect_names[i], stream);
			}
			else
			{
				Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Sound player child of " + this.GetParent().Name + " is missing a name");
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// Plays a specific sound effect.
	/// </summary>
	/// <param name="effect_name">The sound effect to play. </param>
	public void Play_Effect(string effect_name, float volume = 0, float pitch_scale = 1.0f)
	{
		/* Error checking */
		if (effect_streams.ContainsKey(effect_name))
		{
			AudioStream sound_stream = effect_streams[effect_name];
			GameManager.Instance.Sound_Manager().Play_Sound(sound_stream, this.GlobalPosition, volume, pitch_scale);
		}
		else
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Sound player child of " + this.GetParent().Name + " recieved invalid effect name");
		}
	}

	/// <summary>
	/// Plays a specific sound effect to be heard by everyone
	/// </summary>
	/// <param name="effect_name">The sound effect to play. </param>
	public void Play_Effect_Static(string effect_name, float volume = 0, float pitch_scale = 1.0f)
	{
		/* Error checking */
		if (effect_streams.ContainsKey(effect_name))
		{
			AudioStream sound_stream = effect_streams[effect_name];
			GameManager.Instance.Sound_Manager().Play_Sound_Static(sound_stream, volume, pitch_scale);
		}
		else
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Sound player child of " + this.GetParent().Name + " recieved invalid effect name");
		}
	}
}
