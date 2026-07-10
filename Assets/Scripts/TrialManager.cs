
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;
// using UnityEngine.XR.Interaction.Toolkit;

// public class TrialManager : MonoBehaviour
// {
//     private static readonly string LOG_FILE_PATH = 
//     "/Data/TrialLog_" + System.DateTime.Now.ToString("yyyy-MM-dd") + ".json";

//     private const float SURVEY_TO_RESTART_DELAY = 5f;

//     [Header("Configurations")]
//     [Tooltip("Assign Configutation1 through Configutation4 GameObjects")]
//     [SerializeField] private GameObject[] configurations;

//     [Tooltip("The root Configurations GameObject — hidden when survey appears")]
//     [SerializeField] private GameObject configurationsRoot;

//     [Header("Survey")]
//     [Tooltip("The Survey canvas GameObject — shown after trial ends")]
//     [SerializeField] private GameObject surveyCanvas;

//     [Tooltip("The Red toggle on the Survey canvas")]
//     [SerializeField] private Toggle redToggle;

//     [Tooltip("The Green toggle on the Survey canvas")]
//     [SerializeField] private Toggle greenToggle;

//     [Tooltip("Ray Interactors to disable during the survey")]
//     [SerializeField] private XRRayInteractor[] rayInteractors;

//     [Header("Scene References")]
//     [Tooltip("Assign all PegHoleInsertionForceValidated holes in the scene")]
//     [SerializeField] private PegHoleInsertionForceValidated[] holes;

//     [Tooltip("Assign all XRGrabInteractable pegs in the scene")]
//     [SerializeField] private XRGrabInteractable[] pegs;

//     [Tooltip("How many pegs must be inserted for the trial to end (5 red + 5 green = 10)")]
//     [SerializeField] private int requiredInsertions = 10;

//     // ── Trial state ────────────────────────────────────────────────────────

//     private int _selectedConfigIndex;
//     private float _trialStartTime;
//     private float _lastGrabTime;
//     private bool _trialStarted;
//     private bool _trialComplete;
//     private int _insertedCount;

//     private TrialRecord _pendingRecord;

//     // ── JSON data structures ───────────────────────────────────────────────

//     [System.Serializable]
//     private class ForceRangeEntry
//     {
//         public float min;
//         public float max;
//     }

//     [System.Serializable]
//     private class InsertionEvent
//     {
//         public string holeName;
//         public string pegColor;
//         public float grabToPlacementSeconds;
//     }

//     [System.Serializable]
//     private class TrialRecord
//     {
//         public int trialIndex;
//         public string timestamp;
//         public int configurationIndex;
//         public float completionTimeSeconds;
//         public ForceRangeEntry greenForceRange;
//         public ForceRangeEntry redForceRange;
//         public string surveyAnswer;
//         public List<InsertionEvent> insertions = new List<InsertionEvent>();
//     }

//     [System.Serializable]
//     private class TrialLog
//     {
//         public List<TrialRecord> trials = new List<TrialRecord>();
//     }

//     // ── Unity lifecycle ────────────────────────────────────────────────────

//     private void Start()
//     {
//         // Re-seed to guarantee a different configuration each reload.
//         Random.InitState((int)(System.DateTime.Now.Ticks >> 16));

//         if (surveyCanvas != null)
//             surveyCanvas.SetActive(false);

//         SetupSurveyToggles();
//         SelectRandomConfiguration();
//         SubscribeToHoleEvents();
//         SubscribeToPegGrabEvents();
//     }

//     private void OnDestroy()
//     {
//         UnsubscribeFromHoleEvents();
//         UnsubscribeFromPegGrabEvents();

//         if (redToggle != null)   redToggle.onValueChanged.RemoveListener(OnRedToggleChanged);
//         if (greenToggle != null) greenToggle.onValueChanged.RemoveListener(OnGreenToggleChanged);

//         SetRayInteractorUIEnabled(true);
//     }

//     // ── Configuration selection ────────────────────────────────────────────

//     /// <summary>Randomly activates one configuration canvas and hides the rest.</summary>
//     private void SelectRandomConfiguration()
//     {
//         if (configurations == null || configurations.Length == 0)
//         {
//             Debug.LogError("[TrialManager] No configurations assigned!");
//             return;
//         }

//         _selectedConfigIndex = Random.Range(0, configurations.Length);

//         for (int i = 0; i < configurations.Length; i++)
//         {
//             if (configurations[i] != null)
//                 configurations[i].SetActive(i == _selectedConfigIndex);
//         }

//         Debug.Log($"[TrialManager] Configuration {_selectedConfigIndex + 1} selected.");
//     }

//     // ── Survey setup ───────────────────────────────────────────────────────

//     private void SetupSurveyToggles()
//     {
//         if (redToggle != null)
//         {
//             redToggle.isOn = false;
//             redToggle.onValueChanged.AddListener(OnRedToggleChanged);
//         }

//         if (greenToggle != null)
//         {
//             greenToggle.isOn = false;
//             greenToggle.onValueChanged.AddListener(OnGreenToggleChanged);
//         }
//     }

//     /// <summary>Called when the Red toggle changes.</summary>
//     private void OnRedToggleChanged(bool isOn)
//     {
//         if (!isOn) return;

//         if (greenToggle != null)
//             greenToggle.SetIsOnWithoutNotify(false);

//         SaveSurveyAnswer("Red");
//     }

//     /// <summary>Called when the Green toggle changes.</summary>
//     private void OnGreenToggleChanged(bool isOn)
//     {
//         if (!isOn) return;

//         if (redToggle != null)
//             redToggle.SetIsOnWithoutNotify(false);

//         SaveSurveyAnswer("Green");
//     }

//     // ── Ray interactor control ─────────────────────────────────────────────

//     private void SetRayInteractorUIEnabled(bool enabled)
//     {
//         if (rayInteractors == null) return;

//         foreach (XRRayInteractor ray in rayInteractors)
//         {
//             if (ray != null)
//                 ray.enableUIInteraction = enabled;
//         }
//     }

//     // ── Event wiring ───────────────────────────────────────────────────────

//     private void SubscribeToPegGrabEvents()
//     {
//         foreach (XRGrabInteractable peg in pegs)
//         {
//             if (peg != null)
//                 peg.selectEntered.AddListener(OnAnyPegGrabbed);
//         }
//     }

//     private void UnsubscribeFromPegGrabEvents()
//     {
//         foreach (XRGrabInteractable peg in pegs)
//         {
//             if (peg != null)
//                 peg.selectEntered.RemoveListener(OnAnyPegGrabbed);
//         }
//     }

//     private void SubscribeToHoleEvents()
//     {
//         foreach (PegHoleInsertionForceValidated hole in holes)
//         {
//             if (hole != null)
//                 hole.OnPegSnapped += OnPegInserted;
//         }
//     }

