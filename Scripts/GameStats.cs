using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class GameStats : Node
{
    // If true, the last level was a win for the player.
    public bool didPlayerWinLastGame = false;
    // The current level that the player is on.
    public int currentLevel = 1;
    // The maximum level index.
    public int maxLevels = 10;
    // The level the player was on when they lost.
    public int levelLost = 0;

    public float TimeLevelStarted = 0.0f;
    public float TimeLevelFinished = 0.0f;
    public struct ShotStats
    {
        public int TotalNumShots = 0;
        public int TotalNumFriendlyFire = 0;
        public int TotalNumSpookyShots = 0;
        public int TotalNumScaredNPCs = 0;

        public int TotalNumMisses => TotalNumShots - (TotalNumSpookyShots + TotalNumFriendlyFire);

        public ShotStats()
        {
        }
    }

    public ShotStats CurrentLevelShotStats = new ShotStats();


    public void OnStartNextLevel()
    {
        TimeLevelStarted = Root.Timef();
        CurrentLevelShotStats = new ShotStats();
    }

    public void OnEndCurrentLevel()
    {
        TimeLevelFinished = Root.Timef();
    }

    public void OnShoot()
    {
        CurrentLevelShotStats.TotalNumShots++;
    }

    public void OnNPCEaten()
    {
        CurrentLevelShotStats.TotalNumScaredNPCs++;
    }

    public void OnShootSpooky()
    {
        CurrentLevelShotStats.TotalNumSpookyShots++;
    }

    public void OnFriendlyFire()
    {
        CurrentLevelShotStats.TotalNumFriendlyFire++;
    }


    public static GameStats Get(Node node)
    {
        return node.GetNode<GameStats>("/root/GameStats");
    }

    // Gets a scalar from 0-1 indicating the progress along the max number of levels.
    private float GetLevel01()
    {
        return (float)(currentLevel) / maxLevels;
    }

    // Gets a scaled value between minValue and maxValue, scaled by the level.
    public int GetLevelScaledValue(int minValue, int maxValue)
    {
        return (int)(GetLevel01() * (maxValue - minValue)) + minValue;
    }

    // Gets a scaled value between minValue and maxValue, scaled by the level.
    public float GetLevelScaledValue(float minValue, float maxValue)
    {
        return GetLevel01() * (maxValue - minValue) + minValue;
    }
}
