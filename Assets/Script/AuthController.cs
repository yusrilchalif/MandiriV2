using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Proyecto26;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine.Events;
using GoogleSignIn = Google.GoogleSignIn;
using GoogleSignInConfiguration = Google.GoogleSignInConfiguration;
using GoogleSignInUser = Google.GoogleSignInUser;
using Facebook.Unity;
using System;
using Models;

public class AuthController : MonoBehaviour
{
    public static AuthController Instance { get; private set; }
    
    [Header("Google Sign in Configs")]
    private GoogleSignInConfiguration googleConfiguration;

    [Header("Firebase Core Configs")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;

    [Header("Firebase Database Configs")]
    public DatabaseReference coinDBRef;
    public CoinAuth coinAuth;
    public UserAuth userAuth;
    public GlobalUserSettings globalUserSettings;
    public string email;
    public string password;


    public delegate void PostUserCallback();
    public delegate void GetUserCallbackSuccesful(UserData user);
    public delegate void GetUserCallbackFailed(UserData newUser);

    
    public delegate void PostCoinDBCallback();
    public delegate void GetCoinDbCallback(Dictionary<string, NewCoin> coinDB);

    
    private static readonly string webClientId = "346821469395-62m24oc2qa9nr20nqa77nlhtl86mlde2.apps.googleusercontent.com";
    private static readonly string databaseURL = $"https://mandiriproject-94c0c-default-rtdb.asia-southeast1.firebasedatabase.app/";

    private void Awake()
    {
        if (Instance != this && Instance != null)
            Destroy(this);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);

        googleConfiguration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true
        };

