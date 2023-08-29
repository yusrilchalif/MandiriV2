using System;
using System.Collections.Generic;
using UnityEngine;
using ARLocation;

public enum CoinType {
    GOLDEN,
    WHITE,
    BLUE
}

[Serializable]
public class Coin {
    [Tooltip("Must be different for each coins")] public string markerNameID;
    public bool isAvailable;
    public Location coinLocation;
    public CoinType coinType;
}

[CreateAssetMenu(fileName = "Coin Database", menuName = "Data/Coin Database")]
public class SO_CoinDatabase : ScriptableObject
{
    public List<Coin> coinList;
    public bool prioritizeCloudDatabase = true;
}
