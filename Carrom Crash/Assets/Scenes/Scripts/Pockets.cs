using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Pockets : MonoBehaviour
{
    [Header("Settings")]
    public string CoinTag = "Coin";
    public string StrikerTag = "Striker";
    public LayerMask FallingLayer;
    public string PieceLayer = "Default";

    [Header("Visuals")]
    public float FallDuration = 0.38f;
    public Vector3 StrikerResetPos = new Vector3(0, -3.5f, 0);

    [Header("Juice")]
    [Tooltip("How much the coin squishes before sinking")]
    public float squishAmount = 0.3f;
    public float squishDuration = 0.08f;
    [Tooltip("Particle system child on this pocket")]
    public ParticleSystem pocketVFX;

    // Hook this up to broadcast score events — your LogicManager listens
    public static event System.Action<string> OnCoinPocketed;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(CoinTag))
        {
            StartCoroutine(PocketCoin(other.gameObject));
        }
        else if (other.CompareTag(StrikerTag))
        {
            StartCoroutine(PocketStriker(other.gameObject));
        }
    }

    private IEnumerator PocketCoin(GameObject coin)
    {
        // --- Phase 1: Freeze physics immediately ---
        Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false; // disable physics sim entirely
        }

        // Disable collider so nothing else can grab it mid-animation
        Collider2D col = coin.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // --- Phase 2: Squish punch (coin squishes as it "hits" the pocket rim) ---
        coin.transform.DOPunchScale(
            new Vector3(squishAmount, -squishAmount, 0f),
            squishDuration, vibrato: 1, elasticity: 0.5f
        );
        yield return new WaitForSeconds(squishDuration);

        // --- Phase 3: Suck into pocket center ---
        Sequence suckIn = DOTween.Sequence();
        suckIn.Append(
            coin.transform.DOMove(transform.position, FallDuration)
                .SetEase(Ease.InBack) // slight overshoot inward = satisfying
        );
        suckIn.Join(
            coin.transform.DOScale(Vector3.zero, FallDuration)
                .SetEase(Ease.InBack)
        );
        // Optional: spin while falling in
        suckIn.Join(
            coin.transform.DORotate(new Vector3(0, 0, 360f), FallDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.InCubic)
        );

        // --- Phase 4: Fire particles ---
        if (pocketVFX != null)
            pocketVFX.Play();

        // --- Phase 5: Broadcast score event ---
        OnCoinPocketed?.Invoke(coin.tag);

        yield return suckIn.WaitForCompletion();

        // --- Phase 6: Destroy coin ---
        Destroy(coin);
    }

    private IEnumerator PocketStriker(GameObject striker)
    {
        Rigidbody2D rb = striker.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        // Striker sinks in, smaller punch
        Sequence sinkStriker = DOTween.Sequence();
        sinkStriker.Append(
            striker.transform.DOMove(transform.position, FallDuration * 0.7f)
                .SetEase(Ease.InCubic)
        );
        sinkStriker.Join(
            striker.transform.DOScale(Vector3.zero, FallDuration * 0.7f)
                .SetEase(Ease.InCubic)
        );

        if (pocketVFX != null)
            pocketVFX.Play();

        yield return sinkStriker.WaitForCompletion();

        // Reset striker to baseline position
        striker.transform.position = StrikerResetPos;
        striker.transform.localScale = Vector3.one;
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
        }

        // Notify LogicManager about the foul
        OnCoinPocketed?.Invoke(StrikerTag);
    }
}