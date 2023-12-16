using Godot;
using System;

public partial class BGMusicManager : AudioStreamPlayer
{

    [Export] 
    AudioStream defaultBackgroundMusic = null;

    private Settings settings = null;

    public override void _Ready()
    {
        base._Ready();
        settings = new Settings();
        settings.LoadSettings();
        Stream = defaultBackgroundMusic;
        SetVolume(settings.MusicVolume);
        
        Play(0);

    }

    public static float Linear2DB(float linear)
    {
        return Mathf.Log(linear) * 8.6858896380650365530225783783321f; 
    }

    public void SetVolume(double linear)
    {
        VolumeDb = Linear2DB((float)linear);
    }

    public static BGMusicManager Get(Node node)
    {
        return node.GetNode<BGMusicManager>("/root/BgMusicPlayer");
    }
}
