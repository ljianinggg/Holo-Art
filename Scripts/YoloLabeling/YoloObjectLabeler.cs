using System;
// using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealityCollective.ServiceFramework.Services;
using UnityEngine;
using YoloHolo.Services;
using YoloHolo.Utilities;
using System.IO;
using CJM.BBox2DToolkit;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;
using Microsoft.MixedReality.OpenXR;
using Microsoft.MixedReality.EyeTracking;

namespace YoloHolo.YoloLabeling
{
    public class YoloObjectLabeler : MonoBehaviour
    {
        [SerializeField]
        private GameObject labelObject;

        public  LineRenderer lineRenderer;

        [SerializeField]
        private int cameraFPS = 15;

        [SerializeField]
        private Vector2Int requestedCameraSize = new(1054, 846);

        private Vector2Int actualCameraSize;

        [SerializeField]
        private Vector2Int yoloImageSize = new(640, 640);

        [SerializeField]
        private float virtualProjectionPlaneWidth = 1.356f;

        [SerializeField]
        private float minIdenticalLabelDistance = 0.3f;

        [SerializeField]
        private float labelNotSeenTimeOut = 5f;

        [SerializeField]
        private Renderer debugRenderer;
        
        [SerializeField]
        private bool useEyeGaze = false; 

        [SerializeField, Tooltip("Visualizes detected object bounding boxes")]
        
        private WebCamTexture webCamTexture;

        private IYoloProcessor yoloProcessor;

        private readonly List<YoloGameObject> yoloGameObjects = new();

        public ChatVision chatVisionScript;

        private Texture2D  texture;

        public GameObject buttonToActivate;

        private int DetectionCount = 0; 

        private int requiredStableDetections = 5;       

        private YoloItem currentYoloItem;

        public ProcessImage processImage;

        private bool buttonClicked = true;

        public TMP_Text textComponent;

        public TMP_Text textComponent1;
        public TMP_Text textComponent2;

        public TMP_Text textComponent3;

        private bool continueRecognition = true;

        private EyeTracker EyeTracker;

        private float buttonActiveTime = 0.5f; 
        private float timer = 0; 
        private bool isButtonActive = false; 

        public VideoPlayer videoPlayer;
        public RawImage display;

        public GameObject plane;

        public Renderer targetRenderer; 
        public GameObject paintingObject; 

        public void StopRecognition()
        {
            continueRecognition = false;
        }


        public void Start()
        {
            // Get all available WebCam devices
            WebCamDevice[] devices = WebCamTexture.devices;

            // Log each device name
            foreach (var device in devices)
            {
                Debug.Log("Webcam available: " + device.name);
            }
            //Hololens
            string cameraName = devices[0].name;
            // Debug.Log("cameraName: " + cameraName);

            yoloProcessor = ServiceManager.Instance.GetService<IYoloProcessor>();
            webCamTexture = new WebCamTexture(cameraName, requestedCameraSize.x, requestedCameraSize.y, cameraFPS);


            if (debugRenderer != null)
            {
                debugRenderer.material.mainTexture = webCamTexture;
            }
            else
            {
                Debug.LogError("Debug Renderer not set.");
            }
            buttonToActivate.SetActive(false);

            eyeTracker = GetComponent<EyeTracker>();  
            if (eyeTracker == null)
            {
                Debug.LogError("EyeTracker component not found!");
            }

            if (eyeGazeToggle != null)
            {
                eyeGazeToggle.isOn = useEyeGaze;
                eyeGazeToggle.onValueChanged.AddListener(OnEyeGazeToggleChanged);
            }
            else
            {
                Debug.LogWarning("Eye Gaze Toggle not assigned!");
            }

            webCamTexture.Play();

            Debug.Log($"WebCamTexture is playing: {webCamTexture.isPlaying}, Resolution: {webCamTexture.width}x{webCamTexture.height}");
            
            _ = StartRecognizingAsync();
        }

