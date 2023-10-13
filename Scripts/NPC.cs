using Godot;
using System;
using System.Collections.Generic;

public partial class NPC : AnimatableBody3D, Player.IGotShotHandler, Waldo.IEatHandler
{
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
			Vector3 nextPos = pathToFollow.ToGlobal(pathToFollow.Curve.SampleBaked(currDistAlongPath) + pathFollowOffset);
			ConstantLinearVelocity = (nextPos - GlobalPosition) * (1.0f / (float)delta);
			GlobalPosition = nextPos;
		}
	}

	// Called when player shot NPC.
	public void GotShot()
	{
		var root = GetTree().Root.GetChild(0) as Root;
		root?.OnNPCGotShot();
		QueueFree();
	}

	// Called when Waldo ate npc.
	public void GotEaten(NPC eaten)
	{
		if (eaten == this) {
            var root = GetTree().Root.GetChild(0) as Root;
			root?.OnNPCGotEaten();

			Root.Kill(this);
		}
	}
}
