
// using System.Collections.Generic;
// using UnityEngine;

// public enum PegColor
// {
//     Green,
//     Red
// }

// public class AutomaticPegIdentifier : MonoBehaviour, IPegIdentifier
// {
//     private const float FORCE_RANGE_SPAN = 0.2f;

//     private static Dictionary<PegColor, Vector2> s_ColorForceRanges;

//     /// <summary>Clears static ranges before every scene load so Awake() always re-randomizes.</summary>
//     // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
//     // private static void OnBeforeSceneLoad()
//     // {
//     //     s_ColorForceRanges = null;
//     // }
//     [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
//     private static void OnBeforeSceneLoad()
//     {
//         // Seed with current time ticks to guarantee different results on every reload.
//         Random.InitState((int)System.DateTime.Now.Ticks);
//         s_ColorForceRanges = null;
//     }

//     private void Awake()
//     {
//         if (s_ColorForceRanges == null)
//             InitializeForceRanges();

//         if (s_ColorForceRanges.TryGetValue(pegColor, out Vector2 range))
//         {
//             minForce = range.x;
//             maxForce = range.y;
//         }
//     }

//     /// <summary>Generates two distinct non-overlapping force ranges and assigns them to Green and Red.</summary>
//     private static void InitializeForceRanges()
//     {
//         s_ColorForceRanges = new Dictionary<PegColor, Vector2>();

//         float minA = Random.Range(0f, 1f - 2f * FORCE_RANGE_SPAN);
//         float maxA = minA + FORCE_RANGE_SPAN;

//         float minB = Random.Range(maxA, 1f - FORCE_RANGE_SPAN);
//         float maxB = minB + FORCE_RANGE_SPAN;

//         bool greenIsLower = Random.value > 0.5f;

//         s_ColorForceRanges[PegColor.Green] = greenIsLower ? new Vector2(minA, maxA) : new Vector2(minB, maxB);
//         s_ColorForceRanges[PegColor.Red]   = greenIsLower ? new Vector2(minB, maxB) : new Vector2(minA, maxA);

//         Debug.Log($"[AutomaticPegIdentifier] Green: {s_ColorForceRanges[PegColor.Green].x:F2} – {s_ColorForceRanges[PegColor.Green].y:F2}");
//         Debug.Log($"[AutomaticPegIdentifier] Red:   {s_ColorForceRanges[PegColor.Red].x:F2} – {s_ColorForceRanges[PegColor.Red].y:F2}");
//     }

//     /// <summary>Returns the randomized force range for the given color this session.</summary>
//     public static Vector2 GetColorForceRange(PegColor color)
//     {
//         if (s_ColorForceRanges != null && s_ColorForceRanges.TryGetValue(color, out Vector2 range))
//             return range;

//         return Vector2.zero;
//     }

//     [Header("Peg Settings")]
//     [Tooltip("The color type of this peg")]
//     public PegColor pegColor = PegColor.Green;

//     [HideInInspector] public float minForce = 0.2f;
//     [HideInInspector] public float maxForce = 0.4f;
//     [HideInInspector] public float currentForce = 0f;

//     private void OnValidate()
//     {
//         if (minForce > maxForce)
//             minForce = maxForce;
//     }

//     /// <summary>Returns the color type of this peg.</summary>
//     public PegColor GetPegColor() => pegColor;

//     /// <summary>Returns the current force applied to this peg.</summary>
//     public float GetCurrentForce() => currentForce;

//     /// <summary>Returns the valid force range as (min, max).</summary>
//     public Vector2 GetForceRange() => new Vector2(minForce, maxForce);

//     /// <summary>Returns the minimum required force for this peg.</summary>
//     public float GetMinForce() => minForce;

//     /// <summary>Returns the maximum allowed force for this peg.</summary>
//     public float GetMaxForce() => maxForce;

//     /// <summary>Updates the current force from a trigger input value (0–1).</summary>
//     public void UpdateForceFromTrigger(float triggerValue)
//     {
//         currentForce = Mathf.Clamp01(triggerValue);
//     }

//     /// <summary>Returns true if the current force falls within the valid range.</summary>
//     public bool IsCurrentForceValid()
//     {
//         return currentForce >= minForce && currentForce <= maxForce;
//     }
// }


// using System.Collections.Generic;
// using UnityEngine;

// public enum PegColor
// {
//     Green,
//     Red
// }

