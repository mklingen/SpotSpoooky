using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
public partial class NPCRandomizer : Node3D
{

	[ExportGroup("Offsets")]
	[Export]
	Array<string> MeshNames = new Array<string>();

	private static AnimationOffsets[] Offsets = new AnimationOffsets[]{
		new AnimationOffsets {
			Pattern = "base_head",
            OtherPatterns = new string[] { "base_hands", "base_head_cheeks"},
			Offsets = new AnimationOffsets.Offset[] {
				new AnimationOffsets.Offset {
					X = 0,
					Y = 0,
					Frequency = 1
				},
				new AnimationOffsets.Offset
				{
					X = -1,
					Y = -2,
					Frequency = 1
				},
				new AnimationOffsets.Offset
				{
                    X = -1,
                    Y = -0,
                    Frequency = 1
                },
                new AnimationOffsets.Offset
                {
                    X = 1,
                    Y = -0,
                    Frequency = 1
                }
            }
		},
        new AnimationOffsets {
            Pattern = "base_body",
            Offsets = new AnimationOffsets.Offset[] {
                new AnimationOffsets.Offset {
                    X = 0,
                    Y = 0,
                    Frequency = 1
                },
                new AnimationOffsets.Offset
                {
                    X = -8,
                    Y = -8,
                    Frequency = 1
                },
                new AnimationOffsets.Offset
                {
                    X = 8,
                    Y = 0,
                    Frequency = 1
                },
                new AnimationOffsets.Offset
                {
                    X = 8,
                    Y = 8,
                    Frequency = 1
                },
                new AnimationOffsets.Offset
                {
                    X = 0,
                    Y = 8,
                    Frequency = 1
                },
                new AnimationOffsets.Offset
                {
                    X = 0,
                    Y = -8,
                    Frequency = 1
                }
            }
        },
        new AnimationOffsets {
            Pattern = "accessory_hair",
            Offsets = new AnimationOffsets.Offset[] {
                new AnimationOffsets.Offset {
                    X = 0,
                    Y = 0,
                    Frequency = 10
                },
                new AnimationOffsets.Offset
                {
                    X = -26,
                    Y = -3,
                    Frequency = 1
                },
                new AnimationOffsets.Offset
                {
                    X = 1,
                    Y = -3,
                    Frequency = 1
                },
                new AnimationOffsets.Offset
                {
                    X = -10,
                    Y = -2,
                    Frequency = 2
                },
            }
        }
    };

	enum MeshType
	{
		Base,
		Accessory
	}

	string MeshTypeToString(MeshType type)
	{
		return type.ToString().ToLower();
	}

	private MeshType MeshNameToType(string type)
	{
		if (type.Contains(MeshTypeToString(MeshType.Accessory))) {
			return MeshType.Accessory;
		}
		return MeshType.Base;
	}

	private string GetSubmeshType(string name, MeshType type)
	{
		string typeString = MeshTypeToString(type);
		int typeIndex = name.IndexOf(typeString);
        string nextString = name.Substring(typeIndex + typeString.Length + 1);
		string beforeUnderScore = nextString.Substring(0, nextString.IndexOf('_'));
		return beforeUnderScore;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		var accessories = FindChildren(MeshTypeToString(MeshType.Accessory)+"*", "MeshInstance3D");
		
		System.Collections.Generic.Dictionary<string, List<Node>> accessoryDict = new System.Collections.Generic.Dictionary<string, List<Node>>();
		// Gather a list of accessories for each possible type.
		foreach (Node accessory in accessories) {
			var typeName = GetSubmeshType(accessory.Name, MeshType.Accessory);
			if (accessoryDict.ContainsKey(typeName)) {
				accessoryDict[typeName].Add(accessory);
			} else {
				accessoryDict[typeName] = new List<Node>();
				accessoryDict[typeName].Add(accessory);
			}
        }
		// Check each accessory per type.
		foreach(var accessoryKVP in accessoryDict) {
			var list = accessoryKVP.Value;
			int idx = GD.RandRange(0, list.Count - 1);
			for (int i = 0; i < list.Count; i++) {
				if (i != idx) {
					// Delete all but selected.
					list[i].QueueFree();
				}
			}
		}

        // Offset all meshes.
        foreach (var offsets in Offsets) {
            var meshes = FindChildren(offsets.Pattern + "*", "MeshInstance3D");
            if (meshes.Count == 0) {
                GD.PrintErr($"Couldn't find mesh {offsets.Pattern}");
                continue;
            }

            if (offsets.OtherPatterns != null) {
                foreach (string pattern in offsets.OtherPatterns) {
                    meshes.AddRange(FindChildren(pattern + "*", "MeshInstance3D"));
                }
            }
            var offset = offsets.GetRandomOffset();
            foreach (var obj in meshes) {
				if (obj.IsQueuedForDeletion()) {
					continue;
				}
                MeshInstance3D mesh = obj as MeshInstance3D;
                if (mesh == null) {
                    continue;
                }
                mesh.SetInstanceShaderParameter("OffsetX", offset.X);
                mesh.SetInstanceShaderParameter("OffsetY", offset.Y);
            }
        }
    }


}
