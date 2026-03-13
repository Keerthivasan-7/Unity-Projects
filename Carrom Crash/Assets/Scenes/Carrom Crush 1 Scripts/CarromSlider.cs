using UnityEngine;
using UnityEngine.InputSystem;

public class CarromSlider : MonoBehaviour
{
    [Header("References")]
    public StrikerController actualStrikerScript; // Drag the actual striker here

    [Header("Movement Settings")]
    public float xLimit = 2.5f; // How far left/right the striker can go
    public float sliderYOffset = -4.5f; // The Y position for the slider handle itself

    private PlayerInputs controls;
    private Camera mainCamera;
    private bool isSliding = false;
    private Transform actualStrikerTransform;

    private void Awake()
    {
        controls = new PlayerInputs();
        mainCamera = Camera.main;

        if (actualStrikerScript != null)
        {
            actualStrikerTransform = actualStrikerScript.transform;
        }
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
        // 1. Raycast to see if we clicked THIS slider object
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            isSliding = true;
        }
    }

    private void OnRelease(InputAction.CallbackContext context)
    {
        isSliding = false;
    }

    private void Update()
    {
        if (isSliding)
        {
            UpdatePositions();
        }
    }

    private void UpdatePositions()
    {
        Vector3 mousePos = GetMouseWorldPosition();

        // Clamp the X position so it doesn't go off the board
        float clampedX = Mathf.Clamp(mousePos.x, -xLimit, xLimit);

        // 2. Move the Slider Handle (this object)
        transform.position = new Vector3(clampedX, sliderYOffset, 0);

        // 3. Move the Actual Striker inside the board
        if (actualStrikerScript != null)
        {
            // Update the physical position
            actualStrikerTransform.position = new Vector3(clampedX, actualStrikerScript.initialPosition.y, 0);

            // CRITICAL: Update the initialPosition in the Striker script 
            // so it resets to this new X coordinate after a shot.
            actualStrikerScript.initialPosition = new Vector2(clampedX, actualStrikerScript.initialPosition.y);
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