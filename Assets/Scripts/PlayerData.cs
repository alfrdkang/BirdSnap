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
    public string[] achievements;
    public float accuracy;

    public PlayerData(string name, string email, string creationDate, string lastLoginDate, int highScore, int gamesPlayed, int birdsSnapped, string[] achievements, float accuracy)
    {
        this.name = name;
        this.email = email;
        this.creationDate = ConvertNowToTimeStamp();
        this.lastLoginDate = lastLoginDate;
        this.highScore = highScore;
        this.gamesPlayed = gamesPlayed;
        this.birdsSnapped = birdsSnapped;
        this.achievements = achievements;
        this.accuracy = accuracy;
    }
    
    public string ConvertNowToTimeStamp()
    {
        DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
        // Get the unix timestamp in seconds
        return dto.ToUnixTimeSeconds().ToString();
    }
}
