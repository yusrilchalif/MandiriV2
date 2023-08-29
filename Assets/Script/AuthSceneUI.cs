using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AuthSceneUI : MonoBehaviour
{
    [Header("Login UI")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI errorText;

    [Header("Register UI")]
    public TextMeshProUGUI errorRegText;
    public TMP_InputField nameRegInput;
    public TMP_InputField phoneRegInput;
    public TMP_InputField usernameRegInput;
    public TMP_InputField emailRegInput;
    public TMP_InputField passwordRegInput;
    public TMP_InputField passwordConfirmRegInput;

    [Header("UI")]
    public GameObject loginUI;
    public GameObject registerUI;

    
    private HashSet<string> validPrefixes;

    // Start is called before the first frame update
    void Start()
    {
        validPrefixes = new HashSet<string>()
        {
            "0831", "0832", "0833", "0838", "0895", "0896", "0897", "0898", "0899",
            "0817", "0818", "0819", "0859", "0878", "0877", "0814", "0815", "0816",
            "0855", "0856", "0857", "0858", "0812", "0813", "0852", "0853", "0821",
            "0823", "0822", "0851", "0811", "0881", "0882", "0883", "0884", "0885",
            "0886", "0887", "0888", "0889"
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SignGoogle()
    {
       AuthController.Instance.OnSignIn();
    }

    public void Login() {

        if (emailInput.text is null || passwordInput.text is null) {
            errorText.gameObject.SetActive(true);
            errorText.text = "Email atau password tidak boleh kosong!";
        }

        AuthController.Instance.Login(emailInput.text, passwordInput.text, ShowError);
    }

    public void Register() {
        if (string.IsNullOrEmpty(nameRegInput.text) || string.IsNullOrEmpty(phoneRegInput.text) || string.IsNullOrEmpty(usernameRegInput.text) || string.IsNullOrEmpty(usernameRegInput.text) || string.IsNullOrEmpty(passwordRegInput.text) || string.IsNullOrEmpty(passwordConfirmRegInput.text))
        {   
            errorRegText.gameObject.SetActive(true);
            errorRegText.text = "Semua field harus terisi!";
            return;
        }

        if(CheckSpecialChar(usernameRegInput.text) || CheckSpecialChar(nameRegInput.text)) {
            errorRegText.gameObject.SetActive(true);
            errorRegText.text = "Nama / Username mengandung simbol!";
            return;
        }

        if(passwordConfirmRegInput.text != passwordConfirmRegInput.text)
        {
            errorRegText.gameObject.SetActive(true);
            errorRegText.text = "Password tidak sama!";
            return;
        }

        if (phoneRegInput.text.Length < 10 || phoneRegInput.text.Length > 13)
        {
            errorRegText.gameObject.SetActive(true);
            errorRegText.text = "Nomor HP harus terdiri dari 10-13 karakter!";
            return;
        }

        if (!phoneRegInput.text.StartsWith("0"))
        {
            errorRegText.gameObject.SetActive(true);
            errorRegText.text = "Nomor HP tidak dimulai dengan angka '0'.";
            return;
        }

        string prefix = phoneRegInput.text.Substring(0, 4);
        if (!validPrefixes.Contains(prefix))
        {
            errorRegText.gameObject.SetActive(true);
            errorRegText.text = "Awalan nomor HP tidak valid!";
            return;
        }

        if (!emailRegInput.text.Contains("@") || emailRegInput.text.Length < 5 || !emailRegInput.text.Contains(".") || emailRegInput.text.Split('@')[1].Length < 3)
        {
            errorRegText.gameObject.SetActive(true);
            errorRegText.text = "Email tidak valid!";
            return;
        }

        AuthController.Instance.RegisterNewUser(emailRegInput.text, usernameRegInput.text, nameRegInput.text, passwordRegInput.text, 
            (e) => { errorRegText.gameObject.SetActive(true); errorRegText.text = "Gagal mendaftarkan user! " + e; },
            () => { registerUI.SetActive(false); loginUI.SetActive(true); }
        );
    }

    public void ShowPassword() {
        if (passwordInput.contentType == TMP_InputField.ContentType.Password)
        {
            passwordInput.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            passwordInput.contentType = TMP_InputField.ContentType.Password;
        }
        passwordInput.ForceLabelUpdate();
    }

    public void ShowError(string errorMsg) {
        errorText.gameObject.SetActive(true);
        errorText.text = errorMsg;
    }

    public static bool CheckSpecialChar(string input) {
        string[] chars = new string[] { ",", ".", "/", "!", "@", "#", "$", "%", "^", "&", "*", "'", "\"", ";", "_", "(", ")", ":", "|", "[", "]" };

        for (int i = 0; i < chars.Length; i++)
        {
            if (input.Contains(chars[i]))
                return true;
        }
        return false;
    }

}
