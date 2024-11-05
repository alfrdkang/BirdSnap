using System;

[System.Serializable]

public class Leaderboard
{
    public string name;
    public float highScore;
    public string updatedDate;

    public Leaderboard(string name, float highScore, string updatedDate)
    {
        this.name = name;
        this.highScore = highScore;
        this.updatedDate = updatedDate;
    }
}