        if(!FB.IsInitialized) FB.Init(FBInitCallback);
        else FB.ActivateApp();
        
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitFirebase();
            }
            else
            {
                Debug.LogError("Firebase not resolve" + dependencyStatus);
            }
        });

        GetGlobalSettings();
    }

    private void FBInitCallback()
    {
        if(FB.IsInitialized) FB.ActivateApp();
        else Debug.LogWarning("Facebook sdk failed to initialize!");
    }

    public void Login(string email, string password, UnityAction<string> onfailed) {
        GetCoinListFromDB( response => { coinAuth.SetCoinDB(response); });
        StartCoroutine(LoginAsync(email, password, () => { 
                SetUser(email, () => { GlobalSceneController.Instance.ChangeScene("Home 1");}); 
                },
            (e) => { onfailed?.Invoke(e); }));
    }

    public void TestLogin(bool autoChangeScene) {
        GetCoinListFromDB( response => { coinAuth.SetCoinDB(response); });
        StartCoroutine(LoginAsync(email, password, () => { SetUser(email, () => {
                if(autoChangeScene) GlobalSceneController.Instance.ChangeScene("Home 1");
                });
            }, (e) => {}));
    }

    public void LogOut() {
        if(auth != null && user != null) {
            print("Sign out successfully");
            auth.SignOut();
            GoogleSignIn.DefaultInstance.SignOut();
            GlobalSceneController.Instance.ChangeScene(0);
            print("Success sign out and change scene");
        }
        else
        {
            GoogleSignIn.DefaultInstance.SignOut();
            GlobalSceneController.Instance.ChangeScene(0);
            print("Sign out google successfully");
        }

    }
    
    public void TestCoin() {
        GetCoinListFromDB( response => { coinAuth.SetCoinDB(response); });
    }

    void InitFirebase()
    {
        //Base Firebase API
        auth = FirebaseAuth.DefaultInstance;

        //Init Firebase Auth
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
        coinDBRef = FirebaseDatabase.DefaultInstance.GetReference("coins");
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            if (signedIn)
            {
                user = auth.CurrentUser;
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    public void OnSignIn()
    {
        GoogleSignIn.Configuration = googleConfiguration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestEmail = true;
        AddStatusText("Calling SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    public void OnSignOut()
    {
        AddStatusText("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnDisconnect()
    {
        AddStatusText("Calling Disconnect");
        GoogleSignIn.DefaultInstance.Disconnect();
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<System.Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    AddStatusText("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    AddStatusText("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            AddStatusText("Canceled");
        }
        else
        {
            try
            {
                GoogleSignInUser user = task.Result;
                string cleanedEmail = RemoveSpecialChar(user.Email);
                string tempUsername = user.Email;
                char splittedWords = '@';
                string[] splitUsername = tempUsername.Split(splittedWords);
                tempUsername = splitUsername[0];

                // Buat objek UserData berdasarkan data yang telah dikumpulkan
                UserData userData = new UserData(user.UserId, cleanedEmail, user.GivenName, user.DisplayName, 0, 0, globalUserSettings.limit, 0);
                Debug.Log("User name is : " + user.DisplayName + " user given name is : " + user.GivenName + " user family name is " + user.FamilyName);
                Debug.Log("User email is : " + user.Email + "user cleaned email is " + cleanedEmail);

                GetCoinListFromDB( response => { coinAuth.SetCoinDB(response); });
                GetUser(cleanedEmail, userData,
                    (userdata) => {
                        Debug.Log("User found! with uname " + userdata.userName);
                        userAuth.SetCurrentUser(userdata);                        
                        GlobalSceneController.Instance.ChangeScene("Home 1");
                    },
                    (message) => {
                        // Posting berhasil, lakukan sesuatu jika diperlukan
                        Debug.Log("User data saved to Firebase!");
                        userAuth.SetCurrentUser(userData);
                        GlobalSceneController.Instance.ChangeScene("Home 1");
                });
                
                //Change scene
                // GlobalSceneController.Instance.ChangeScene("Home 1");

            }
            catch (System.Exception ex)
            {
                // Tangani kondisi error di sini
                AddStatusText("Error during authentication: " + ex.Message);
            }
        }
    }

    public void OnSignInSilently()
    {
        GoogleSignIn.Configuration = googleConfiguration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddStatusText("Calling SignIn Silently");

        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    }

    public void OnGamesSignIn()
    {
        GoogleSignIn.Configuration = googleConfiguration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;

        AddStatusText("Calling Games SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    public void FBLogin() {
        var perms = new List<string>() {"public_profile", "email"};
        FB.LogInWithReadPermissions(perms, FBLoginCallback);
    }

    private void FBLoginCallback(ILoginResult result)
    {
        if(FB.IsLoggedIn) {
            var fbToken = AccessToken.CurrentAccessToken;
            
            Credential credential = FacebookAuthProvider.GetCredential(fbToken.ToString());
            auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith( task => {
                if(task.IsCanceled) {
                    Debug.Log("FB login cancelled");
                    return;
                }
                if(task.IsFaulted) {
                    Debug.LogError("Getting fb cred error!" + task.Exception);
                    return;
                }

                var result = task.Result;
                Debug.Log("sign in with user data : " + result.User.Email + " " + result.User.DisplayName);

                string cleanedEmail = RemoveSpecialChar(result.User.Email);
                UserData newUser = new UserData(cleanedEmail, cleanedEmail, result.User.DisplayName, result.User.DisplayName, 0, 0, globalUserSettings.limit, 0);

                GetCoinListFromDB( response => { coinAuth.SetCoinDB(response); }); 
                GetUser(cleanedEmail, newUser,
                    (userget) => {
                        Debug.Log("Found user with the fb data!");
                        userAuth.SetCurrentUser(userget);
                        GlobalSceneController.Instance.ChangeScene("Home 1");
                    },
                    (message) => {
                        Debug.Log("Created new user from fb data!");
                        userAuth.SetCurrentUser(newUser);
                        GlobalSceneController.Instance.ChangeScene("Home 1");

                });
            });
        }
        else {
            Debug.Log("User login fb cancelled.");
        }
    }

    private List<string> messages = new List<string>();

    void AddStatusText(string text)
    {
        if (messages.Count == 5)
        {
            messages.RemoveAt(0);
        }
        messages.Add(text);
        string txt = "";
        foreach (string s in messages)
        {
            txt += "\n" + s;
        }
    }


    public void SetUser(string email, UnityAction onDone) {
        Debug.Log("Getting user data...");

        var firebaseID = RemoveSpecialChar(email);
        Debug.Log("Getting user data from : " + firebaseID);
        GetUser(firebaseID, 
        (userdata) => { userAuth.SetCurrentUser(userdata); onDone?.Invoke(); },
        (newuser) => {
            Debug.Log($"Failed to get user data. Creating new data for user...");
            userAuth.SetCurrentUser(newuser); 
            onDone?.Invoke();
        }
        );
    }

    private IEnumerator LoginAsync(string email, string password, UnityAction onSuccess, UnityAction<string> onFailed)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);
        if (loginTask.Exception != null)
        {
            Debug.LogError(loginTask.Exception);
            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;
            string failedMessage = "Login gagal : ";
            switch (authError)
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Email tidak ditemukan!";
                    break;
                case AuthError.WrongPassword:
                    failedMessage += "Password Salah!";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Email kosong!";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Password Kosong!";
                    break;
                default:
                    failedMessage = "Akun tidak ditemukan!";
                    break;
            }
            Debug.Log(failedMessage);
            onFailed?.Invoke(failedMessage);
        }
        else
        {
            user = loginTask.Result;
            Debug.LogFormat($"You Are Successfully Logged In {user.Email}");
            onSuccess?.Invoke();
        }
    }

    
    public UserData GetUser(string firebaseID, GetUserCallbackSuccesful success, GetUserCallbackFailed failed)
    {
        Debug.Log($"Getting user from DB : {databaseURL} with userID : {firebaseID} ");
        UserData currentUser = null;
        RestClient.Get<UserData>($"{databaseURL}users/{firebaseID}.json").Done(
            response => {
                Debug.Log("Succesful");
                currentUser = response;
                success?.Invoke(response);
            },
            (response2) => {
                Debug.Log("Failed");
                string tempUsername = firebaseID;
                char splittedWords = '@';
                string[] username = tempUsername.Split(splittedWords);
                tempUsername = username[0];
                string cleanedEmail = RemoveSpecialChar(firebaseID);
                UserData newUser = new UserData(cleanedEmail, cleanedEmail, tempUsername, tempUsername, 0, 0, globalUserSettings.limit, 0);
                currentUser = newUser;
                PostUser(newUser, newUser.email, () => failed?.Invoke(currentUser));
            });

        return currentUser;
    }
    
    public UserData GetUser(string email, UserData onetapUser, GetUserCallbackSuccesful success, GetUserCallbackFailed failed)
    {
        UserData currentUser = onetapUser;

        Debug.Log($"Getting user from DB : {databaseURL} with userID : {email} ");
        
        RestClient.Get<UserData>($"{databaseURL}users/{email}.json").Done(
            response => {
                Debug.Log("Succesful get user : " + response.userName);
                if (response == null)
                {
                    PostUser(currentUser, email, () => {
                        success?.Invoke(currentUser);
                    });
                }
                else
                {
                    currentUser = response;
                    success?.Invoke(response);
                }
            },
            (response2) => {
                Debug.Log("Failed with " + response2.Message);
                string newFirebaseID = email;
                char splittedWords = '@';
                string[] fixedUsername = newFirebaseID.Split(splittedWords);
                newFirebaseID = RemoveSpecialChar(newFirebaseID);
                currentUser = onetapUser;
                PostUser(currentUser, newFirebaseID, () => failed?.Invoke(currentUser));
            });

        return currentUser;
    }

    public void RegisterNewUser(string email, string username, string name, string password, UnityAction<string> onFailed, PostUserCallback onSuccess) {
        StartCoroutine(CreateNewUser(email, password, username, name, onFailed, onSuccess));
    }

    public IEnumerator CreateNewUser(string email, string password, string username, string name, UnityAction<string> onFailed, PostUserCallback onSuccess) {
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null) {
            var errorMsg = registerTask.Exception;
            onFailed?.Invoke(errorMsg.Message);
        }
        else {
            string firebaseID = email;
            firebaseID = RemoveSpecialChar(firebaseID);
            UserData newUser = new UserData(firebaseID, firebaseID, name, username, 0, 0, globalUserSettings.limit, 0);
            PostUser(newUser, firebaseID, () => onSuccess?.Invoke());
        }
    }

    public void GetGlobalSettings() {
        Debug.Log($"Getting global settings from DB....");
        RestClient.Get($"{databaseURL}configs/user.json").Then( response => {
            Debug.Log("Trying to parse json to global settings...");
            var responsejson = response.Text;
            Debug.Log($"Parsing {responsejson}");
            globalUserSettings = JsonConvert.DeserializeObject<GlobalUserSettings>(responsejson);
            Debug.Log($"Global settings updated! : user limit {globalUserSettings.limit} and max distance {globalUserSettings.coinDistance}");
        });
    }
    
    public void PostUser(UserData user, string firebaseID, PostUserCallback callback) {
        Debug.Log($"Posting to DB to : {databaseURL} with userID : {firebaseID} ");
        RestClient.Put<UserData>($"{databaseURL}users/{firebaseID}.json", user).Then(response => {
            callback();
        });
    }

    public delegate void OnDonePostCoinCallback();
    public delegate void OnFailedPostCoinCallback(Exception reason);

    public void UpdateCoinDatabase(Dictionary<string, NewCoin> coinDB, OnDonePostCoinCallback onDone, OnFailedPostCoinCallback onFailed) {
        RestClient.Put<Dictionary<string, NewCoin>>($"{databaseURL}coin_tests.json", coinDB).Then(response => {
            onDone?.Invoke();
        }).Catch( (reason) => { onFailed?.Invoke(reason); });
    }

    public Dictionary<string, NewCoin> GetCoinListFromDB(GetCoinDbCallback callback) {
        Dictionary<string, NewCoin> coinList = new Dictionary<string, NewCoin>();

        RestClient.Get($"{databaseURL}coin_tests.json").Then(response => {
            Debug.Log("Trying to parse json to coin list...");
            Debug.Log($"Parsing {response.Text}");
            var responseJson = response.Text;
            var coinDict = JsonConvert.DeserializeObject<Dictionary<string, NewCoin>>(responseJson);
            coinList = coinDict;
            Debug.Log($"Parsed with result : {coinDict.Values}");
            callback(coinDict);
        });

        return coinList;
    }

    public string GetDBURL() {
        return databaseURL;
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
}
