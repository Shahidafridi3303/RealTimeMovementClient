using System.Collections;
using System.Collections.Generic;
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
        // Capture movement input and calculate velocity
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

            // Send movement update to the server
            string msg = $"{ClientToServerSignifiers.UpdatePosition},{characterVelocityInPercent.x},{characterVelocityInPercent.y}";
            NetworkClientProcessing.SendMessageToServer(msg, TransportPipeline.ReliableAndInOrder);
        }
    }

    public void UpdateCharacterPosition(Vector2 newPositionInPercent)
    {
        // Update the character's position based on the server's message
        characterPositionInPercent = newPositionInPercent;

        // Convert percentage position to world position
        Vector2 screenPos = new Vector2(characterPositionInPercent.x * Screen.width, characterPositionInPercent.y * Screen.height);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
        worldPos.z = 0;
        character.transform.position = worldPos;
    }
}
