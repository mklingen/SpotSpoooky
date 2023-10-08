using Godot;
using System;

public partial class Reticle : Sprite3D, Player.IShootHandler
{

    [ExportGroup("Screen Shake")]
    [Export]
    private Curve shakeCurve;
    [Export]
    private float shakeAmount = 0.1f;
    [Export]
    private float shakeTime = 0.1f;
    
    private bool isShaking = false;
    private Vector3 scaleDefault;
    private Vector3 scaleShaking;
    private float timeStartedShaking;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        scaleDefault = Scale;
        scaleShaking = Scale * (1.0f + shakeAmount);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!isShaking) {
            return;
        }

        float t = (Root.Timef() - timeStartedShaking) / shakeTime;
        float alpha = shakeCurve.Sample(t);
        Scale = (1.0f - alpha) * scaleDefault + alpha * scaleShaking;
        if (t > 1.0f) {
            isShaking = false;
            Scale = scaleDefault;
        }
    }


    public void OnShoot(Vector3 shootFrom, Vector3 shootTo)
	{
        timeStartedShaking = Root.Timef();
        isShaking = true;
	}
}
