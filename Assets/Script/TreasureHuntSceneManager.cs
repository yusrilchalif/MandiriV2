using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Database;
using System;
using ARLocation;
using Models;
using UnityEngine.XR.ARFoundation;

public class TreasureHuntSceneManager : MonoBehaviour
{

    public static TreasureHuntSceneManager Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI coinTextScreenshot;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private GameObject panelErrorBottom;
    [SerializeField] private GameObject panelError;
    [SerializeField] private List<GameObject> allCoins = new List<GameObject>();
    [SerializeField] private ARRaycastManager raycastManager;

    private UserData localUserData;
    private List<ARRaycastHit> hitResult = new List<ARRaycastHit>();

    private void Awake()
    {
        if (Instance != this && Instance != null)
            Destroy(this);
        else
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        localUserData = UserAuth.Instance.CurrentUser;
        UpdateCoinText(0);

        // AuthManager.Instance.coinDBRef.ValueChanged += OnDatabaseValueChanged;
        ARLocationManager.Instance.Camera = Camera.main;
        ARLocationManager.Instance.OnARTrackingRestored( () => {
            ARLocationManager.Instance.ResetARSession();
            ARLocationManager.Instance.Restart();
        });
        StartSpawningCoin();

        panelErrorBottom.SetActive(false);
    }

    void Update() {
        if(Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);

            if(touch.phase == TouchPhase.Began) {
                if(raycastManager.Raycast(touch.position, hitResult, UnityEngine.XR.ARSubsystems.TrackableType.AllTypes)) {
                    foreach (var hit in hitResult)
                    {
                        Debug.Log("Touch detected!");
                        hit.trackable.GetComponent<Coins>().InteractCoin();
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var coin in allCoins)
        {
            Destroy(coin);
        }
        Instance = null;
        // AuthManager.Instance.coinDBRef.ValueChanged -= OnDatabaseValueChanged;
    }


    public void UpdateCoinText(int coinAmt)
    {
        var currCoinAt = localUserData.coinAmount + coinAmt;

        print("Coin A: " + currCoinAt);

        // Perbarui nilai teks pada kedua TextMeshProUGUI
        coinText.text = UserAuth.Instance.CurrentUser.coinAmount.ToString();

        // Perbarui nilai pada coinTextScreenshot dengan nilai sebenarnya
        coinTextScreenshot.text = UserAuth.Instance.CurrentUser.coinAmount.ToString();
        Debug.Log("Coin text update " + currCoinAt.ToString());
    }

    public void ShowError(string msg) {
        StartCoroutine(ShowErrorTimed(msg));
    }

    IEnumerator ShowErrorTimed(string msg) {
        errorText.gameObject.SetActive(true);
        errorText.text = msg;
        yield return new WaitForSeconds(2f);
        errorText.gameObject.SetActive(false);
    }

    public void ClosePanel()
    {
        panelError.SetActive(false);
        panelErrorBottom.SetActive(true);
    }

    public void OnDatabaseValueChanged(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        foreach (var coin in allCoins)
        {
            Destroy(coin);
        }

        StartSpawningCoin();
    }

    public void StartSpawningCoin() {
        print("Spawning coins....");
        allCoins = CoinAuth.Instance.SpawnCoin( (coin) => { UpdateCoinText(coin.coin.value);}, (msg) => { ShowError(msg); });
        
        // Tambah baris ini untuk memperbarui coinResultText
        var totalCoins = UserAuth.Instance.CurrentUser.coinAmount;
        coinTextScreenshot.text = totalCoins.ToString();
    }

    public void ResetARSession(object sender, ValueChangedEventArgs args) {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        ARLocationManager.Instance.ResetARSession();
        ARLocationManager.Instance.Restart();
    }
}
