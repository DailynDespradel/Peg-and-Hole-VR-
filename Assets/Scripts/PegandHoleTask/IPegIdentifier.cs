using UnityEngine;

/// <summary>Shared contract for all peg identifier components.</summary>
public interface IPegIdentifier
{
    PegColor GetPegColor();
    float GetCurrentForce();
    Vector2 GetForceRange();
    float GetMinForce();
    float GetMaxForce();
    void UpdateForceFromTrigger(float triggerValue);
    bool IsCurrentForceValid();
}
