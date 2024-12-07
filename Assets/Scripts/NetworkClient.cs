using UnityEngine;
using Unity.Networking.Transport;
using System.Text;
using Unity.Collections;

public class NetworkClient : MonoBehaviour
{
    NetworkDriver networkDriver;
    NetworkConnection networkConnection;
    NetworkPipeline reliableAndInOrderPipeline;
    NetworkPipeline nonReliableNotInOrderedPipeline;
    const ushort NetworkPort = 9001;
    const string IPAddress = "192.168.4.102";
    private bool isConnected = false;

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
        if (!networkConnection.IsCreated) return;

        NetworkEvent.Type networkEventType;
        DataStreamReader streamReader;
        NetworkPipeline pipelineUsedToSendEvent;

        while (PopNetworkEventAndCheckForData(out networkEventType, out streamReader, out pipelineUsedToSendEvent))
        {
            ProcessNetworkEvent(networkEventType, streamReader);
        }
    }

    private void ProcessNetworkEvent(NetworkEvent.Type eventType, DataStreamReader streamReader)
    {
        switch (eventType)
        {
            case NetworkEvent.Type.Connect:
                isConnected = true;
                break;

            case NetworkEvent.Type.Data:
                int sizeOfDataBuffer = streamReader.ReadInt();
                NativeArray<byte> buffer = new NativeArray<byte>(sizeOfDataBuffer, Allocator.Persistent);
                streamReader.ReadBytes(buffer);
                string msg = Encoding.Unicode.GetString(buffer.ToArray());
                NetworkClientProcessing.ReceivedMessageFromServer(msg, TransportPipeline.ReliableAndInOrder);
                buffer.Dispose();
                break;

            case NetworkEvent.Type.Disconnect:
                isConnected = false;
                networkConnection = default(NetworkConnection);
                break;
        }
    }

    private bool PopNetworkEventAndCheckForData(out NetworkEvent.Type networkEventType, out DataStreamReader streamReader, out NetworkPipeline pipelineUsedToSendEvent)
    {
        networkEventType = networkConnection.PopEvent(networkDriver, out streamReader, out pipelineUsedToSendEvent);
        return networkEventType != NetworkEvent.Type.Empty;
    }

    public void SendMessageToServer(string msg, TransportPipeline pipeline)
    {
        if (!isConnected || !networkDriver.IsCreated || !networkConnection.IsCreated) return;

        NativeArray<byte> buffer = new NativeArray<byte>(Encoding.Unicode.GetBytes(msg), Allocator.Temp);
        DataStreamWriter streamWriter;

        networkDriver.BeginSend(reliableAndInOrderPipeline, networkConnection, out streamWriter);
        streamWriter.WriteInt(buffer.Length);
        streamWriter.WriteBytes(buffer);
        networkDriver.EndSend(streamWriter);

        buffer.Dispose();
    }

    public void Connect()
    {
        networkDriver = NetworkDriver.Create();
        reliableAndInOrderPipeline = networkDriver.CreatePipeline(typeof(FragmentationPipelineStage), typeof(ReliableSequencedPipelineStage));
        nonReliableNotInOrderedPipeline = networkDriver.CreatePipeline(typeof(FragmentationPipelineStage));

        NetworkEndPoint endpoint = NetworkEndPoint.Parse(IPAddress, NetworkPort, NetworkFamily.Ipv4);
        networkConnection = networkDriver.Connect(endpoint);
    }

    public bool IsConnected() => networkConnection.IsCreated;
    public void Disconnect() => networkConnection.Disconnect(networkDriver);
}

public enum TransportPipeline
{
    NotIdentified,
    ReliableAndInOrder,
    FireAndForget
}
