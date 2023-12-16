using Godot;
using System;

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

	private void OkButton_Pressed()
	{
		GetTree().CurrentScene.QueueFree();
		GetTree().ChangeSceneToFile(nextSceneToLoad);
	}

}
