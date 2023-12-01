using Godot;
using System;

public partial class UISound : AudioStreamPlayer
{
    [Export]
    AudioStream sliderSound = null;

    [Export]
    AudioStream clickSound = null;

    private Settings settings = null;

    public override void _Ready()
    {
        base._Ready();
        settings = new Settings();
        settings.LoadSettings();
        // When _Ready is called, there might already be nodes in the tree,
        // so connect all existing buttons
        ConnectButtons(GetTree().Root);
        GetTree().NodeAdded += (Node node) =>
        {
            _OnSceneTreeNodeAdded(node);
        };
    }
    private void _OnSceneTreeNodeAdded(Node node)
    {
        if (node is Button) {
            ConnectToButton(node as Button);
        }
        if (node is Slider) {
            ConnectToSlider(node as Slider);
        }
    }

    private void _OnButtonPressed()
    {
        Stream = clickSound;
        settings.LoadSettings();
        VolumeDb = BGMusicManager.Linear2DB((float)settings.SFXVolume);
        Play();
    }


    private void _OnSliderDragStarted()
    {
        Stream = sliderSound;
        settings.LoadSettings();
        VolumeDb = BGMusicManager.Linear2DB((float)settings.SFXVolume);
        Play();
    }

    private void _OnSliderDragEnded()
    {
        settings.LoadSettings();
        VolumeDb = BGMusicManager.Linear2DB((float)settings.SFXVolume);
        Stop();
    }

    // Recursively connect all buttons
    private void ConnectButtons(Node root)
    {
        foreach (Node child in root.GetChildren()) {
            if (child is BaseButton) {
                ConnectToButton(child as BaseButton);
            }
            if (child is Slider) {
                ConnectToSlider(child as Slider);
            }
            ConnectButtons(child);
        }
    }

    private void ConnectToButton(BaseButton button)
    {
        button.Pressed += () =>
        {
            _OnButtonPressed();
        };
    }


    private void ConnectToSlider(Slider slider)
    {
        slider.DragStarted += () =>
        {
            _OnSliderDragStarted();
        };
        slider.DragEnded += (arg) =>
        {
            _OnSliderDragEnded();
        };
    }

}
