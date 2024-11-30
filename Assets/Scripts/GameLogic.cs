using UnityEngine;

public class GameLogic : MonoBehaviour
{
    GameObject character;
    Vector2 characterPositionInPercent = new Vector2(0.5f, 0.5f); // Initial position
    Vector2 characterVelocityInPercent;
    const float CharacterSpeed = 0.06f;
    const float SmoothMovementSpeed = 10f;
    float DiagonalCharacterSpeed;

    void Start()
    {
        DiagonalCharacterSpeed = Mathf.Sqrt(CharacterSpeed * CharacterSpeed + CharacterSpeed * CharacterSpeed) / 2f;
        NetworkClientProcessing.SetGameLogic(this);

        // Create character GameObject
        character = new GameObject("Character");
        var renderer = character.AddComponent<SpriteRenderer>();
        renderer.sprite = Resources.Load<Sprite>("Circle");
    }

    void Update()
    {
        HandleLocalMovement();

        character.transform.position = ConvertToWorldPosition(characterPositionInPercent);
    }

    private void HandleLocalMovement()
    {
        characterVelocityInPercent = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) characterVelocityInPercent.y += CharacterSpeed;
        if (Input.GetKey(KeyCode.S)) characterVelocityInPercent.y -= CharacterSpeed;
        if (Input.GetKey(KeyCode.D)) characterVelocityInPercent.x += CharacterSpeed;
        if (Input.GetKey(KeyCode.A)) characterVelocityInPercent.x -= CharacterSpeed;

        if (characterVelocityInPercent.magnitude > 1)
        {
            characterVelocityInPercent.Normalize();
        }

        characterPositionInPercent += characterVelocityInPercent * Time.deltaTime;

        // Send updates to the server
        NetworkClientProcessing.SendMessageToServer($"{ClientToServerSignifiers.UpdatePosition},{characterVelocityInPercent.x},{characterVelocityInPercent.y}", TransportPipeline.ReliableAndInOrder);
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

    public void UpdateAvatarPosition(GameObject avatar, Vector2 newPosition)
    {
        Vector3 targetPosition = ConvertToWorldPosition(newPosition);
        if (avatar != null)
        {
            avatar.transform.position = Vector3.Lerp(avatar.transform.position, targetPosition, Time.deltaTime * 10);
        }
    }

    private Vector3 ConvertToWorldPosition(Vector2 percentPosition)
    {
        Vector2 screenPos = new Vector2(percentPosition.x * Screen.width, percentPosition.y * Screen.height);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0));
        worldPos.z = 0;
        return worldPos;
    }

}
