using Godot;
using System;

public partial class NPCAnimator : Node3D
{
	[ExportGroup("Bobbing")]
	[Export]
	private float bobSpeed = 1.0f;
	[Export]
	private float bobAmount = 1.0f;
	private float bobOffset = 0.01f;
	[Export]
	private float initialBobRandomization = 0.1f;

	public void SetBobSpeed(float speed)
	{
		bobSpeed = speed;
	}

	public void SetBobAmount(float amount)
	{
		bobAmount = amount;
	}

	private Vector3 nominalPos;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		bobOffset = GD.Randf() * Mathf.Pi * 2;
		bobSpeed += initialBobRandomization * GD.Randf();
		nominalPos = Position;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float st = Mathf.Sin(Root.Timef() * bobSpeed + bobOffset);
		Position = nominalPos + Vector3.Up * st * bobAmount;
	}
}
