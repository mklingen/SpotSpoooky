using Godot;
using System;

public partial class Settings : Resource
{
    public bool Fullscreen { get; set; } = true;
    public bool VSync { get; set; } = true;
    public bool Antialiasing { get; set; } = true;
    public bool InvertVerticalAxis { get; set; } = false;


    public void SaveSettings()
    {
        ConfigFile config = new ConfigFile();
        config.SetValue("display", "fullscreen", Fullscreen);
        config.SetValue("display", "vsync", VSync);
        config.SetValue("display", "antialiasing", Antialiasing);
        config.SetValue("input", "invert_vertical_axis", InvertVerticalAxis);

        config.Save("user://settings.cfg");
    }

    public void LoadSettings()
    {
        ConfigFile config = new ConfigFile();
        Error err = config.Load("user://settings.cfg");

        if (err == Error.Ok) {
            Fullscreen = (bool)config.GetValue("display", "fullscreen", Fullscreen);
            VSync = (bool)config.GetValue("display", "vsync", VSync);
            Antialiasing = (bool)config.GetValue("display", "antialiasing", Antialiasing);
            InvertVerticalAxis = (bool)config.GetValue("input", "invert_vertical_axis", InvertVerticalAxis);
        }
    }

}
