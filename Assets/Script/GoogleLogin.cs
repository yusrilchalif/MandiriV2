using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Google;
using GoogleSignIn = Google.GoogleSignIn;
using GoogleSignInConfiguration = Google.GoogleSignInConfiguration;
using GoogleSignInUser = Google.GoogleSignInUser;
using Proyecto26;

public class GoogleLogin : MonoBehaviour
{
    public TextMeshProUGUI statusText;

    public string webClientId = "346821469395-62m24oc2qa9nr20nqa77nlhtl86mlde2.apps.googleusercontent.com";
    public static string UserID { get; private set; }
    public static string UserName { get; private set; }
    public static int UserCoin { get; private set; }


    private GoogleSignInConfiguration configuration;
    private static readonly string databaseURL = $"https://mandiriproject-94c0c-default-rtdb.asia-southeast1.firebasedatabase.app/";
    private UserData currentUserData;

    // Defer the configuration creation until Awake so the web Client ID
    // Can be set via the property inspector in the Editor.
    void Awake()
    {
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true
        };
    }

    public void OnSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
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

                // Buat objek UserData berdasarkan data yang telah dikumpulkan
                UserData userData = new UserData(cleanedEmail, user.DisplayName, 0);

                GetUser(cleanedEmail,
                        (success) => {
                            print("User data get from firebase!");
                        },
                        () => {
                            // Posting berhasil, lakukan sesuatu jika diperlukan
                            Debug.Log("User data saved to Firebase!");
                        }
                );
                SceneManager.LoadSceneAsync("Home 1");

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
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddStatusText("Calling SignIn Silently");

        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    }

    public void OnGamesSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;

        AddStatusText("Calling Games SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
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
        statusText.text = txt;
    }

    public delegate void PostUserCallback();
    public delegate void GetUserCallbackSuccesful(UserData user);
    public delegate void GetUserCallbackFailed();

    public void PostUser(UserData user, string userID, PostUserCallback callback)
    {
        Debug.Log($"Posting to DB to : {databaseURL} with userID : {userID}");
        RestClient.Put<UserData>($"{databaseURL}users/{userID}.json", user).Then(response =>
        {
            print("Posting user succesful!");
            callback();
        });
    }

    // Method to retrieve user data (GET)
    public void GetUser(string userID, GetUserCallbackSuccesful success, GetUserCallbackFailed failed)
    {
        Debug.Log($"Getting user from DB : {databaseURL} with userID : {userID}");
        RestClient.Get<UserData>($"{databaseURL}users/{userID}.json").Done(
            response =>
            {
                Debug.Log("Successful");
                UserData user = response;
                success(user);
            },
            (response2) =>
            {
                Debug.Log("Rejected");
                string tempUsername = userID;
                char splittedWords = '@';
                string[] username = tempUsername.Split(splittedWords);
                tempUsername = username[0];
                UserData newUser = new UserData(userID, tempUsername, 0);
                PostUser(newUser, newUser.ID, () => { failed(); });
            });
    }

    public static string RemoveSpecialChar(string input)
    {
        string[] chars = new string[] { ",", ".", "/", "!", "@", "#", "$", "%", "^", "&", "*", "'", "\"", ";", "_", "(", ")", ":", "|", "[", "]" };

        for (int i = 0; i < chars.Length; i++)
        {
            if (input.Contains(chars[i]))
                input = input.Replace(chars[i], "");
        }
        return input;
    }
}