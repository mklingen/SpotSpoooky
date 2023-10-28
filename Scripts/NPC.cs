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

		var bobber = Root.FindNodeRecusive<NPCAnimator>(this);
		if (bobber != null) {
			bobber.SetBobSpeed(speed * 5);
			bobber.SetBobAmount(speed * 0.01f);
		}
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (pathToFollow != null) {
			SetPath(pathToFollow, pathFollowSpeed, pathFollowOffset);
		}
		RotateY(GD.Randf() * Mathf.Pi * 2.0f);
		updateEveryFrame = GD.RandRange(1, 10);
	}

	private int updateEveryFrame = 1;
	private int frameCounter = 0;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		frameCounter++;
		// Move along the path.
		if (pathToFollow != null && frameCounter % updateEveryFrame == 0) {
			currDistAlongPath = currDistAlongPath + pathFollowSpeed * (float)delta ;
			// Wrap the path (it is supposed to circular, I guess.
			if (currDistAlongPath > pathLength) {
				currDistAlongPath = (currDistAlongPath - pathLength);
			} else if (currDistAlongPath < 0.0f) {
				currDistAlongPath = pathLength - currDistAlongPath;
			}
			// Get the position of the object along the path.
			Vector3 nextPos = pathToFollow.ToGlobal(pathToFollow.Curve.SampleBaked(currDistAlongPath) + pathFollowOffset);
			Vector3 dPos = nextPos - GlobalPosition;

            ConstantLinearVelocity = (dPos) * (1.0f / (float)delta);
			if (!nextPos.IsEqualApprox(GlobalPosition)) {
				LookAt(nextPos, Vector3.Up);
			}
            GlobalPosition = nextPos;
        }
	}

	// Called when player shot NPC.
	public void GotShot()
	{
        var root = Root.Get(GetTree());
        root?.OnNPCGotShot();
		QueueFree();
	}

	// Called when Waldo ate npc.
	public void GotEaten(NPC eaten)
	{
		if (eaten == this) {
			var root = Root.Get(GetTree());
			root?.OnNPCGotEaten();

			Root.Kill(this);
		}
	}
}
