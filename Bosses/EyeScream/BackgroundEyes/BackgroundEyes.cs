using Godot;
using System;
using System.Collections.Generic;

public partial class BackgroundEyes : Node2D
{
	private PackedScene eye_prefab = GD.Load<PackedScene>("res://Bosses/EyeScream/BackgroundEyes/BackgroundEye.tscn");

	private List<BackgroundEye> background_eyes = new List<BackgroundEye>();
	private float ROOM_LEFT = EyeScreamController.ROOM_LEFT,
		ROOM_RIGHT = EyeScreamController.ROOM_RIGHT,
		ROOM_TOP = EyeScreamController.ROOM_TOP,
		ROOM_BOTTOM = EyeScreamController.ROOM_BOTTOM;

	private float ROOM_CENTERX = (EyeScreamController.ROOM_RIGHT + EyeScreamController.ROOM_LEFT) / 2;
	private float ROOM_CENTERY = (EyeScreamController.ROOM_TOP + EyeScreamController.ROOM_BOTTOM) / 2;
	private float ROOM_WIDTHX = (EyeScreamController.ROOM_RIGHT - EyeScreamController.ROOM_LEFT) / 2;
	private float ROOM_WIDTHY = (EyeScreamController.ROOM_TOP - EyeScreamController.ROOM_BOTTOM) / 2;

	private const int EYE_COUNT = 40;
	private const int EYE_COUNT_LOWER = 10;
	private float timer = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		/* Create eyes */
		for (int i = 0; i < EYE_COUNT; i++)
		{
			var eye_inst = eye_prefab.Instantiate<BackgroundEye>();
			background_eyes.Add(eye_inst);
			this.AddChild(eye_inst);
		}
		for (int i = 0; i < EYE_COUNT_LOWER; i++)
		{
			var eye_inst = eye_prefab.Instantiate<BackgroundEye>();
			background_eyes.Add(eye_inst);
			this.AddChild(eye_inst);
		}
		Modulate = new Color(0, 0, 0, 1);
		Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		/* Move eyes */
		timer += (float)delta / 5;
		for (int i = 0; i < EYE_COUNT; i++)
		{
			float roomx = (Mathf.Cos(timer + i * 15) + Mathf.Sin(timer * 2 + i * 19)) / 2;
			float roomy = (Mathf.Sin(timer + i * 15) + Mathf.Sin(timer * 2 + i * 17)) / 2;
			background_eyes[i].GlobalPosition = new Vector2(
				ROOM_CENTERX + ROOM_WIDTHX * roomx, ROOM_CENTERY + ROOM_WIDTHY * roomy - 600
			);
		}
		for (int i = EYE_COUNT; i < EYE_COUNT + EYE_COUNT_LOWER; i++)
		{
			float roomx = (Mathf.Cos(timer + i * 15) + Mathf.Sin(timer * 2 + i * 19)) / 2;
			/* Emphasize towards edges */
			roomx = (1 - Mathf.Pow(1 - Mathf.Abs(roomx), 2)) * Mathf.Sign(roomx);
			float roomy = (Mathf.Sin(timer + i * 15) + Mathf.Sin(timer * 2 + i * 17)) / 2;
			background_eyes[i].GlobalPosition = new Vector2(
				ROOM_CENTERX + ROOM_WIDTHX * roomx, ROOM_CENTERY + ROOM_WIDTHY * roomy
			);
		}
	}
}
