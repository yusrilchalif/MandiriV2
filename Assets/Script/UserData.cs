using System;

[Serializable]
public class UserData
{
    public string ID;
    public string name;
    public string userName;
    public string email;
    public int coinAmount;
    public int currentLimit;
    public int maxLimit;

    public UserData(string userID, string username, int coinAmt) {
        this.ID = userID;
        this.userName = username;
        this.coinAmount = coinAmt;
    }

    public UserData(string userID, string email, string name, string username, int coinAmt, int currentLimit, int maxLimit) {
        this.ID = userID;
        this.name = name;
        this.userName = username;
        this.email = email;
        this.coinAmount = coinAmt;
        this.currentLimit = currentLimit;
        this.maxLimit = maxLimit;
    }
}