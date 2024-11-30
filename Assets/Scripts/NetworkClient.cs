using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Networking.Transport;
using System.Text;
using System;

public class NetworkClient : MonoBehaviour
{
    NetworkDriver networkDriver;
    NetworkConnection networkConnection;
    NetworkPipeline reliableAndInOrderPipeline;
    NetworkPipeline nonReliableNotInOrderedPipeline;
    const ushort NetworkPort = 9001;
    const string IPAddress = "192.168.4.102";

    private bool connectionEstablished = false;
    private bool isConnected = false; // Add this at the top of the class

    void Start()
    {
        if (NetworkClientProcessing.GetNetworkedClient() == null)
        {
            DontDestroyOnLoad(this.gameObject);
            NetworkClientProcessing.SetNetworkedClient(this);
            Connect();
        }
        else
        {
            Debug.Log("Singleton-ish architecture violation detected, investigate where NetworkClient.cs Start() is being called.");
            Destroy(this.gameObject);
        }
    }

    public void OnDestroy()
    {
        if (networkConnection.IsCreated)
            networkConnection.Disconnect(networkDriver);

        if (networkDriver.IsCreated)
            networkDriver.Dispose();
    }

    void Update()
    {
        networkDriver.ScheduleUpdate().Complete();

        if (!networkConnection.IsCreated)
        {
            if (isConnected)
            {
                Debug.LogError("Lost connection to server.");
                isConnected = false;
            }
            return;
        }

        NetworkEvent.Type networkEventType;
        DataStreamReader streamReader;
        NetworkPipeline pipelineUsedToSendEvent;

        while (PopNetworkEventAndCheckForData(out networkEventType, out streamReader, out pipelineUsedToSendEvent))
        {
            TransportPipeline pipelineUsed = TransportPipeline.NotIdentified;
            if (pipelineUsedToSendEvent == reliableAndInOrderPipeline)
                pipelineUsed = TransportPipeline.ReliableAndInOrder;
            else if (pipelineUsedToSendEvent == nonReliableNotInOrderedPipeline)
                pipelineUsed = TransportPipeline.FireAndForget;

            switch (networkEventType)
            {
                case NetworkEvent.Type.Connect:
                    Debug.Log("Client connected to server.");
                    isConnected = true; // Mark connection as established
                    break;

                case NetworkEvent.Type.Data:
                    int sizeOfDataBuffer = streamReader.ReadInt();
                    NativeArray<byte> buffer = new NativeArray<byte>(sizeOfDataBuffer, Allocator.Persistent);
                    streamReader.ReadBytes(buffer);
                    string msg = Encoding.Unicode.GetString(buffer.ToArray());
                    NetworkClientProcessing.ReceivedMessageFromServer(msg, pipelineUsed);
                    buffer.Dispose();
                    break;

                case NetworkEvent.Type.Disconnect:
                    Debug.LogError("Disconnected from server.");
                    isConnected = false; // Mark connection as lost
                    networkConnection = default(NetworkConnection);
                    break;
            }
        }
    }


    private bool PopNetworkEventAndCheckForData(out NetworkEvent.Type networkEventType, out DataStreamReader streamReader, out NetworkPipeline pipelineUsedToSendEvent)
    {
        networkEventType = networkConnection.PopEvent(networkDriver, out streamReader, out pipelineUsedToSendEvent);
        return networkEventType != NetworkEvent.Type.Empty;
    }

    public void SendMessageToServer(string msg, TransportPipeline pipeline)
    {
        if (!isConnected)
        {
            Debug.LogError("Failed to send message to server: Not connected.");
            return;
        }

        if (!networkDriver.IsCreated)
        {
            Debug.LogError("Failed to send message to server: Network driver is not created.");
            return;
        }

        if (!networkConnection.IsCreated)
        {
            Debug.LogError("Failed to send message to server: Network connection is not created.");
            return;
        }

        try
        {
            NetworkPipeline networkPipeline = reliableAndInOrderPipeline;
            if (pipeline == TransportPipeline.FireAndForget)
                networkPipeline = nonReliableNotInOrderedPipeline;

            byte[] msgAsByteArray = Encoding.Unicode.GetBytes(msg);
            NativeArray<byte> buffer = new NativeArray<byte>(msgAsByteArray, Allocator.Temp);

            DataStreamWriter streamWriter;
            if (networkDriver.BeginSend(networkPipeline, networkConnection, out streamWriter) == 0)
            {
                streamWriter.WriteInt(buffer.Length);
                streamWriter.WriteBytes(buffer);
                networkDriver.EndSend(streamWriter);
                Debug.Log($"Message sent to server: {msg}");
            }
            else
            {
                Debug.LogError("Failed to send message to server: Unable to start send operation.");
            }

            buffer.Dispose();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to send message to server: {ex.Message}");
        }
    }


    public void Connect()
    {
        try
        {
            if (networkDriver.IsCreated)
            {
                Debug.LogWarning("Network driver already created. Skipping initialization.");
                return;
            }

            networkDriver = NetworkDriver.Create();
            reliableAndInOrderPipeline = networkDriver.CreatePipeline(typeof(FragmentationPipelineStage), typeof(ReliableSequencedPipelineStage));
            nonReliableNotInOrderedPipeline = networkDriver.CreatePipeline(typeof(FragmentationPipelineStage));

            networkConnection = default(NetworkConnection);

            NetworkEndPoint endpoint = NetworkEndPoint.Parse(IPAddress, NetworkPort, NetworkFamily.Ipv4);
            networkConnection = networkDriver.Connect(endpoint);

            if (networkConnection.IsCreated)
            {
                Debug.Log("Client is attempting to connect to the server...");
            }
            else
            {
                Debug.LogError("Failed to initialize network connection.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error during Connect: {ex.Message}");
        }
    }


    public bool IsConnected()
    {
        return networkConnection.IsCreated;
    }

    public void Disconnect()
    {
        networkConnection.Disconnect(networkDriver);
        networkConnection = default(NetworkConnection);
    }

}

public enum TransportPipeline
{
    NotIdentified,
    ReliableAndInOrder,
    FireAndForget
}