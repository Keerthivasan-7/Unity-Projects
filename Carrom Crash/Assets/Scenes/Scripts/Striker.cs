/*using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class Striker : MonoBehaviour
{
    [Header("Physics Settings")]
    public float forceMultiplier = 15f;
    public float maxDragDistance = 2.5f;
    public float baselineY = -16.2f;


    private Rigidbody2D rb;
    private Camera mainCamera;
    private LineRenderer line;
    
    private Vector2 dragStartPos;
    private bool isAiming = false;
    private bool isPositioning = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        line = GetComponent<LineRenderer>();
        mainCamera = Camera.main;

        // Hide the line at the start
        line.enabled = false;
        line.positionCount = 2;
        
        transform.position = new Vector3(transform.position.x, baselineY, 0);
    }

    void Update()
    {
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(mousePos);

        // PHASE 1: Sliding on the line
        if (isPositioning)
        {
            HandlePositioning(worldMousePos);
        }
        // PHASE 2: Aiming and Shooting
        else
        {
            HandleShooting(worldMousePos);
        }
    }

    void HandlePositioning(Vector2 worldMousePos)
    {
        if (Mouse.current.leftButton.isPressed)
        {
            transform.position = new Vector3(worldMousePos.x, baselineY, 0);
        }
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isPositioning = false;
        }
    }

    void HandleShooting(Vector2 worldMousePos)
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            dragStartPos = worldMousePos;
            isAiming = true;
            line.enabled = true;
        }

        if (isAiming)
        {
            UpdateTrajectory(worldMousePos);

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                FireStriker(worldMousePos);
            }
        }
    }

    void UpdateTrajectory(Vector2 currentMousePos)
    {
        // Calculate the direction (opposite of drag)
        Vector2 dragVector = dragStartPos - currentMousePos;
        float distance = Mathf.Min(dragVector.magnitude, maxDragDistance);
        Vector2 direction = dragVector.normalized;

        // Set line points
        // Point 0: The striker's current position
        line.SetPosition(0, transform.position);
        
        // Point 1: Where the striker will head
        Vector3 endPoint = transform.position + (Vector3)(direction * distance);
        line.SetPosition(1, endPoint);
    }

    void FireStriker(Vector2 dragEndPos)
    {
        isAiming = false;
        line.enabled = false;

        Vector2 dragVector = dragStartPos - dragEndPos;
        float distance = Mathf.Min(dragVector.magnitude, maxDragDistance);
        Vector2 force = dragVector.normalized * distance * forceMultiplier;

        rb.AddForce(force, ForceMode2D.Impulse);
        
        // Re-enable positioning once the striker stops
        StartCoroutine(WaitUntilStopped());
    }

    System.Collections.IEnumerator WaitUntilStopped()
    {
        yield return new WaitForSeconds(0.5f);
        // Wait for velocity to drop near zero
        while (rb.linearVelocity.magnitude > 0.1f) 
        {
            yield return null;
        }
        isPositioning = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
    }
    void OnCollisionEnter2D(Collision2D collision)
{
    Debug.Log("Striker Hit: " + collision.gameObject.name);
}
}*/
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class CarromStriker : MonoBehaviour
{
    [Header("Physics Settings")]
    public float forceMultiplier = 15f;
    public float maxDragDistance = 2.5f;
    public float baselineY = -3.5f;

    [HideInInspector] public bool canPosition = true; // Controlled by the slider/state

    private Rigidbody2D rb;
    private Camera mainCamera;
    private LineRenderer line;
    private Vector2 dragStartPos;
    private bool isAiming = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        line = GetComponent<LineRenderer>();
        mainCamera = Camera.main;

        line.enabled = false;
        line.positionCount = 2;
        rb.gravityScale = 0f; // Ensure no gravity
    }

    void Update()
    {
        if (Mouse.current == null || !canPosition) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(mousePos);

        HandleShooting(worldMousePos);
    }

    void HandleShooting(Vector3 worldMousePos)
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Only start aiming if we click directly on the striker
            RaycastHit2D hit = Physics2D.Raycast(worldMousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                dragStartPos = worldMousePos;
                isAiming = true;
                line.enabled = true;
            }
        }

        if (isAiming)
        {
            UpdateTrajectory(worldMousePos);

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                FireStriker(worldMousePos);
            }
        }
    }

    void UpdateTrajectory(Vector2 currentMousePos)
    {
        Vector2 dragVector = dragStartPos - currentMousePos;
        float distance = Mathf.Min(dragVector.magnitude, maxDragDistance);
        Vector2 direction = dragVector.normalized;

        line.SetPosition(0, transform.position);
        line.SetPosition(1, (Vector2)transform.position + (direction * distance));
    }

    void FireStriker(Vector2 dragEndPos)
    {
        isAiming = false;
        line.enabled = false;
        canPosition = false; // Lock the slider while moving

        Vector2 dragVector = dragStartPos - dragEndPos;
        float distance = Mathf.Min(dragVector.magnitude, maxDragDistance);
        Vector2 force = dragVector.normalized * distance * forceMultiplier;

        rb.AddForce(force, ForceMode2D.Impulse);
        StartCoroutine(WaitUntilStopped());
    }

    System.Collections.IEnumerator WaitUntilStopped()
    {
        yield return new WaitForSeconds(0.5f);
        while (rb.linearVelocity.magnitude > 0.1f) 
        {
            yield return null;
        }
        
        // Reset and unlock slider
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        transform.position = new Vector3(transform.position.x, baselineY, 0);
        canPosition = true; 
    }
}