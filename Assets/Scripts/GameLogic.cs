using UnityEngine;

public class GameLogic : MonoBehaviour
{
    const float CharacterSpeed = 0.06f;

    // Initialize only the necessary components
    void Start()
    {
        NetworkClientProcessing.SetGameLogic(this); // Set the GameLogic instance for network processing
    }

    // Update is called once per frame
    void Update()
    {
        HandleLocalMovement(); // Handle player movement every frame
    }

    private void HandleLocalMovement()
    {
        Vector2 characterVelocityInPercent = Vector2.zero;

        // Handle input for movement (WASD keys)
        if (Input.GetKey(KeyCode.W)) characterVelocityInPercent.y += CharacterSpeed;
        if (Input.GetKey(KeyCode.S)) characterVelocityInPercent.y -= CharacterSpeed;
        if (Input.GetKey(KeyCode.D)) characterVelocityInPercent.x += CharacterSpeed;
        if (Input.GetKey(KeyCode.A)) characterVelocityInPercent.x -= CharacterSpeed;

        // Normalize if the movement magnitude is too large to keep consistent movement speed
        if (characterVelocityInPercent.magnitude > 1)
        {
            characterVelocityInPercent.Normalize();
        }

        // Send movement updates to the server (only the velocity in the X and Y directions)
        NetworkClientProcessing.SendMessageToServer(
            $"{ClientToServerSignifiers.UpdatePosition},{characterVelocityInPercent.x},{characterVelocityInPercent.y}",
            TransportPipeline.ReliableAndInOrder
        );
    }

    // This method spawns the avatar at a given percentage of screen space with a specified color
    public GameObject SpawnAvatar(Vector2 positionInPercent, Color avatarColor)
    {
        // Create a new game object for the avatar
        GameObject avatar = new GameObject("Avatar");
        SpriteRenderer renderer = avatar.AddComponent<SpriteRenderer>(); // Add the sprite renderer component

        // Load the "Circle" sprite from the Resources folder
        renderer.sprite = Resources.Load<Sprite>("Circle");

        // Assign the color to the avatar
        renderer.color = avatarColor;

        // Update the avatar's position
        UpdateAvatarPosition(avatar, positionInPercent);

        return avatar; // Return the newly spawned avatar
    }

    // Update the avatar's position in world space based on a percentage of screen space
    public void UpdateAvatarPosition(GameObject avatar, Vector2 positionInPercent)
    {
        // Convert the percentage-based position to world coordinates
        Vector3 worldPos = ConvertToWorldPosition(positionInPercent);

        // Set the avatar's position in world space
        avatar.transform.position = worldPos;
    }

    // Convert a percentage-based position to world space based on screen dimensions
    private Vector3 ConvertToWorldPosition(Vector2 percentPosition)
    {
        // Convert the percentage to actual screen coordinates
        Vector2 screenPos = new Vector2(percentPosition.x * Screen.width, percentPosition.y * Screen.height);

        // Convert screen coordinates to world space
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));

        // Ensure the Z coordinate is 0 to keep the avatar in the 2D plane
        worldPos.z = 0;

        return worldPos; // Return the world space position
    }
}