// public class AutomaticPegIdentifier : MonoBehaviour, IPegIdentifier
// {
//     private const float MIN_SPAN    = 0.10f;
//     private const float MAX_SPAN    = 0.35f;
//     private const float MIN_GAP     = 0.10f;
//     private const float MAX_GAP     = 0.30f;

//     private static Dictionary<PegColor, Vector2> s_ColorForceRanges;

//     /// <summary>
//     /// Called by TrialManager.Awake() on every scene load.
//     /// Generates two non-overlapping ranges with variable spans and a random gap,
//     /// allowing one color to be very low and the other very high on the 0–1 scale.
//     /// </summary>
//     public static void Reinitialize()
//     {
//         float spanA = Random.Range(MIN_SPAN, MAX_SPAN);
//         float spanB = Random.Range(MIN_SPAN, MAX_SPAN);
//         float gap   = Random.Range(MIN_GAP, MAX_GAP);

//         float totalNeeded = spanA + gap + spanB;

//         // If spans + gap exceed the 0–1 space, scale the gap down to fit.
//         if (totalNeeded > 1f)
//             gap = Mathf.Max(MIN_GAP, 1f - spanA - spanB);

//         float maxMinA = 1f - spanA - gap - spanB;

//         // Safety clamp — should never be negative given MIN_SPAN/GAP constants.
//         maxMinA = Mathf.Max(0f, maxMinA);

//         float minA = Random.Range(0f, maxMinA);
//         float maxA = minA + spanA;

//         float minBStart = maxA + gap;
//         float minBEnd   = Mathf.Max(minBStart, 1f - spanB);
//         float minB      = Random.Range(minBStart, minBEnd);
//         float maxB      = minB + spanB;

//         // Clamp to [0, 1] as a final safety net.
//         maxB = Mathf.Min(maxB, 1f);

//         // Randomly assign which color gets the lower range.
//         bool greenIsLower = Random.value > 0.5f;

//         s_ColorForceRanges = new Dictionary<PegColor, Vector2>
//         {
//             [PegColor.Green] = greenIsLower ? new Vector2(minA, maxA) : new Vector2(minB, maxB),
//             [PegColor.Red]   = greenIsLower ? new Vector2(minB, maxB) : new Vector2(minA, maxA)
//         };

//         Debug.Log($"[AutomaticPegIdentifier] Green: {s_ColorForceRanges[PegColor.Green].x:F2} – {s_ColorForceRanges[PegColor.Green].y:F2} (span: {spanA:F2})");
//         Debug.Log($"[AutomaticPegIdentifier] Red:   {s_ColorForceRanges[PegColor.Red].x:F2} – {s_ColorForceRanges[PegColor.Red].y:F2} (span: {spanB:F2})");
//         Debug.Log($"[AutomaticPegIdentifier] Gap between ranges: {gap:F2}");
//     }

//     /// <summary>Returns the randomized force range for the given color this session.</summary>
//     public static Vector2 GetColorForceRange(PegColor color)
//     {
//         if (s_ColorForceRanges != null && s_ColorForceRanges.TryGetValue(color, out Vector2 range))
//             return range;

//         return Vector2.zero;
//     }

//     [Header("Peg Settings")]
//     [Tooltip("The color type of this peg")]
//     public PegColor pegColor = PegColor.Green;

//     [HideInInspector] public float minForce = 0.2f;
//     [HideInInspector] public float maxForce = 0.4f;
//     [HideInInspector] public float currentForce = 0f;

//     // Start() is used instead of Awake() so TrialManager.Awake() always
//     // calls Reinitialize() first — all Awake() calls complete before any Start().
//     private void Start()
//     {
//         if (s_ColorForceRanges != null && s_ColorForceRanges.TryGetValue(pegColor, out Vector2 range))
//         {
//             minForce = range.x;
//             maxForce = range.y;
//         }
//     }

//     private void OnValidate()
//     {
//         if (minForce > maxForce)
//             minForce = maxForce;
//     }

//     /// <summary>Returns the color type of this peg.</summary>
//     public PegColor GetPegColor() => pegColor;

//     /// <summary>Returns the current force applied to this peg.</summary>
//     public float GetCurrentForce() => currentForce;

//     /// <summary>Returns the valid force range as (min, max).</summary>
//     public Vector2 GetForceRange() => new Vector2(minForce, maxForce);

//     /// <summary>Returns the minimum required force for this peg.</summary>
//     public float GetMinForce() => minForce;

//     /// <summary>Returns the maximum allowed force for this peg.</summary>
//     public float GetMaxForce() => maxForce;

