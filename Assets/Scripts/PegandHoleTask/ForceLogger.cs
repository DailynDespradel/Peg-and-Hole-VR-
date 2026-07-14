using System.IO;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ForceLogger : MonoBehaviour
{
    private static readonly string LOG_FILE_PATH =
        "/Data/ForceLog_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".csv";

    [Header("Scene References")]
    [Tooltip("Assign all XRGrabInteractable pegs in the scene")]
    [SerializeField] private XRGrabInteractable[] pegs;

    [Tooltip("Assign all PegHoleInsertionForceValidated holes in the scene")]
    [SerializeField] private PegHoleInsertionForceValidated[] holes;

    [Tooltip("The TrialManager in the scene")]
    [SerializeField] private TrialManager trialManager;

    [Tooltip("How often to sample force while a peg is held, in seconds (0.05 = 20Hz)")]
    [SerializeField] private float sampleInterval = 0.05f;

    // ── State ──────────────────────────────────────────────────────────────

    private AutomaticPegIdentifier _heldPegIdentifier;
    private AutomaticPegIdentifier _lastHeldPegIdentifier;
    private float _sampleTimer;
    private StreamWriter _writer;

    // ── Unity lifecycle ────────────────────────────────────────────────────

    private void Start()
    {
        string fullPath  = Application.dataPath + LOG_FILE_PATH;
        string directory = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Debug.Log($"[ForceLogger] Created directory: {directory}");
        }

        try
        {
            bool isNewFile = !File.Exists(fullPath) || new FileInfo(fullPath).Length == 0;
            //_writer = new StreamWriter(fullPath, append: true);
            FileStream stream = new FileStream(fullPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            _writer = new StreamWriter(stream);

            if (isNewFile)
            {
                _writer.WriteLine("timeSec,trialIndex,configurationIndex,pegColor,force,event");
                _writer.Flush();
            }

            Debug.Log($"[ForceLogger] Logging to: {fullPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ForceLogger] Failed to open log file: {e.Message}");
            return;
        }

        int pegCount  = 0;
        int holeCount = 0;

        foreach (XRGrabInteractable peg in pegs)
        {
            if (peg != null)
            {
                peg.selectEntered.AddListener(OnPegPickedUp);
                peg.selectExited.AddListener(OnPegReleased);
                pegCount++;
            }
        }

        foreach (PegHoleInsertionForceValidated hole in holes)
        {
            if (hole != null)
            {
                hole.OnPegSnapped += OnPegPlaced;
                holeCount++;
            }
        }

        Debug.Log($"[ForceLogger] Subscribed to {pegCount} pegs and {holeCount} holes.");

        if (trialManager == null)
            Debug.LogWarning("[ForceLogger] TrialManager reference is not assigned — trial/config columns will be 0.");

        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }

    private void OnDestroy()
    {
        _writer?.Flush();
        _writer?.Close();

        foreach (XRGrabInteractable peg in pegs)
        {
            if (peg != null)
            {
                peg.selectEntered.RemoveListener(OnPegPickedUp);
                peg.selectExited.RemoveListener(OnPegReleased);
            }
        }

        foreach (PegHoleInsertionForceValidated hole in holes)
        {
            if (hole != null)
                hole.OnPegSnapped -= OnPegPlaced;
        }

        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }

    private void Update()
    {
        if (_heldPegIdentifier == null) return;

        _sampleTimer += Time.deltaTime;
        if (_sampleTimer < sampleInterval) return;

        _sampleTimer = 0f;
        WriteRow(_heldPegIdentifier, string.Empty);
    }

    // ── Peg events ─────────────────────────────────────────────────────────

    private void OnPegPickedUp(SelectEnterEventArgs args)
    {
        // Search the whole hierarchy in case AutomaticPegIdentifier is on a parent or child.
        AutomaticPegIdentifier identifier =
            args.interactableObject.transform.GetComponentInParent<AutomaticPegIdentifier>() ??
            args.interactableObject.transform.GetComponentInChildren<AutomaticPegIdentifier>();

        if (identifier == null)
        {
            Debug.LogWarning("[ForceLogger] Grabbed peg has no AutomaticPegIdentifier component.");
            return;
        }

        _heldPegIdentifier     = identifier;
        _lastHeldPegIdentifier = identifier;
        _sampleTimer           = 0f;

        WriteRow(identifier, "PickedUp");
        Debug.Log($"[ForceLogger] PickedUp: {identifier.GetPegColor()} peg.");
    }

    private void OnPegReleased(SelectExitEventArgs args)
    {
        // Keep _lastHeldPegIdentifier alive so OnPegPlaced can still reference it.
        _heldPegIdentifier = null;
    }

    /// <summary>
    /// Fires after a peg snaps into a hole. Uses _lastHeldPegIdentifier
    /// because selectExited clears _heldPegIdentifier before this fires.
    /// </summary>
    private void OnPegPlaced(string holeName, PegColor pegColor)
    {
        if (_lastHeldPegIdentifier != null)
        {
            WriteRow(_lastHeldPegIdentifier, "Placed");
            Debug.Log($"[ForceLogger] Placed: {pegColor} peg in {holeName}.");
        }
    }

    // ── CSV writing ────────────────────────────────────────────────────────

    /// <summary>Writes a single row to the CSV.</summary>
    private void WriteRow(AutomaticPegIdentifier identifier, string eventLabel)
    {
        if (_writer == null) return;

        int    trialIndex  = trialManager != null ? trialManager.CurrentTrialIndex  : 0;
        int    configIndex = trialManager != null ? trialManager.CurrentConfigIndex : 0;
        float  force       = identifier.GetCurrentForce();
        string color       = identifier.GetPegColor().ToString();

        _writer.WriteLine($"{Time.time:F3},{trialIndex},{configIndex},{color},{force:F4},{eventLabel}");
        _writer.Flush();

        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }
}
