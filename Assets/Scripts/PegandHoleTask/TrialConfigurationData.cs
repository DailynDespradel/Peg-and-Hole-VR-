using UnityEngine;

/// <summary>
/// Defines which peg color is expected in each hole for one trial configuration.
/// Create one asset per configuration via Assets > Create > Trial > Configuration Data.
/// </summary>
[CreateAssetMenu(fileName = "TrialConfigurationData", menuName = "Trial/Configuration Data")]
public class TrialConfigurationData : ScriptableObject
{
    [System.Serializable]
    public class HoleAssignment
    {
        [Tooltip("Must match the hole's GameObject name exactly, e.g. Hole3")]
        public string holeName;

        [Tooltip("The peg color expected in this hole for this configuration")]
        public PegColor expectedColor;
    }

    [Tooltip("All 10 hole assignments for this configuration (5 Red + 5 Green)")]
    public HoleAssignment[] holeAssignments;

    /// <summary>
    /// Returns the expected color for the given hole name, or null if the hole
    /// is not part of this configuration (i.e. should be empty).
    /// </summary>
    public PegColor? GetExpectedColor(string holeName)
    {
        foreach (HoleAssignment assignment in holeAssignments)
        {
            if (assignment.holeName == holeName)
                return assignment.expectedColor;
        }

        return null; // hole is not used in this configuration
    }
}
