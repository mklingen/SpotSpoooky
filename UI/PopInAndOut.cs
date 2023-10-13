using Godot;
using System;

public partial class PopInAndOut : Control, Root.IDieHandler
{
	[Export]
	private float popInTime = 0.5f;
	[Export]
	private float popOutTime = 0.5f;
	[Export]
	private Curve popInCurve;
	[Export]
	private Curve popOutCurve;

	private float timeStartedPoppingIn = 0;
	private float timeStartedPoppingOut = 0;
	bool isPoppingOut = false;
	bool isPoppingIn = false;
	private Vector2 startScale = Vector2.Zero;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		isPoppingIn = true;
		startScale = Scale;
		timeStartedPoppingIn = Root.Timef();
		this.Scale = Vector2.Zero;
		Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (isPoppingIn) {
            float t = (Root.Timef() - timeStartedPoppingIn) / popInTime;
            float alpha = popInCurve.Sample(t);
            Scale = startScale * alpha;
            if (t > 1.0f) {
                isPoppingIn = false;
            }
        }
        if (isPoppingOut) {
            float t = (Root.Timef() - timeStartedPoppingOut) / popOutTime;
            float alpha = popOutCurve.Sample(t);
            Scale = startScale * alpha;
            if (t > 1.0f) {
                isPoppingOut = false;
                QueueFree();
            }
        }
        Visible = true;
	}

	public void PopOut()
	{
		timeStartedPoppingOut = Root.Timef();
		isPoppingOut = true;
	}

	public void OnDied()
	{
		PopOut();
	}

	public bool IsDead()
	{
		return isPoppingOut;
	}
}
