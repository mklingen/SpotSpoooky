using Godot;
using System;
using System.Collections.Generic;

public partial class Root : Node3D, Player.IZoomHandler, Player.IShootHandler
{
    [ExportGroup("Gameplay")]
    [Export]
    private int numSpooks = 3;
    [Export]
    private int maxSpooks = 6;

    public int NumSpooks {  get { return numSpooks; } }

    [Signal]
    public delegate void OnAlertEventHandler(string alert);

    [ExportGroup("State Management")]
    [Export(PropertyHint.File, "*.tscn,")]
    private string gameOverScene;

    [Export(PropertyHint.File, "*.tscn,")]
    private string mainMenuScene;

    private Timer gameOverTimer;

    public interface IAlertHandler
    {
        abstract void OnAlert(string alert);
    }

    [Signal]
    public delegate void OnScoreChangedEventHandler(int numSpooks, int maxSppoks);

    public interface IScoreChangedHandler
    {
        abstract void OnScoreChanged(int numSpooks, int maxSpooks);
    }

    public interface IDieHandler
    {
        abstract bool IsDead();
        abstract void OnDied();
    }

    public static Root Get(SceneTree tree)
    {
        foreach (var child in tree.Root.GetChildren()) {
            if (child is Root) {
                return child as Root;
            }
        }
        return null;
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

    public static void GetInterfaceRecursive<T>(Node parent, List<T> outNodes) where T : class
    {
        if (parent is T) {
            outNodes.Add(parent as T);
        }
        foreach (var child in parent.GetChildren()) {
            GetInterfaceRecursive<T>(child, outNodes);
        }

    }

    public static T FindNodeInterfaceRecusive<T>(Node parent) where T : class
    {
        if (parent is T) {
            return parent as T;
        }
        foreach (var child in parent.GetChildren()) {
            T node = FindNodeInterfaceRecusive<T>(child);
            if (node != null) {
                return node;
            }
        }
        return null;
    }

    public static void GetRecursive<T>(Node parent, List<T> outNodes) where T : Node
    {
        if (parent is T || parent.IsClass(typeof(T).Name)) {
            outNodes.Add((T)parent);
        }
        foreach (var child in parent.GetChildren()) {
            GetRecursive<T>(child, outNodes);
        }

    }

    public static T FindNodeRecusive<T>(Node parent) where T : Node
    {
        if (parent is T || parent.IsClass(typeof(T).Name)) {
            return (T)parent;
        }
        foreach (var child in parent.GetChildren()) {
            T node = FindNodeRecusive<T>(child);
            if (node != null) {
                return node;
            }
        }
        return null;
    }

    public class SineTable
    {
        float slowestSpeed = 0.5f;
        float fastestSpeed = 4.0f;
        float minOffset = 0.0f;
        float maxOffset = 6.0f;
        int numSpeedSamples = 4;
        int numOffsetSamples = 4;

        List<float> samples;
        public SineTable()
        {
            samples = new List<float>(numSpeedSamples * numOffsetSamples);
            for (int i = 0; i < numSpeedSamples * numOffsetSamples; i++) {
                samples.Add(0.0f);
            }
        }

        public void Sample(float time)
        {
            for (int speed = 0; speed < numSpeedSamples; speed++) {
                float spd = ((float)(speed) / (float)(numSpeedSamples)) * (fastestSpeed - slowestSpeed);
                for (int offset = 0; offset < numOffsetSamples; offset++) {
                    float o = ((float)(offset) / (float)(numOffsetSamples)) * (maxOffset - minOffset);
                    samples[speed * numOffsetSamples + offset] = Mathf.Sin(time * spd + o);
                }
            }
        }

        public float GetNearest(float speed, float offset)
        {
            int speedIdx = Math.Clamp((int)(numSpeedSamples * (speed - slowestSpeed) / (fastestSpeed - slowestSpeed)), 0, numSpeedSamples - 1);
            int offsetIdx = Math.Clamp((int)(numOffsetSamples * (offset - minOffset) / (maxOffset - minOffset)), 0, numOffsetSamples - 1);
            return samples[speedIdx * numOffsetSamples + offsetIdx];
        }
    }

    private bool isZoomed = false;
    public bool IsZoomed { get { return  isZoomed;  } }


    public static float LastFrameStartTime = -1.0f;
    public static float SinOfLastFrameTime = -1.0f;
    public static SineTable RandomSinTable = new SineTable();
    private static double lastSmoothedFPS = 0.0;
    private Settings settings;

    [ExportGroup("Tutorial")]
    // Controls whether Waldo is in tutorial mode.
    [Export]
    private bool isTutorial = false;

    public bool IsTutorial {  get { return isTutorial;  } }

    public static double SmoothedFPS()
    {
        return lastSmoothedFPS;
    }

    public static double FPS()
    {
        return Engine.GetFramesPerSecond();
    }
    public static float Timef()
    {
        return Time.GetTicksMsec() / 1000.0f;
    }

    public bool IsGameWon()
    {
        return numSpooks < 0;
    }

    public bool IsGameLost()
    {
        return numSpooks > maxSpooks;
    }

    public override void _Ready()
	{
        settings = new Settings();
        settings.LoadSettings();

        Viewport rootViewport = GetTree().Root;
        rootViewport.ScreenSpaceAA = settings.Antialiasing ? Viewport.ScreenSpaceAAEnum.Fxaa : Viewport.ScreenSpaceAAEnum.Disabled;


        List<IScoreChangedHandler> scoreChangers = new List<IScoreChangedHandler>();
        GetInterfaceRecursive<IScoreChangedHandler>(this, scoreChangers);
        foreach (var scoreChanger in scoreChangers) {
            OnScoreChanged += (int a, int b) => scoreChanger.OnScoreChanged(a, b);
        }
        EmitSignal(SignalName.OnScoreChanged, numSpooks, maxSpooks);


        List<IAlertHandler> alertHandlers = new List<IAlertHandler>();
        GetInterfaceRecursive<IAlertHandler>(this, alertHandlers);
        foreach (var alertHandler in alertHandlers) {
            OnAlert += (string alert) => alertHandler.OnAlert(alert);
        }

        var gameStats = GameStats.Get(this);
        if (gameStats != null) {
            gameStats.OnStartNextLevel();
        }
    }

    public override void _Process(double delta)
	{
        LastFrameStartTime = Timef();
        SinOfLastFrameTime = Mathf.Sin(LastFrameStartTime);
        RandomSinTable.Sample(LastFrameStartTime);
        if (Input.IsActionJustReleased("ui_cancel")) {
            GetTree().ChangeSceneToFile(mainMenuScene);
        }
        lastSmoothedFPS = lastSmoothedFPS * 0.99 + 0.01 * FPS();
	}

    public void OnNPCGotEaten()
    {
        AddSpooks(1);
        EmitSignal(SignalName.OnAlert, "TOO SLOW!");
        var gameStats = GameStats.Get(this);
        if (gameStats != null) {
            gameStats.OnNPCEaten();
        }
    }

    public void OnNPCGotShot()
    {
        AddSpooks(1);
        EmitSignal(SignalName.OnAlert, "FRIENDLY FIRE!");
        var gameStats = GameStats.Get(this);
        if (gameStats != null) {
            gameStats.OnFriendlyFire();
        }

    }

    public void AddSpooks(int dSpooks)
    {
        numSpooks += dSpooks;
        if (numSpooks > maxSpooks) {
            numSpooks = maxSpooks;
            DoAfterGameOverTimer(1.0f, LoseGame);
        }
        if (numSpooks < 0) {
            numSpooks = -1;
            DoAfterGameOverTimer(1.0f, WinGame);
        }
        EmitSignal(SignalName.OnScoreChanged, numSpooks, maxSpooks);

    }

    public void OnWaldoShot()
    {
        AddSpooks(-1);
        EmitSignal(SignalName.OnAlert, "GOT SPOOKY!");
        var gameStats = GameStats.Get(this);
        if (gameStats != null) {
            gameStats.OnShootSpooky();
        }
    }

    public void DoAfterGameOverTimer(float time, Action action)
    {
        gameOverTimer = new Timer();
        gameOverTimer.Autostart = true;
        gameOverTimer.OneShot = true;
        gameOverTimer.Timeout += action;
        AddChild(gameOverTimer);
    }



    public void LoseGame()
    {
        var gameStats = GameStats.Get(this);
        if (gameStats != null) {
            gameStats.didPlayerWinLastGame = false;
            gameStats.levelLost = gameStats.currentLevel;
            gameStats.currentLevel = 1;
            gameStats.OnEndCurrentLevel();
        }
        GetTree().ChangeSceneToFile(gameOverScene);
    }

    public void WinGame()
    {
        var gameStats = GameStats.Get(this);
        if (gameStats != null) {
            gameStats.didPlayerWinLastGame = true;
            gameStats.OnEndCurrentLevel();
            gameStats.currentLevel++;
        }
        GetTree().ChangeSceneToFile(gameOverScene);
    }

    public void OnZoomChange(bool zoomed)
    {
        isZoomed = zoomed;
    }

    public void OnShoot(Vector3 shootFrom, Vector3 shootTo)
    {
        var gameStats = GameStats.Get(this);
        if (gameStats != null) {
            gameStats.OnShoot();
        }
    }
}