//     private void UnsubscribeFromHoleEvents()
//     {
//         foreach (PegHoleInsertionForceValidated hole in holes)
//         {
//             if (hole != null)
//                 hole.OnPegSnapped -= OnPegInserted;
//         }
//     }

//     // ── Timer & grab tracking ──────────────────────────────────────────────

//     /// <summary>
//     /// Starts the overall trial timer on the first grab.
//     /// Keeps listening for every subsequent grab to track grab time per peg.
//     /// </summary>
//     private void OnAnyPegGrabbed(SelectEnterEventArgs args)
//     {
//         if (!_trialStarted)
//         {
//             _trialStarted = true;
//             _trialStartTime = Time.time;
//             Debug.Log("[TrialManager] Timer started — first peg grabbed.");
//         }

//         // Always record the moment this grab happened for grab→placement measurement.
//         _lastGrabTime = Time.time;
//     }

//     /// <summary>Records hole, color, and grab→placement duration for every successful snap.</summary>
//     private void OnPegInserted(string holeName, PegColor pegColor)
//     {
//         if (_trialComplete) return;

//         float grabToPlacement = Time.time - _lastGrabTime;

//         if (_pendingRecord == null)
//             _pendingRecord = CreatePendingRecord(0f);

//         _pendingRecord.insertions.Add(new InsertionEvent
//         {
//             holeName               = holeName,
//             pegColor               = pegColor.ToString(),
//             grabToPlacementSeconds = grabToPlacement
//         });

//         Debug.Log($"[TrialManager] {pegColor} peg → {holeName} | grab→placement: {grabToPlacement:F2}s");

//         _insertedCount++;

//         if (_insertedCount >= requiredInsertions)
//             CompleteTrial();
//     }

//     // ── Trial completion ───────────────────────────────────────────────────

//     private void CompleteTrial()
//     {
//         _trialComplete = true;

//         float elapsed = _trialStarted ? Time.time - _trialStartTime : 0f;

//         // Stop tracking grabs and lock all remaining pegs.
//         UnsubscribeFromPegGrabEvents();

//         foreach (XRGrabInteractable peg in pegs)
//         {
//             if (peg != null)
//                 peg.enabled = false;
//         }

//         SetRayInteractorUIEnabled(false);

//         if (configurationsRoot != null)
//             configurationsRoot.SetActive(false);

//         if (surveyCanvas != null)
//             surveyCanvas.SetActive(true);

//         if (_pendingRecord == null)
//             _pendingRecord = CreatePendingRecord(elapsed);
//         else
//             _pendingRecord.completionTimeSeconds = elapsed;

//         Debug.Log($"[TrialManager] Trial complete in {elapsed:F2}s — awaiting survey response.");
//     }

//     // ── Record helpers ─────────────────────────────────────────────────────

//     private TrialRecord CreatePendingRecord(float elapsed)
//     {
//         Vector2 greenRange = AutomaticPegIdentifier.GetColorForceRange(PegColor.Green);
//         Vector2 redRange   = AutomaticPegIdentifier.GetColorForceRange(PegColor.Red);

//         return new TrialRecord
//         {
//             trialIndex            = GetNextTrialIndex(),
//             timestamp             = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
//             configurationIndex    = _selectedConfigIndex + 1,
//             completionTimeSeconds = elapsed,
//             greenForceRange       = new ForceRangeEntry { min = greenRange.x, max = greenRange.y },
//             redForceRange         = new ForceRangeEntry { min = redRange.x,   max = redRange.y  },
//             surveyAnswer          = "Unanswered",
//             insertions            = new List<InsertionEvent>()
//         };
//     }

//     // ── Survey logging & restart ───────────────────────────────────────────

//     /// <summary>Saves the survey answer, writes to disk, then begins the restart countdown.</summary>
//     private void SaveSurveyAnswer(string answer)
//     {
//         if (_pendingRecord == null)
//         {
//             Debug.LogWarning("[TrialManager] No pending trial record to attach survey answer to.");
//             return;
//         }

//         _pendingRecord.surveyAnswer = answer;
//         WriteTrialToLog(_pendingRecord);

//         if (redToggle != null)   redToggle.interactable = false;
//         if (greenToggle != null) greenToggle.interactable = false;

//         Debug.Log($"[TrialManager] Survey answer saved: {answer} — restarting in {SURVEY_TO_RESTART_DELAY}s.");

//         StartCoroutine(RestartAfterDelay());
//     }

//     private int GetNextTrialIndex()
//     {
//         string fullPath = Application.dataPath + LOG_FILE_PATH;
//         if (!File.Exists(fullPath)) return 1;

//         TrialLog log = JsonUtility.FromJson<TrialLog>(File.ReadAllText(fullPath));
//         return log != null ? log.trials.Count + 1 : 1;
//     }

//     private void WriteTrialToLog(TrialRecord record)
//     {
//         string fullPath = Application.dataPath + LOG_FILE_PATH;

//         TrialLog log;

//         if (File.Exists(fullPath))
//         {
//             string existingJson = File.ReadAllText(fullPath);
//             log = JsonUtility.FromJson<TrialLog>(existingJson) ?? new TrialLog();
//         }
//         else
//         {
//             log = new TrialLog();
//         }

//         int existingIndex = log.trials.FindIndex(t => t.trialIndex == record.trialIndex);

//         if (existingIndex >= 0)
//             log.trials[existingIndex] = record;
//         else
//             log.trials.Add(record);

//         File.WriteAllText(fullPath, JsonUtility.ToJson(log, prettyPrint: true));

//         Debug.Log($"[TrialManager] Trial {record.trialIndex} written to {fullPath}");
//     }

//     // ── Scene restart ──────────────────────────────────────────────────────

//     /// <summary>Counts down then reloads the scene with fresh force ranges and configuration.</summary>
//     private IEnumerator RestartAfterDelay()
//     {
//         float remaining = SURVEY_TO_RESTART_DELAY;

//         while (remaining > 0f)
//         {
//             Debug.Log($"[TrialManager] New trial starting in {remaining:F0}s...");
//             yield return new WaitForSeconds(1f);
//             remaining -= 1f;
//         }

//         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//     }
// }

// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;
// using UnityEngine.XR.Interaction.Toolkit;

// public class TrialManager : MonoBehaviour
// {
//     private static readonly string LOG_FILE_PATH =
//         "/Data/TrialLog_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".json";


//     private const float SURVEY_TO_RESTART_DELAY = 5f;

//     [Header("Configurations")]
//     [Tooltip("Assign Configutation1 through Configutation4 GameObjects")]
//     [SerializeField] private GameObject[] configurations;

//     [Tooltip("The root Configurations GameObject — hidden when survey appears")]
//     [SerializeField] private GameObject configurationsRoot;

