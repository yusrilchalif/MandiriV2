using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;

public class RegisterManager : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField phoneNumberInput;
    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public Button registerButton;
    public TextMeshProUGUI errorText;

    private FirebaseAuth auth;
    private HashSet<string> validPrefixes;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Firebase sudah terinisialisasi
                auth = FirebaseAuth.DefaultInstance;
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });

        Task.WaitAll(FirebaseApp.CheckAndFixDependenciesAsync());

        validPrefixes = new HashSet<string>()
        {
            "0831", "0832", "0833", "0838", "0895", "0896", "0897", "0898", "0899",
            "0817", "0818", "0819", "0859", "0878", "0877", "0814", "0815", "0816",
            "0855", "0856", "0857", "0858", "0812", "0813", "0852", "0853", "0821",
            "0823", "0822", "0851", "0811", "0881", "0882", "0883", "0884", "0885",
            "0886", "0887", "0888", "0889"
        };

        registerButton.onClick.AddListener(Register);
    }

    public void Register()
    {
        string name = nameInput.text;
        string phoneNumber = phoneNumberInput.text;
        string username = usernameInput.text;
        string email = emailInput.text;
        string password = passwordInput.text;
        string confirm = confirmPasswordInput.text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirm))
        {
            errorText.text = "All fields must be filled.";
            return;
        }

        if(password != confirm)
        {
            errorText.text = "Password not same";
            return;
        }

        if (phoneNumber.Length < 10 || phoneNumber.Length > 13)
        {
            errorText.text = "Phone number must be between 10 and 13 digits.";
            return;
        }

        if (!phoneNumber.StartsWith("0"))
        {
            errorText.text = "Phone number must start with '0'.";
            return;
        }

        string prefix = phoneNumber.Substring(0, 4);
        if (!validPrefixes.Contains(prefix))
        {
            errorText.text = "Invalid phone number prefix.";
            return;
        }

        if (!email.Contains("@") || email.Length < 5 || !email.Contains(".") || email.Split('@')[1].Length < 3)
        {
            errorText.text = "Invalid email format.";
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }
            errorText.text = "Success Register";
            Debug.Log("User registration successful.");
        });
    }
}