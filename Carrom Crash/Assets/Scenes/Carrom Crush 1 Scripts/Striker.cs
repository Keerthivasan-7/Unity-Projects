using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class StrikerController : MonoBehaviour
{
    [Header("Physics & Logic")]
    public Vector2 initialPosition = new Vector2(0, -3.5f);
    public float maxDragDistance = 3f;
    public float forceMultiplier = 15f;
    public float stopThreshold = 0.1f; // Speed below which we consider it "stopped"

    private PlayerInputs controls;
    private Camera mainCamera;
    private Rigidbody2D rb;
    private LineRenderer line;

    private bool isDragging = false;
    private bool hasBeenShot = false;

    private void Awake()
    {
        controls = new PlayerInputs();
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        line = GetComponent<LineRenderer>();

        // Set the striker to the starting spot immediately
        transform.position = initialPosition;
        line.enabled = false;
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Gameplay.Click.started += OnPress;
        controls.Gameplay.Click.canceled += OnRelease;
    }

    private void OnDisable()
    {
        controls.Gameplay.Click.started -= OnPress;
        controls.Gameplay.Click.canceled -= OnRelease;
        controls.Disable();
    }

    private void OnPress(InputAction.CallbackContext context)
    {
        // Don't allow clicking if the striker is still moving from a previous shot
        if (rb.linearVelocity.magnitude > stopThreshold) return;

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            isDragging = true;
            line.enabled = true;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Update()
    {
        if (isDragging)
        {
            UpdateLine();
        }
        else if (hasBeenShot)
        {
            CheckForStop();
        }
    }

    private void UpdateLine()
    {
        Vector3 currentMousePos = GetMouseWorldPosition();
        Vector3 dragVector = transform.position - currentMousePos;

        // Clamp the line length to match your maxDragDistance
        float distance = Mathf.Min(dragVector.magnitude, maxDragDistance);
        Vector3 lineEndPoint = transform.position - (dragVector.normalized * distance);

        line.SetPosition(0, transform.position); // Start at center
        line.SetPosition(1, lineEndPoint);      // End at mouse (clamped)
    }

    private void OnRelease(InputAction.CallbackContext context)
    {
        if (!isDragging) return;

        isDragging = false;
        line.enabled = false;

        Vector3 releasePoint = GetMouseWorldPosition();
        Vector3 dragVector = transform.position - releasePoint;

        float distance = Mathf.Min(dragVector.magnitude, maxDragDistance);
        Vector3 finalForce = dragVector.normalized * distance * forceMultiplier;

        rb.AddForce(finalForce, ForceMode2D.Impulse);
        hasBeenShot = true;
    }

    private void CheckForStop()
    {
        // Check if the striker has slowed down enough to reset
        if (rb.linearVelocity.magnitude < stopThreshold)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            transform.position = initialPosition;
            hasBeenShot = false;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;
        return worldPos;
    }
}