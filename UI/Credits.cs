using Godot;
using System;

public partial class Credits : Control
{
    [Export(PropertyHint.File, "*.tscn,")]
    private string mainMenuScene;

    private T GetChild<T>(string name) where T : Control
    {
        return FindChild(name) as T;
    }

    public override void _Ready()
    {
        base._Ready();
        GetChild<Control>("BackButton").GrabFocus();
    }

    public void _OnBackButtonPressed()
    {
        GetTree().ChangeSceneToFile(mainMenuScene);
    }
}
