using Godot;
using System;
using System.Collections.Generic;

public partial class Waldo : AnimatableBody3D, Player.IGotShotHandler, Player.IOnReticleNearHandler, NPCManager.IOnLoadedHandler
{
	private NPCManager npcManager;


	[ExportGroup("Special Effects")]
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
    [Export] // Time spent idle before the turn begins.
    float proportionTimeIdle = 0.4f;
    [Export] // Time spent moving. Same for all targets for balance reasons.
    float proportionTimeMoving = 0.5f;
    [Export] //Time spent at the target, waiting for eating.
    float proportionTimeWarmingUp = 0.1f;
    [Export] // Amount of time spent between turns.
    float timeBetweenTurns = 1.0f;
	[Export] // Distance required for Waldo to be from an NPC to eat.
	float eatDist = 3.5f;
	[Export] // Total amount of time it takes Waldo to take a turn.
	float maxTotalTime = 30.0f;
	[Export] // Minimum total amount of time it takes Waldo to take a turn.
	float minTotalTime = 3.0f;
	private float totalTime = 0.0f;
	// Time the state last changed.
    float stateChangeTime = -1.0f;
	// The time that the last turn started.
	float timeTurnStarted = -1.0f;
	float timeTurnEnded = -1.0f;
    // Number from 0 to 1 indicating how far along we are in a turn.
    float lastNormalizedTurnTime = -1.0f;
	float timeOffset = 0.0f;

	Vector3 globalPosBeforeMoving;


    // Called when the game state changes.
    [Signal]
    public delegate void OnTurnTimeChangedEventHandler(float normalizedTime);

	private class AudioManager
	{
		public RandomSFXPlayer DieSound;
        public RandomSFXPlayer ScareSound;

		public AudioManager(Node parent)
		{
            DieSound = parent.FindChild("DieSound") as RandomSFXPlayer;
            ScareSound = parent.FindChild("ScareSound") as RandomSFXPlayer;
        }

	}
	private AudioManager audioManager;

	private Settings settings;


    [ExportGroup("Tutorial")]
    // Controls whether Waldo is in tutorial mode.
    [Export]
	private bool isTutorial = false;

    public interface ITurnTimeChangedHandler
    {
        abstract void OnTurnTimeChanged(float normalizedTime);
    }


    // Number from 0 to 1 indicating how far along we are in a turn.
    private void ResetNormalizedTurnTime()
	{
		float t = Root.Timef();
		lastNormalizedTurnTime =  Mathf.Clamp((t - timeTurnStarted + timeOffset) / totalTime, 0.0f, 1.0f);
	}

	private void UpdateNormalizedTurnTime(float dt)
	{
		if (isTutorial) {
			return;
		}
		// We are paused.
		if (huntState == HuntState.Hiding) {
			timeOffset -= dt;
			return;
		}
		ResetNormalizedTurnTime();
	}



	private NPC targetNPC;

