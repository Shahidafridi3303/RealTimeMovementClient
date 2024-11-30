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

    public static Vector3 ConvertToWorldPosition(Vector2 percentPosition)
    {
        Camera mainCamera = Camera.main; // Assumes using the main camera
        // Converts from percentage (0-1) to viewport space (still 0-1), then to world space
        Vector3 worldPosition = mainCamera.ViewportToWorldPoint(new Vector3(percentPosition.x, percentPosition.y, 0));
        worldPosition.z = 0; // Ensure the z-coordinate is set correctly for a 2D game
        return worldPosition;
    }


    public static void ReceivedMessageFromServer(string msg, TransportPipeline pipeline)
    {
        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);

        if (signifier == ServerToClientSignifiers.AssignClientID)
        {
            int assignedID = int.Parse(csv[1]);
            SetLocalClientID(assignedID);
        }
        else if (signifier == ServerToClientSignifiers.SpawnAvatar)
        {
            HandleSpawnAvatarMessage(csv);
        }
        else if (signifier == ServerToClientSignifiers.UpdatePosition)
        {
            int clientID = int.Parse(csv[1]);
            Vector2 newPosition = new Vector2(float.Parse(csv[2]), float.Parse(csv[3]));

            // Ensure the client is updating the correct avatar position
            if (clientAvatars.ContainsKey(clientID))
            {
                GameObject avatar = clientAvatars[clientID];
                avatar.transform.position = ConvertToWorldPosition(newPosition);
            }
        }
        else if (signifier == ServerToClientSignifiers.RemoveAvatar)
        {
            int clientID = int.Parse(csv[1]);

            if (clientAvatars.ContainsKey(clientID))
            {
                GameObject avatar = clientAvatars[clientID];
                GameObject.Destroy(avatar);
                clientAvatars.Remove(clientID);
            }
        }
    }

    private static void HandleSpawnAvatarMessage(string[] csv)
    {
        int clientID = int.Parse(csv[1]);
        float xPercent = float.Parse(csv[2]);
        float yPercent = float.Parse(csv[3]);
        float r = float.Parse(csv[4]);
        float g = float.Parse(csv[5]);
        float b = float.Parse(csv[6]);

        Color avatarColor = new Color(r, g, b);

        if (clientID == localClientID) return; // Skip spawning for local client

        if (clientAvatars.ContainsKey(clientID))
        {
            Debug.LogWarning($"Avatar for Client {clientID} already exists. Ignoring spawn request.");
            return;
        }

        GameObject avatar = gameLogic.SpawnAvatar(new Vector2(xPercent, yPercent), avatarColor);
        clientAvatars[clientID] = avatar;

        Debug.Log($"Spawned avatar for Client {clientID} at ({xPercent}, {yPercent}) with color {avatarColor}");
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
