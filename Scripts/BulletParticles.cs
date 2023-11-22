using Godot;
using System;

public partial class BulletParticles : Node3D, Player.IShootHandler
{

    [ExportGroup("Special Effects")]
    [Export]
    private PackedScene bulletHole;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}


	public void OnShoot(Vector3 shootFrom, Vector3 shootTo)
	{
        Visible = true;
        GlobalPosition = shootTo;
        CpuParticles3D particles = GetChild<CpuParticles3D>(0);
        if (particles != null) {
            particles.Emitting = true;
            particles.Restart();
        }
        if (bulletHole != null) {
            var instantiate = bulletHole.Instantiate<Node3D>();
            GetTree().CurrentScene.AddChild(instantiate);
            instantiate.GlobalPosition = this.GlobalPosition;
            instantiate.LookAt(shootFrom);
        }
    }
}
