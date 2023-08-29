using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using TMPro;
using System.Text;
using UnityEngine.UI;
using UnityEngine.Events;

public class HomeSceneManager : MonoBehaviour
{
    public static HomeSceneManager Instance { get; private set; }

    //public TextMeshProUGUI fyiText;
    public TextMeshProUGUI userName;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI coinRedeemText;
    [SerializeField] private SO_InformationLibrary informationLibrary;

    private UserData currentUserData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Get User based from userID
        UpdateUsername(UserAuth.Instance.CurrentUser.userName);
        UpdateUserCoin(UserAuth.Instance.CurrentUser.coinAmount);
        UpdateRedeemCoin(UserAuth.Instance.CurrentUser.coinAmount);
        
        int indexLibrary = Random.Range(0, informationLibrary.infoLibrary.Count);
        string text = informationLibrary.infoLibrary[indexLibrary].info;
    }

    // Update is called once per frame
    void Update()
    {
        if(Application.platform == RuntimePlatform.Android)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Logout();
            }
        }
    }

    public void Logout() {
        AuthController.Instance.LogOut();
    }


    public void SetUser(UserData user) {
        this.currentUserData = user;
    }

    //public void UpdateInfoText(string infoText)
    //{
    //    fyiText.text = infoText;
    //}

    public void UpdateUsername(string username)
    {
        userName.text = $"Halo, {username} si Penjelajah";
    }

    public void UpdateUserCoin(int coinAmt) {
        coinText.text = coinAmt.ToString();
    }

    public void UpdateRedeemCoin(int coinAmt) {
        coinRedeemText.text = $"Jumlah koin kamu : {coinAmt}";
    }

    public void UpdateRedeemCoin(int coinAmt, string updateText) {
        coinRedeemText.text = $"{updateText} {coinAmt}";
    }

    public static string RemoveSpecialChar(string input) {
        string[] chars = new string[] { ",", ".", "/", "!", "@", "#", "$", "%", "^", "&", "*", "'", "\"", ";", "_", "(", ")", ":", "|", "[", "]" };

        for (int i = 0; i < chars.Length; i++)
        {
            if (input.Contains(chars[i]))
                input = input.Replace(chars[i], "");
        }
        return input;
    }

    public static void GetUserCallback(UserData userdata) {
        HomeSceneManager.Instance.SetUser(userdata);
        Debug.Log(userdata.userName + " " + userdata.ID);
    }

}
