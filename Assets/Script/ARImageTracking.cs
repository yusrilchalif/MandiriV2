using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ARImageTracking : MonoBehaviour
{
    [SerializeField ] private List<GameObject> prefabList;
    public XRReferenceImageLibrary referenceLibrary;

    private ARTrackedImageManager arTracked;
    private ARSession arSession;

    private GameObject spawnedObject;


    private void Awake()
    {
        arTracked = GetComponent<ARTrackedImageManager>(); 
        arSession = FindObjectOfType<ARSession>();

        arSession.Reset();
        arSession.enabled = true;
    }

    private void Start()
    {
        spawnedObject = new GameObject();
        arTracked.referenceLibrary = referenceLibrary;
    }

    private void OnEnable()
    {
        arTracked.trackedImagesChanged += ImageChanged;
    }

    private void OnDisable()
    {
        arTracked.trackedImagesChanged -= ImageChanged;
    }

    private void OnDestroy()
    {
        arTracked.trackedImagesChanged -= ImageChanged;
    }

    private void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            InitImage(trackedImage);
        }
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateImage(trackedImage);
        }
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            Destroy(spawnedObject);
        }
        
    }

    private void InitImage(ARTrackedImage trackedImage)
    {
        int randomAR = Random.Range(0, prefabList.Count);
        spawnedObject = Instantiate(prefabList[randomAR], trackedImage.transform);
        spawnedObject.transform.Rotate(new Vector3(90, 0, 0));
        spawnedObject.SetActive(true);
    }

    private void UpdateImage(ARTrackedImage trackedImage)
    {
        if (spawnedObject == null)
            InitImage(trackedImage);

        spawnedObject.SetActive(true);
        spawnedObject.transform.position = trackedImage.transform.position;
    }

    public void ResetSession() {
        LoaderUtility.Deinitialize();
        LoaderUtility.Initialize();
    }
}
