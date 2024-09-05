using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class ProcessImage : MonoBehaviour
{
    //the url of the server
    private string uploadURL = "";
    public Texture2D image;

    public VideoPlayer videoPlayer;


    public void Start(){
        UploadImage(image);        
    } 
    public void UploadImage(Texture2D image)
    {
        StartCoroutine(UploadImageCoroutine(image));
    }


    public Texture2D DeCompress(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }

    IEnumerator UploadImageCoroutine(Texture2D image)
        {

            Texture2D decopmpresseimage = DeCompress(image);

            byte[] imageData = decopmpresseimage.EncodeToPNG();

            WWWForm form = new WWWForm();
            form.AddBinaryData("prompt_image", imageData, "image.jpg", "image/jpg");
            form.AddField("prompt", "she is winking"); // Add the prompt text to the form

            using (UnityWebRequest www = UnityWebRequest.Post(uploadURL, form))
            {
                www.timeout = 20 * 60; 
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Error: " + www.error);
                }
                else
                {
                    Debug.Log("Image uploaded successfully!");
                    // Get the downloaded data (which is the MP4 file)
                    byte[] results = www.downloadHandler.data;
                    // Write the data to a file
                    string filePath = Path.Combine(Application.persistentDataPath, "downloadedVideo.mp4");
                    File.WriteAllBytes(filePath, results);
                    videoPlayer.url = filePath;
                    videoPlayer.Play();
                }
            }
        }



}