//     [Header("Survey")]
//     [Tooltip("The Survey canvas GameObject — shown after trial ends")]
//     [SerializeField] private GameObject surveyCanvas;

//     [Tooltip("The Red toggle on the Survey canvas")]
//     [SerializeField] private Toggle redToggle;

//     [Tooltip("The Green toggle on the Survey canvas")]
//     [SerializeField] private Toggle greenToggle;

//     [Tooltip("Ray Interactors to disable during the survey")]
//     [SerializeField] private XRRayInteractor[] rayInteractors;

//     [Header("Scene References")]
//     [Tooltip("Assign all PegHoleInsertionForceValidated holes in the scene")]
//     [SerializeField] private PegHoleInsertionForceValidated[] holes;

//     [Tooltip("Assign all XRGrabInteractable pegs in the scene")]
//     [SerializeField] private XRGrabInteractable[] pegs;

//     [Tooltip("How many pegs must be inserted for the trial to end (5 red + 5 green = 10)")]
//     [SerializeField] private int requiredInsertions = 10;

//     // ── Trial state ────────────────────────────────────────────────────────

//     private int _selectedConfigIndex;
//     private float _trialStartTime;
//     private float _lastGrabTime;
//     private bool _trialStarted;
//     private bool _trialComplete;
//     private int _insertedCount;

//     private TrialRecord _pendingRecord;

//     // ── JSON data structures ───────────────────────────────────────────────

//     [System.Serializable]
//     private class ForceRangeEntry
//     {
//         public float min;
//         public float max;
//     }

//     [System.Serializable]
//     private class InsertionEvent
//     {
//         public string holeName;
//         public string pegColor;
//         public float grabToPlacementSeconds;
//     }

//     [System.Serializable]
//     private class TrialRecord
//     {
//         public int trialIndex;
//         public string timestamp;
//         public int configurationIndex;
//         public float completionTimeSeconds;
//         public ForceRangeEntry greenForceRange;
//         public ForceRangeEntry redForceRange;
//         public string surveyAnswer;
//         public List<InsertionEvent> insertions = new List<InsertionEvent>();
//     }

//     [System.Serializable]
//     private class TrialLog
//     {
//         public List<TrialRecord> trials = new List<TrialRecord>();
//     }

//     // ── Unity lifecycle ────────────────────────────────────────────────────

//     private void Awake()
//     {
//         // Seed with current time so every scene reload produces different values.
//         Random.InitState((int)System.DateTime.Now.Ticks);

//         // Reinitialize runs here — in Awake — so all AutomaticPegIdentifier.Start()
//         // calls (which happen after all Awake() calls) pick up the fresh ranges.
//         AutomaticPegIdentifier.Reinitialize();
//     }

//     private void Start()
//     {
//         if (surveyCanvas != null)
//             surveyCanvas.SetActive(false);

//         SetupSurveyToggles();
//         SelectRandomConfiguration();
//         SubscribeToHoleEvents();
//         SubscribeToPegGrabEvents();
//     }

//     private void OnDestroy()
//     {
//         UnsubscribeFromHoleEvents();
//         UnsubscribeFromPegGrabEvents();

//         if (redToggle != null)   redToggle.onValueChanged.RemoveListener(OnRedToggleChanged);
//         if (greenToggle != null) greenToggle.onValueChanged.RemoveListener(OnGreenToggleChanged);

//         SetRayInteractorUIEnabled(true);
//     }

//     // ── Configuration selection ────────────────────────────────────────────

//     /// <summary>Randomly activates one configuration canvas and hides the rest.</summary>
//     private void SelectRandomConfiguration()
//     {
//         if (configurations == null || configurations.Length == 0)
//         {
//             Debug.LogError("[TrialManager] No configurations assigned!");
//             return;
//         }

//         _selectedConfigIndex = Random.Range(0, configurations.Length);

//         for (int i = 0; i < configurations.Length; i++)
//         {
//             if (configurations[i] != null)
//                 configurations[i].SetActive(i == _selectedConfigIndex);
//         }

//         Debug.Log($"[TrialManager] Configuration {_selectedConfigIndex + 1} selected.");
//     }

//     // ── Survey setup ───────────────────────────────────────────────────────

//     private void SetupSurveyToggles()
//     {
//         if (redToggle != null)
//         {
//             redToggle.isOn = false;
//             redToggle.onValueChanged.AddListener(OnRedToggleChanged);
//         }

//         if (greenToggle != null)
//         {
//             greenToggle.isOn = false;
//             greenToggle.onValueChanged.AddListener(OnGreenToggleChanged);
//         }
//     }

//     /// <summary>Called when the Red toggle changes.</summary>
//     private void OnRedToggleChanged(bool isOn)
//     {
//         if (!isOn) return;

//         if (greenToggle != null)
//             greenToggle.SetIsOnWithoutNotify(false);

//         SaveSurveyAnswer("Red");
//     }

//     /// <summary>Called when the Green toggle changes.</summary>
//     private void OnGreenToggleChanged(bool isOn)
//     {
//         if (!isOn) return;

//         if (redToggle != null)
//             redToggle.SetIsOnWithoutNotify(false);

//         SaveSurveyAnswer("Green");
//     }

//     // ── Ray interactor control ─────────────────────────────────────────────

//     private void SetRayInteractorUIEnabled(bool enabled)
//     {
//         if (rayInteractors == null) return;

//         foreach (XRRayInteractor ray in rayInteractors)
//         {
//             if (ray != null)
//                 ray.enableUIInteraction = enabled;
//         }
//     }

//     // ── Event wiring ───────────────────────────────────────────────────────

//     private void SubscribeToPegGrabEvents()
//     {
//         foreach (XRGrabInteractable peg in pegs)
//         {
//             if (peg != null)
//                 peg.selectEntered.AddListener(OnAnyPegGrabbed);
//         }
//     }

//     private void UnsubscribeFromPegGrabEvents()
//     {
//         foreach (XRGrabInteractable peg in pegs)
//         {
//             if (peg != null)
//                 peg.selectEntered.RemoveListener(OnAnyPegGrabbed);
//         }
//     }

//     private void SubscribeToHoleEvents()
//     {
//         foreach (PegHoleInsertionForceValidated hole in holes)
//         {
//             if (hole != null)
//                 hole.OnPegSnapped += OnPegInserted;
//         }
//     }

//     private void UnsubscribeFromHoleEvents()
//     {
//         foreach (PegHoleInsertionForceValidated hole in holes)
//         {
//             if (hole != null)
//                 hole.OnPegSnapped -= OnPegInserted;
//         }
//     }

//     // ── Timer & grab tracking ──────────────────────────────────────────────

//     /// <summary>
//     /// Starts the overall trial timer on the first grab.
//     /// Keeps tracking every subsequent grab for grab→placement measurement.
//     /// </summary>
//     private void OnAnyPegGrabbed(SelectEnterEventArgs args)
//     {
//         if (!_trialStarted)
//         {
//             _trialStarted = true;
//             _trialStartTime = Time.time;
//             Debug.Log("[TrialManager] Timer started — first peg grabbed.");
//         }

