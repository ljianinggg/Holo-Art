using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
// using NUnit.Framework;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using OpenAI_API.Models;
using OpenAI_API.Moderation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static OpenAI_API.Chat.ChatMessage;
using System.Collections;
using OpenAI_API; 
using UnityEngine;
using System.Collections;
using UnityEngine;
using OpenAI_API.Chat;
using Mono.Cecil.Cil; 
using System.Collections;
using UnityEngine;
using OpenAI_API; 
using System.IO;
using TMPro;
public class ChatVision : MonoBehaviour
{
    private OpenAIAPI api;

    public TMP_Text responseText;

    public TMP_Text textComponent;

    public float displayDuration = 10.0f; // the time to display the text   
    void Start()
    {
        // Initialize your OpenAI API
        api = new OpenAIAPI("");
        // Start the coroutine to send a message to OpenAI's ChatGPT
        StartCoroutine(SendImageToChatGPT(api));
    }

    public void SendToGPT(string base64Image)
    {
        StartCoroutine(SendImageToChatGPT(base64Image));
    }

    // This coroutine sends a message to the OpenAI API and waits for the response
    private IEnumerator SendImageToChatGPT(string base64Image)
    {
        // Construct your message with the base64 image
        string message = "What is this painting? please provide brief description";
        // Create an ImageInput object from the base64 string
        ImageInput imageInput = new ImageInput("data:image/jpeg;base64," + base64Image, ImageInput.DetailAuto);
        // Start the API call using the OpenAIAPI instance
        var task = api.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.GPT4_Vision,
                Temperature = 0.1,
                MaxTokens = 100,
                Messages = new ChatMessage[] {
					new ChatMessage(ChatMessageRole.System, "You are a helpful artist"),
					new ChatMessage(ChatMessageRole.User, message,imageInput)
                }

            });


        while (!task.IsCompleted)
        {
            yield return null; 
        }

        if (task.Status == TaskStatus.RanToCompletion)
        {
            ChatResult result = task.Result;
            // var temp = result.ToString();
            UpdateTextWithTimeout(result.ToString());
        }
        else
        {
            // Handle error
            Debug.LogError("Chat completion failed");
        }
    }
    public void UpdateTextWithTimeout(string text)
    {
        responseText.text = text;
        StartCoroutine(HideTextAfterTime(displayDuration));
    }

        private IEnumerator HideTextAfterTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        responseText.text = "timeout";
    }

}
