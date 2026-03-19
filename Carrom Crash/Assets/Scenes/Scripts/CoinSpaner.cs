using UnityEngine;

public class CoinSpaner : MonoBehaviour
{
    [Header("Coin Prefabs")]
    public GameObject redCoin;
    public GameObject whiteCoin;
    public GameObject blackCoin;

    [Header("Layout Settings")]
    [Tooltip("The distance between the center and the first ring of coins.")]
    public float innerRadius = 0.5f;

    [Tooltip("The distance between the center and the second ring of coins.")]
    public float outerRadius = 1.0f;

    void Start()
    {
        SpanBoard();
    }

    public void SpanBoard()
    {
        // 1. Spawn the Red Queen at the center (0,0)
        Instantiate(redCoin, Vector3.zero, Quaternion.identity, transform);

        // 2. Spawn the Inner Circle (6 coins total: 3 White, 3 Black)
        // We alternate colors: White, Black, White, Black...
        SpawnCircle(6, innerRadius, true);

        // 3. Spawn the Outer Circle (12 coins total: 6 White, 6 Black)
        // We alternate colors again to finish the 9/9 count.
        SpawnCircle(12, outerRadius, false);
    }

    private void SpawnCircle(int count, float radius, bool startWithWhite)
    {
        for (int i = 0; i < count; i++)
        {
            // Calculate angle for each coin (360 degrees / number of coins)
            float angle = i * (360f / count) * Mathf.Deg2Rad;

            // Basic Trig: x = cos(a) * r , y = sin(a) * r
            Vector3 spawnPos = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0
            );

            // Determine color based on index (Even = Color A, Odd = Color B)
            GameObject prefabToUse;
            if (i % 2 == 0)
            {
                prefabToUse = startWithWhite ? whiteCoin : blackCoin;
            }
            else
            {
                prefabToUse = startWithWhite ? blackCoin : whiteCoin;
            }

            Instantiate(prefabToUse, spawnPos, Quaternion.identity, transform);
        }
    }
}