//         _lastGrabTime = Time.time;
//     }

//     /// <summary>Records hole, color, and grab→placement duration for every successful snap.</summary>
//     private void OnPegInserted(string holeName, PegColor pegColor)
//     {
//         if (_trialComplete) return;

//         float grabToPlacement = Time.time - _lastGrabTime;

//         if (_pendingRecord == null)
//             _pendingRecord = CreatePendingRecord(0f);

//         _pendingRecord.insertions.Add(new InsertionEvent
//         {
//             holeName               = holeName,
//             pegColor               = pegColor.ToString(),
//             grabToPlacementSeconds = grabToPlacement
//         });

//         Debug.Log($"[TrialManager] {pegColor} peg → {holeName} | grab→placement: {grabToPlacement:F2}s");

//         _insertedCount++;

//         if (_insertedCount >= requiredInsertions)
//             CompleteTrial();
//     }

//     // ── Trial completion ───────────────────────────────────────────────────

//     private void CompleteTrial()
//     {
//         _trialComplete = true;

//         float elapsed = _trialStarted ? Time.time - _trialStartTime : 0f;

//         UnsubscribeFromPegGrabEvents();

//         foreach (XRGrabInteractable peg in pegs)
//         {
//             if (peg != null)
//                 peg.enabled = false;
//         }

//         SetRayInteractorUIEnabled(false);

//         if (configurationsRoot != null)
//             configurationsRoot.SetActive(false);

//         if (surveyCanvas != null)
//             surveyCanvas.SetActive(true);

//         if (_pendingRecord == null)
//             _pendingRecord = CreatePendingRecord(elapsed);
//         else
//             _pendingRecord.completionTimeSeconds = elapsed;

//         Debug.Log($"[TrialManager] Trial complete in {elapsed:F2}s — awaiting survey response.");
//     }

//     // ── Record helpers ─────────────────────────────────────────────────────

//     private TrialRecord CreatePendingRecord(float elapsed)
//     {
//         Vector2 greenRange = AutomaticPegIdentifier.GetColorForceRange(PegColor.Green);
//         Vector2 redRange   = AutomaticPegIdentifier.GetColorForceRange(PegColor.Red);

//         return new TrialRecord
//         {
//             trialIndex            = GetNextTrialIndex(),
//             timestamp             = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
//             configurationIndex    = _selectedConfigIndex + 1,
//             completionTimeSeconds = elapsed,
//             greenForceRange       = new ForceRangeEntry { min = greenRange.x, max = greenRange.y },
//             redForceRange         = new ForceRangeEntry { min = redRange.x,   max = redRange.y  },
//             surveyAnswer          = "Unanswered",
//             insertions            = new List<InsertionEvent>()
//         };
//     }

//     // ── Survey logging & restart ───────────────────────────────────────────

//     /// <summary>Saves the survey answer, writes to disk, then begins the restart countdown.</summary>
//     private void SaveSurveyAnswer(string answer)
//     {
//         if (_pendingRecord == null)
//         {
//             Debug.LogWarning("[TrialManager] No pending trial record to attach survey answer to.");
//             return;
//         }

//         _pendingRecord.surveyAnswer = answer;
//         WriteTrialToLog(_pendingRecord);

//         if (redToggle != null)   redToggle.interactable = false;
//         if (greenToggle != null) greenToggle.interactable = false;

//         Debug.Log($"[TrialManager] Survey answer saved: {answer} — restarting in {SURVEY_TO_RESTART_DELAY}s.");

//         StartCoroutine(RestartAfterDelay());
//     }

//     private int GetNextTrialIndex()
//     {
//         string fullPath = Application.dataPath + LOG_FILE_PATH;
//         if (!File.Exists(fullPath)) return 1;

//         TrialLog log = JsonUtility.FromJson<TrialLog>(File.ReadAllText(fullPath));
//         return log != null ? log.trials.Count + 1 : 1;
//     }

//     private void WriteTrialToLog(TrialRecord record)
//     {
//         string fullPath = Application.dataPath + LOG_FILE_PATH;

//         TrialLog log;

//         if (File.Exists(fullPath))
//         {
//             string existingJson = File.ReadAllText(fullPath);
//             log = JsonUtility.FromJson<TrialLog>(existingJson) ?? new TrialLog();
//         }
//         else
//         {
//             log = new TrialLog();
//         }

//         int existingIndex = log.trials.FindIndex(t => t.trialIndex == record.trialIndex);

//         if (existingIndex >= 0)
//             log.trials[existingIndex] = record;
//         else
//             log.trials.Add(record);

//         File.WriteAllText(fullPath, JsonUtility.ToJson(log, prettyPrint: true));

//         Debug.Log($"[TrialManager] Trial {record.trialIndex} written to {fullPath}");
//     }

//     // ── Scene restart ──────────────────────────────────────────────────────

//     /// <summary>Counts down then reloads the scene with fresh force ranges and configuration.</summary>
//     private IEnumerator RestartAfterDelay()
//     {
//         float remaining = SURVEY_TO_RESTART_DELAY;

//         while (remaining > 0f)
//         {
//             Debug.Log($"[TrialManager] New trial starting in {remaining:F0}s...");
//             yield return new WaitForSeconds(1f);
//             remaining -= 1f;
//         }

//         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//     }
// }


// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;
// using UnityEngine.XR.Interaction.Toolkit;

// public class TrialManager : MonoBehaviour
// {
//     private static readonly string LOG_FILE_PATH =
//         "/Data/TrialLog_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".json";

//     private const float SURVEY_TO_RESTART_DELAY = 5f;

//     [Header("Configurations")]
//     [Tooltip("Assign Configutation1 through Configutation4 GameObjects")]
//     [SerializeField] private GameObject[] configurations;

//     [Tooltip("The root Configurations GameObject — hidden when survey appears")]
//     [SerializeField] private GameObject configurationsRoot;

//     [Header("Survey")]
//     [Tooltip("The Survey canvas GameObject — shown after trial ends")]
//     [SerializeField] private GameObject surveyCanvas;

//     [Tooltip("The Red toggle on the Survey canvas")]
//     [SerializeField] private Toggle redToggle;

//     [Tooltip("The Green toggle on the Survey canvas")]
//     [SerializeField] private Toggle greenToggle;

//     [Tooltip("Ray Interactors to disable during the survey")]
//     [SerializeField] private XRRayInteractor[] rayInteractors;

//     [Header("Scene References")]
//     [Tooltip("Assign all PegHoleInsertionForceValidated holes in the scene")]
//     [SerializeField] private PegHoleInsertionForceValidated[] holes;

