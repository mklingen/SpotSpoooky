using Godot;
using System;
using System.Collections.Generic;

public partial class RandomSFXPlayer : AudioStreamPlayer3D
{
    [Export]
    private Godot.Collections.Array<AudioStream> streams;

    [Export] private bool playOnAwake = false;


    public override void _Ready()
    {
        base._Ready();
        if (playOnAwake) {
            var settings = new Settings();
            settings.LoadSettings();
            PlayRandom((float)settings.SFXVolume);
        }
    }


    public void PlayRandom(float volume)
    {
        if (streams != null && streams.Count > 0) {
            this.VolumeDb = BGMusicManager.Linear2DB(volume);
            this.Stream = streams[GD.RandRange(0, streams.Count - 1)];
            this.Play();
        }
    }

}