//     /// <summary>Updates the current force from a trigger input value (0–1).</summary>
//     public void UpdateForceFromTrigger(float triggerValue)
//     {
//         currentForce = Mathf.Clamp01(triggerValue);
//     }

//     /// <summary>Returns true if the current force falls within the valid range.</summary>
//     public bool IsCurrentForceValid()
//     {
//         return currentForce >= minForce && currentForce <= maxForce;
//     }
// }


using System.Collections.Generic;
using UnityEngine;

public enum PegColor
{
    Green,
    Red
}

public class AutomaticPegIdentifier : MonoBehaviour, IPegIdentifier
{
    private const float SPAN    = 0.20f;
    private const float MIN_GAP = 0.05f;
    private const float MAX_GAP = 0.50f;

    private static Dictionary<PegColor, Vector2> s_ColorForceRanges;

    /// <summary>
    /// Called by TrialManager.Awake() on every scene load.
    /// Both ranges are always exactly 0.2 wide, positioned randomly across 0–1
    /// with a random gap between them so they are never close to each other.
    /// </summary>
    public static void Reinitialize()
    {
        float gap = Random.Range(MIN_GAP, MAX_GAP);

        // Place A anywhere that leaves room for the gap and B after it.
        float minA = Random.Range(0f, 1f - SPAN - gap - SPAN);
        float maxA = minA + SPAN;

        // Place B anywhere after A + gap.
        float minB = Random.Range(maxA + gap, 1f - SPAN);
        float maxB = minB + SPAN;

        // Randomly assign which color gets the lower range.
        bool greenIsLower = Random.value > 0.5f;

        s_ColorForceRanges = new Dictionary<PegColor, Vector2>
        {
            [PegColor.Green] = greenIsLower ? new Vector2(minA, maxA) : new Vector2(minB, maxB),
            [PegColor.Red]   = greenIsLower ? new Vector2(minB, maxB) : new Vector2(minA, maxA)
        };

        Debug.Log($"[AutomaticPegIdentifier] Green: {s_ColorForceRanges[PegColor.Green].x:F2} – {s_ColorForceRanges[PegColor.Green].y:F2}");
        Debug.Log($"[AutomaticPegIdentifier] Red:   {s_ColorForceRanges[PegColor.Red].x:F2} – {s_ColorForceRanges[PegColor.Red].y:F2}");
        Debug.Log($"[AutomaticPegIdentifier] Gap: {gap:F2}");
    }

    /// <summary>Returns the randomized force range for the given color this session.</summary>
    public static Vector2 GetColorForceRange(PegColor color)
    {
        if (s_ColorForceRanges != null && s_ColorForceRanges.TryGetValue(color, out Vector2 range))
            return range;

        return Vector2.zero;
    }

    [Header("Peg Settings")]
    [Tooltip("The color type of this peg")]
    public PegColor pegColor = PegColor.Green;

    [HideInInspector] public float minForce = 0.2f;
    [HideInInspector] public float maxForce = 0.4f;
    [HideInInspector] public float currentForce = 0f;

    // Start() is used instead of Awake() so TrialManager.Awake() always
    // calls Reinitialize() first — all Awake() calls complete before any Start().
    private void Start()
    {
        if (s_ColorForceRanges != null && s_ColorForceRanges.TryGetValue(pegColor, out Vector2 range))
        {
            minForce = range.x;
            maxForce = range.y;
        }
    }

    private void OnValidate()
    {
        if (minForce > maxForce)
            minForce = maxForce;
    }

    /// <summary>Returns the color type of this peg.</summary>
    public PegColor GetPegColor() => pegColor;

    /// <summary>Returns the current force applied to this peg.</summary>
    public float GetCurrentForce() => currentForce;

    /// <summary>Returns the valid force range as (min, max).</summary>
    public Vector2 GetForceRange() => new Vector2(minForce, maxForce);

    /// <summary>Returns the minimum required force for this peg.</summary>
    public float GetMinForce() => minForce;

    /// <summary>Returns the maximum allowed force for this peg.</summary>
    public float GetMaxForce() => maxForce;

    /// <summary>Updates the current force from a trigger input value (0–1).</summary>
    public void UpdateForceFromTrigger(float triggerValue)
    {
        currentForce = Mathf.Clamp01(triggerValue);
    }

    /// <summary>Returns true if the current force falls within the valid range.</summary>
    public bool IsCurrentForceValid()
    {
        return currentForce >= minForce && currentForce <= maxForce;
    }
}
