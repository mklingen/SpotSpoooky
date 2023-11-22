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



    private T GetChild<T>(string name)where T : Control
    {
        return FindChild(name) as T;
    }

    private CheckBox GetCheckbox(string name)
    {
        return GetChild<CheckBox>(name);
    }
    private Slider GetSlider(string name)
    {
        return GetChild<Slider>(name);
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
        GetCheckbox("InstantZoomCheckbox").SetPressedNoSignal(settings.InstantZoom);
        GetSlider("SFXVolume").SetValueNoSignal(settings.SFXVolume);
        GetSlider("MusicVolume").SetValueNoSignal(settings.MusicVolume);
    }

    // Called when the user clicks "Save" button in the UI
    public void OnSaveButtonPressed()
    {
        // Update settings with UI values
        settings.Fullscreen = GetCheckbox("FullscreenCheckBox").ButtonPressed;
        settings.VSync = GetCheckbox("VSyncCheckBox").ButtonPressed;
        settings.Antialiasing = GetCheckbox("AntialiasingCheckBox").ButtonPressed;
        settings.InvertVerticalAxis = GetCheckbox("InvertVerticalAxisCheckbox").ButtonPressed;
        settings.InstantZoom = GetCheckbox("InstantZoomCheckbox").ButtonPressed;
        settings.SFXVolume = GetSlider("SFXVolume").Value;
        settings.MusicVolume = GetSlider("MusicVolume").Value;
        // Save settings
        settings.SaveSettings();
        BGMusicManager.Get(this).SetVolume(settings.MusicVolume);
        GetTree().ChangeSceneToFile(mainMenuScene);
    }
}