//     [Tooltip("Assign all XRGrabInteractable pegs in the scene")]
//     [SerializeField] private XRGrabInteractable[] pegs;

//     [Tooltip("How many pegs must be inserted for the trial to end (5 red + 5 green = 10)")]
//     [SerializeField] private int requiredInsertions = 10;

//     // ── Shuffled configuration queue (static — persists across scene reloads) ──

//     private static readonly Queue<int> s_ConfigQueue = new Queue<int>();

//     // ── Trial state ────────────────────────────────────────────────────────

//     private int _selectedConfigIndex;
//     private float _trialStartTime;
//     private float _lastGrabTime;
//     private bool _trialStarted;
//     private bool _trialComplete;
//     private int _insertedCount;

//     private TrialRecord _pendingRecord;

//     public int CurrentTrialIndex  => _pendingRecord != null ? _pendingRecord.trialIndex : GetNextTrialIndex();
//     public int CurrentConfigIndex => _selectedConfigIndex + 1;


//     // ── JSON data structures ───────────────────────────────────────────────

//     [System.Serializable]
//     private class ForceRangeEntry
//     {
//         public float min;
//         public float max;
//     }

//     [System.Serializable]
//     private class InsertionEvent
//     {
//         public string holeName;
//         public string pegColor;
//         public float grabToPlacementSeconds;
//     }

//     [System.Serializable]
//     private class TrialRecord
//     {
//         public int trialIndex;
//         public string timestamp;
//         public int configurationIndex;
//         public float completionTimeSeconds;
//         public ForceRangeEntry greenForceRange;
//         public ForceRangeEntry redForceRange;
//         public string surveyAnswer;
//         public List<InsertionEvent> insertions = new List<InsertionEvent>();
//     }

//     [System.Serializable]
//     private class TrialLog
//     {
//         public List<TrialRecord> trials = new List<TrialRecord>();
//     }

//     // ── Unity lifecycle ────────────────────────────────────────────────────

//     private void Awake()
//     {
//         // Seed with current time so every scene reload produces different values.
//         Random.InitState((int)System.DateTime.Now.Ticks);

//         // Reinitialize runs in Awake so all AutomaticPegIdentifier.Start()
//         // calls pick up the fresh ranges — all Awake() complete before any Start().
//         AutomaticPegIdentifier.Reinitialize();
//     }

//     private void Start()
//     {
//         if (surveyCanvas != null)
//             surveyCanvas.SetActive(false);

//         SetupSurveyToggles();
//         SelectRandomConfiguration();
//         SubscribeToHoleEvents();
//         SubscribeToPegGrabEvents();
//     }

//     private void OnDestroy()
//     {
//         UnsubscribeFromHoleEvents();
//         UnsubscribeFromPegGrabEvents();

//         if (redToggle != null)   redToggle.onValueChanged.RemoveListener(OnRedToggleChanged);
//         if (greenToggle != null) greenToggle.onValueChanged.RemoveListener(OnGreenToggleChanged);

//         SetRayInteractorUIEnabled(true);
//     }

//     // ── Configuration selection ────────────────────────────────────────────

//     /// <summary>
//     /// Pulls the next configuration from a shuffled queue.
//     /// When the queue empties it refills with a new shuffle,
//     /// guaranteeing all 4 appear before any repeats.
//     /// </summary>
//     private void SelectRandomConfiguration()
//     {
//         if (configurations == null || configurations.Length == 0)
//         {
//             Debug.LogError("[TrialManager] No configurations assigned!");
//             return;
//         }

//         if (s_ConfigQueue.Count == 0)
//             RefillConfigQueue();

//         _selectedConfigIndex = s_ConfigQueue.Dequeue();

//         for (int i = 0; i < configurations.Length; i++)
//         {
//             if (configurations[i] != null)
//                 configurations[i].SetActive(i == _selectedConfigIndex);
//         }

//         Debug.Log($"[TrialManager] Configuration {_selectedConfigIndex + 1} selected. ({s_ConfigQueue.Count} remaining in this round)");
//     }

//     /// <summary>Fills the queue with all configuration indices in a random shuffled order.</summary>
//     private void RefillConfigQueue()
//     {
//         List<int> indices = new List<int>();
//         for (int i = 0; i < configurations.Length; i++)
//             indices.Add(i);

//         // Fisher-Yates shuffle.
//         for (int i = indices.Count - 1; i > 0; i--)
//         {
//             int j = Random.Range(0, i + 1);
//             (indices[i], indices[j]) = (indices[j], indices[i]);
//         }

//         foreach (int index in indices)
//             s_ConfigQueue.Enqueue(index);

//         Debug.Log("[TrialManager] Config queue refilled and shuffled.");
//     }

//     // ── Survey setup ───────────────────────────────────────────────────────

//     private void SetupSurveyToggles()
//     {
//         if (redToggle != null)
//         {
//             redToggle.isOn = false;
//             redToggle.onValueChanged.AddListener(OnRedToggleChanged);
//         }

//         if (greenToggle != null)
//         {
//             greenToggle.isOn = false;
//             greenToggle.onValueChanged.AddListener(OnGreenToggleChanged);
//         }
//     }

//     /// <summary>Called when the Red toggle changes.</summary>
//     private void OnRedToggleChanged(bool isOn)
//     {
//         if (!isOn) return;

//         if (greenToggle != null)
//             greenToggle.SetIsOnWithoutNotify(false);

//         SaveSurveyAnswer("Red");
//     }

//     /// <summary>Called when the Green toggle changes.</summary>
//     private void OnGreenToggleChanged(bool isOn)
//     {
//         if (!isOn) return;

//         if (redToggle != null)
//             redToggle.SetIsOnWithoutNotify(false);

//         SaveSurveyAnswer("Green");
//     }

//     // ── Ray interactor control ─────────────────────────────────────────────

//     private void SetRayInteractorUIEnabled(bool enabled)
//     {
//         if (rayInteractors == null) return;

//         foreach (XRRayInteractor ray in rayInteractors)
//         {
//             if (ray != null)
//                 ray.enableUIInteraction = enabled;
//         }
//     }

//     // ── Event wiring ───────────────────────────────────────────────────────

//     private void SubscribeToPegGrabEvents()
//     {
//         foreach (XRGrabInteractable peg in pegs)
//         {
//             if (peg != null)
//                 peg.selectEntered.AddListener(OnAnyPegGrabbed);
//         }
//     }

//     private void UnsubscribeFromPegGrabEvents()
//     {
//         foreach (XRGrabInteractable peg in pegs)
//         {
//             if (peg != null)
//                 peg.selectEntered.RemoveListener(OnAnyPegGrabbed);
//         }
//     }

//     private void SubscribeToHoleEvents()
//     {
//         foreach (PegHoleInsertionForceValidated hole in holes)
//         {
//             if (hole != null)
//                 hole.OnPegSnapped += OnPegInserted;
//         }
//     }

