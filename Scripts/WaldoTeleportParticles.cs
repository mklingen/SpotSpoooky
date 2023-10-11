using Godot;
using System;
using System.Collections.Generic;

public partial class WaldoTeleportParticles : Node3D
{
	[Export]
	float lifeTime = 1.5f;

	private bool firstIter = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (firstIter) {
            List<CpuParticles3D> particles = new List<CpuParticles3D>();
            Root.GetRecursive<CpuParticles3D>(this, particles);
            foreach (var p in particles) {
                p.Emitting = true;
            }
            firstIter = false;

        }
		lifeTime -= (float)delta;
		if (lifeTime < 0) {
			QueueFree();
		}
	}
}
