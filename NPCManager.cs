using Godot;
using System;
using System.Collections.Generic;

public partial class NPCManager : Node3D
{
    [ExportGroup("NPC Spawn")]
    [Export]
	private int numNPCs;
	[Export]
	private Aabb npcSpawnAABB;
	[Export]
	private PackedScene npcPrefab;

	private List<NPC> npcs;
	private List<Path3D> paths;
	private List<NPCKeepoutZone> keepoutZones;

	private void GetRecursive<T>(Node parent, List<T> outNodes) where T : class
	{
		if (parent is T) {
			outNodes.Add(parent as T);
		}
		foreach (var child in parent.GetChildren()) {
			GetRecursive<T>(child, outNodes);
		}

	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		npcs = new List<NPC>();
		paths = new List<Path3D>();
		keepoutZones = new List<NPCKeepoutZone>();
		GetRecursive(GetTree().Root, keepoutZones);
		foreach (var node in GetChildren()) {
			if (node is Path3D) {
				paths.Add(node as Path3D);
			}
		}
		GD.Print(String.Format("There are {0} paths.", paths.Count));

		for (int i = 0; i < numNPCs; i++) {
            SpawnNPC();
		}
		GD.Print(String.Format("There are {0} npcs.", npcs.Count));
	}

	private void SpawnNPC()
	{
		bool spawned = false;
		do {
			spawned = TrySpawnNPC();
		} while (!spawned);
	}


    private bool TrySpawnNPC()
    {
		Vector3 randPos = -npcSpawnAABB.Size * 0.5f + new Vector3((float)GD.RandRange(npcSpawnAABB.Position.X, npcSpawnAABB.End.X),
			(float)GD.RandRange(npcSpawnAABB.Position.Y, npcSpawnAABB.End.Y), (float)GD.RandRange(npcSpawnAABB.Position.Z, npcSpawnAABB.End.Z));
		foreach (var keepout in keepoutZones) {
			if (keepout.Query(ToGlobal(randPos))) {
				return false;
			}
		}
		NPC spawnedNPC = npcPrefab.Instantiate<NPC>();
		AddChild(spawnedNPC);
		spawnedNPC.Position = randPos;
		foreach (var path in paths) {
			if (path.ToGlobal(path.Curve.GetClosestPoint(path.ToLocal(spawnedNPC.GlobalPosition))).DistanceTo(spawnedNPC.GlobalPosition) < 5) {
				spawnedNPC.SetPath(path, (float)GD.RandRange(-10.0, 10.0), new Vector3((float)GD.RandRange(-2.0f, 2.0f), 0.0f, (float)GD.RandRange(-2.0f, 2.0f)));
				break;
			}
		}
		npcs.Add(spawnedNPC);
		GD.Print("Spawn.");
		return true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
