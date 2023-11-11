using Godot;
using System;

public partial class Options : Control
{

    [Export(PropertyHint.File, "*.tscn,")]
    private string mainMenuScene;

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    private Settings settings;

    private CheckBox GetCheckbox(string name)
    {
        return FindChild(name) as CheckBox;
    }

    public override void _Ready()
    {
        settings = new Settings();
        settings.LoadSettings();
        GetCheckbox("FullscreenCheckBox").GrabFocus();
        // Populate UI elements with settings values
        GetCheckbox("FullscreenCheckBox").SetPressedNoSignal(settings.Fullscreen);
        GetCheckbox("VSyncCheckBox").SetPressedNoSignal(settings.VSync);
        GetCheckbox("AntialiasingCheckBox").SetPressedNoSignal(settings.Antialiasing);
        GetCheckbox("InvertVerticalAxisCheckbox").SetPressedNoSignal(settings.InvertVerticalAxis);
    }

    // Called when the user clicks "Save" button in the UI
    public void OnSaveButtonPressed()
    {
        // Update settings with UI values
        settings.Fullscreen = GetCheckbox("FullscreenCheckBox").ButtonPressed;
        settings.VSync = GetCheckbox("VSyncCheckBox").ButtonPressed;
        settings.Antialiasing = GetCheckbox("AntialiasingCheckBox").ButtonPressed;
        settings.InvertVerticalAxis = GetCheckbox("InvertVerticalAxisCheckbox").ButtonPressed;

        // Save settings
        settings.SaveSettings();
        GetTree().ChangeSceneToFile(mainMenuScene);
    }
}
