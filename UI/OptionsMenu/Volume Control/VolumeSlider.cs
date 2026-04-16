using Godot;
using System;

public partial class VolumeSlider : VSlider
{
	/// <summary>
	/// Set this to the name of the audio bus that the slide controls.
	/// </summary>
	[Export]
	public string _bus_name;
	/// <summary>
	/// Index of the audio bus that is named _bus_name.
	/// </summary>
	private int _bus_idx;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_bus_idx = AudioServer.GetBusIndex(_bus_name);
		if (_bus_idx == -1)
		{
			return;
		}
		this.Value = db_2_linear(AudioServer.GetBusVolumeDb(_bus_idx));

	}

	public void _on_value_changed(float val)
	{
		if (_bus_idx == -1)
		{
			return;
		}
		AudioServer.SetBusVolumeDb(
			_bus_idx, (float)linear_2_db((double)val));
	}

	/// <summary>
	/// Converts linear gain factor to decibels as decibels are logarithmic
	/// </summary>
	/// <param name="lin_val"> Linear value to convert to decibels.</param>
	/// <returns> Double decibel value.</returns>
	private double linear_2_db(double lin_val)
	{
		return Math.Log(lin_val) * 8.6858896380650365530225783783321;
	}
	/// <summary>
	/// Converts decibel to linear gain factor.
	/// </summary>
	/// <param name="db_val">Decibel to convert to linear value.</param>
	/// <returns> Double linear value from scale 0-1. </returns>
	private double db_2_linear(double db_val)
	{
		return Math.Pow(10.0f, db_val / 8.6858896380650365530225783783321);
	}
}
