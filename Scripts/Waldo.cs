using Godot;
using System;

public partial class Waldo : Node3D, Player.IGotShotHandler, Player.IOnReticleNearHandler
{
	private NPCManager npcManager;
	private CharacterBody3D characterBody3D;

	private Material normalMaterial;
	[Export]
	private Material materialForHiding;

	public enum HuntState
	{
		Idle,
		MovingToTarget,
		WarmingUp,
		PostHunt,
		Hiding
	}

	[ExportGroup("Hunt Behavior")]
	[Export]
	private HuntState huntState = HuntState.Idle;
	[Export]
	float stateChangeTime = -1.0f;
    [Export]
    float idleTime = 5.0f;
    [Export]
    float moveSpeed = 10.0f;
    [Export]
    float warmupTime = 1.0f;
    [Export]
    float postHuntTime = 1.0f;
	[Export]
	float eatDist = 3.5f;

	private NPC targetNPC;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		npcManager = Root.FindNodeRecusive<NPCManager>(GetTree().Root);
		stateChangeTime = Root.Timef();
		TeleportToNewLocation();
		TransitionState(HuntState.Idle);
		characterBody3D = Root.FindNodeRecusive<CharacterBody3D>(this);
		normalMaterial = Root.FindNodeRecusive<MeshInstance3D>(this).MaterialOverride;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		float t = Root.Timef() - stateChangeTime;
		switch (huntState) {
			case HuntState.Idle:
				if (t > idleTime) {
					SelectTarget();
					TransitionState(HuntState.MovingToTarget);
				}
				break;
			case HuntState.MovingToTarget:
				MoveToTarget();
				if (CloseEnoughToEat()) {
					TransitionState(HuntState.WarmingUp);
				}
				break;
			case HuntState.WarmingUp:
				if (!CloseEnoughToEat()) {
					TransitionState(HuntState.MovingToTarget);
				}
                if (t > warmupTime) {
                    EatNPC();
                    TransitionState(HuntState.PostHunt);
                }
                break;
			case HuntState.PostHunt:
                if (t > postHuntTime) {
                    TeleportToNewLocation();
                    TransitionState(HuntState.Idle);
                }
                break;
			case HuntState.Hiding: {
					OnHiding();
					break;
			}
		}
	}

	private void OnHiding()
	{
		// Do something special.
	}

	private bool CloseEnoughToEat()
	{
		return targetNPC.GlobalPosition.DistanceTo(GlobalPosition) < eatDist;
    }

	private void SelectTarget()
	{
		var npcs = npcManager.GetNPCS();
		float closestDist = float.MaxValue;
		NPC closestNPC = null;
		foreach (var npc in npcs) {
			float dist = npc.GlobalPosition.DistanceSquaredTo(GlobalPosition);
			if (dist < closestDist) {
				closestNPC = npc;
				closestDist = dist;
			}
		}
		targetNPC = closestNPC;

    }

	private void EatNPC()
	{
		npcManager.EatNPC(targetNPC);
	}

	private void TeleportToNewLocation()
	{
		bool isValid = false;
		do {
			GlobalPosition = npcManager.GetValidSpawnLocation();
			isValid = !npcManager.IsInKeepOutZone(GlobalPosition);
		} while (!isValid);
	}

    private void MoveToTarget()
    {
		var diff = targetNPC.GlobalPosition - GlobalPosition;
		if (diff.Length() < 1e-3) {
			return;
		}
        characterBody3D.Velocity = diff.Normalized() * moveSpeed;
		characterBody3D.MoveAndSlide();
		GlobalPosition = characterBody3D.GlobalPosition;
    }

    private void TransitionState(HuntState newState)
	{
		huntState = newState;
		stateChangeTime = Root.Timef();
	}

	public void GotShot()
	{
		GD.Print("WALDO GOT SHOT!!");
		TeleportToNewLocation();
		TransitionState(HuntState.Idle);

    }


	private bool wasReticleNear;
	private HuntState huntStateBeforeHiding;
	public bool WasNear()
	{
		return wasReticleNear;

    }

	public Node3D GetThis()
	{
		return this;
	}

	public void OnReticleNear(Node3D reticleIsNearObject)
	{
		if (reticleIsNearObject != this) {
			return;
		}
		if (wasReticleNear) {
			return;
		}
		GD.Print("Reticle near.");
		wasReticleNear = true;
		huntStateBeforeHiding = huntState;
		TransitionState(HuntState.Hiding);
		MeshInstance3D mesh = Root.FindNodeRecusive<MeshInstance3D>(this);
		mesh.MaterialOverride = materialForHiding;
	}

	public void OnReticleLeft(Node3D reticleIsNearObject)
	{
        if (reticleIsNearObject != this) {
            return;
        }
		if (!wasReticleNear) {
			return;
		}
		GD.Print("Reticle left");
        wasReticleNear = false;
		TransitionState(huntStateBeforeHiding);
        MeshInstance3D mesh = Root.FindNodeRecusive<MeshInstance3D>(this);
        mesh.MaterialOverride = normalMaterial;
    }
}
