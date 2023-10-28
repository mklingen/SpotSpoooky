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


	[Export(PropertyHint.Layers3DPhysics)]
	private uint buildingCollisionMask;

	public List<NPC> GetNPCS()
	{
		return npcs;
	}

	public List<Path3D> GetPaths()
	{
		return paths;
	}

	public List<NPCKeepoutZone> GetKeepoutZones()
	{
		return keepoutZones;
	}

	private Player player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		npcs = new List<NPC>();
		paths = new List<Path3D>();
		keepoutZones = new List<NPCKeepoutZone>();
		Root.GetRecursive(GetTree().Root, keepoutZones);
		foreach (var node in GetChildren()) {
			if (node is Path3D) {
				paths.Add(node as Path3D);
			}
		}
		player = Root.FindNodeRecusive<Player>(GetTree().Root);
	}

	private void SpawnNPC()
	{
		bool spawned = false;
		do {
			spawned = TrySpawnNPC();
		} while (!spawned);
	}

	public Vector3 GetValidSpawnLocation()
	{
        return ToGlobal(-npcSpawnAABB.Size * 0.5f + new Vector3((float)GD.RandRange(npcSpawnAABB.Position.X, npcSpawnAABB.End.X),
            (float)GD.RandRange(npcSpawnAABB.Position.Y, npcSpawnAABB.End.Y), (float)GD.RandRange(npcSpawnAABB.Position.Z, npcSpawnAABB.End.Z)));
    }

	public bool IsInKeepOutZone(Vector3 randPos)
	{
        foreach (var keepout in keepoutZones) {
            if (keepout.Query(randPos)) {
                return true;
            }
        }
		return false;
    }

	public bool CanPlayerSee(Vector3 globalPos)
	{
		return player.CanPlayerSee(globalPos, buildingCollisionMask);

    }

    private bool TrySpawnNPC()
    {
		Vector3 randPos = ToLocal(GetValidSpawnLocation());
		if (IsInKeepOutZone(ToGlobal(randPos))) {
			return false;
		}
		if (!CanPlayerSee(ToGlobal(randPos))) {
			return false;
		}
		NPC spawnedNPC = npcPrefab.Instantiate<NPC>();
		AddChild(spawnedNPC);
		spawnedNPC.GlobalPosition = ToGlobal(randPos);

        foreach (var path in paths) {
			if (path.ToGlobal(path.Curve.GetClosestPoint(path.ToLocal(spawnedNPC.GlobalPosition))).DistanceTo(spawnedNPC.GlobalPosition) < 5) {
				spawnedNPC.SetPath(path, (float)GD.RandRange(-10.0, 10.0), new Vector3((float)GD.RandRange(-2.0f, 2.0f), 0.0f, (float)GD.RandRange(-2.0f, 2.0f)));
				break;
			}
		}
		npcs.Add(spawnedNPC);
		{
			var character = Root.FindNodeRecusive<AnimatableBody3D>(spawnedNPC);
			if (character != null) {
				character.MoveAndCollide(Vector3.Zero);
			}
		}
		return true;
    }

	private bool spawnedAllNPCs = false;
	private int frameCounter = 0;

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		if (!spawnedAllNPCs) {
			frameCounter++;
			if (frameCounter % 2 == 0) {
				int numToSpawnPerFrame = 20;
				int origCount = npcs.Count;
				for (int i = npcs.Count; i < Math.Min(numNPCs, origCount + numToSpawnPerFrame); i++) {
					SpawnNPC();
				}
				if (npcs.Count >= numNPCs) {
					spawnedAllNPCs = true;
				}
			}
		}
    }

	public void EatNPC(NPC selected)
	{
		npcs.Remove(selected);
		selected.GotEaten(selected);
	}
}
