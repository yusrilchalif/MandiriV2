using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalSceneController : MonoBehaviour
{
    public static GlobalSceneController Instance { get; private set; }

    void Awake() {
        if (Instance != this && Instance != null)
            Destroy(this);
        else
            Instance = this;
        
        DontDestroyOnLoad(this);
    }

    public void ChangeScene(int sceneIndex) {
        SceneManager.LoadScene(sceneIndex);
    }

    public void ChangeScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

}
