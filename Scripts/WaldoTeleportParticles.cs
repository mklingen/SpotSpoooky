using Godot;
using System;
using System.Collections.Generic;

public partial class WaldoTeleportParticles : Node3D
{
	[Export]
	float lifeTime = 1.5f;
	float maxLifeTime = 0;

	[Export]
	private Curve lightCurve;

	private bool firstIter = true;

	private Light3D light;
	private float startLightIntensity = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		maxLifeTime = lifeTime;
		light = Root.FindNodeRecusive<Light3D>(this);
		if (light != null) {
			startLightIntensity = light.LightEnergy;
		}
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
		if (light != null && lightCurve != null) {
			light.LightEnergy = lightCurve.Sample(lifeTime / maxLifeTime) * startLightIntensity;
		}
		if (lifeTime < 0) {
			QueueFree();
		}

	}
}
