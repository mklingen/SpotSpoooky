using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UVOffsetChanger : Node3D
{
	private void setOffset(ShaderMaterial material, Vector2 offset)
	{ 
        material.SetShaderParameter("OffsetX", offset.X);
        material.SetShaderParameter("OffsetY", offset.Y);
    }

	[Export]
	Array<string> MeshNames = new Array<string>();
	[Export]
	Array<Array<Vector2>> Offsets = new Array<Array<Vector2>>();

	private static System.Collections.Generic.Dictionary<Vector2I, ShaderMaterial> MaterialOffsetDictionary = new System.Collections.Generic.Dictionary<Vector2I, ShaderMaterial>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for (int i = 0; i < MeshNames.Count; i++) { 
			MeshInstance3D mesh = FindChildren(MeshNames[i], "MeshInstance3D").FirstOrDefault<Node>() as MeshInstance3D;
			if (mesh == null) {
				GD.PrintErr($"Couldn't find mesh {MeshNames[i]}");
				continue;
			}

			var offset = Offsets[i][GD.RandRange(0, Offsets[i].Count - 1)];
			mesh.SetInstanceShaderParameter("OffsetX", offset.X + GD.Randf());
			mesh.SetInstanceShaderParameter("OffsetY", offset.Y);
		}
	}


}
