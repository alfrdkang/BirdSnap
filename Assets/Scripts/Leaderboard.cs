using System;

[System.Serializable]

public class Leaderboard
{
    public string name;
    public int highScore;
    public int birdsSnapped;
    public float accuracy;
    public string updatedDate;

    public Leaderboard(string name, int highScore, int birdsSnapped, float accuracy, string updatedDate)
    {
        this.name = name;
        this.highScore = highScore;
        this.birdsSnapped = birdsSnapped;
        this.accuracy = accuracy;
        this.updatedDate = updatedDate;
    }
}
