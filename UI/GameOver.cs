using Godot;
using System;

public partial class GameOver : Control
{
	[Export]
	private PackedScene okScene;
	[Export]
	private string winText = "You win!\nYou spotted Spooky in time!";
	[Export]
	private string loseText = "You lose!\nMaybe next time!";
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Input.MouseMode = Input.MouseModeEnum.Visible;
        var okButton = FindChild("OKButton") as Button;
		if (okButton != null) {
			okButton.Pressed += OkButton_Pressed;
			okButton.GrabFocus();
		}
        var gameStats = GetNode<GameStats>("/root/GameStats");
        if (gameStats != null) {
			SetText(gameStats.didPlayerWinLastGame ? winText : loseText);
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
		GetTree().ChangeSceneToPacked(okScene);
	}

}
