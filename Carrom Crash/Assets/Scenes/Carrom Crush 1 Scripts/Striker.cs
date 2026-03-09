using UnityEngine;
using UnityEngine.InputSystem;

public class Striker : MonoBehaviour
{
    private PlayerInputs controls;
    private Camera mainCamera;

    private void Awake()
    {
        controls = new PlayerInputs();
        mainCamera = Camera.main; // Cache the camera for performance
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Gameplay.Click.performed += OnClick;
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        // 1. Get the current mouse position in pixels
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // 2. Convert that pixel position to a point in the 3D/2D Game World
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        // 3. Shoot a "Point" raycast to see if there is a collider at that spot
        // (Vector2.zero means we aren't shooting a line, just checking a single point)
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

        // 4. Check if we hit something
        if (hit.collider != null)
        {
            // 5. Check if the thing we hit is actually the Striker
            if (hit.collider.gameObject == gameObject)
            {
                Debug.Log("Striker touched! Ready to aim.");
                StartAiming();
            }
        }
    }

    private void StartAiming()
    {
        // Your logic for dragging the striker goes here
    }
}