using Godot;
using System;

public partial class BulletParticles : Node3D, Player.IShootHandler
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}


	public void OnShoot(Vector3 shootFrom, Vector3 shootTo)
	{
        CpuParticles3D particles = GetChild<CpuParticles3D>(0);
        if (particles != null) {
            particles.Emitting = true;
        }
        Visible = true;
        GlobalPosition = shootTo;
    }
}
