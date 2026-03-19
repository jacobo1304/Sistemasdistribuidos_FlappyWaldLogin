using System;

[Serializable]
public class UserData
{
    public int score;

    public UserData() { }

    public UserData(int score)
    {
        this.score = score;
    }
}

[Serializable]
public class User
{
    public string _id;
    public string username;
    public bool estado;
    public UserData data;
}

[Serializable]
public class UserResponse
{
    public User usuario;
    public string token;
}

[Serializable]
public class ScoreUpdateRequest
{
    public string username;
    public UserData data;

    public ScoreUpdateRequest(string username, int score)
    {
        this.username = username;
        this.data = new UserData(score);
    }
}

[Serializable]
public class AuthRequest
{
    public string username;
    public string password;

    public AuthRequest(string username, string password)
    {
        this.username = username;
        this.password = password;
    }
}
