using Godot;
using System;
using System.Collections.Generic;

public partial class PianoPlayer : Node
{
	private string[] keys = new string[] { "a", "a-", "b", "c", "c-", "d", "d-", "e", "f", "f-", "g", "g-" };
	private string[] octaves = new string[] { "3", "4", "5" };
	private Dictionary<string, AudioStream> effect_streams = new Dictionary<string, AudioStream>();
	private string piano_path = "res://Bosses/EyeScream/SoundEffects/PianoKeys/";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Load piano keys */
		foreach (string key in keys)
		{
			foreach (string octave in octaves)
			{
				AudioStream stream = GD.Load<AudioStream>(piano_path + key + octave + ".mp3");
				effect_streams.Add(key + octave, stream);
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Play_Key(string key, int octave, float volume = 0)
	{
		string effect_name = key + octave.ToString();
		/* Error checking */
		if (effect_streams.ContainsKey(effect_name))
		{
			AudioStream sound_stream = effect_streams[effect_name];
			GameManager.Instance.Sound_Manager().Play_Sound_Static(sound_stream, volume, 1);
		}
		else
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Sound player child of " + this.GetParent().Name + " recieved invalid effect name");
		}
	}
	public void Play_Sound(int index, float volume)
	{
		string key = keys[index % 12];
		string octave = octaves[index / 12];
		string effect_name = key + octave.ToString();
		/* Error checking */
		if (effect_streams.ContainsKey(effect_name))
		{
			AudioStream sound_stream = effect_streams[effect_name];
			GameManager.Instance.Sound_Manager().Play_Sound_Static(sound_stream, volume, 1);
		}
		else
		{
			Logger.Instance.Log(Logger.LOG_LEVELS.ERROR, "Sound player child of " + this.GetParent().Name + " recieved invalid effect name");
		}
	}
}
