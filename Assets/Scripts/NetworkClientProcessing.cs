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

    #region Message Handling

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

    // Handle the avatar spawning
    private static void HandleSpawnAvatarMessage(string[] csv)
    {
        int clientID = int.Parse(csv[1]);
        float xPercent = float.Parse(csv[2]);
        float yPercent = float.Parse(csv[3]);
        Color avatarColor = new Color(float.Parse(csv[4]), float.Parse(csv[5]), float.Parse(csv[6]));

        if (clientID == localClientID || clientAvatars.ContainsKey(clientID)) return;

        GameObject avatar = gameLogic.SpawnAvatar(new Vector2(xPercent, yPercent), avatarColor);
        clientAvatars[clientID] = avatar;
        Debug.Log($"Spawned avatar for Client {clientID} at ({xPercent}, {yPercent}) with color {avatarColor}");
    }

    // Update the position of the avatar
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

    // Remove the avatar when a client disconnects
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

    #endregion

    #region Sending Messages

    // Send a message to the server
    public static void SendMessageToServer(string msg, TransportPipeline pipeline)
    {
        networkClient.SendMessageToServer(msg, pipeline);
    }

    #endregion

    #region Setup Methods

    // Set the network client
    public static void SetNetworkedClient(NetworkClient client)
    {
        networkClient = client;
    }

    // Get the network client
    public static NetworkClient GetNetworkedClient()
    {
        return networkClient;
    }

    // Set the game logic
    public static void SetGameLogic(GameLogic logic)
    {
        gameLogic = logic;
    }

    // Set the local client ID
    private static void SetLocalClientID(int id)
    {
        localClientID = id;
    }

    #endregion
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
