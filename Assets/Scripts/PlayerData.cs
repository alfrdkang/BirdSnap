using System;

[System.Serializable]
public class PlayerData
{
    public string name;
    public string email;
    public string password;
    public string creationDate;
    public string lastLoginDate;
    public int highScore;
    public int gamesPlayed;
    public int rocksShot;
    public string[] achievements;
    public int level;

    public PlayerData(string name, string email, string password, string creationDate, string lastLoginDate, int highScore, int gamesPlayed, int rocksShot, string[] achievements, int level)
    {
        this.name = name;
        this.email = email;
        this.password = password;
        this.creationDate = ConvertNowToTimeStamp();
        this.lastLoginDate = lastLoginDate;
        this.highScore = highScore;
        this.gamesPlayed = gamesPlayed;
        this.rocksShot = rocksShot;
        this.achievements = achievements;
        this.level = level;
    }
    
    public string ConvertNowToTimeStamp()
    {
        DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
        // Get the unix timestamp in seconds
        return dto.ToUnixTimeSeconds().ToString();
        
    }
}
