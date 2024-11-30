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
    }

    public static void SetNetworkedClient(NetworkClient client) => networkClient = client;
    public static void SetGameLogic(GameLogic logic) => gameLogic = logic;
}


#region Protocol Signifiers
static public class ClientToServerSignifiers
{
    public const int asd = 1;
}

static public class ServerToClientSignifiers
{
    public const int asd = 1;
}

#endregion

