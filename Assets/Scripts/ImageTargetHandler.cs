using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageTargetHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject infoPrefab;

    private ARTrackedImageManager trackedImageManager;
    private Dictionary<string, GameObject> spawnedObjects = new Dictionary<string, GameObject>();

    // Add the exact names of your reference images here, matched to their info text
    private Dictionary<string, string> imageInfo = new Dictionary<string, string>()
    {
        { "Helmetimage", "SAFETY HELMET\n\nProtects against falling objects and head impact.\n\nAlways wear before entering the mine site." },
        { "MaskImage", "GAS MASK / RESPIRATOR\n\nProtects against toxic gases and dust inhalation.\n\nCheck seal and filter before each use." },
        { "DangerSignImage", "DANGER: TOXIC GAS\n\nIndicates presence of hazardous gas in this zone.\n\nDo not enter without proper gas detection equipment." },
        { "Testing", "TEST IMAGE\n\nThis is a placeholder test marker." }
    };

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnChanged);
    }

    void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnChanged);
    }

    void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            SpawnOrUpdate(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            SpawnOrUpdate(trackedImage);
        }

        foreach (var pair in eventArgs.removed)
        {
            var trackedImage = pair.Value;
            if (spawnedObjects.ContainsKey(trackedImage.referenceImage.name))
            {
                Destroy(spawnedObjects[trackedImage.referenceImage.name]);
                spawnedObjects.Remove(trackedImage.referenceImage.name);
            }
        }
    }

    void SpawnOrUpdate(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;

        if (!spawnedObjects.ContainsKey(imageName))
        {
            GameObject newObject = Instantiate(infoPrefab, trackedImage.transform.position, trackedImage.transform.rotation);

            // Set the text based on which image was recognized
            TextMeshProUGUI textComponent = newObject.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                if (imageInfo.ContainsKey(imageName))
                {
                    textComponent.text = imageInfo[imageName];
                }
                else
                {
                    textComponent.text = imageName;
                }
            }

            spawnedObjects[imageName] = newObject;
        }

        GameObject obj = spawnedObjects[imageName];

        // Offset slightly above the image so it doesn't overlap
        Vector3 offsetPosition = trackedImage.transform.position + trackedImage.transform.up * 0.1f;
        obj.transform.position = offsetPosition;
        if (Camera.main != null)
        {
            obj.transform.rotation = Quaternion.LookRotation(obj.transform.position - Camera.main.transform.position);
        }

        obj.SetActive(trackedImage.trackingState == TrackingState.Tracking);
    }
}