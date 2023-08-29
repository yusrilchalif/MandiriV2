using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using System;

public class UserAuth : MonoBehaviour
{
    public static UserAuth Instance { get; private set; }

    public UserData CurrentUser { get; private set; }
    public UserData user;
    public delegate void OnDonePostUserCallback();
    public delegate void OnFailedPostUserCallback(Exception reason);

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
        
    }

    public void SetCurrentUser(UserData newUser) {
        print("Setting user to : " + newUser.userName);
        CurrentUser = newUser;
        user = CurrentUser;
    }

    public void PostUser(UserData user, OnDonePostUserCallback onDone, OnFailedPostUserCallback onFailed) {
        var databaseURL = AuthController.Instance.GetDBURL();
        Debug.Log($"Posting to DB to : {databaseURL} with userID : {user.email} ");
        RestClient.Put<UserData>($"{databaseURL}users/{user.email}.json", user).Catch( onRejected => { onFailed(onRejected); }).Done( () => { onDone?.Invoke(); } );
    }
}
