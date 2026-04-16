using Godot;
using System;
using System.Collections.Generic;

public partial class SoundManager : Node
{

	public enum sound_types
	{
		SFX,
	}

	/// <summary> The number of unique sound channels. </summary>
	const int channel_num = 16;

	/// <summary> Sound channels for global sounds </summary>
	const int static_channel_num = 4;


	/// <summary> List of available audio channels.</summary>
	List<AudioStreamPlayer2D> channels = new List<AudioStreamPlayer2D>();

	/// <summary> List of available static audio channels.</summary>
	List<AudioStreamPlayer> static_channels = new List<AudioStreamPlayer>();

	/// <summary> Least recently used channel. </summary>
	int channel_index = 0;

	/// <summary> Least recently used static channel. </summary>
	int static_channel_index;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Create audio streams */
		for (int i = 0; i < channel_num; i++)
		{
			var channel = new AudioStreamPlayer2D();
			this.AddChild(channel);
			channels.Add(channel);
		}
		/* Create audio streams */
		for (int i = 0; i < static_channel_num; i++)
		{
			var channel = new AudioStreamPlayer();
			this.AddChild(channel);
			static_channels.Add(channel);
		}
	}

	public void Play_Sound(AudioStream sound_stream, Vector2 position, float volume = 0, float pitch_scale = 1)
	{
		/* Get the next channel */
		channel_index = (channel_index + 1) % channel_num;

		/* Play sound */
		AudioStreamPlayer2D curr_channel = channels[channel_index];
		curr_channel.Stream = sound_stream;
		curr_channel.GlobalPosition = position;
		curr_channel.PitchScale = pitch_scale;
		curr_channel.VolumeDb = volume;
		curr_channel.Play();
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void Play_Sound_Static(AudioStream sound_stream, float volume = 0, float pitch_scale = 1)
	{
		/* Get the next channel */
		static_channel_index = (static_channel_index + 1) % static_channel_num;

		/* Play sound */
		AudioStreamPlayer curr_channel = static_channels[static_channel_index];
		curr_channel.Stream = sound_stream;
		curr_channel.PitchScale = pitch_scale;
		curr_channel.VolumeDb = volume;
		curr_channel.Play();
	}
}
