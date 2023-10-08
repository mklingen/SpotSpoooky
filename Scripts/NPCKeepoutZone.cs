using Godot;
using System;

public partial class NPCKeepoutZone : Node3D
{
    [Export]
	Aabb keepOutShape;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

	}

    public bool Query(Vector3 globalPt)
    {
        return keepOutShape.HasPoint(ToLocal(globalPt));
    }

}
