using Godot;
using System;

public partial class InteractablePatchSpawner : Interactable
{
	private Node WORLD;
	private FrostingPatch existing_patch;
	private PackedScene patch_preload;
	[Export]
	public int upward_spawn_range = 700;
	[Export]
	public int sideways_spawn_range = 1024;
	private Random rand;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		this.interactable_name = "InteractablePatchSpawner";

		rand = new Random();

		patch_preload = GD.Load<PackedScene>("res://Bosses/IsaiahBoss/StagePlatform/FrostingPatch/FrostingPatch.tscn");

	}
	/// <summary>
	/// Overriding get_type of this interactable type.
	/// </summary>
	public override InteractableTypes Get_Type()
	{
		return InteractableTypes.Utility;
	}

	/// <summary>
	/// Refills water bucket if its carried.
	/// </summary>
	/// <param name="player"> Player who interacts with interactable </param>
	public override void Handle_Action(PlayerItem player)
	{
		if (existing_patch != null) {
			Delete_Patch();
		}
		Add_Patch();
	}
	private Vector2 Get_Random_Pos()
	{
		float y = this.GlobalPosition.Y - (float) rand.Next(0,upward_spawn_range);
		float x = this.GlobalPosition.X + rand.Next(0,sideways_spawn_range) * (rand.Next(2) - 1);
		return new Vector2(x,y);		

	}
	public void Add_Patch()
	{
		Vector2 rand_pos = Get_Random_Pos();
		Rpc("RPC_Add_Patch", rand_pos);
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Add_Patch(Vector2 pos)
	{
		var patch = patch_preload.Instantiate<FrostingPatch>();
		AddChild(patch);
		patch.Initialize(pos);
		this.existing_patch = patch;

	}
	public void Delete_Patch()
	{
		Rpc("RPC_Delete_Patch");
	}
	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void RPC_Delete_Patch()
	{
		if (existing_patch != null) {
			existing_patch.Destroy();
			existing_patch = null;
		}
		return;

	}
}
