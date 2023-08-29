using System;

[Serializable]
public class GlobalUserSettings
{
    public int coinDistance;
    public int limit;

    public GlobalUserSettings (int limit, int coindistance) {
        this.coinDistance = coindistance;
        this.limit = limit;
    }
}
