using Godot;
using System;

public partial class Highlightable : Sprite2D
{
	/// <summary>
	/// Shader that produces item select outline.
	/// </summary>
	protected ShaderMaterial outline_shader;

	public override void _Ready()
	{
		/* Initially disable outline */
		outline_shader = Material as ShaderMaterial;
		Toggle_Outline(false);
	}

	/// <summary>
	/// Toggles the outline of the item.
	/// </summary>
	/// <param name="state">The outline state of the item </param>
	public void Toggle_Outline(bool state)
	{
		this.outline_shader.SetShaderParameter("active", state);
	}

}
