using Godot;
using System;
using static System.Net.Mime.MediaTypeNames;

public partial class GameOver : Control
{
    [Export(PropertyHint.File, "*.tscn,")]
    private string loseScene;
    [Export(PropertyHint.File, "*.tscn,")]
    private string winScene;
    [Export(PropertyHint.File, "*.tscn,")]
    private string nextLevelScene;

	private string nextSceneToLoad;

	[Export]
	private string winText = "You win!\nYou spotted Spooky in time!";
	[Export]
	private string loseText = "You lost level {0}!\nMaybe next time!";
	[Export]
	private string nextLevelText = "You spotted spooky in time!\nLevel {0} is next.";

	[Export]
	private string statsText = "Time: {0}\nShots: {1}\nHits: {2}\nMisses: {3}\nFriendly: {4}\nToo Slow: {5}";

	private AnimationPlayer animationSpooky;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		animationSpooky = Root.FindNodeRecusive<AnimationPlayer>(this);
		if (animationSpooky != null) {
			animationSpooky.CurrentAnimation = "Attack_spooky";
			animationSpooky.Play();
        }
        Input.MouseMode = Input.MouseModeEnum.Visible;
        var okButton = FindChild("OKButton") as Button;
		if (okButton != null) {
			okButton.Pressed += OkButton_Pressed;
			okButton.GrabFocus();
		}
        var gameStats = GameStats.Get(this);
        if (gameStats != null) {
			UpdateStats(gameStats);
			if (gameStats.didPlayerWinLastGame && gameStats.currentLevel > gameStats.maxLevels) {
				SetText(winText);
				nextSceneToLoad = winScene;
			}
			else if (!gameStats.didPlayerWinLastGame) {
				SetText(String.Format(loseText, gameStats.levelLost));
				nextSceneToLoad = loseScene;
			}
			else {
				SetText(String.Format(nextLevelText, gameStats.currentLevel));
				nextSceneToLoad = nextLevelScene;
			}
        }
    }

	public void SetText(string text)
	{
		var label = FindChild("WinOrLose") as Label;
		if (label != null) {
			label.Text = text;
		}
	}

    string ConvertSecondsToTime(float totalSeconds)
    {
        // Calculate minutes and seconds
        int minutes = (int)(totalSeconds / 60);
        int seconds = (int)(totalSeconds % 60);

        // Format the result with two digits of precision for seconds
        string formattedTime = $"{minutes:D}:{seconds:D2}";

        return formattedTime;
    }

    public void UpdateStats(GameStats stats)
	{
        var label = FindChild("Stats") as Label;
        if (label != null) {
			GameStats.ShotStats shots = stats.CurrentLevelShotStats;
			label.Text = string.Format(statsText,
				ConvertSecondsToTime(stats.TimeLevelFinished - stats.TimeLevelStarted),
				shots.TotalNumShots, shots.TotalNumSpookyShots, shots.TotalNumMisses, shots.TotalNumFriendlyFire, shots.TotalNumScaredNPCs);
        }
    }

	private void OkButton_Pressed()
	{
		GetTree().CurrentScene.QueueFree();
		GetTree().ChangeSceneToFile(nextSceneToLoad);
	}

}
