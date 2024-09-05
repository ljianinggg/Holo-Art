using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.EyeTracking;
using System;
using TMPro;
using System.IO;
public class EyeTracker : MonoBehaviour
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
    private Dictionary<GameObject, float> gazeTimeDictionary = new Dictionary<GameObject, float>();
    private DateTime lastUpdateTimestamp;
    public TMP_Text textComponent;
    public TMP_Text textComponent2;
    public TMP_Text textComponent3;
    public GameObject objectOfInterest;
    private ExtendedEyeGazeDataProvider.GazeReading gazeReading;
    private string csvFilePath;
    void Start()
    {
        lastUpdateTimestamp = DateTime.Now;
        csvFilePath = Path.Combine(Application.persistentDataPath, "GazeData.csv");
        InitializeCSV();    
    }

    void Update() {
        timestamp = DateTime.Now;

        DateTime currentTimestamp = DateTime.Now;
        
        var gazeReading = extendedEyeGazeDataProvider.GetWorldSpaceGazeReading(ExtendedEyeGazeDataProvider.GazeType.Combined, currentTimestamp);
        // textComponent2.text = $"gazeReading.IsValid)={gazeReading.IsValid}" ;
        Ray gazeRay = new Ray(gazeReading.EyePosition, gazeReading.GazeDirection);
        // textComponent2.text = $"gazeReading.EyePosition={gazeReading.EyePosition},  gazeReading.GazeDirection={gazeReading.GazeDirection}" ;
        // textComponent3.text = $"objectOfInterest Position={objectOfInterest.transform.position}" ;
        WriteGazeDataToCSV(gazeReading, timestamp);
        // Debug.Log(gazeReading.EyePosition+ "gazeReading.EyePositio.");
        if (gazeReading.IsValid)
        {
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

                    // Check if gaze time exceeds 10 seconds
                    if (gazeTimeDictionary[hitObject] >= 10.0f)
                    {
                        textComponent.text = $"has been gazed" ;
                        Debug.Log(hitObject.name + " has been gazed at for 10 seconds.");
                        // Perform actions for the object gazed at for 10 seconds
                        // For example, you could trigger an event here

                        // Reset the gaze time if necessary
                        gazeTimeDictionary[hitObject] = 0;
                    }
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

    public bool CheckifGazed(Rect detectedImageRect)
    {
        DateTime currentTimestamp = DateTime.Now;
        var gazeReading = extendedEyeGazeDataProvider.GetWorldSpaceGazeReading(ExtendedEyeGazeDataProvider.GazeType.Combined, currentTimestamp);
        
        if (gazeReading.IsValid)
        {
            Ray gazeRay = new Ray(gazeReading.EyePosition, gazeReading.GazeDirection);
            RaycastHit hitInfo;
            
            if (Physics.Raycast(gazeRay, out hitInfo))
            {
                Vector2 hitPoint2D = Camera.main.WorldToScreenPoint(hitInfo.point);
                
                if (detectedImageRect.Contains(hitPoint2D))
                {
                    if (!gazeTimeDictionary.ContainsKey(objectOfInterest))
                    {
                        gazeTimeDictionary[objectOfInterest] = 0;
                    }

                    gazeTimeDictionary[objectOfInterest] += Time.deltaTime;

                    // if the object is gazed at for 10 seconds, return true 
                    if (gazeTimeDictionary[objectOfInterest] >= 10.0f)
                    {
                        gazeTimeDictionary[objectOfInterest] = 0;
                        return true;
                    }
                }
                else
                {
                    gazeTimeDictionary[objectOfInterest] = 0;
                }
            }
        }

        return false;
    }

}
