using UnityEngine;

public class GameLogic : MonoBehaviour
{
    const float CharacterSpeed = 0.9f;
    private bool isMoving = false;

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

        // Check movement input and update velocity
        if (Input.GetKey(KeyCode.W)) characterVelocityInPercent.y += CharacterSpeed;
        if (Input.GetKey(KeyCode.S)) characterVelocityInPercent.y -= CharacterSpeed;
        if (Input.GetKey(KeyCode.D)) characterVelocityInPercent.x += CharacterSpeed;
        if (Input.GetKey(KeyCode.A)) characterVelocityInPercent.x -= CharacterSpeed;

        // Normalize velocity if magnitude > 1
        if (characterVelocityInPercent.magnitude > 1)
        {
            characterVelocityInPercent.Normalize();
        }

        int localID = NetworkClientProcessing.GetLocalClientID();
        if (NetworkClientProcessing.GetAvatarByID(localID) != null)
        {
            // Update the local avatar position for smooth visual feedback
            GameObject avatar = NetworkClientProcessing.GetAvatarByID(localID);
            UpdateAvatarPosition(avatar, avatar.transform.position + (Vector3)characterVelocityInPercent * Time.deltaTime);
        }

        // Check if the character is moving
        if (characterVelocityInPercent != Vector2.zero)
        {
            if (!isMoving)
            {
                isMoving = true; // Start movement
            }

            // Send movement updates to the server continuously while moving
            NetworkClientProcessing.SendMessageToServer(
                $"{ClientToServerSignifiers.UpdatePosition},{characterVelocityInPercent.x},{characterVelocityInPercent.y}",
                TransportPipeline.ReliableAndInOrder
            );
        }
        else
        {
            if (isMoving)
            {
                // Stop movement and notify the server once
                isMoving = false;
                NetworkClientProcessing.SendMessageToServer(
                    $"{ClientToServerSignifiers.UpdatePosition},0,0",
                    TransportPipeline.ReliableAndInOrder
                );
            }
        }
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

    public void UpdateAvatarPosition(GameObject avatar, Vector2 positionInPercent)
    {
        Vector3 worldPos = ConvertToWorldPosition(positionInPercent);
        avatar.transform.position = worldPos;
    }

    private Vector3 ConvertToWorldPosition(Vector2 percentPosition)
    {
        Vector2 screenPos = new Vector2(percentPosition.x * Screen.width, percentPosition.y * Screen.height);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane));
        worldPos.z = 0;
        return worldPos;
    }
}