        private async Task StartRecognizingAsync()
        {

            await Task.Delay(1000);

            textComponent1.text="starting";

            actualCameraSize = new Vector2Int(webCamTexture.width, webCamTexture.height);
            Debug.Log($"actualCameraSize: {actualCameraSize}");
            var resizedTexture = new RenderTexture(webCamTexture.width, webCamTexture.height, 24);
            var renderTexture = new RenderTexture(yoloImageSize.x, yoloImageSize.y, 24); 
            if (debugRenderer != null && debugRenderer.gameObject.activeInHierarchy)
            {
                debugRenderer.material.mainTexture = renderTexture;
            }
            textComponent.text = $"actualCameraSize: {actualCameraSize}";

            while (continueRecognition)
            {
                
                // Debug.Log($"Yolo Image Size: {yoloImageSize.x}x{yoloImageSize.y}");
                // Debug.Log($"RenderTexture size after creation: {renderTexture.width}x{renderTexture.height}");
                var cameraTransform = Camera.main.CopyCameraTransForm();
                Graphics.Blit(webCamTexture, renderTexture);
                Graphics.Blit(webCamTexture, resizedTexture);
                await Task.Delay(32);

                texture = renderTexture.ToTexture2D();
                var foundObjects = await yoloProcessor.RecognizeObjects(texture);
                
                ShowRecognitions(foundObjects, cameraTransform);
                Destroy(texture);
                Destroy(cameraTransform.gameObject);
            }
        }


        public void OnButtonClick()
        {
            // Debug.Log("OnButtonClick");    
            // textComponent.text = "OnButtonClick";
            buttonClicked = true;                    
            plane.SetActive(true);
            if (currentYoloItem != null)
            {
                Rect cropRect = new Rect(
                    currentYoloItem.TopLeft.x, 
                    currentYoloItem.TopLeft.y, 
                    currentYoloItem.BottomRight.x - currentYoloItem.TopLeft.x, 
                    currentYoloItem.BottomRight.y - currentYoloItem.TopLeft.y
                );
                textComponent.text = $"Cropping with Rect: x={cropRect.x}, y={cropRect.y}, width={cropRect.width}, height={cropRect.height}" ;
                // Debug.Log($"Cropping with Rect: x={cropRect.x}, y={cropRect.y}, width={cropRect.width}, height={cropRect.height}");

                float originalWidth = 895f;
                float originalHeight = 504f;
                float compressedWidth = 640f;
                float compressedHeight = 640f;

                float widthRatio = originalWidth / compressedWidth;
                float heightRatio = originalHeight / compressedHeight;

                Texture2D croppedAndRestoredTexture = CropAndRestore(cropRect, texture, currentYoloItem.TopLeft, currentYoloItem.Size, widthRatio, heightRatio);

                var base64String = Texture2DToBase64(croppedAndRestoredTexture);

                chatVisionScript.SendToGPT(base64String); 
                processImage.UploadImage(croppedAndRestoredTexture);
                textComponent.text = $"sent" ;

                plane.SetActive(true);
            }
        }

