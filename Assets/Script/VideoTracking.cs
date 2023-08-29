using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;


[RequireComponent(typeof(ARTrackedImageManager))]
public class VideoTracking : MonoBehaviour
{
    [SerializeField] private List<GameObject> prefabList;

    private ARTrackedImageManager arTracked;
    private GameObject spawnedObject;

    private void Awake()
    {
        arTracked = GetComponent<ARTrackedImageManager>();

    }

    private void OnEnable()
    {
        arTracked.trackedImagesChanged += ImageChanged;
    }

    private void OnDisable()
    {
        arTracked.trackedImagesChanged -= ImageChanged;
    }

    private void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {

        }
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            Destroy(spawnedObject);
        }

    }
}
