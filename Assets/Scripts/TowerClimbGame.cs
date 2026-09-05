using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class TowerClimbGame : MonoBehaviour
{
    [Header("Tower")]
    [SerializeField] private int platformCount = 30;
    [SerializeField] private float platformGap = 1.55f;
    [SerializeField] private float fallDistance = 6f;

    private readonly Color[] platformColors =
    {
        new Color(0.25f, 0.86f, 1f),
        new Color(0.44f, 0.72f, 1f),
        new Color(0.74f, 0.92f, 1f)
    };

    private Camera gameplayCamera;
    private Rigidbody2D playerBody;
    private Vector3 playerStartPosition;
    private Vector3 cameraStartPosition;
    private float highestPosition;
    private Sprite platformSprite;

    private void Start()
    {
        gameplayCamera = Camera.main;
        playerBody = GetComponent<Rigidbody2D>();
        playerStartPosition = transform.position;
        highestPosition = playerStartPosition.y;

        if (gameplayCamera != null)
        {
            cameraStartPosition = gameplayCamera.transform.position;
            gameplayCamera.backgroundColor = new Color(0.05f, 0.12f, 0.24f);
        }

        BuildTower();
    }

    private void Update()
    {
        highestPosition = Mathf.Max(highestPosition, transform.position.y);

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartRun();
        }

        if (gameplayCamera != null && transform.position.y < gameplayCamera.transform.position.y - fallDistance)
        {
            RestartRun();
        }
    }

    private void LateUpdate()
    {
        if (gameplayCamera == null)
        {
            return;
        }

        float targetY = Mathf.Max(cameraStartPosition.y, transform.position.y + 1.25f);
        Vector3 cameraPosition = gameplayCamera.transform.position;
        if (targetY > cameraPosition.y)
        {
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, targetY, Time.deltaTime * 3.5f);
            gameplayCamera.transform.position = cameraPosition;
        }
    }

    private void BuildTower()
    {
        float previousX = 0f;

        for (int index = 0; index < platformCount; index++)
        {
            float y = index * platformGap;
            float width = index == 0 ? 4.5f : Random.Range(2.1f, 3.4f);
            float x = index == 0 ? 0f : Mathf.Clamp(previousX + Random.Range(-2.5f, 2.5f), -3.8f, 3.8f);
            CreatePlatform(index, new Vector2(x, y), width);
            previousX = x;
        }
    }

    private void CreatePlatform(int index, Vector2 position, float width)
    {
        GameObject platform = new GameObject($"Ice Platform {index + 1}");
        platform.transform.position = position;
        platform.transform.localScale = new Vector3(width, 0.32f, 1f);

        SpriteRenderer renderer = platform.AddComponent<SpriteRenderer>();
        renderer.sprite = GetPlatformSprite();
        renderer.color = platformColors[index % platformColors.Length];
        renderer.sortingOrder = -1;

        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.usedByEffector = true;

        PlatformEffector2D oneWaySurface = platform.AddComponent<PlatformEffector2D>();
        oneWaySurface.useOneWay = true;
        oneWaySurface.surfaceArc = 160f;
        oneWaySurface.useSideFriction = false;
        oneWaySurface.useSideBounce = false;
    }

    private Sprite GetPlatformSprite()
    {
        if (platformSprite != null)
        {
            return platformSprite;
        }

        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        platformSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        return platformSprite;
    }

    private void RestartRun()
    {
        transform.position = playerStartPosition;
        playerBody.linearVelocity = Vector2.zero;
        highestPosition = playerStartPosition.y;

        if (gameplayCamera != null)
        {
            gameplayCamera.transform.position = cameraStartPosition;
        }
    }

    private void OnGUI()
    {
        GUI.color = Color.white;
        GUI.Box(new Rect(18f, 18f, 240f, 96f), "TOWER CLIMB");
        GUI.Label(new Rect(34f, 48f, 210f, 24f), $"Wysokość: {Mathf.Max(0f, highestPosition) * 10f:0} m");
        GUI.Label(new Rect(34f, 74f, 210f, 24f), "← → ruch   |   Spacja: skok   |   R: od nowa");
    }
}
