using Godot;
using System;

public partial class FakeReticle : MeshInstance3D
{
    public override void _Process(double delta)
    {
        float zerone = Mathf.Sin(Root.Timef() * 4);
        Scale = Vector3.One * 1.1f + Vector3.One * (zerone * 0.25f);
        base._Process(delta);
    }
}
