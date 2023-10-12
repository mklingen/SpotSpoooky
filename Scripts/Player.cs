using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class Player : Camera3D
{
	[ExportGroup("Sniper Camera")]
	[Export]
	private float zoomFactor = 0.1f;
	[Export]
	private Curve zoomCurve;
	[Export]
	private float zoomTime = 0.5f;
	[Export]
	private float zoomedMouseSensitivity = 0.01f;

	private float unZoomedScale;
	private float zoomedScale;
	private Vector3 startPosition;


	private enum ZoomMode
	{
		UnZoomed = 0,
		Zoomed = 1
	}
	private ZoomMode Mode = ZoomMode.UnZoomed;
	private bool isZooming = false;
	private float timeStartedZooming = -1.0f;
	private Vector3 targetPosition;
	private Vector3 preZoomPosition;
	private float targetZoom;
	private float startZoom;
	private Sprite3D reticleNode;
	private Vector3 reticleStartPositon;
	private float reticleStartDepth;

	[Export(PropertyHint.Layers3DPhysics)]
    private uint collisionRayMask;

	private Dictionary lastRayHit;

	[ExportGroup("Screen Shake")]
    [Export]
    private Curve shakeCurve;
	[Export]
	private float shakeAmount = 0.1f;
	[Export]
	private float shakeTime = 0.1f;

    struct ScreenShakeState
	{
		public float startedShakingAt;
        public float totalShakeTime;
        public Vector3 shakeStartPos;
		public Vector3 shakeEndPos;
		public bool isShaking;
	}

	ScreenShakeState shakeState;

	[ExportGroup("Events")]
	private float reticleEventSizePct = 0.5f;

	// Called when the zoom changes.
	[Signal]
	public delegate void OnZoomChangeEventHandler(bool zoomed);

	public interface IZoomHandler
	{
		abstract void OnZoomChange(bool zoomed);
	}

	// Called when the user shoots.
	[Signal]
	public delegate void OnShootEventHandler(Vector3 shootFrom, Vector3 shootTo);

	public interface IShootHandler
	{
		abstract void OnShoot(Vector3 shootFrom, Vector3 shootTo);
	}

	public interface IGotShotHandler
	{
		abstract void GotShot();
	}


    [Signal]
    public delegate void OnReticleNearEventHandler(Node3D reticleIsNearObject);
    [Signal]
    public delegate void OnReticleLeftEventHandler(Node3D reticleIsNearObject);
    public interface IOnReticleNearHandler
	{
		public abstract bool WasNear();
        public abstract Node3D GetThis();
        public abstract void OnReticleNear(Node3D reticleIsNearObject);
        public abstract void OnReticleLeft(Node3D reticleIsNearObject);
	}
	private List<IOnReticleNearHandler> onReticleNearHandlers = new List<IOnReticleNearHandler>();


	public void AddCallbacksRecursive(Node child)
	{
        var asZoomable = child as IZoomHandler;
        if (asZoomable != null) {
            OnZoomChange += (bool param) => asZoomable?.OnZoomChange(param);
        }
        var asShootable = child as IShootHandler;
        if (asShootable != null) {
            OnShoot += (Vector3 from, Vector3 to) => asShootable?.OnShoot(from, to);
        }
        foreach (var subchild in child.GetChildren()) {
            AddCallbacksRecursive(subchild);
        }
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		unZoomedScale = this.Size;
		zoomedScale = unZoomedScale * zoomFactor;
		startPosition = Position;
		reticleNode = GetChild<Sprite3D>(0);
		reticleStartPositon = reticleNode.Position + GetCamOffset();
		Input.MouseMode = Input.MouseModeEnum.Hidden;
		foreach (var child in GetChildren()) {
			AddCallbacksRecursive(child);
		}
		Root.GetRecursive<IOnReticleNearHandler>(GetTree().Root, onReticleNearHandlers);
		foreach (var handler in onReticleNearHandlers) {
			OnReticleNear += handler.OnReticleNear;
			OnReticleLeft += handler.OnReticleLeft;
		}
    }

	public Vector3 GetCamOffset()
	{
		return new Vector3(HOffset, VOffset, 0.0f);

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// If we're currently zooming, animate the zoom.
		if (isZooming) {
			reticleNode.Visible = true;
			// Determine where to put the reticle, either on the mouse, or in the center of the screen.
            Vector3 preZoomReticlePos =  ToLocal(ProjectPosition(this.GetViewport().GetMousePosition(), Near + 0.1f));
			Vector3 postZoomReticlePos = reticleStartPositon;
			if (Mode == ZoomMode.UnZoomed) {
				postZoomReticlePos = preZoomReticlePos;
				preZoomReticlePos = reticleStartPositon;
			}
			// Calculate zoom animation.
            float t = Root.Timef() - timeStartedZooming;
			if (t > zoomTime) {
				isZooming = false;
				Size = targetZoom;
				Position = targetPosition;
                reticleNode.Position = postZoomReticlePos;
            } else {
				// Everything is sampled from the same animation curve from 0-1
				float alpha = zoomCurve.Sample(t / zoomTime);
				Size = startZoom * (1.0f - alpha) + targetZoom * alpha;
				Position = preZoomPosition * (1.0f - alpha) + targetPosition * alpha;
                reticleNode.Position = preZoomReticlePos * (1.0f - alpha) + postZoomReticlePos * alpha;
            }
        } else {
			// If we're not zooming, calculate the reticle position.
			reticleNode.Visible = true;
			if (Mode == ZoomMode.UnZoomed) {
                reticleNode.Position = ToLocal(ProjectPosition(this.GetViewport().GetMousePosition(), Near + 0.1f));
				Input.MouseMode = Input.MouseModeEnum.Hidden;
            } else {
				reticleNode.Position = reticleStartPositon;
				Input.MouseMode = Input.MouseModeEnum.Captured;
				var rect = GetViewport().GetVisibleRect();
                // Determine what is near the center of the screen to trigger events on it.
                foreach (var handler in onReticleNearHandlers) {
                    Vector2 pos = this.UnprojectPosition(handler.GetThis().GlobalPosition);
                    Vector2 center = rect.GetCenter();
                    var isNear = pos.DistanceTo(center) / rect.Size.X < reticleEventSizePct;
                    if (handler.WasNear() && !isNear) {
                        EmitSignal(SignalName.OnReticleLeft, handler.GetThis());
                    }
                    else if (!handler.WasNear() && isNear) {
                        EmitSignal(SignalName.OnReticleNear, handler.GetThis());
                    }
                }
            }
		}

		HandleShaking();
	}

	public void ToggleZoom(InputEventMouseButton mouseEvent)
	{
		switch (Mode) {
			// Switch to zoomed mode.
			case ZoomMode.UnZoomed: {
					Mode = ZoomMode.Zoomed;
					targetPosition = ProjectPosition(mouseEvent.GlobalPosition, 0.0f) - GetCamOffset();
					targetZoom = zoomedScale;
					startZoom = unZoomedScale;
					preZoomPosition = startPosition;
					reticleNode.Position = reticleStartPositon;
					EmitSignal(SignalName.OnZoomChange, true);
                    break;
				}
			// Switch to unzoomed mode.
			case ZoomMode.Zoomed: {
					Mode = ZoomMode.UnZoomed;
					targetZoom = unZoomedScale;
                    targetPosition = startPosition;
					startZoom = Size;
                    preZoomPosition = Position;
					EmitSignal(SignalName.OnZoomChange, false);
					foreach (var handler in onReticleNearHandlers) {
						EmitSignal(SignalName.OnReticleLeft, handler.GetThis());
					}
					break;
				}
		}
		isZooming = true;
		timeStartedZooming = Root.Timef();

	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
        Vector3 bulletOffset = GetCamOffset();
		var from = GlobalPosition + bulletOffset;
        var to = from + ProjectRayNormal(Vector2.Zero) * 1000;
        var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(from, to);
            //collisionRayMask);
        lastRayHit = spaceState.IntersectRay(query);
    }

	private void Shoot()
	{
		Vector3 bulletOffset = GetCamOffset();
		Vector3 start = GlobalPosition;
		Vector3 end = GlobalPosition - GlobalTransform.Basis.Z * 100 + bulletOffset;

		if (lastRayHit != null && lastRayHit.Count > 0) {
			end = (Vector3)lastRayHit["position"];
			var collider = (Node)lastRayHit["collider"];

			// All colliders should be in null parents for this to work :(
			var shotHandler = Root.FindNodeRecusive<IGotShotHandler>(collider.GetParent());
			if (shotHandler != null) {
                shotHandler.GotShot();
			}

        }
		EmitSignal(SignalName.OnShoot, start, end);
		ShakeScreen();
	}

	private void HandleShaking()
	{
		if (!shakeState.isShaking) {
			return;
		}
		float t = (Root.Timef() - shakeState.startedShakingAt) / shakeState.totalShakeTime;
		if (t > 1.0f) {
			shakeState.isShaking = false;
			return;
		}
		float alpha = shakeCurve.Sample(t);
		GlobalPosition = (1.0f - alpha) * shakeState.shakeStartPos + alpha * shakeState.shakeEndPos;
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);

		if (@event is InputEventMouseButton eventMouseButton) {
			if (eventMouseButton.ButtonIndex == MouseButton.Right && !eventMouseButton.Pressed) {
				ToggleZoom(eventMouseButton);
			} else if (eventMouseButton.ButtonIndex == MouseButton.Left && !eventMouseButton.Pressed && Mode != ZoomMode.UnZoomed) {
				Shoot();
			}
		} else if (@event is InputEventMouseMotion eventMouseMotion) {
			if (Mode == ZoomMode.Zoomed && !isZooming) {
				Position += GlobalTransform.Basis.Y * eventMouseMotion.Relative.Y * -1.0f * zoomedMouseSensitivity + GlobalTransform.Basis.X * eventMouseMotion.Relative.X * zoomedMouseSensitivity;
            }
		}
	}

	public void ShakeScreen()
	{
		shakeState.isShaking = true;
		shakeState.startedShakingAt = Root.Timef();
		shakeState.totalShakeTime = shakeTime;
		shakeState.shakeStartPos = GlobalPosition;
		shakeState.shakeEndPos = GlobalPosition + Basis.X * ((GD.Randf() - 0.5f) * shakeAmount) + Basis.Y * (GD.Randf() - 0.5f) * shakeAmount;
    }
}