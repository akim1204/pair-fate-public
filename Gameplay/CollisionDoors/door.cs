using Godot;
using System;
/// <summary>
/// Door is currently the implementation that works with saveLoad and GameManager to navigate
/// Scenes.
/// </summary>
public partial class door : Area2D
{
	/// <summary> Path to the scene the door loads to.</summary>
	[Export] 
	public string Scene_Path;
	
	/// <summary>
	/// Spawn index in the corresponding scene being loaded.
	/// </summary>
	[Export]
	public int Spawn_Index = 0;
	

	/// <summary>
	/// Once entered, if its a Player, call GameManager's LoadLevel
	/// </summary>
	/// <param name="body">Object that touches the collide2D</param>
				
	private void _on_body_entered(Node2D body)
	 {
		GD.Print("trying to change scenes");
		if (body is Player) {
			GameManager.Instance.Load_Level(Scene_Path, Spawn_Index);		
		}
	}
}

