using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScreenshotAndShare : MonoBehaviour
{
    public GameObject UIParent, UIScreenshot;

    // Start is called before the first frame update
    void Start()
    {
        UIScreenshot.SetActive(false);
        UIParent.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ScreenShot()
    {
        UIParent.SetActive(false);
        UIScreenshot.SetActive(true);
        StartCoroutine("TakeScreenshot");
    }

    private IEnumerator TakeScreenshot()
    {
        yield return new WaitForEndOfFrame();
        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        string name = "Screenshot MandiriAR " + System.DateTime.Now.ToString("yyyy-MM-dd") + ".png";
        //Mobile
        NativeGallery.SaveImageToGallery(ss, "Mandiri AR", name);

        string filePath = Path.Combine(Application.temporaryCachePath, name);
        File.WriteAllBytes(filePath, ss.EncodeToPNG());
        // To avoid memory leaks
        Destroy(ss);
        new NativeShare().AddFile(filePath)
            .SetSubject("Mandiri AR Apps").SetText("Berikut adalah score yang saya dapatkan di Mandiri AR Apps").SetUrl("https://bankmandiri.co.id")
            .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
            .Share();
        // Share on WhatsApp only, if installed (Android only)
        //if( NativeShare.TargetExists( "com.whatsapp" ) )
        //	new NativeShare().AddFile( filePath ).AddTarget( "com.whatsapp" ).Share();

        
        Destroy(ss);
        UIScreenshot.SetActive(false);
        UIParent.SetActive(false);

        //ShareMedsos();
        Invoke("BackToStart", 3);
    }

    void BackToStart()
    {
        UIScreenshot.SetActive(false);
        UIParent.SetActive(true);
    }
}