//     private void UnsubscribeFromHoleEvents()
//     {
//         foreach (PegHoleInsertionForceValidated hole in holes)
//         {
//             if (hole != null)
//                 hole.OnPegSnapped -= OnPegInserted;
//         }
//     }

//     // ── Timer & grab tracking ──────────────────────────────────────────────

//     /// <summary>
//     /// Starts the overall trial timer on the first grab.
//     /// Keeps tracking every subsequent grab for grab→placement measurement.
//     /// </summary>
//     private void OnAnyPegGrabbed(SelectEnterEventArgs args)
//     {
//         if (!_trialStarted)
//         {
//             _trialStarted = true;
//             _trialStartTime = Time.time;
//             Debug.Log("[TrialManager] Timer started — first peg grabbed.");
//         }

//         _lastGrabTime = Time.time;
//     }

//     /// <summary>Records hole, color, and grab→placement duration for every successful snap.</summary>
//     private void OnPegInserted(string holeName, PegColor pegColor)
//     {
//         if (_trialComplete) return;

//         float grabToPlacement = Time.time - _lastGrabTime;

//         if (_pendingRecord == null)
//             _pendingRecord = CreatePendingRecord(0f);

//         _pendingRecord.insertions.Add(new InsertionEvent
//         {
//             holeName               = holeName,
//             pegColor               = pegColor.ToString(),
//             grabToPlacementSeconds = grabToPlacement
//         });

//         Debug.Log($"[TrialManager] {pegColor} peg → {holeName} | grab→placement: {grabToPlacement:F2}s");

//         _insertedCount++;

//         if (_insertedCount >= requiredInsertions)
//             CompleteTrial();
//     }

//     // ── Trial completion ───────────────────────────────────────────────────

//     private void CompleteTrial()
//     {
//         _trialComplete = true;

//         float elapsed = _trialStarted ? Time.time - _trialStartTime : 0f;

//         UnsubscribeFromPegGrabEvents();

//         foreach (XRGrabInteractable peg in pegs)
//         {
//             if (peg != null)
//                 peg.enabled = false;
//         }

//         SetRayInteractorUIEnabled(false);

//         if (configurationsRoot != null)
//             configurationsRoot.SetActive(false);

//         if (surveyCanvas != null)
//             surveyCanvas.SetActive(true);

//         if (_pendingRecord == null)
//             _pendingRecord = CreatePendingRecord(elapsed);
//         else
//             _pendingRecord.completionTimeSeconds = elapsed;

//         Debug.Log($"[TrialManager] Trial complete in {elapsed:F2}s — awaiting survey response.");
//     }

//     // ── Record helpers ─────────────────────────────────────────────────────

//     private TrialRecord CreatePendingRecord(float elapsed)
//     {
//         Vector2 greenRange = AutomaticPegIdentifier.GetColorForceRange(PegColor.Green);
//         Vector2 redRange   = AutomaticPegIdentifier.GetColorForceRange(PegColor.Red);

//         return new TrialRecord
//         {
//             trialIndex            = GetNextTrialIndex(),
//             timestamp             = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
//             configurationIndex    = _selectedConfigIndex + 1,
//             completionTimeSeconds = elapsed,
//             greenForceRange       = new ForceRangeEntry { min = greenRange.x, max = greenRange.y },
//             redForceRange         = new ForceRangeEntry { min = redRange.x,   max = redRange.y  },
//             surveyAnswer          = "Unanswered",
//             insertions            = new List<InsertionEvent>()
//         };
//     }

//     // ── Survey logging & restart ───────────────────────────────────────────

//     /// <summary>Saves the survey answer, writes to disk, then begins the restart countdown.</summary>
//     private void SaveSurveyAnswer(string answer)
//     {
//         if (_pendingRecord == null)
//         {
//             Debug.LogWarning("[TrialManager] No pending trial record to attach survey answer to.");
//             return;
//         }

//         _pendingRecord.surveyAnswer = answer;
//         WriteTrialToLog(_pendingRecord);

//         if (redToggle != null)   redToggle.interactable = false;
//         if (greenToggle != null) greenToggle.interactable = false;

//         Debug.Log($"[TrialManager] Survey answer saved: {answer} — restarting in {SURVEY_TO_RESTART_DELAY}s.");

//         StartCoroutine(RestartAfterDelay());
//     }

//     private int GetNextTrialIndex()
//     {
//         string fullPath = Application.dataPath + LOG_FILE_PATH;
//         if (!File.Exists(fullPath)) return 1;

//         TrialLog log = JsonUtility.FromJson<TrialLog>(File.ReadAllText(fullPath));
//         return log != null ? log.trials.Count + 1 : 1;
//     }

//     private void WriteTrialToLog(TrialRecord record)
//     {
//         string fullPath = Application.dataPath + LOG_FILE_PATH;

//         TrialLog log;

//         if (File.Exists(fullPath))
//         {
//             string existingJson = File.ReadAllText(fullPath);
//             log = JsonUtility.FromJson<TrialLog>(existingJson) ?? new TrialLog();
//         }
//         else
//         {
//             log = new TrialLog();
//         }

//         int existingIndex = log.trials.FindIndex(t => t.trialIndex == record.trialIndex);

//         if (existingIndex >= 0)
//             log.trials[existingIndex] = record;
//         else
//             log.trials.Add(record);

//         File.WriteAllText(fullPath, JsonUtility.ToJson(log, prettyPrint: true));

//         Debug.Log($"[TrialManager] Trial {record.trialIndex} written to {fullPath}");
//     }

//     // ── Scene restart ──────────────────────────────────────────────────────

//     /// <summary>Counts down then reloads the scene with fresh force ranges and configuration.</summary>
//     private IEnumerator RestartAfterDelay()
//     {
//         float remaining = SURVEY_TO_RESTART_DELAY;

//         while (remaining > 0f)
//         {
//             Debug.Log($"[TrialManager] New trial starting in {remaining:F0}s...");
//             yield return new WaitForSeconds(1f);
//             remaining -= 1f;
//         }

