// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using YoloHolo.Services;
using YoloHolo.Utilities;
using System;
using UnityEngine;
using TMPro;
using System.IO;

using System.Collections.Generic;
public class EETDataProviderTest : MonoBehaviour
{
    [SerializeField]
    private GameObject LeftGazeObject;
    [SerializeField]
    private GameObject RightGazeObject;
    [SerializeField]
    private GameObject CombinedGazeObject;
    [SerializeField]
    private GameObject CameraRelativeCombinedGazeObject;
    [SerializeField]
    private ExtendedEyeGazeDataProvider extendedEyeGazeDataProvider;

    private DateTime timestamp;
    private ExtendedEyeGazeDataProvider.GazeReading gazeReading;
    public TMP_Text textComponent;
    public TMP_Text textComponent2;
    public TMP_Text textComponent3;
    public GameObject objectOfInterest;
    private string csvFilePath;

    
    private Dictionary<GameObject, float> gazeTimeDictionary = new Dictionary<GameObject, float>();
    void Start()
    {
        csvFilePath = Path.Combine(Application.persistentDataPath, "GazeData.csv");
        InitializeCSV();
        CheckifGazed();
    }

    public void CheckifGazed()
    {
        timestamp = DateTime.Now;

        // positioning for combined gaze object
        gazeReading = extendedEyeGazeDataProvider.GetWorldSpaceGazeReading(ExtendedEyeGazeDataProvider.GazeType.Combined, timestamp);
        textComponent2.text = $"gazeReading.IsValid)={gazeReading.IsValid}" ;
        Ray gazeRay = new Ray(gazeReading.EyePosition, gazeReading.GazeDirection);
          
        // float deltaTime = (float)(timestamp - lastUpdateTimestamp).TotalSeconds;
        
        if (gazeReading.IsValid)
        {
            WriteGazeDataToCSV(gazeReading, timestamp);
            textComponent2.text = $"gazeReading.EyePosition={gazeReading.EyePosition},  gazeReading.GazeDirection={gazeReading.GazeDirection}" ;
            textComponent3.text = $"objectOfInterest Position={objectOfInterest.transform.position}" ;
            if (Physics.Raycast(gazeRay, out RaycastHit hitInfo))
            {
                GameObject hitObject = hitInfo.collider.gameObject;
                if (hitObject == objectOfInterest){
                    if (!gazeTimeDictionary.ContainsKey(hitObject))
                    {
                        gazeTimeDictionary[hitObject] = 0;
                    }

                    // Update the gaze time for the hit object
                    gazeTimeDictionary[hitObject] += Time.deltaTime;;
                    textComponent.text = $"has been gazed for :{gazeTimeDictionary[hitObject] }" ;
                    // Check if gaze time exceeds 10 seconds
                    if (gazeTimeDictionary[hitObject] >= 10.0f)
                    {
                        textComponent.text = $"has been gazed" ;
                        Debug.Log(hitObject.name + " has been gazed at for 10 seconds.");
                        // Perform actions for the object gazed at for 10 seconds
                        // For example, you could trigger an event here
                        // yoloObjectLabeler.OnButtonClick();

                        // Reset the gaze time if necessary
                        gazeTimeDictionary[hitObject] = 0;
                    };
                }
            }
        }
                // Reset gaze time for objects that are no longer being gazed at
        List<GameObject> keys = new List<GameObject>(gazeTimeDictionary.Keys);
        foreach (GameObject obj in keys)
        {
            if (!Physics.Raycast(new Ray(gazeReading.EyePosition, gazeReading.GazeDirection), out RaycastHit hitInfo2) || hitInfo2.collider.gameObject != obj)
            {
                gazeTimeDictionary[obj] = 0;
                textComponent.text = $"not gazed" ;
            }
        }



    }
    private void InitializeCSV()
    {
        if (!File.Exists(csvFilePath))
        {
            using (StreamWriter sw = File.CreateText(csvFilePath))
            {
                sw.WriteLine("Timestamp,EyePositionX,EyePositionY,EyePositionZ,GazeDirectionX,GazeDirectionY,GazeDirectionZ");
            }
        }
    }
    private void WriteGazeDataToCSV(ExtendedEyeGazeDataProvider.GazeReading reading, DateTime timeStamp)
    {
        string newData = $"{timeStamp:yyyy-MM-dd HH:mm:ss.fff},{reading.EyePosition.x},{reading.EyePosition.y},{reading.EyePosition.z},{reading.GazeDirection.x},{reading.GazeDirection.y},{reading.GazeDirection.z}";
        using (StreamWriter sw = File.AppendText(csvFilePath))
        {
            sw.WriteLine(newData);
        }
    }

}
