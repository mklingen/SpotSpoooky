using Godot;
using System;
using System.Collections.Generic;

public partial class NPC : Node3D
{
	private CharacterBody3D character;
	[ExportCategory("Path Following")]
	[Export]
	private Path3D pathToFollow;
	[Export]
	private float pathFollowSpeed;
	[Export]
	private Vector3 pathFollowOffset;
	private float currDistAlongPath = 0.0f;
	private float pathLength = 0.0f;
	private float pathOffset = 0.0f;


	public void SetPath(Path3D path, float speed, Vector3 offsetRTPath)
	{
		pathFollowOffset = offsetRTPath;
		pathFollowSpeed = speed;
		pathToFollow = path;
        pathLength = pathToFollow.Curve.GetBakedLength();
        pathOffset = pathToFollow.Curve.GetClosestOffset(pathToFollow.ToLocal(GlobalPosition));
        currDistAlongPath = pathOffset;
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		character = GetChild<CharacterBody3D>(0);
		if (pathToFollow != null) {
			SetPath(pathToFollow, pathFollowSpeed, pathFollowOffset);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Move along the path.
		if (pathToFollow != null) {
			currDistAlongPath = currDistAlongPath + pathFollowSpeed * (float)delta ;
			// Wrap the path (it is supposed to circular, I guess.
			if (currDistAlongPath > pathLength) {
				currDistAlongPath = (currDistAlongPath - pathLength);
			} else if (currDistAlongPath < 0.0f) {
				currDistAlongPath = pathLength - currDistAlongPath;
			}
			// Get the position of the object along the path.
			GlobalPosition = pathToFollow.ToGlobal(pathToFollow.Curve.SampleBaked(currDistAlongPath) + pathFollowOffset);
		} else {
			GlobalPosition = character.GlobalPosition;
			character.Velocity = Vector3.Zero;
		}
	}
}
