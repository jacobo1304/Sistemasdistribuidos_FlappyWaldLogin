[System.Serializable]
public class UserModel
{
    public string username;
    public string password;

    public UserModel(string u, string p)
    {
        username = u;
        password = p;
    }
}