    public interface IEatHandler
    {
        abstract void GotEaten(NPC eaten);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		settings = new Settings();
		settings.LoadSettings();
		// Total time is scaled by the level inversely, such that the minimum total time is achieved
		// at the very end of the game. This makes the game harder and harder!
		totalTime = -GameStats.Get(this).GetLevelScaledValue(-maxTotalTime, -minTotalTime);
		npcManager = Root.FindNodeRecusive<NPCManager>(GetTree().Root);
		stateChangeTime = Root.Timef();
		if (!isTutorial) {
			TeleportToNewLocation(false);
			TransitionState(HuntState.Idle);
		}
		List<ITurnTimeChangedHandler> handlers = new List<ITurnTimeChangedHandler>();
		Root.GetInterfaceRecursive<ITurnTimeChangedHandler>(GetTree().Root, handlers);
		foreach (var handler in handlers) {
			OnTurnTimeChanged += handler.OnTurnTimeChanged;
        }
		audioManager = new AudioManager(this);
        globalPosBeforeMoving = GlobalPosition;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!doneLoading) {
			return;
		}
		UpdateNormalizedTurnTime((float)delta);
		if (targetNPC != null) {
			Vector3 t = targetNPC.GlobalPosition;
			t.Y = GlobalPosition.Y;

            LookAt(t);
		}
        switch (huntState) {
			case HuntState.Idle:
				if (!isTutorial) {
					EmitSignal(SignalName.OnTurnTimeChanged, lastNormalizedTurnTime);
					if (lastNormalizedTurnTime >= proportionTimeIdle) {
						SelectTarget();
						TransitionState(HuntState.MovingToTarget);
					}
				}
				break;
			case HuntState.MovingToTarget:
                EmitSignal(SignalName.OnTurnTimeChanged, lastNormalizedTurnTime);
                this.ConstantLinearVelocity = Vector3.Zero;
                MoveToTarget((float)delta);
				if (lastNormalizedTurnTime >= proportionTimeIdle + proportionTimeMoving) {
					TransitionState(HuntState.WarmingUp);
				}
				break;
			case HuntState.WarmingUp:
                EmitSignal(SignalName.OnTurnTimeChanged, lastNormalizedTurnTime);
                if (lastNormalizedTurnTime >= 0.9999f) {
                    EatNPC();
                    TransitionState(HuntState.PostHunt);
                }
                break;
			case HuntState.PostHunt:
                EmitSignal(SignalName.OnTurnTimeChanged, 0.0f);
                if (Root.Timef() - timeTurnEnded > timeBetweenTurns) {
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
		if (isTutorial) {
			return;
		}
		var npcs = npcManager.GetNPCS();
		float closestDist = float.MaxValue;
		NPC closestNPC = null;
		foreach (var npc in npcs) {
			if (npc.NativeInstance == IntPtr.Zero || npc.IsFrozen) {
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

	public void EatNPC()
	{
		if (targetNPC != null) {
			npcManager.EatNPC(targetNPC);
		}
		timeTurnEnded = Root.Timef();
		audioManager.ScareSound.PlayRandom((float)this.settings.SFXVolume);
    }

	public void MaybeCreateTeleportEffect()
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
		if (npcManager != null) {
			bool isValid = false;
			do {
				GlobalPosition = npcManager.GetValidSpawnLocation();
				isValid = !npcManager.IsInKeepOutZone(this.GlobalPosition) && npcManager.CanPlayerSee(this.GlobalPosition);
			} while (!isValid);
		}

		if (createEffect) {
			MaybeCreateTeleportEffect();
		}
		var root = Root.Get(GetTree());
		if (root != null && (root.IsGameWon() || root.IsGameLost())) {
			this.Hide();
		}
	}

	public void SetNormalizedTurnTime(float normalizedTime)
	{
		lastNormalizedTurnTime = normalizedTime;
	}

	public void MoveToTarget(float dt)
    {
		if (targetNPC == null) {
			return;
		}
		var diff = targetNPC.GlobalPosition - this.GlobalPosition;
		if (diff.Length() < eatDist) {
			return;
		}
		float alpha = Mathf.Clamp((lastNormalizedTurnTime - proportionTimeIdle) / (proportionTimeMoving), 0.0f, 1.0f);
		Vector3 targetVelocity = diff.Normalized(); ;
		this.GlobalPosition = globalPosBeforeMoving * (1.0f - alpha) + targetNPC.GlobalPosition * alpha;
		this.ConstantLinearVelocity = targetVelocity;
    }

    private void TransitionState(HuntState newState)
	{
		if (newState == HuntState.Idle) {
			timeTurnStarted = Root.Timef();
			timeOffset = 0.0f;
		} else if (newState == HuntState.MovingToTarget) {
			globalPosBeforeMoving = GlobalPosition;
        }
		huntState = newState;
		if (newState != HuntState.Hiding) {
			huntStateBeforeHiding = newState;
		}
		stateChangeTime = Root.Timef();
	}

	public void GotShot()
	{
        audioManager.DieSound.PlayRandom((float)this.settings.SFXVolume);
		var root = Root.Get(GetTree());
        root?.OnWaldoShot();
		TeleportToNewLocation(true);
		SelectTarget();
		TransitionState(HuntState.Idle);
    }

    public void SetTarget(NPC target)
	{
		targetNPC = target;
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
    }

	private bool doneLoading = false;
	public void OnLoaded()
	{
		doneLoading = true;
		TransitionState(HuntState.Idle);
	}
}
