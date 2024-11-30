using UnityEngine;
using System.Collections.Generic;

static public class NetworkClientProcessing
{
    static NetworkClient networkClient;
    static GameLogic gameLogic;

    // Dictionary to track avatars of other clients
    static Dictionary<int, GameObject> clientAvatars = new Dictionary<int, GameObject>();

    public static void ReceivedMessageFromServer(string msg, TransportPipeline pipeline)
    {
        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);

        if (signifier == ServerToClientSignifiers.SpawnAvatar)
        {
            int clientID = int.Parse(csv[1]);
            float xPercent = float.Parse(csv[2]);
            float yPercent = float.Parse(csv[3]);

            Debug.Log($"Client: Spawning avatar for client ID {clientID} at ({xPercent}, {yPercent})");

            // Spawn an avatar for the specified client ID
            if (!clientAvatars.ContainsKey(clientID))
            {
                GameObject avatar = gameLogic.SpawnAvatar(new Vector2(xPercent, yPercent));
                clientAvatars[clientID] = avatar;
            }
        }
        else if (signifier == ServerToClientSignifiers.RemoveAvatar)
        {
            int clientID = int.Parse(csv[1]);
            Debug.Log($"Client: Removing avatar for client ID {clientID}");

            // Remove the avatar for the specified client ID
            if (clientAvatars.ContainsKey(clientID))
            {
                GameObject avatar = clientAvatars[clientID];
                GameObject.Destroy(avatar);
                clientAvatars.Remove(clientID);
            }
        }
        else if (signifier == ServerToClientSignifiers.UpdatePosition)
        {
            int clientID = int.Parse(csv[1]);
            float xPercent = float.Parse(csv[2]);
            float yPercent = float.Parse(csv[3]);

            if (clientAvatars.ContainsKey(clientID))
            {
                gameLogic.UpdateAvatarPosition(clientAvatars[clientID], new Vector2(xPercent, yPercent));
            }
        }
    }

    public static void SendMessageToServer(string msg, TransportPipeline pipeline)
    {
        networkClient.SendMessageToServer(msg, pipeline);
    }

    public static void SetNetworkedClient(NetworkClient client) => networkClient = client;
    public static void SetGameLogic(GameLogic logic) => gameLogic = logic;

    // Added missing method to resolve the red error
    public static NetworkClient GetNetworkedClient()
    {
        return networkClient;
    }
}

#region Protocol Signifiers
static public class ClientToServerSignifiers
{
    public const int UpdatePosition = 1; // Sent when a client updates their velocity
}

static public class ServerToClientSignifiers
{
    public const int SpawnAvatar = 1; // Sent to spawn an avatar
    public const int UpdatePosition = 2; // Sent to update a client's position
    public const int RemoveAvatar = 3; // Sent to remove a disconnected client's avatar
}
#endregion
