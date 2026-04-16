using Godot;
using System;

public partial class MusicManager : Node2D
{

	/// <summary> AudioStreamPlayer that plays the background music. </summary>
	private AudioStreamPlayer music_player;
	/// <summary> String of the current music path to prevent awkward cuts. </summary>
	private string current_music = "";

	public override void _Ready()
	{
		/* Get the music player */
		music_player = GetNode<AudioStreamPlayer>("MusicPlayer");
	}

	/// <summary>
	/// Gets the music player, in cases where playing music is done before initialization.
	/// </summary>
	public void Get_Music_Player()
	{
		/* Get the music player */
		music_player = GetNode<AudioStreamPlayer>("MusicPlayer");
	}

	/// <summary>
	/// Loads music from a specific file and plays it.
	/// </summary>
	/// <param name="music_path"> The path to the music </param>
	public void Play_Music(string music_path)
	{
		/* Stopping music */
		if (music_path == "")
		{
			current_music = music_path;
			music_player.Stop();
		}
		/* Only play music if it is different */
		if (current_music != music_path)
		{
			/* Store most recently loaded */
			current_music = music_path;

			/* Load music TODO: ERROR CHECK */
			AudioStream music_stream = GD.Load<AudioStream>(music_path);
			music_player.Stream = music_stream;
			music_player.VolumeDb = -30;
			music_player.Play();
		}
	}

	/// <summary>
	/// Loop music after each play.
	/// </summary>
	public void _on_music_player_finished()
	{
		music_player.Play();
	}
}
