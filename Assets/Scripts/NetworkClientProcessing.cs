using UnityEngine;
using System.Collections.Generic;

static public class NetworkClientProcessing
{
    static NetworkClient networkClient;
    static GameLogic gameLogic;

    // Dictionary to track avatars of other clients
    static Dictionary<int, GameObject> clientAvatars = new Dictionary<int, GameObject>();

    // Local client ID assigned by the server
    static int localClientID = -1;

    public static void ReceivedMessageFromServer(string msg, TransportPipeline pipeline)
    {
        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);

        switch (signifier)
        {
            case ServerToClientSignifiers.AssignClientID:
                int assignedID = int.Parse(csv[1]);
                SetLocalClientID(assignedID);
                break;

            case ServerToClientSignifiers.SpawnAvatar:
                HandleSpawnAvatarMessage(csv);
                break;

            case ServerToClientSignifiers.UpdatePosition:
                HandleUpdatePositionMessage(csv);
                break;

            case ServerToClientSignifiers.RemoveAvatar:
                HandleRemoveAvatarMessage(csv);
                break;
        }
    }

    private static void HandleSpawnAvatarMessage(string[] csv)
    {
        int clientID = int.Parse(csv[1]);
        float xPercent = float.Parse(csv[2]);
        float yPercent = float.Parse(csv[3]);

        if (clientID == localClientID) return; // Skip spawning local client avatar

        if (!clientAvatars.ContainsKey(clientID))
        {
            GameObject avatar = gameLogic.SpawnAvatar(new Vector2(xPercent, yPercent));
            clientAvatars[clientID] = avatar;
        }
    }

    private static void HandleUpdatePositionMessage(string[] csv)
    {
        int clientID = int.Parse(csv[1]);
        float xPercent = float.Parse(csv[2]);
        float yPercent = float.Parse(csv[3]);

        if (clientAvatars.ContainsKey(clientID))
        {
            gameLogic.UpdateAvatarPosition(clientAvatars[clientID], new Vector2(xPercent, yPercent));
        }
    }

    private static void HandleRemoveAvatarMessage(string[] csv)
    {
        int clientID = int.Parse(csv[1]);

        if (clientAvatars.ContainsKey(clientID))
        {
            GameObject avatar = clientAvatars[clientID];
            GameObject.Destroy(avatar);
            clientAvatars.Remove(clientID);
        }
    }

    public static void SendMessageToServer(string msg, TransportPipeline pipeline)
    {
        networkClient.SendMessageToServer(msg, pipeline);
    }

    public static void SetNetworkedClient(NetworkClient client)
    {
        networkClient = client;
    }

    public static NetworkClient GetNetworkedClient()
    {
        return networkClient;
    }

    public static void SetGameLogic(GameLogic logic)
    {
        gameLogic = logic;
    }

    private static void SetLocalClientID(int id)
    {
        localClientID = id;
    }
}

#region Protocol Signifiers
static public class ClientToServerSignifiers
{
    public const int UpdatePosition = 1; // Sent when a client updates their velocity
}

static public class ServerToClientSignifiers
{
    public const int AssignClientID = 0; // Sent to assign the client's ID
    public const int SpawnAvatar = 1;    // Sent to spawn an avatar
    public const int UpdatePosition = 2; // Sent to update a client's position
    public const int RemoveAvatar = 3;   // Sent to remove a disconnected client's avatar
}
#endregion
