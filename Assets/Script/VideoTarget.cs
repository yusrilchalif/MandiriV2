using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class VideoTarget : MonoBehaviour
{
    [SerializeField] GameObject[] placeVideo;
    public XRReferenceImageLibrary referenceLibrary;

    private ARTrackedImageManager imageManager;
    private ARSession arSession;

    private Dictionary<string, GameObject> videoPlayer = new Dictionary<string, GameObject>();
    private GameObject videoPrefab;

    private void Awake()
    {
        imageManager = GetComponent<ARTrackedImageManager>();
        arSession = FindObjectOfType<ARSession>();

        arSession.Reset();
        arSession.enabled = true;
    }

    private void Start()
    {
        videoPrefab = new GameObject();
        imageManager.referenceLibrary = referenceLibrary;

        foreach (GameObject prefab in placeVideo)
        {
            videoPlayer.Add(prefab.name, prefab);
            Debug.Log("Video player added");
        }
    }

    private void OnEnable()
    {
        imageManager.trackedImagesChanged += ImageChanged;
    }

    private void OnDisable()
    {
        imageManager.trackedImagesChanged -= ImageChanged;
    }

    private void OnDestroy()
    {
        imageManager.trackedImagesChanged -= ImageChanged;
    }

    private void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            InitVideo(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateVideo(trackedImage);
        }
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            Destroy(videoPrefab);
            //videoPlayer[trackedImage.name].SetActive(false);
        }
    }

    private void InitVideo(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;
        Vector3 pos = trackedImage.transform.position;

        videoPrefab = Instantiate(videoPlayer[name], trackedImage.transform);
        videoPrefab.transform.position = pos;
        videoPrefab.transform.Rotate(new Vector3(90, 0, 0));
        videoPrefab.SetActive(true);
    }

    public void UpdateVideo(ARTrackedImage trackedImage) {
        if (videoPrefab == null)
            InitVideo(trackedImage);

        videoPrefab.SetActive(true);
        videoPrefab.transform.position = trackedImage.transform.position;
    }
    public void ResetSession()
    {
        LoaderUtility.Deinitialize();
        LoaderUtility.Initialize();
    }
}
