using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Proyecto26;
using System;
using UnityEngine.Events;


public enum PostalLocation{
    DKI,
    SUMTRA,
    SUMSEL,
}

public class CoinAuth : MonoBehaviour
{
    public static CoinAuth Instance { get; private set; }

    public Dictionary<string, NewCoin> coinDictionary = new Dictionary<string, NewCoin>();
    public GameObject coinPrefab;
    public List<NewCoin> coinList = new List<NewCoin>();
    public delegate void OnDonePostCoinCallback();
    public delegate void OnFailedPostCoinCallback(Exception reason);

    private void Awake()
    {
        if (Instance != this && Instance != null)
            Destroy(this);
        else
            Instance = this;

        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) SpawnCoin( (coin)=> {} );
    }

    public void SetCoinDB(Dictionary<string, NewCoin> coindb) {
        coinList = coindb.Values.ToList();
        coinDictionary = coindb;
        print(coinDictionary.Count);
        foreach (var item in coinDictionary)
        {
            Debug.Log($"coins with {item.Key} is filled with {item.Value}");
        }
    }
    
    public List<GameObject> SpawnCoin(UnityAction<Coins> onInteract) {
        
        var spawnedCoin = new List<GameObject>();

        foreach (var item in coinDictionary)
        {
            if(!item.Value.is_available) continue;

            //Check geolocation for filtering
            

            var newCoin = Instantiate(coinPrefab, Vector3.zero, Quaternion.identity);
            
            Coins coinItem = newCoin.GetComponent<Coins>();
            coinItem.InitCoin(item.Value, item.Key, (on) => onInteract?.Invoke(coinItem));
            
            spawnedCoin.Add(newCoin);
        }

        return spawnedCoin;
    }
    
    public List<GameObject> SpawnCoin(UnityAction<Coins> onInteract, UnityAction<string> onFailed) {
        
        var spawnedCoin = new List<GameObject>();

        foreach (var item in coinDictionary)
        {
            if(!item.Value.is_available) continue;

            var newCoin = Instantiate(coinPrefab, Vector3.zero, Quaternion.identity);
            
            Coins coinItem = newCoin.GetComponent<Coins>();
            coinItem.InitCoin(item.Value, item.Key, (on) => onInteract?.Invoke(coinItem), (msg) => onFailed?.Invoke(msg));
            
            spawnedCoin.Add(newCoin);
        }

        return spawnedCoin;
    }

    public void PostCoin(Coins coin, OnDonePostCoinCallback onDone, OnFailedPostCoinCallback onFailed) {
        var databaseURL = AuthController.Instance.GetDBURL();
        var coinKey = coinDictionary.FirstOrDefault(x => x.Value == coin.coin).Key;
        coin.coin.is_available = false;
        coinDictionary[coinKey] = coin.coin;
        
        Debug.Log("Posting coin" + coinKey + " to : " + databaseURL);
        RestClient.Put<NewCoin>($"{databaseURL}coin_tests/{coinKey}.json", coin.coin).Catch( onRejected => { onFailed?.Invoke(onRejected); }).Then( () => { onDone?.Invoke(); });
    }

}
