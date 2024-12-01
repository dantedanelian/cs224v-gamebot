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
        string gameState = GetGameState();
        string messageWithState = $"{newText}\n\nCurrent Game State:\n{gameState}";

        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = messageWithState;
        newMessage.Role = "user";

        messages.Add(newMessage);

        newMessage.Content = "Succinctly answer the user query using the ingested game state in a human-readable format. Do not output raw coordinates or anything that would not be helpful to the user directly, since there is a limited character count. Also do not give away solutions. If an NPC is not in frame, do not reveal its location directly.";
        newMessage.Role = "system";

        messages.Add(newMessage);

        newMessage.Content = "If the user query is vague, do not volunteer information. Instead, ask leading questions about what the user is asking. Still be friendly and acknowledge the response.";
        newMessage.Role = "system";

        messages.Add(newMessage);

        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-4o-mini";

        var response = await openAI.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);

            onReponse.Invoke(chatResponse.Content);
        }
    }

    private string GetGameState()
    {
        string state = "Player coordinates: (" + character.transform.position.x + ", " + character.transform.position.y + ")\n";
        int i = 0;
        foreach (GameObject npc in npcs)
        {
            i++;
            state += "NPC " + i + " coordinates: (" + npc.transform.position.x + ", " + npc.transform.position.y + ")\n";
        }
        return state;
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
