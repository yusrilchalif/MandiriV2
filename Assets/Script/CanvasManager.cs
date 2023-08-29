using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Extensions;
using Random = UnityEngine.Random;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance {get; private set;}

    public TextMeshProUGUI fyiText;
    public TextMeshProUGUI userName;
    [SerializeField] private SO_InformationLibrary informationLibrary;

    [SerializeField] private TextMeshProUGUI playerCoinText;
    //DEMO
    private bool isDemo;

    private void Awake() {
        if(Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Randomize FYI Text from Library
        int indexLibrary = Random.Range(0, informationLibrary.infoLibrary.Count);
        string text = informationLibrary.infoLibrary[indexLibrary].info;
        UpdateInfoText(text);

        //Update username
        string username = UserAuth.Instance.CurrentUser.userName;
        char splittedWords = '@';
        string[] realUser = username.Split(splittedWords);
        UpdateUsername(realUser[0]);

        //Get Coin from Firestore
        /*int coin = 0;
        testDB = FirebaseFirestore.DefaultInstance;
        DocumentReference coinRef = testDB.Collection("coins").Document("coinsID");
        coinRef.Listen( snapshot => {
            snapshot.TryGetValue<int>("coinsAmount", out coin);
            Debug.Log(snapshot.Id + " " + coin.ToString());
            playerCoinText.text = coin.ToString();
        });
        Debug.Log(coinRef + " " + coin);*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateInfoText(string infoText) {
        fyiText.text = infoText;
    }

    public void UpdateUsername(string username) {
        userName.text = $"Halo, {username} si Penjelajah";
    }


}
