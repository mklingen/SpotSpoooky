using Godot;
using System;

public partial class MainMenu : Control
{
    [Export(PropertyHint.File, "*.tscn,")]
    private string mainScene;

    [Export(PropertyHint.File, "*.tscn,")]
    private string optionsScene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
        (FindChild("Start") as Button)?.GrabFocus();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _OnStartPressed()
	{
		GetTree().ChangeSceneToFile(mainScene);
	}

	public void _OnOptionsPressed()
	{
        GetTree().ChangeSceneToFile(optionsScene);
    }

	public void _OnQuitPressed()
	{
		GetTree().Quit();
	}
}