//         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
//     }
// }
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class TrialManager : MonoBehaviour
{
    private static readonly string LOG_FILE_PATH =
        "/Data/TrialLog_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".json";

    private const float SURVEY_TO_RESTART_DELAY = 5f;
    private const int   MAX_TRIALS              = 20;

    // ── Static state — persists across scene reloads ───────────────────────

    private static readonly Queue<int> s_ConfigQueue           = new Queue<int>();
    private static int                 s_TotalTrialsCompleted  = 0;

    [Header("Configurations")]
    [Tooltip("Assign Configutation1 through Configutation4 GameObjects")]
    [SerializeField] private GameObject[] configurations;

    [Tooltip("The root Configurations GameObject — hidden when survey appears")]
    [SerializeField] private GameObject configurationsRoot;

    [Header("Survey")]
    [Tooltip("The Survey canvas GameObject — shown after trial ends")]
    [SerializeField] private GameObject surveyCanvas;

    [Tooltip("The Red toggle on the Survey canvas")]
    [SerializeField] private Toggle redToggle;

    [Tooltip("The Green toggle on the Survey canvas")]
    [SerializeField] private Toggle greenToggle;

    [Tooltip("Ray Interactors to disable during the survey")]
    [SerializeField] private XRRayInteractor[] rayInteractors;

    [Header("Trials Display")]
    [Tooltip("The Trials canvas GameObject")]
    [SerializeField] private GameObject trialsCanvas;

    [Tooltip("The TMP text inside the Trials canvas — shows trial number and completion message")]
    [SerializeField] private TMP_Text trialsText;

    [Header("Scene References")]
    [Tooltip("Assign all PegHoleInsertionForceValidated holes in the scene")]
    [SerializeField] private PegHoleInsertionForceValidated[] holes;

    [Tooltip("Assign all XRGrabInteractable pegs in the scene")]
    [SerializeField] private XRGrabInteractable[] pegs;

    [Tooltip("How many pegs must be inserted for the trial to end (5 red + 5 green = 10)")]
    [SerializeField] private int requiredInsertions = 10;

    // ── Trial state ────────────────────────────────────────────────────────

    private int _selectedConfigIndex;
    private float _trialStartTime;
    private float _lastGrabTime;
    private bool _trialStarted;
    private bool _trialComplete;
    private int _insertedCount;

    private TrialRecord _pendingRecord;

    // ── Public getters for ForceLogger ────────────────────────────────────

    public int CurrentTrialIndex  => _pendingRecord != null ? _pendingRecord.trialIndex : GetNextTrialIndex();
    public int CurrentConfigIndex => _selectedConfigIndex + 1;

    // ── JSON data structures ───────────────────────────────────────────────

    [System.Serializable]
    private class ForceRangeEntry
    {
        public float min;
        public float max;
    }

    [System.Serializable]
    private class InsertionEvent
    {
        public string holeName;
        public string pegColor;
        public float grabToPlacementSeconds;
    }

    [System.Serializable]
    private class TrialRecord
    {
        public int trialIndex;
        public string timestamp;
        public int configurationIndex;
        public float completionTimeSeconds;
        public ForceRangeEntry greenForceRange;
        public ForceRangeEntry redForceRange;
        public string surveyAnswer;
        public List<InsertionEvent> insertions = new List<InsertionEvent>();
    }

    [System.Serializable]
    private class TrialLog
    {
        public List<TrialRecord> trials = new List<TrialRecord>();
    }

    // ── Unity lifecycle ────────────────────────────────────────────────────

    private void Awake()
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
        AutomaticPegIdentifier.Reinitialize();
    }

    private void Start()
    {
        if (surveyCanvas != null)
            surveyCanvas.SetActive(false);

        UpdateTrialsDisplay();
        SetupSurveyToggles();
        SelectRandomConfiguration();
        SubscribeToHoleEvents();
        SubscribeToPegGrabEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromHoleEvents();
        UnsubscribeFromPegGrabEvents();

        if (redToggle != null)   redToggle.onValueChanged.RemoveListener(OnRedToggleChanged);
        if (greenToggle != null) greenToggle.onValueChanged.RemoveListener(OnGreenToggleChanged);

        SetRayInteractorUIEnabled(true);
    }

    // ── Trials display ─────────────────────────────────────────────────────

    /// <summary>Updates the Trials canvas text with the current trial number.</summary>
    private void UpdateTrialsDisplay()
    {
        if (trialsText == null) return;

        trialsText.text = $"Trial {s_TotalTrialsCompleted + 1} / {MAX_TRIALS}";
    }

    /// <summary>Shows the completion message and hides everything else.</summary>
    private void ShowCompletionMessage()
    {
        if (trialsText != null)
            trialsText.text = "Congrats, Task Completed!";

        if (surveyCanvas != null)      surveyCanvas.SetActive(false);
        if (configurationsRoot != null) configurationsRoot.SetActive(false);

        foreach (XRGrabInteractable peg in pegs)
        {
            if (peg != null)
                peg.enabled = false;
        }

        SetRayInteractorUIEnabled(false);

        Debug.Log("[TrialManager] All 20 trials completed!");
    }

    // ── Configuration selection ────────────────────────────────────────────

    /// <summary>
    /// Pulls the next configuration from a shuffled queue.
    /// Guarantees all 4 appear before any repeats.
    /// </summary>
    private void SelectRandomConfiguration()
    {
        if (configurations == null || configurations.Length == 0)
        {
            Debug.LogError("[TrialManager] No configurations assigned!");
            return;
        }

        if (s_ConfigQueue.Count == 0)
            RefillConfigQueue();

        _selectedConfigIndex = s_ConfigQueue.Dequeue();

        for (int i = 0; i < configurations.Length; i++)
        {
            if (configurations[i] != null)
                configurations[i].SetActive(i == _selectedConfigIndex);
        }

        Debug.Log($"[TrialManager] Configuration {_selectedConfigIndex + 1} selected. ({s_ConfigQueue.Count} remaining in this round)");
    }

    /// <summary>Fills the queue with all configuration indices in a random shuffled order.</summary>
    private void RefillConfigQueue()
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < configurations.Length; i++)
            indices.Add(i);

        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        foreach (int index in indices)
            s_ConfigQueue.Enqueue(index);

        Debug.Log("[TrialManager] Config queue refilled and shuffled.");
    }

    // ── Survey setup ───────────────────────────────────────────────────────

    private void SetupSurveyToggles()
    {
        if (redToggle != null)
        {
            redToggle.isOn = false;
            redToggle.onValueChanged.AddListener(OnRedToggleChanged);
        }

        if (greenToggle != null)
        {
            greenToggle.isOn = false;
            greenToggle.onValueChanged.AddListener(OnGreenToggleChanged);
        }
    }

    /// <summary>Called when the Red toggle changes.</summary>
    private void OnRedToggleChanged(bool isOn)
    {
        if (!isOn) return;

        if (greenToggle != null)
            greenToggle.SetIsOnWithoutNotify(false);

        SaveSurveyAnswer("Red");
    }

    /// <summary>Called when the Green toggle changes.</summary>
    private void OnGreenToggleChanged(bool isOn)
    {
        if (!isOn) return;

        if (redToggle != null)
            redToggle.SetIsOnWithoutNotify(false);

        SaveSurveyAnswer("Green");
    }

    // ── Ray interactor control ─────────────────────────────────────────────

    private void SetRayInteractorUIEnabled(bool enabled)
    {
        if (rayInteractors == null) return;

        foreach (XRRayInteractor ray in rayInteractors)
        {
            if (ray != null)
                ray.enableUIInteraction = enabled;
        }
    }

    // ── Event wiring ───────────────────────────────────────────────────────

    private void SubscribeToPegGrabEvents()
    {
        foreach (XRGrabInteractable peg in pegs)
        {
            if (peg != null)
                peg.selectEntered.AddListener(OnAnyPegGrabbed);
        }
    }

    private void UnsubscribeFromPegGrabEvents()
    {
        foreach (XRGrabInteractable peg in pegs)
        {
            if (peg != null)
                peg.selectEntered.RemoveListener(OnAnyPegGrabbed);
        }
    }

    private void SubscribeToHoleEvents()
    {
        foreach (PegHoleInsertionForceValidated hole in holes)
        {
            if (hole != null)
                hole.OnPegSnapped += OnPegInserted;
        }
    }

    private void UnsubscribeFromHoleEvents()
    {
        foreach (PegHoleInsertionForceValidated hole in holes)
        {
            if (hole != null)
                hole.OnPegSnapped -= OnPegInserted;
        }
    }

    // ── Timer & grab tracking ──────────────────────────────────────────────

    /// <summary>
    /// Starts the overall trial timer on the first grab.
    /// Keeps tracking every subsequent grab for grab→placement measurement.
    /// </summary>
    private void OnAnyPegGrabbed(SelectEnterEventArgs args)
    {
        if (!_trialStarted)
        {
            _trialStarted = true;
            _trialStartTime = Time.time;
            Debug.Log("[TrialManager] Timer started — first peg grabbed.");
        }

        _lastGrabTime = Time.time;
    }

    /// <summary>Records hole, color, and grab→placement duration for every successful snap.</summary>
    private void OnPegInserted(string holeName, PegColor pegColor)
    {
        if (_trialComplete) return;

        float grabToPlacement = Time.time - _lastGrabTime;

        if (_pendingRecord == null)
            _pendingRecord = CreatePendingRecord(0f);

        _pendingRecord.insertions.Add(new InsertionEvent
        {
            holeName               = holeName,
            pegColor               = pegColor.ToString(),
            grabToPlacementSeconds = grabToPlacement
        });

        Debug.Log($"[TrialManager] {pegColor} peg → {holeName} | grab→placement: {grabToPlacement:F2}s");

        _insertedCount++;

        if (_insertedCount >= requiredInsertions)
            CompleteTrial();
    }

    // ── Trial completion ───────────────────────────────────────────────────

    private void CompleteTrial()
    {
        _trialComplete = true;

        float elapsed = _trialStarted ? Time.time - _trialStartTime : 0f;

        UnsubscribeFromPegGrabEvents();

        foreach (XRGrabInteractable peg in pegs)
        {
            if (peg != null)
                peg.enabled = false;
        }

        SetRayInteractorUIEnabled(false);

        if (configurationsRoot != null)
            configurationsRoot.SetActive(false);

        if (surveyCanvas != null)
            surveyCanvas.SetActive(true);

        if (_pendingRecord == null)
            _pendingRecord = CreatePendingRecord(elapsed);
        else
            _pendingRecord.completionTimeSeconds = elapsed;

        Debug.Log($"[TrialManager] Trial complete in {elapsed:F2}s — awaiting survey response.");
    }

    // ── Record helpers ─────────────────────────────────────────────────────

    private TrialRecord CreatePendingRecord(float elapsed)
    {
        Vector2 greenRange = AutomaticPegIdentifier.GetColorForceRange(PegColor.Green);
        Vector2 redRange   = AutomaticPegIdentifier.GetColorForceRange(PegColor.Red);

        return new TrialRecord
        {
            trialIndex            = GetNextTrialIndex(),
            timestamp             = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            configurationIndex    = _selectedConfigIndex + 1,
            completionTimeSeconds = elapsed,
            greenForceRange       = new ForceRangeEntry { min = greenRange.x, max = greenRange.y },
            redForceRange         = new ForceRangeEntry { min = redRange.x,   max = redRange.y  },
            surveyAnswer          = "Unanswered",
            insertions            = new List<InsertionEvent>()
        };
    }

    // ── Survey logging & restart ───────────────────────────────────────────

    /// <summary>Saves the survey answer, writes to disk, then either restarts or ends the session.</summary>
    private void SaveSurveyAnswer(string answer)
    {
        if (_pendingRecord == null)
        {
            Debug.LogWarning("[TrialManager] No pending trial record to attach survey answer to.");
            return;
        }

        _pendingRecord.surveyAnswer = answer;
        WriteTrialToLog(_pendingRecord);

        if (redToggle != null)   redToggle.interactable = false;
        if (greenToggle != null) greenToggle.interactable = false;

        s_TotalTrialsCompleted++;

        Debug.Log($"[TrialManager] Survey answer saved: {answer}. Trials completed: {s_TotalTrialsCompleted} / {MAX_TRIALS}");

        if (s_TotalTrialsCompleted >= MAX_TRIALS)
        {
            if (surveyCanvas != null)
                surveyCanvas.SetActive(false);

            ShowCompletionMessage();
        }
        else
        {
            Debug.Log($"[TrialManager] Restarting in {SURVEY_TO_RESTART_DELAY}s.");
            StartCoroutine(RestartAfterDelay());
        }
    }

    private int GetNextTrialIndex()
    {
        string fullPath = Application.dataPath + LOG_FILE_PATH;
        if (!File.Exists(fullPath)) return 1;

        TrialLog log = JsonUtility.FromJson<TrialLog>(File.ReadAllText(fullPath));
        return log != null ? log.trials.Count + 1 : 1;
    }

    private void WriteTrialToLog(TrialRecord record)
    {
        string fullPath = Application.dataPath + LOG_FILE_PATH;

        TrialLog log;

        if (File.Exists(fullPath))
        {
            string existingJson = File.ReadAllText(fullPath);
            log = JsonUtility.FromJson<TrialLog>(existingJson) ?? new TrialLog();
        }
        else
        {
            log = new TrialLog();
        }

        int existingIndex = log.trials.FindIndex(t => t.trialIndex == record.trialIndex);

        if (existingIndex >= 0)
            log.trials[existingIndex] = record;
        else
            log.trials.Add(record);

        File.WriteAllText(fullPath, JsonUtility.ToJson(log, prettyPrint: true));

        Debug.Log($"[TrialManager] Trial {record.trialIndex} written to {fullPath}");
    }

    // ── Scene restart ──────────────────────────────────────────────────────

    /// <summary>Counts down then reloads the scene for the next trial.</summary>
    private IEnumerator RestartAfterDelay()
    {
        float remaining = SURVEY_TO_RESTART_DELAY;

        while (remaining > 0f)
        {
            Debug.Log($"[TrialManager] New trial starting in {remaining:F0}s...");
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
