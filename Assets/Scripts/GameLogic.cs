using UnityEngine;

public class GameLogic : MonoBehaviour
{
    GameObject character;

    Vector2 characterPositionInPercent; // Current position in percent (sent by the server)
    Vector2 characterVelocityInPercent; // Velocity based on input
    const float CharacterSpeed = 0.25f; // Speed multiplier
    float DiagonalCharacterSpeed;

    void Start()
    {
        DiagonalCharacterSpeed = Mathf.Sqrt(CharacterSpeed * CharacterSpeed + CharacterSpeed * CharacterSpeed) / 2f;
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
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)
            || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D))
        {
            characterVelocityInPercent = Vector2.zero;

            if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
            {
                characterVelocityInPercent.x = DiagonalCharacterSpeed;
                characterVelocityInPercent.y = DiagonalCharacterSpeed;
            }
            else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
            {
                characterVelocityInPercent.x = -DiagonalCharacterSpeed;
                characterVelocityInPercent.y = DiagonalCharacterSpeed;
            }
            else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
            {
                characterVelocityInPercent.x = DiagonalCharacterSpeed;
                characterVelocityInPercent.y = -DiagonalCharacterSpeed;
            }
            else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A))
            {
                characterVelocityInPercent.x = -DiagonalCharacterSpeed;
                characterVelocityInPercent.y = -DiagonalCharacterSpeed;
            }
            else if (Input.GetKey(KeyCode.D))
                characterVelocityInPercent.x = CharacterSpeed;
            else if (Input.GetKey(KeyCode.A))
                characterVelocityInPercent.x = -CharacterSpeed;
            else if (Input.GetKey(KeyCode.W))
                characterVelocityInPercent.y = CharacterSpeed;
            else if (Input.GetKey(KeyCode.S))
                characterVelocityInPercent.y = -CharacterSpeed;

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

    // Client's GameLogic
    public void UpdateAvatarPosition(GameObject avatar, Vector2 positionInPercent)
    {
        Vector2 screenPos = new Vector2(positionInPercent.x * Screen.width, positionInPercent.y * Screen.height);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
        worldPos.z = 0;

        Debug.Log($"Converting percent position ({positionInPercent.x}, {positionInPercent.y}) to world position ({worldPos.x}, {worldPos.y}, {worldPos.z})");

        avatar.transform.position = worldPos;
        Debug.Log($"Updated avatar position to: {avatar.transform.position}");
    }


}
