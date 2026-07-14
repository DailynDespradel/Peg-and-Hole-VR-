using UnityEngine;

// public enum PegColor 
// { 
//     Green, 
//     Red 
// }

public class PegIdentifier : MonoBehaviour, IPegIdentifier

{
    [Header("Peg Settings")]
    [Tooltip("The color type of this peg")]
    public PegColor pegColor = PegColor.Green;
    
    [Header("Force Range Requirements")]
    [Tooltip("Minimum force required for this peg")]
    [Range(0f, 1f)]
    public float minForce = 0.2f;
    
    [Tooltip("Maximum force allowed for this peg")]
    [Range(0f, 1f)]
    public float maxForce = 0.4f;
    
    [HideInInspector]
    public float currentForce = 0f;
    
    void OnValidate()
    {
        if (minForce > maxForce)
        {
            minForce = maxForce;
        }
    }
    
    public PegColor GetPegColor()
    {
        return pegColor;
    }
    
    public float GetCurrentForce()
    {
        return currentForce;
    }
    
    public Vector2 GetForceRange()
    {
        return new Vector2(minForce, maxForce);
    }
    
    public float GetMinForce()
    {
        return minForce;
    }
    
    public float GetMaxForce()
    {
        return maxForce;
    }
    
    public void UpdateForceFromTrigger(float triggerValue)
    {
        currentForce = Mathf.Clamp01(triggerValue);
    }
    
    public bool IsCurrentForceValid()
    {
        return currentForce >= minForce && currentForce <= maxForce;
    }
}
