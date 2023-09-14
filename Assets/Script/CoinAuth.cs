using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Proyecto26;
using System;
using UnityEngine.Events;
using Newtonsoft.Json;


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
        Debug.Log("Posting coin invoked!");

        // var coinKey = coinDictionary.FirstOrDefault(x => x.Value == coin.coin).Key;
        // coin.coin.is_available = false;
        coinDictionary[coin.ID] = coin.coin;
        Debug.Log("Updating coin dictionay complete!");
        
        var databaseURL = AuthController.Instance.GetDBURL();
        Debug.Log("Posting coin " + coin.ID + " to : " + databaseURL);
        var coinjson = JsonUtility.ToJson(coin.coin);
        // AuthController.Instance.UpdateCoinDatabase(coinDictionary, () => { onDone?.Invoke(); }, (reason) => { onFailed?.Invoke(reason); });
        RestClient.Put<string>($"{databaseURL}coin_tests/{coin.ID}.json", coinjson).Then( (done) => { onDone?.Invoke(); }).Catch( (onRejected) => { onFailed?.Invoke(onRejected); });
    }

    public void GetCoinStatus(string coinID, UnityAction onAvailable, UnityAction onUnavailable) {
        AuthController.Instance.GetCoinListFromDB( (done) => {
                SetCoinDB(done);
                if (coinDictionary[coinID].is_available) {
                    onAvailable?.Invoke();
                }
                else {
                    onUnavailable?.Invoke();
                }
            }
        );
    }

}
