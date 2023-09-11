using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RedeemSceneManager : MonoBehaviour
{
    public static RedeemSceneManager Instance { get; private set; }
    
    public TextMeshProUGUI errorText;
    
    public void SetErrorTextMessage(string message) {
        errorText.text = message;
    }

}
