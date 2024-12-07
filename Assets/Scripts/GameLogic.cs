using UnityEngine;

public class GameLogic : MonoBehaviour
{
    const float CharacterSpeed = 0.9f;

    void Start()
    {
        Screen.SetResolution(968, 704, false);
        NetworkClientProcessing.SetGameLogic(this);
    }

    void Update()
    {
        HandleLocalMovement();
    }

    private void HandleLocalMovement()
    {
        Vector2 characterVelocityInPercent = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) characterVelocityInPercent.y += CharacterSpeed;
        if (Input.GetKey(KeyCode.S)) characterVelocityInPercent.y -= CharacterSpeed;
        if (Input.GetKey(KeyCode.D)) characterVelocityInPercent.x += CharacterSpeed;
        if (Input.GetKey(KeyCode.A)) characterVelocityInPercent.x -= CharacterSpeed;

        if (characterVelocityInPercent.magnitude > 1)
        {
            characterVelocityInPercent.Normalize();
        }

        // Update the local avatar immediately
        int localID = NetworkClientProcessing.GetLocalClientID();
        if (NetworkClientProcessing.GetAvatarByID(localID) != null)
        {
            GameObject avatar = NetworkClientProcessing.GetAvatarByID(localID);
            UpdateAvatarPosition(avatar, avatar.transform.position + (Vector3)characterVelocityInPercent * Time.deltaTime);
        }

        // Send movement updates to the server
        NetworkClientProcessing.SendMessageToServer(
            $"{ClientToServerSignifiers.UpdatePosition},{characterVelocityInPercent.x},{characterVelocityInPercent.y}",
            TransportPipeline.ReliableAndInOrder
        );
    }


    public GameObject SpawnAvatar(Vector2 position, Color color)
    {
        GameObject avatar = new GameObject("Avatar");
        SpriteRenderer renderer = avatar.AddComponent<SpriteRenderer>();
        renderer.sprite = Resources.Load<Sprite>("Circle");
        renderer.color = color;
        UpdateAvatarPosition(avatar, position);
        return avatar;
    }

    public void UpdateAvatarPosition(GameObject avatar, Vector2 position)
    {
        avatar.transform.position = ConvertToWorldPosition(position);
    }

    private Vector3 ConvertToWorldPosition(Vector2 percentPosition)
    {
        Vector2 screenPos = new Vector2(percentPosition.x * Screen.width, percentPosition.y * Screen.height);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
        worldPos.z = 0;
        return worldPos;
    }
}