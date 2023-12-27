using Godot;
using System;

public partial class NPCKeepoutZone : Node3D
{
    [Export]
	Aabb keepOutShape;


    public override void _Ready()
	{
        // Recenter AAbb because "x,y,w,h" actually refer to lowest point, not center, which is so, so dumb.
        keepOutShape = new Aabb(keepOutShape.Position - keepOutShape.Size * 0.5f, keepOutShape.Size);
	}

    public bool Query(Vector3 globalPt)
    {
        return keepOutShape.HasPoint(ToLocal(globalPt));
    }

}
