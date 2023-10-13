using Godot;
using System;
using System.Collections.Generic;

public partial class Root : Node3D
{
    [ExportGroup("Gameplay")]
    [Export]
    private int numStrikes = 0;
    [Export]
    private int maxStrikes = 3;
    [Export]
    private int numHearts = 3;
    [Export]
    private int maxHearts = 4;

    [Signal]
    public delegate void OnAlertEventHandler(string alert);

    public interface IAlertHandler
    {
        abstract void OnAlert(string alert);
    }

    [Signal]
    public delegate void OnScoreChangedEventHandler(int numStrikes, int numHearts, bool animate);

    public interface IScoreChangedHandler
    {
        abstract void OnScoreChanged(int numStrikes, int numHearts, bool animate);
    }

    public interface IDieHandler
    {
        abstract bool IsDead();
        abstract void OnDied();
    }


    public static void Kill(Node node)
    {
        if (node is IDieHandler) {
            var dieHandler = node as IDieHandler;
            if (!dieHandler.IsDead()) {
                dieHandler.OnDied();
            }
        }
        else {
            node.QueueFree();
        }
    }


    public static void GetRecursive<T>(Node parent, List<T> outNodes) where T : class
    {
        if (parent is T) {
            outNodes.Add(parent as T);
        }
        foreach (var child in parent.GetChildren()) {
            GetRecursive<T>(child, outNodes);
        }

    }

    public static T FindNodeRecusive<T>(Node parent) where T : class
    {
        if (parent is T) {
            return parent as T;
        }
        foreach (var child in parent.GetChildren()) {
            T node = FindNodeRecusive<T>(child);
            if (node != null) {
                return node;
            }
        }
        return null;
    }

    public static float Timef()
    {
        return Time.GetTicksMsec() / 1000.0f;
    }

    public override void _Ready()
	{
        List<IScoreChangedHandler> scoreChangers = new List<IScoreChangedHandler>();
        GetRecursive<IScoreChangedHandler>(this, scoreChangers);
        foreach (var scoreChanger in scoreChangers) {
            OnScoreChanged += (int a, int b, bool animate) => scoreChanger.OnScoreChanged(a, b, animate);
        }
        EmitSignal(SignalName.OnScoreChanged, numStrikes, numHearts, false);


        List<IAlertHandler> alertHandlers = new List<IAlertHandler>();
        GetRecursive<IAlertHandler>(this, alertHandlers);
        foreach (var alertHandler in alertHandlers) {
            OnAlert += (string alert) => alertHandler.OnAlert(alert);
        }

    }

    public override void _Process(double delta)
	{

	}

    public void OnNPCGotEaten()
    {
        IncrementStrikes();
        IncrementHearts();
        EmitSignal(SignalName.OnAlert, "TOO SLOW!");
    }

    public void OnNPCGotShot()
    {
        IncrementStrikes();
        EmitSignal(SignalName.OnAlert, "FRIENDLY FIRE!");

    }

    public void IncrementStrikes()
    {
        numStrikes++;
        if (numStrikes > maxStrikes) {
            GD.Print("TODO: end game.");
        }
        EmitSignal(SignalName.OnScoreChanged, numStrikes, numHearts, true);
    }

    public void IncrementHearts()
    {
        numHearts++;
        if (numHearts > maxHearts) {
            numHearts = maxHearts;
        }
        EmitSignal(SignalName.OnScoreChanged, numStrikes, numHearts, true);
    }

    public void DecrementHearts()
    {
        numHearts--;
        if (numHearts < 0) {
            numHearts = 0;
            GD.Print("End game.");
        }
        EmitSignal(SignalName.OnScoreChanged, numStrikes, numHearts, true);
        EmitSignal(SignalName.OnAlert, "GOT SPOOKY!");
    }
}
