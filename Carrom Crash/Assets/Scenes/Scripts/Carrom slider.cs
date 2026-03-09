using UnityEngine;
using UnityEngine.InputSystem;

public class CarromSlider : MonoBehaviour
{
    [Header("References")]
    public Transform strikerTransform;
    public SpriteRenderer boardBounds;

    [Header("Settings")]
    public float minX = -2.5f;
    public float maxX = 2.5f;

    [Header("Smoothing")]
    [Range(0f, 1f)]
    public float smoothing = 0.1f; // 0 = instant, higher = more lag/smoothness

    private Camera mainCamera;
    private bool isDragging = false;
    private float dragOffsetX = 0f;
    private float targetX;
    private CarromStriker strikerScript;

    void Start()
    {
        mainCamera = Camera.main;
        strikerScript = strikerTransform.GetComponent<CarromStriker>();
        targetX = transform.position.x;
    }

    void Update()
    {
        if (Mouse.current == null || strikerScript == null) return;

        // Only allow sliding if striker is on the baseline
        if (!strikerScript.canPosition)
        {
            isDragging = false;
            return;
        }

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -mainCamera.transform.position.z));

        // --- Start Drag ---
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                // Store offset so the handle doesn't jump to mouse center
                dragOffsetX = transform.position.x - worldPos.x;
            }
        }

        // --- While Dragging ---
        if (isDragging)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                targetX = Mathf.Clamp(worldPos.x + dragOffsetX, minX, maxX);
            }
            else
            {
                // Button released — stop dragging
                isDragging = false;
            }
        }

        // --- Smooth Movement ---
        float newX = smoothing > 0f
            ? Mathf.Lerp(transform.position.x, targetX, Time.deltaTime / smoothing)
            : targetX;

        // Move slider handle
        transform.position = new Vector3(newX, transform.position.y, 0f);

        // Sync striker position
        strikerTransform.position = new Vector3(newX, strikerScript.baselineY, 0f);
    }
}