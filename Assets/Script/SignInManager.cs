using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase;
using UnityEngine.SceneManagement;
using Google;
using Firebase.Extensions;
using System.Threading.Tasks;

public class SignInManager : MonoBehaviour
{
    public Button signInButton;

    private FirebaseAuth auth;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            auth = FirebaseAuth.DefaultInstance;
        });

        signInButton.onClick.AddListener(LoginWithGoogle);
    }

    void LoginWithGoogle()
    {
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            RequestEmail = true,
        };
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnGoogleSignInCompleted);
    }

    void OnGoogleSignInCompleted(Task<GoogleSignInUser> task)
    {
        if(task.IsCanceled)
        {
            Debug.LogError("Login Failled");
            return;
        }
        if(task.IsFaulted)
        {
            Debug.LogError("Error: " + task.Exception);
            return;
        }

        GoogleSignInUser signInUser = task.Result;

        //Get string ID Token and Access Token
        string idToken = signInUser.IdToken;
        string accessToken = signInUser.AuthCode;

        Firebase.Auth.Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(idToken, accessToken);

        auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
        {
            if(authTask.IsCanceled)
            {
                Debug.LogError("Login Failled");
                return;
            }

            if(authTask.IsFaulted)
            {
                Debug.LogError("Error: " + authTask.Exception);
                return;
            }

            Firebase.Auth.FirebaseUser user = authTask.Result;
            Debug.Log("Login success with user: " + user.DisplayName);

            SceneManager.LoadScene("Home 1");
        });
    }
}
