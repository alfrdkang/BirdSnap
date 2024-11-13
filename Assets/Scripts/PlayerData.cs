using System;

[System.Serializable]
public class PlayerData
{
    public string name;
    public string email;
    public string creationDate;
    public string lastLoginDate;
    public int highScore;
    public int gamesPlayed;
    public int birdsSnapped;
    public int totalSnaps;
    public string[] achievements;
    public float accuracy;

    public PlayerData(string name, string email, string creationDate, string lastLoginDate, int highScore, int gamesPlayed, int birdsSnapped, int totalSnaps, string[] achievements, float accuracy)
    {
        this.name = name;
        this.email = email;
        this.creationDate = creationDate;
        this.lastLoginDate = lastLoginDate;
        this.highScore = highScore;
        this.gamesPlayed = gamesPlayed;
        this.birdsSnapped = birdsSnapped;
        this.totalSnaps = totalSnaps;
        this.achievements = achievements;
        this.accuracy = accuracy;
    }
}
