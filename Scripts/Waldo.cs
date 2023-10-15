using Godot;
using System;

public partial class Waldo : AnimatableBody3D, Player.IGotShotHandler, Player.IOnReticleNearHandler
{
	private NPCManager npcManager;

	private Material normalMaterial;
	[ExportGroup("Special Effects")]
	[Export]
	private Material materialForHiding;
	[Export]
	private PackedScene teleportEffect;

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


    public interface IEatHandler
    {
        abstract void GotEaten(NPC eaten);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		npcManager = Root.FindNodeRecusive<NPCManager>(GetTree().Root);
		stateChangeTime = Root.Timef();
		TeleportToNewLocation(false);
		TransitionState(HuntState.Idle);
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
                this.ConstantLinearVelocity = Vector3.Zero;
                MoveToTarget((float)delta);
				if (CloseEnoughToEat()) {
					TransitionState(HuntState.WarmingUp);
				}
				break;
			case HuntState.WarmingUp:
				if (!CloseEnoughToEat()) {
					TransitionState(HuntState.MovingToTarget);
				}
                else if (t > warmupTime) {
                    EatNPC();
                    TransitionState(HuntState.PostHunt);
                }
                break;
			case HuntState.PostHunt:
                if (t > postHuntTime) {
                    TeleportToNewLocation(true);
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

	}

	private bool CloseEnoughToEat()
	{
		return targetNPC.GlobalPosition.DistanceTo(this.GlobalPosition) < eatDist;
    }

	private void SelectTarget()
	{
		var npcs = npcManager.GetNPCS();
		float closestDist = float.MaxValue;
		NPC closestNPC = null;
		foreach (var npc in npcs) {
			if (npc.NativeInstance == IntPtr.Zero) {
				// Dead npc!
				continue;
			}
			float dist = npc.GlobalPosition.DistanceSquaredTo(this.GlobalPosition);
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

	private void MaybeCreateTeleportEffect()
	{
        if (teleportEffect != null) {
            var instantiate = teleportEffect.Instantiate<Node3D>();
            GetTree().Root.AddChild(instantiate);
            instantiate.GlobalPosition = this.GlobalPosition;
        }
    }

	private void TeleportToNewLocation(bool createEffect)
	{
		if (createEffect) {
			MaybeCreateTeleportEffect();
		}
		bool isValid = false;
		do {
			GlobalPosition = npcManager.GetValidSpawnLocation();
			isValid = !npcManager.IsInKeepOutZone(this.GlobalPosition);
		} while (!isValid);

		if (createEffect) {
			MaybeCreateTeleportEffect();
		}
	}

	private void MoveToTarget(float dt)
    {
		var diff = targetNPC.GlobalPosition - this.GlobalPosition;
		if (diff.Length() < 1e-3) {
			return;
		}
		Vector3 targetVelocity = diff.Normalized() * moveSpeed;
        this.GlobalPosition = this.GlobalPosition + targetVelocity * dt;
		this.ConstantLinearVelocity = targetVelocity;
    }

    private void TransitionState(HuntState newState)
	{
		huntState = newState;
		if (newState != HuntState.Hiding) {
			huntStateBeforeHiding = newState;
		}
		stateChangeTime = Root.Timef();
	}

	public void GotShot()
	{
		var root = GetTree().Root.GetChild(0) as Root;
		root?.OnWaldoShot();
		TeleportToNewLocation(true);
		SelectTarget();
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
        wasReticleNear = false;
		TransitionState(huntStateBeforeHiding);
        MeshInstance3D mesh = Root.FindNodeRecusive<MeshInstance3D>(this);
        mesh.MaterialOverride = normalMaterial;
    }
}
