using System.Collections;
using System.Collections.Generic;
using RPGM.Gameplay;
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
    public GameObject[] items;


    public async void AskChatGPT(string newText)
    {
        string gameState = GetGameState();
        string messageWithState = $"{newText}\n\nCurrent Game State:\n{gameState}";

        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = messageWithState;
        newMessage.Role = "user";

        messages.Add(newMessage);

        newMessage.Content = "Succinctly answer the user query using the ingested game state in a human-readable format. You are not allowed to provide coordinates for any NPC or item. Instead, give directions in relative terms like north or south. If a player has not yet interacted with an NPC, do not reveal their dialogue and assume the player does not know what they will say.";
        newMessage.Role = "system";

        messages.Add(newMessage);

        newMessage.Content = "If the user query is vague, do not volunteer information. Instead, ask leading questions about what the user is asking. Still be friendly and acknowledge the response.";
        newMessage.Role = "system";

        messages.Add(newMessage);

        newMessage.Content = "Note that the player can only see 5 units in either direction along the x-axis and 3 in either direction on the y-axis. The entire accessible area is bounded by the coordinates (-8, 2) to (23, -14).";
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
        
        foreach (GameObject npc in npcs)
        {
            state += npc.name + " NPC coordinates: (" + npc.transform.position.x + ", " + npc.transform.position.y + ")\n";
            
            NPCController npcController = npc.GetComponent<NPCController>();
            if (npcController.hasInteracted)
            {
                state += " status: already interacted with" + npc.name + ".\n";
            } 
            else
            {
                state += " status: not yet interacted with" + npc.name + ".\n";
            }

            // Retrieve npc dialogue
            ConversationScript conversationScript = npc.GetComponent<ConversationScript>();
            if (conversationScript.items.Count > 0)
            {
                state += npc.name + " NPC dialogue: ";
                foreach (ConversationPiece item in conversationScript.items)
                {
                    state += item.text;
                }
                state += "\n";
            }
        }

        foreach (GameObject item in items)
        {
            state += item.name + " item coordinates: (" + item.transform.position.x + ", " + item.transform.position.y + ")";
            if (!item.activeInHierarchy)
            {
                state += " status: found.\n";
            } 
            else
            {
                state += " status: not yet found.\n";
            }
        }
        return state;
    }

    public void PrintState()
    {
        Debug.Log("GAME STATE:");
        Debug.Log("Player coordinates: (" + character.transform.position.x + ", " + character.transform.position.y + ")");
        foreach (GameObject npc in npcs)
        {
            Debug.Log(npc.name + " NPC coordinates: (" + npc.transform.position.x + ", " + npc.transform.position.y + ")");
    
            NPCController npcController = npc.GetComponent<NPCController>();
            if (npcController.hasInteracted)
            {
                Debug.Log(" status: already interacted with.\n");
            } 
            else
            {
                Debug.Log(" status: not yet interacted with.\n");
            }

            // Retrieve npc dialogue
            ConversationScript conversationScript = npc.GetComponent<ConversationScript>();
            if (conversationScript.items.Count > 0)
            {
                Debug.Log(npc.name + " NPC dialogue: ");
                foreach (ConversationPiece item in conversationScript.items)
                {
                    Debug.Log(item.text);
                }
            }
        }
        foreach (GameObject item in items)
        {
            Debug.Log(item.name + " coordinates: (" + item.transform.position.x + ", " + item.transform.position.y + ")");
            if (!item.activeInHierarchy)
            {
                Debug.Log(" status: found.\n");
            } 
            else
            {
                Debug.Log(" status: not yet found.\n");
            }
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
