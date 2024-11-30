using UnityEngine;

public class GameLogic : MonoBehaviour
{
    const float CharacterSpeed = 0.06f;

    // Initialize only the necessary components
    void Start()
    {
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

        // Send movement updates to the server
        NetworkClientProcessing.SendMessageToServer(
            $"{ClientToServerSignifiers.UpdatePosition},{characterVelocityInPercent.x},{characterVelocityInPercent.y}",
            TransportPipeline.ReliableAndInOrder
        );
    }

    public GameObject SpawnAvatar(Vector2 positionInPercent, Color avatarColor)
    {
        GameObject avatar = new GameObject("Avatar");
        SpriteRenderer renderer = avatar.AddComponent<SpriteRenderer>();
        renderer.sprite = Resources.Load<Sprite>("Circle");

        // Assign the color
        renderer.color = avatarColor;

        // Update the position
        UpdateAvatarPosition(avatar, positionInPercent);

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
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
        worldPos.z = 0;
        return worldPos;
    }
}
