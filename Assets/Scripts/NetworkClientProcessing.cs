using System.Collections.Generic;
using UnityEngine;

static public class NetworkClientProcessing
{
    static NetworkClient networkClient;
    static GameLogic gameLogic;
    static Dictionary<int, GameObject> clientAvatars = new Dictionary<int, GameObject>();
    static int localClientID = -1;
    public static int GetLocalClientID() => localClientID;

    public static void ReceivedMessageFromServer(string msg, TransportPipeline pipeline)
    {
        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);

        switch (signifier)
        {
            case ServerToClientSignifiers.AssignClientID:
                SetLocalClientID(int.Parse(csv[1]));
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

    public static GameObject GetAvatarByID(int clientID)
    {
        return clientAvatars.ContainsKey(clientID) ? clientAvatars[clientID] : null;
    }

    private static void HandleSpawnAvatarMessage(string[] csv)
    {
        int clientID = int.Parse(csv[1]);
        float xPercent = float.Parse(csv[2]);
        float yPercent = float.Parse(csv[3]);
        Color avatarColor = new Color(float.Parse(csv[4]), float.Parse(csv[5]), float.Parse(csv[6]));

        if (clientID == localClientID || clientAvatars.ContainsKey(clientID)) return;

        GameObject avatar = gameLogic.SpawnAvatar(new Vector2(xPercent, yPercent), avatarColor);
        clientAvatars[clientID] = avatar;
    }

    private static void HandleUpdatePositionMessage(string[] csv)
    {
        int clientID = int.Parse(csv[1]);
        float xPercent = float.Parse(csv[2]);
        float yPercent = float.Parse(csv[3]);

        if (clientAvatars.ContainsKey(clientID))
        {
            Vector2 newPosition = new Vector2(xPercent, yPercent);
            gameLogic.UpdateAvatarPosition(clientAvatars[clientID], newPosition);
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

    public static void SetNetworkedClient(NetworkClient client) => networkClient = client;
    public static NetworkClient GetNetworkedClient() => networkClient; // Add this method
    public static void SetGameLogic(GameLogic logic) => gameLogic = logic;
    private static void SetLocalClientID(int id) => localClientID = id;
}

static public class ClientToServerSignifiers
{
    public const int UpdatePosition = 1;
}

static public class ServerToClientSignifiers
{
    public const int AssignClientID = 0;
    public const int SpawnAvatar = 1;
    public const int UpdatePosition = 2;
    public const int RemoveAvatar = 3;
}
