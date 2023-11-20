using Godot;
using System;
using System.Collections.Generic;

public partial class RandomSFXPlayer : AudioStreamPlayer3D
{
    [Export]
    private Godot.Collections.Array<AudioStream> streams;

    public void PlayRandom(float volume)
    {
        if (streams != null && streams.Count > 0) {
            this.VolumeDb = BGMusicManager.Linear2DB(volume);
            this.Stream = streams[GD.RandRange(0, streams.Count - 1)];
            this.Play();
        }
    }

}
