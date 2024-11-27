using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using UnityEngine.Events;

public class LLMManager : MonoBehaviour
{
    public OnResponseEvent onReponse;

    [System.Serializable]
    public class OnResponseEvent : UnityEvent<string> { }

    private OpenAIApi openAI = new OpenAIApi("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"); 
    private List<ChatMessage> messages = new List<ChatMessage>();

    //Game Objects for PrintState:
    public GameObject character;
    public GameObject[] npcs;


    public async void AskChatGPT(string newText)
    {
        PrintState();
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = newText;
        newMessage.Role = "user";

        messages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-4o-mini";

        var response = await openAI.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse =  response.Choices[0].Message;
            messages.Add(chatResponse);

            onReponse.Invoke(chatResponse.Content);
        }
    }

    public void PrintState()
    {
        Debug.Log("GAME STATE:");
        Debug.Log("Player coordinates: (" + character.transform.position.x + ", " + character.transform.position.y + ")");
        int i = 0;
        foreach (GameObject npc in npcs)
        {
            i++;
            Debug.Log("NPC " + i + " coordinates: (" + npc.transform.position.x + ", " + npc.transform.position.y + ")");
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        PrintState();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
