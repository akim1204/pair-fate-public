using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class textbox : CanvasLayer
{

	private enum STATES
	{
		READING,
		FINISHED
	}

	private TextureRect container;
	private Label dialogue_box;
	private string current_text = "";
	private float text_timer;
	private const float TEXT_SPEED = 50;
	private Label name;
	private Label cont;
	private STATES curr_state;
	private List<CharacterDialogue> script;
	private int queue_val = 0;

	/* Sound player for effects */

	private SoundPlayer sound_player;



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.container = GetNode<TextureRect>("TextureRect");
		this.dialogue_box = GetNode<Label>("TextureRect/Panel/dialogue");
		this.name = GetNode<Label>("TextureRect/Panel/name");
		this.cont = GetNode<Label>("TextureRect/Panel/continue");
		this.sound_player = GetNode<SoundPlayer>("SoundPlayer");

		// start queueing + displaying text
		this.curr_state = STATES.FINISHED;

		hide_textbox();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		switch (this.curr_state)
		{
			case STATES.READING:
				text_timer += (float)delta * TEXT_SPEED;
				if (text_timer >= current_text.Length)
				{
					text_timer = current_text.Length;
					curr_state = STATES.FINISHED;
				}
				this.dialogue_box.Text = current_text.Substring(0, (int)text_timer);
				break;
			case STATES.FINISHED:
				break;
		}
	}

	private void hide_textbox()
	{
		this.container.Visible = false;
		this.dialogue_box.Text = "";
		this.name.Text = "";
		this.cont.Text = "";
	}

	private void show_textbox()
	{
		this.container.Visible = true;
	}

	/// <summary>
	/// Parses a given json file for dialogue.
	/// </summary>
	/// <param name="filepath"></param>
	public void Parse_Textfile(string filepath)
	{
		Logger.Instance.Log(Logger.LOG_LEVELS.TRACE, "Dialogue File Accessed");

		using (StreamReader r = new StreamReader(filepath))
		{
			string json = r.ReadToEnd();
			// Deserialize the JSON array into a list of CharacterDialogue objects
			this.script = JsonSerializer.Deserialize<List<CharacterDialogue>>(json);
		}
	}

	public bool queue_text()
	{
		if (queue_val < this.script.Count)
		{
			show_textbox();
			//Logger.Instance.Log(Logger.LOG_LEVELS.DEBUG, "queue position: " + this.queue_val);
			// first get the information at the current queue position
			CharacterDialogue curr_line = script[this.queue_val];

			sound_player.Play_Effect("DialogueShort");

			// update the text
			this.name.Text = curr_line.char_name;
			this.current_text = curr_line.char_dialogue;
			this.dialogue_box.Text = "";
			this.text_timer = 0;
			this.curr_state = STATES.READING;


			// move on to the next position in the queue
			this.queue_val += 1;
			return true;
		}
		else
		{
			// Hide the textbox when done.
			hide_textbox();
			return false;
		}
	}

	public bool progress_textbox()
	{
		/* If currently in the middle of displaying */
		if (curr_state == STATES.READING)
		{
			this.dialogue_box.Text = current_text;
			this.curr_state = STATES.FINISHED;
			//sound_player.Play_Effect("DialogueShort");
			return true;
		}
		/* Otherwise, queue text */
		return queue_text();
	}


}
public class CharacterDialogue
{
	public string char_name { get; set; }
	public string char_dialogue { get; set; }
}
