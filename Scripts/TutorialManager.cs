using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class TutorialManager : Node3D
{

    [Export(PropertyHint.File, "*.tscn,")]
    private string tutorialCompleteScene;

    private abstract class TutorialSegment
    {
        public Player player;
        public Label TextLabel;
        public abstract bool GoToNextSegment();
        public abstract void Start();
        public abstract void Update(double dt);
        public  virtual void OnPlayerZoomed() {}
        public virtual void OnPlayerUnZoomed() { }


    }


    // Tutorial segment that changes with a timer.
    private class TimedTutorialSegment : TutorialSegment
    {
        protected float startTime;
        public float TotalTime;

        public override bool GoToNextSegment()
        {
            return Root.Timef() - startTime > TotalTime;
        }

        public override void Start()
        {
            startTime = Root.Timef();
        }

        public override void Update(double dt)
        {

        }
    }



    // First describe the mouse and moving the cursor.
    private class StartTutorial : TutorialSegment
    {
        private bool didPlayerZoom = false;
        public override void OnPlayerZoomed()
        {
            didPlayerZoom = true;
        }
        public override bool GoToNextSegment()
        {
            return didPlayerZoom;
        }

        public override void Start()
        {
            TextLabel.Text = "Use the mouse to move the cursor. Zoom in with Right Mouse Button or Ctrl.";
        }

        public override void Update(double dt)
        {
            
        }
    }

    private class UnZoomTutorial : TutorialSegment
    {
        private bool didPlayerUnZoom = false;

        public override void OnPlayerUnZoomed()
        {
            didPlayerUnZoom = true;
        }

        public override bool GoToNextSegment()
        {
            return didPlayerUnZoom;
        }

        public override void Start()
        {
            TextLabel.Text = "Now Zoom out again with Right Mouse Button or Ctrl.";
        }

        public override void Update(double dt)
        {

        }
    }

    private class SayThingForTime : TimedTutorialSegment
    {
        private string sayThing;

        public SayThingForTime(string thing, float time)
        {
            TotalTime = time;
            sayThing = thing;
        }
        public override void Start()
        {
            base.Start();
            TextLabel.Text = sayThing;
        }
    }

    private class DoThing : TutorialSegment
    {
        private Action thing;

        public DoThing(Action action)
        {
            thing = action;
        }

        public override bool GoToNextSegment()
        {
            return true;
        }

        public override void Start()
        {
            thing();
        }

        public override void Update(double dt)
        {

        }
    }

    private class GenericSegment : TutorialSegment
    {
        private Action onStart;
        private Func<bool> shouldTransition;
        private Action onUpdate;
        string text;

        public GenericSegment(string text, Action onStart, Func<bool> shouldTransition, Action onUpdate)
        {
            this.text = text;
            this.onStart = onStart;
            this.onUpdate = onUpdate;
            this.shouldTransition = shouldTransition;
        }

        public override bool GoToNextSegment()
        {
            return shouldTransition();
        }

        public override void Start()
        {
            this.TextLabel.Text = text;
            onStart();
        }

        public override void Update(double dt)
        {
            onUpdate();
        }
    }

    private class ShootWaldoTutorial : TutorialSegment
    {
        private Root root;
        private int numSpooksOnStart = 0;
        public ShootWaldoTutorial(Root root)
        {
            this.root = root;
        }

        public override bool GoToNextSegment()
        {
            return root.NumSpooks < numSpooksOnStart;
        }

        public override void Start()
        {
            TextLabel.Text = "Shoot Spooky using left mouse button or space!";
            numSpooksOnStart = root.NumSpooks;
        }

        public override void Update(double dt)
        {

        }
    }


    private class ShootWaldoUntilDead : TutorialSegment
    {
        private Root root;
        public ShootWaldoUntilDead(Root root)
        {
            this.root = root;
        }

        public override bool GoToNextSegment()
        {
            return root.NumSpooks == 0;
        }

        public override void Start()
        {
            TextLabel.Text = "Keep Shooting until there is no more Spooky Energy!";
        }

        public override void Update(double dt)
        {

        }
    }



    private NPC npc;
    private Waldo waldo;
    private Player player;
    private Label textLabel;
    private List<TutorialSegment> Segments;
    private int currentSegment = 0;
    private float lastDt = 0;
    private float timeSpookyStartedMoving = 0;
    private float spookyMoveTime = 3.0f;
    private SpotLight3D spotLight;
    public override void _Ready()
    {
        base._Ready();
        textLabel = FindChild("TutorialLabel") as Label;
        spotLight = FindChild("SpotLight") as SpotLight3D;
        spotLight.Hide();
        player = Root.FindNodeRecusive<Player>(this);
        player.IsShootingEnabled = false;
        npc = Root.FindNodeRecusive<NPC>(this);
        waldo = Root.FindNodeRecusive<Waldo>(this);
        npc.Hide();
        waldo.Hide();
        Vector3 origWaldoPosition = waldo.GlobalPosition;
        var root = Root.Get(GetTree());
        int origSpooks = root.NumSpooks;
        Segments = new List<TutorialSegment>()
        {
            new StartTutorial(),
            new UnZoomTutorial(),
            new DoThing(() =>
            {
                npc.Show();
            }),
            new DoThing(()=>
            {
                spotLight.Show();
                spotLight.GlobalPosition = npc.GlobalPosition + Vector3.Up * 6;
            }
            ),
            new SayThingForTime("This is a trick-or-treater!", 5.0f),
            new DoThing(() =>
            {
                waldo.Show();
                waldo.MaybeCreateTeleportEffect();
                spotLight.GlobalPosition = waldo.GlobalPosition + Vector3.Up * 6;
            }
            ),
            new SayThingForTime("This is Spooky!", 2.0f),
            new SayThingForTime("Spooky will try to Scare trick-or-treaters.", 2.0f),
            new GenericSegment("Spooky moves toward a trick-or-treater...",
            ()=>
            {
                spotLight.Hide();
                waldo.SetTarget(npc);
                timeSpookyStartedMoving = Root.Timef();
            },
            ()=>
            {
                return waldo.GlobalPosition.DistanceTo(npc.GlobalPosition) < 2.0f;
            },
            () => {
                waldo.SetNormalizedTurnTime((Root.Timef() - timeSpookyStartedMoving) / spookyMoveTime);
                waldo.MoveToTarget(this.lastDt);
            }),
            new SayThingForTime("Then scares the trick-or-treater!", 2.0f),
            new DoThing(() =>
            {
                waldo.EatNPC();
            }),
            new SayThingForTime("Spooky must be stopped!", 2.0f),
            new DoThing(() =>
            {
                npc.Unfreeze();
                waldo.GlobalPosition = origWaldoPosition;
                waldo.MaybeCreateTeleportEffect();
                player.IsShootingEnabled = true;
            }
            ),
            new StartTutorial(),
            new ShootWaldoTutorial(root),
            new ShootWaldoUntilDead(root),
            new SayThingForTime("All Done!", 2.0f)

        };
        foreach (TutorialSegment segment in Segments) {
            segment.TextLabel = textLabel;
            segment.player = player;
        }
        Segments[0].Start();
        player.OnZoomChange += Player_OnZoomChange;
    }

    private void Player_OnZoomChange(bool zoomed)
    {
        if (currentSegment < 0 || currentSegment > Segments.Count - 1) {
            return;
        }
        if (zoomed) {
            Segments[currentSegment].OnPlayerZoomed();
        } else {
            Segments[currentSegment].OnPlayerUnZoomed();
        }
    }

    public override void _Process(double delta)
    {
        lastDt = (float)delta;
        base._Process(delta);
        if (currentSegment < 0 || currentSegment > Segments.Count - 1) {
            return;
        }
        Segments[currentSegment]?.Update(delta);
        if (Segments[currentSegment].GoToNextSegment()) {
            currentSegment++;
            if (currentSegment <= Segments.Count - 1) {
                Segments[currentSegment].Start();
            }
            else {
                // Finish the tutorial.
                GetTree().ChangeSceneToFile(tutorialCompleteScene);
            }
        }
    }
}
