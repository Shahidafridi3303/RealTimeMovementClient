using UnityEngine;

public class GameLogic : MonoBehaviour
{
    GameObject character;

    Vector2 characterPositionInPercent; // Current position in percent (sent by the server)
    Vector2 characterVelocityInPercent; // Velocity based on input
    [SerializeField] private float CharacterSpeed = 0.06f; // Editable speed in Inspector

    void Start()
    {
        NetworkClientProcessing.SetGameLogic(this);

        Sprite circleTexture = Resources.Load<Sprite>("Circle");

        // Create the character GameObject
        character = new GameObject("Character");
        SpriteRenderer renderer = character.AddComponent<SpriteRenderer>();
        renderer.sprite = circleTexture;

        // Set an initial position (center of the screen)
        characterPositionInPercent = new Vector2(0.5f, 0.5f);
    }

    void Update()
    {
        // Reset velocity
        characterVelocityInPercent = Vector2.zero;

        // Check which keys are currently being held down
        if (Input.GetKey(KeyCode.W))
        {
            characterVelocityInPercent.y = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            characterVelocityInPercent.y = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            characterVelocityInPercent.x = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            characterVelocityInPercent.x = -1;
        }

        // Normalize diagonal movement to maintain consistent speed
        if (characterVelocityInPercent.magnitude > 1)
        {
            characterVelocityInPercent.Normalize();
        }

        // Apply speed multiplier
        characterVelocityInPercent *= CharacterSpeed;

        // Send velocity to server if it's non-zero
        if (characterVelocityInPercent != Vector2.zero)
        {
            Debug.Log($"Client: Calculated Velocity: {characterVelocityInPercent}");

            string msg = $"{ClientToServerSignifiers.UpdatePosition},{characterVelocityInPercent.x},{characterVelocityInPercent.y}";
            Debug.Log($"Client: Sending message to server: {msg}");
            NetworkClientProcessing.SendMessageToServer(msg, TransportPipeline.ReliableAndInOrder);
        }
    }

    public GameObject SpawnAvatar(Vector2 positionInPercent)
    {
        GameObject avatar = new GameObject("Avatar");
        SpriteRenderer renderer = avatar.AddComponent<SpriteRenderer>();
        renderer.sprite = Resources.Load<Sprite>("Circle");

        UpdateAvatarPosition(avatar, positionInPercent);

        return avatar;
    }

    public void UpdateAvatarPosition(GameObject avatar, Vector2 positionInPercent)
    {
        // Convert percent position to world position
        Vector2 screenPos = new Vector2(positionInPercent.x * Screen.width, positionInPercent.y * Screen.height);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
        worldPos.z = 0;

        // Debug the conversion
        Debug.Log($"Converting percent position ({positionInPercent.x}, {positionInPercent.y}) to world position ({worldPos.x}, {worldPos.y}, {worldPos.z})");

        // Apply the world position to the avatar
        avatar.transform.position = worldPos;

        Debug.Log($"Updated avatar position to: {avatar.transform.position}");
    }
}
