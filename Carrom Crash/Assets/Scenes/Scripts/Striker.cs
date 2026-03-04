using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Striker : MonoBehaviour
{
    private Rigidbody2D rb;

    private Vector2 startMousePosition;
    private Vector2 endMousePosition;

    private bool isDragging = false;

    public float powerMultiplier = 5f;
    public float maxPower = 10f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnMouseDown()
    {
        // Save initial mouse position
        startMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isDragging = true;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;

        endMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2 dragVector = startMousePosition - endMousePosition;

        float power = Mathf.Clamp(dragVector.magnitude, 0, maxPower);

        Vector2 direction = dragVector.normalized;

        rb.linearVelocity = Vector2.zero; // reset old movement
        rb.AddForce(direction * power * powerMultiplier, ForceMode2D.Impulse);

        isDragging = false;
    }
}