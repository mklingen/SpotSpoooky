using Godot;
using System;

public class AnimationOffsets
{
    public string Pattern { get; set; }
    public string[] OtherPatterns { get; set; }
    
    public  class Offset {
        public int X;
        public int Y;

        [Export]
        public int Frequency;
    }
    public Offset[] Offsets;

    public Vector2 GetRandomOffset()
    {
        int total = 0;
        foreach (Offset offset in Offsets) {
            total += offset.Frequency;
        }
        int idx = GD.RandRange(0, total);
        int carat = 0;
        foreach (Offset offset in Offsets) {
            carat += offset.Frequency;
            if (carat >= idx) {
                return new Vector2(offset.X, offset.Y);
            }
        }
        return new Vector2(0, 0);

    }
}
