using UnityEngine;

public static class ForceValidator
{
    /// <summary>Returns true if force is within [minForce, maxForce].</summary>
    public static bool IsForceValid(float force, float minForce, float maxForce)
    {
        return force >= minForce && force <= maxForce;
    }

    /// <summary>Returns true if force is within the given range vector.</summary>
    public static bool IsForceValid(float force, Vector2 forceRange)
    {
        return force >= forceRange.x && force <= forceRange.y;
    }

    /// <summary>Works with either PegIdentifier or AutomaticPegIdentifier.</summary>
    public static bool IsForceValid(IPegIdentifier pegIdentifier)
    {
        if (pegIdentifier == null) return false;
        return IsForceValid(pegIdentifier.GetCurrentForce(), pegIdentifier.GetMinForce(), pegIdentifier.GetMaxForce());
    }

    /// <summary>Returns a formatted "min-max" range string.</summary>
    public static string GetForceRangeText(float minForce, float maxForce)
    {
        return $"{minForce:F2}-{maxForce:F2}";
    }

    /// <summary>Returns a formatted range string from a range vector.</summary>
    public static string GetForceRangeText(Vector2 forceRange)
    {
        return GetForceRangeText(forceRange.x, forceRange.y);
    }

    /// <summary>Works with either PegIdentifier or AutomaticPegIdentifier.</summary>
    public static string GetForceRangeText(IPegIdentifier pegIdentifier)
    {
        if (pegIdentifier == null) return "Unknown";
        return GetForceRangeText(pegIdentifier.GetMinForce(), pegIdentifier.GetMaxForce());
    }
}