        Texture2D CropTexture(Texture2D originalTexture, Rect cropRect)
        {
            Texture2D croppedTexture = new Texture2D((int)cropRect.width, (int)cropRect.height);
            Color[] pixels = originalTexture.GetPixels((int)cropRect.x, (int)cropRect.y, (int)cropRect.width, (int)cropRect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
            return croppedTexture;
        }
        Texture2D CropAndRestore(Rect cropRect, Texture2D source, Vector2 topLeft, Vector2 size, float widthRatio, float heightRatio)
        {

            int width = Mathf.FloorToInt(size.x);
            int height = Mathf.FloorToInt(size.y);

            Texture2D croppedTexture = new Texture2D((int)cropRect.width, (int)cropRect.height);
            Color[] pixels = source.GetPixels((int)cropRect.x, (int)cropRect.y, (int)cropRect.width, (int)cropRect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();

            int restoredWidth = Mathf.FloorToInt(width * widthRatio);
            int restoredHeight = Mathf.FloorToInt(height * heightRatio);
            Texture2D restoredTexture = new Texture2D(restoredWidth, restoredHeight);

            RenderTexture rt = RenderTexture.GetTemporary(restoredWidth, restoredHeight);
            Graphics.Blit(croppedTexture, rt);
            RenderTexture.active = rt;

            restoredTexture.ReadPixels(new Rect(0, 0, restoredWidth, restoredHeight), 0, 0);
            restoredTexture.Apply();

            RenderTexture.ReleaseTemporary(rt);
            return restoredTexture;
        }

        private void ShowRecognitions(List<YoloItem> recognitions, Transform cameraTransform)
        {
            foreach (var recognition in recognitions)
            {
                currentYoloItem = recognition;
                // Debug.Log("ShowRecognitions");
                // Debug.Log($"Found:  {recognition.MostLikelyObject} : {recognition.Confidence}");
                // Debug.Log($"obj.TopLeft.x: {recognition.TopLeft.x} obj.TopLeft.y : {recognition.TopLeft.y} index :  obj.Size.x : {recognition.Size.x} obj.Size.y : {recognition.Size.y}");
                textComponent2.text =$"obj.TopLeft.x: {recognition.TopLeft.x} obj.TopLeft.y : {recognition.TopLeft.y} obj.Size.x : {recognition.Size.x} obj.Size.y : {recognition.Size.y}";
                textComponent3.text=$" {recognition.MostLikelyObject} : {recognition.Confidence}";

                // use 0.8f as threshold
                if (recognition.Confidence > 0.8f)
                {
                    DetectionCount++;
                    if (DetectionCount >= requiredStableDetections)
                    {
                        if (!isButtonActive)
                        {
                            if (useEyeGaze)
                            {
                                Rect detectedRect = new Rect(
                                    recognition.TopLeft.x, 
                                    recognition.TopLeft.y, 
                                    recognition.BottomRight.x - recognition.TopLeft.x, 
                                    recognition.BottomRight.y - recognition.TopLeft.y
                                );

                                if (EyeTracker.CheckifGazed(detectedRect))
                                {
                                    buttonToActivate.SetActive(true);
                                    isButtonActive = true;
                                    timer = buttonActiveTime;
                                }
                            }
                            else
                            {
                                buttonToActivate.SetActive(true);
                                isButtonActive = true;
                                timer = buttonActiveTime;
                            }

                        }
                    }
                }
                else
                {
                    DetectionCount = 0;
                }
                if (isButtonActive)
                {
                    timer -= Time.deltaTime;
                    Debug.Log(timer);

                    if (timer <= 0)
                    {
                        buttonToActivate.SetActive(false);
                        isButtonActive = false;
                    }
                }

                
                var newObj = new YoloGameObject(recognition, cameraTransform,
                    actualCameraSize, yoloImageSize, virtualProjectionPlaneWidth);
                if (newObj.PositionInSpace != null && !HasBeenSeenBefore(newObj))
                {
                    // textComponent3.text=$"topLeft3D: {newObj.topLeft3D} bottomRight3D: {newObj.bottomRight3D} ";

                    yoloGameObjects.Add(newObj);
                    newObj.DisplayObject = Instantiate(labelObject,
                        newObj.PositionInSpace.Value, Quaternion.identity);
                    newObj.DisplayObject.transform.parent = transform;
                    var labelController = newObj.DisplayObject.GetComponent<ObjectLabelController>();
                    labelController.SetText(newObj.Name);
                }
            }

            for (var i = yoloGameObjects.Count - 1; i >= 0; i--)
            {
                if (Time.time - yoloGameObjects[i].TimeLastSeen > labelNotSeenTimeOut)
                {
                    Destroy(yoloGameObjects[i].DisplayObject);
                    yoloGameObjects.RemoveAt(i);
                }
            }
        }

        private bool HasBeenSeenBefore(YoloGameObject obj)
        {
            var seenBefore = yoloGameObjects.FirstOrDefault(
                ylo => ylo.Name == obj.Name &&
                Vector3.Distance(obj.PositionInSpace.Value,
                    ylo.PositionInSpace.Value) < minIdenticalLabelDistance);
            if (seenBefore != null)
            {
                seenBefore.TimeLastSeen = Time.time;
            }
            return seenBefore != null;
        }

        //Texture2DToBase
        private String Texture2DToBase64(Texture2D t2d)
        {
            byte[] bytesArr = t2d.EncodeToJPG();
            string strbaser64 = Convert.ToBase64String(bytesArr);
            return strbaser64;
        }

        public void SetPaintingPosition(Vector3 worldPosition)
        {
            paintingObject.transform.position = worldPosition;

        }

    }
}
