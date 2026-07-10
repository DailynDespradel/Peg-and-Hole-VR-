// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;

// public class PegHoleInsertionForceValidated : MonoBehaviour
// {
//     [Header("Insertion Settings")]
//     [SerializeField] private Transform attachPoint;
//     [SerializeField] private float validForceHoldTime = 0.5f;
    
//     [Header("In-Game Visual Feedback")]
//     [SerializeField] private bool showInGameVisuals = true;
//     [SerializeField] private Renderer ringRenderer;
//     [SerializeField] private Color readyColor = Color.green;
//     [SerializeField] private Color invalidForceColor = Color.red;
//     [SerializeField] private Color lockedColor = new Color(0f, 0.5f, 1f);
//     [SerializeField] private Color defaultColor = Color.blue;
    
//     private GameObject currentPeg;
//     private PegIdentifier currentPegIdentifier;
//     private XRGrabInteractable pegGrabInteractable;
//     private Rigidbody pegRigidbody;
//     private bool isInserted = false;
//     private bool isForceValid = false;
    
//     private float validForceTimer = 0f;
    
//     private LineRenderer connectionLine;
//     private Material ringMaterial;
    
//     void Start()
//     {
//         if (showInGameVisuals)
//         {
//             CreateVisualFeedback();
//         }
        
//         if (ringRenderer != null)
//         {
//             ringMaterial = ringRenderer.material;
//         }
//     }
    
//     void CreateVisualFeedback()
//     {
//         GameObject lineObj = new GameObject("ConnectionLine");
//         lineObj.transform.SetParent(transform);
//         connectionLine = lineObj.AddComponent<LineRenderer>();
//         connectionLine.material = new Material(Shader.Find("Sprites/Default"));
//         connectionLine.startColor = readyColor;
//         connectionLine.endColor = readyColor;
//         connectionLine.startWidth = 0.002f;
//         connectionLine.endWidth = 0.002f;
//         connectionLine.positionCount = 2;
//         connectionLine.enabled = false;
//     }
    
//     void OnTriggerEnter(Collider other)
//     {
//         if (currentPeg != null) return;
        
//         if (other.CompareTag("Peg") || other.GetComponent<XRGrabInteractable>() != null)
//         {
//             currentPeg = other.gameObject;
//             currentPegIdentifier = currentPeg.GetComponentInChildren<PegIdentifier>();
//             pegGrabInteractable = currentPeg.GetComponent<XRGrabInteractable>();
//             pegRigidbody = currentPeg.GetComponent<Rigidbody>();
            
//             validForceTimer = 0f;
            
//             if (currentPegIdentifier == null)
//             {
//                 Debug.LogWarning($"Peg {currentPeg.name} does not have PegIdentifier component in children!");
//             }
            
//             if (pegGrabInteractable != null)
//             {
//                 pegGrabInteractable.selectEntered.AddListener(OnPegGrabbed);
//                 pegGrabInteractable.selectExited.AddListener(OnPegReleased);
//             }
            
//             ValidateForce();
            
//             Debug.Log($"✅ Peg {currentPeg.name} ENTERED hole {gameObject.name}");
//         }
//     }
    
//     void OnTriggerStay(Collider other)
//     {
//         if (currentPeg == null || other.gameObject != currentPeg) return;
//         if (attachPoint == null) return;
//         if (isInserted) return;
        
//         ValidateForce();
        
//         if (isForceValid)
//         {
//             validForceTimer += Time.deltaTime;
            
//             if (validForceTimer >= validForceHoldTime)
//             {
//                 Debug.Log($"⏱️ Valid force held for {validForceHoldTime}s - auto-snapping!");
//                 SnapPegToHole();
//                 return;
//             }
//         }
//         else
//         {
//             validForceTimer = 0f;
//         }
        
//         Vector3 pegCenter = currentPeg.transform.position;
//         Collider pegCollider = currentPeg.GetComponent<Collider>();
//         if (pegCollider != null)
//         {
//             pegCenter = pegCollider.bounds.center;
//         }
        
//         if (showInGameVisuals && connectionLine != null && connectionLine.enabled)
//         {
//             connectionLine.SetPosition(0, pegCenter);
//             connectionLine.SetPosition(1, attachPoint.position);
//         }
//     }
    
//     void OnTriggerExit(Collider other)
//     {
//         if (other.gameObject == currentPeg)
//         {
//             Debug.Log($"⚠️ Peg {currentPeg.name} EXITED hole {gameObject.name}");
            
//             if (pegGrabInteractable != null)
//             {
//                 pegGrabInteractable.selectEntered.RemoveListener(OnPegGrabbed);
//                 pegGrabInteractable.selectExited.RemoveListener(OnPegReleased);
//             }
            
//             if (showInGameVisuals)
//             {
//                 if (connectionLine != null)
//                 {
//                     connectionLine.enabled = false;
//                 }
                
//                 if (ringRenderer != null && ringMaterial != null && !isInserted)
//                 {
//                     ringMaterial.color = defaultColor;
//                     if (ringMaterial.HasProperty("_EmissionColor"))
//                     {
//                         ringMaterial.SetColor("_EmissionColor", Color.black);
//                     }
//                 }
//             }
            
//             currentPeg = null;
//             currentPegIdentifier = null;
//             pegGrabInteractable = null;
//             pegRigidbody = null;
//             isInserted = false;
//             isForceValid = false;
//             validForceTimer = 0f;
//         }
//     }
    
//     void ValidateForce()
//     {
//         if (currentPegIdentifier == null)
//         {
//             isForceValid = false;
//             return;
//         }
        
//         bool wasValid = isForceValid;
//         isForceValid = ForceValidator.IsForceValid(currentPegIdentifier);
        
//         if (wasValid != isForceValid)
//         {
//             PegColor pegColor = currentPegIdentifier.GetPegColor();
//             float currentForce = currentPegIdentifier.GetCurrentForce();
//             string requiredRange = ForceValidator.GetForceRangeText(currentPegIdentifier);
            
//             Debug.Log($"Force validation changed for {currentPeg.name} ({pegColor}): {(isForceValid ? "VALID ✅" : "INVALID ❌")} (Force: {currentForce:F2}, Required: {requiredRange})");
            
//             if (isForceValid)
//             {
//                 validForceTimer = 0f;
//             }
//         }
        
//         UpdateVisualFeedback();
//     }
    
//     void UpdateVisualFeedback()
//     {
//         if (!showInGameVisuals) return;
        
//         if (connectionLine != null && currentPeg != null && !isInserted)
//         {
//             connectionLine.enabled = true;
//             Color lineColor = isForceValid ? readyColor : invalidForceColor;
//             connectionLine.startColor = lineColor;
//             connectionLine.endColor = lineColor;
//         }
        
//         if (ringRenderer != null && ringMaterial != null && !isInserted)
//         {
//             Color targetColor = isForceValid ? readyColor : invalidForceColor;
//             ringMaterial.color = targetColor;
            
//             if (ringMaterial.HasProperty("_EmissionColor"))
//             {
//                 ringMaterial.EnableKeyword("_EMISSION");
//                 ringMaterial.SetColor("_EmissionColor", targetColor * 1.5f);
//             }
//         }
//     }
    
//     void OnPegGrabbed(SelectEnterEventArgs args)
//     {
//         if (isInserted)
//         {
//             Debug.Log($"🔓 Peg {currentPeg.name} GRABBED from hole {gameObject.name}");
            
//             if (pegRigidbody != null)
//             {
//                 pegRigidbody.isKinematic = false;
//                 pegRigidbody.useGravity = true;
//             }
            
//             isInserted = false;
//             validForceTimer = 0f;
//         }
//     }
    
//     void OnPegReleased(SelectExitEventArgs args)
//     {
//         if (pegRigidbody != null)
//         {
//             pegRigidbody.useGravity = true;
//         }
        
//         if (currentPeg != null && !isInserted)
//         {
//             ValidateForce();
            
//             if (!isForceValid)
//             {
//                 string requiredRange = ForceValidator.GetForceRangeText(currentPegIdentifier);
//                 Debug.Log($"❌ Peg {currentPeg.name} RELEASED with invalid force - will NOT snap (Current: {currentPegIdentifier.GetCurrentForce():F2}, Required: {requiredRange})");
//             }
//         }
        
//         validForceTimer = 0f;
//     }
    
//     void SnapPegToHole()
//     {
//         if (!isForceValid)
//         {
//             Debug.LogWarning($"❌ Cannot snap - force is invalid!");
//             return;
//         }
        
//         if (isInserted)
//         {
//             return;
//         }
        
//         isInserted = true;
        
//         Vector3 pegOffset = Vector3.zero;
//         Collider pegCollider = currentPeg.GetComponent<Collider>();
//         if (pegCollider != null)
//         {
//             pegOffset = currentPeg.transform.position - pegCollider.bounds.center;
//         }
        
//         currentPeg.transform.position = attachPoint.position + pegOffset;
//         currentPeg.transform.rotation = attachPoint.rotation;
        
//         if (pegRigidbody != null)
//         {
//             pegRigidbody.velocity = Vector3.zero;
//             pegRigidbody.angularVelocity = Vector3.zero;
//             pegRigidbody.isKinematic = true;
//             pegRigidbody.useGravity = false;
//         }
        
//         if (showInGameVisuals && ringRenderer != null && ringMaterial != null)
//         {
//             ringMaterial.color = lockedColor;
//             if (ringMaterial.HasProperty("_EmissionColor"))
//             {
//                 ringMaterial.SetColor("_EmissionColor", lockedColor * 1.5f);
//             }
//         }
        
//         if (connectionLine != null)
//         {
//             connectionLine.enabled = false;
//         }
        
//         Debug.Log($"🔒 Peg {currentPeg.name} LOCKED into hole {gameObject.name} at {attachPoint.position}");
//     }
    
//     void OnDestroy()
//     {
//         if (connectionLine != null)
//         {
//             Destroy(connectionLine.gameObject);
//         }
//     }
// }

// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;

// public class PegHoleInsertionForceValidated : MonoBehaviour
// {
//     [Header("Insertion Settings")]
//     [SerializeField] private Transform attachPoint;
//     [SerializeField] private float validForceHoldTime = 0.5f;
    
//     [Header("In-Game Visual Feedback")]
//     [SerializeField] private bool showInGameVisuals = true;
//     [SerializeField] private Renderer ringRenderer;
//     [SerializeField] private Color readyColor = Color.green;
//     [SerializeField] private Color invalidForceColor = Color.red;
//     [SerializeField] private Color lockedColor = new Color(0f, 0.5f, 1f);
//     [SerializeField] private Color defaultColor = Color.blue;
    
//     private GameObject currentPeg;
//     private PegIdentifier currentPegIdentifier;
//     private XRGrabInteractable pegGrabInteractable;
//     private Rigidbody pegRigidbody;
//     private bool isInserted = false;
//     private bool isForceValid = false;
    
//     private float validForceTimer = 0f;
    
//     private LineRenderer connectionLine;
//     private Material ringMaterial;
    
//     void Start()
//     {
//         if (showInGameVisuals)
//         {
//             CreateVisualFeedback();
//         }
        
//         if (ringRenderer != null)
//         {
//             ringMaterial = ringRenderer.material;
//         }
//     }
    
//     void CreateVisualFeedback()
//     {
//         GameObject lineObj = new GameObject("ConnectionLine");
//         lineObj.transform.SetParent(transform);
//         connectionLine = lineObj.AddComponent<LineRenderer>();
//         connectionLine.material = new Material(Shader.Find("Sprites/Default"));
//         connectionLine.startColor = readyColor;
//         connectionLine.endColor = readyColor;
//         connectionLine.startWidth = 0.002f;
//         connectionLine.endWidth = 0.002f;
//         connectionLine.positionCount = 2;
//         connectionLine.enabled = false;
//     }
    
//     void OnTriggerEnter(Collider other)
//     {
//         if (currentPeg != null) return;
        
//         if (other.CompareTag("Peg") || other.GetComponent<XRGrabInteractable>() != null)
//         {
//             currentPeg = other.gameObject;
//             currentPegIdentifier = currentPeg.GetComponentInChildren<PegIdentifier>();
//             pegGrabInteractable = currentPeg.GetComponent<XRGrabInteractable>();
//             pegRigidbody = currentPeg.GetComponent<Rigidbody>();
            
//             validForceTimer = 0f;
            
//             if (currentPegIdentifier == null)
//             {
//                 Debug.LogWarning($"Peg {currentPeg.name} does not have PegIdentifier component in children!");
//             }
            
//             if (pegGrabInteractable != null)
//             {
//                 pegGrabInteractable.selectEntered.AddListener(OnPegGrabbed);
//                 pegGrabInteractable.selectExited.AddListener(OnPegReleased);
//             }
            
//             ValidateForce();
            
//             Debug.Log($"✅ Peg {currentPeg.name} ENTERED hole {gameObject.name}");
//         }
//     }
    
//     void OnTriggerStay(Collider other)
//     {
//         if (currentPeg == null || other.gameObject != currentPeg) return;
//         if (attachPoint == null) return;
//         if (isInserted) return;
        
//         ValidateForce();
        
//         if (isForceValid)
//         {
//             validForceTimer += Time.deltaTime;
            
//             if (validForceTimer >= validForceHoldTime)
//             {
//                 Debug.Log($"⏱️ Valid force held for {validForceHoldTime}s - auto-snapping!");
//                 SnapPegToHole();
//                 return;
//             }
//         }
//         else
//         {
//             validForceTimer = 0f;
//         }
        
//         Vector3 pegCenter = currentPeg.transform.position;
//         Collider pegCollider = currentPeg.GetComponent<Collider>();
//         if (pegCollider != null)
//         {
//             pegCenter = pegCollider.bounds.center;
//         }
        
//         if (showInGameVisuals && connectionLine != null && connectionLine.enabled)
//         {
//             connectionLine.SetPosition(0, pegCenter);
//             connectionLine.SetPosition(1, attachPoint.position);
//         }
//     }
    
//     void OnTriggerExit(Collider other)
//     {
//         if (other.gameObject == currentPeg)
//         {
//             Debug.Log($"⚠️ Peg {currentPeg.name} EXITED hole {gameObject.name}");
            
//             if (pegGrabInteractable != null)
//             {
//                 pegGrabInteractable.selectEntered.RemoveListener(OnPegGrabbed);
//                 pegGrabInteractable.selectExited.RemoveListener(OnPegReleased);
//             }
            
//             if (showInGameVisuals)
//             {
//                 if (connectionLine != null)
//                 {
//                     connectionLine.enabled = false;
//                 }
                
//                 if (ringRenderer != null && ringMaterial != null && !isInserted)
//                 {
//                     ringMaterial.color = defaultColor;
//                     if (ringMaterial.HasProperty("_EmissionColor"))
//                     {
//                         ringMaterial.SetColor("_EmissionColor", Color.black);
//                     }
//                 }
//             }
            
//             currentPeg = null;
//             currentPegIdentifier = null;
//             pegGrabInteractable = null;
//             pegRigidbody = null;
//             isInserted = false;
//             isForceValid = false;
//             validForceTimer = 0f;
//         }
//     }
    
//     void ValidateForce()
//     {
//         if (currentPegIdentifier == null || isInserted)
//         {
//             return;
//         }
        
//         bool wasValid = isForceValid;
//         isForceValid = ForceValidator.IsForceValid(currentPegIdentifier);
        
//         if (wasValid != isForceValid)
//         {
//             PegColor pegColor = currentPegIdentifier.GetPegColor();
//             float currentForce = currentPegIdentifier.GetCurrentForce();
//             string requiredRange = ForceValidator.GetForceRangeText(currentPegIdentifier);
            
//             Debug.Log($"Force validation changed for {currentPeg.name} ({pegColor}): {(isForceValid ? "VALID ✅" : "INVALID ❌")} (Force: {currentForce:F2}, Required: {requiredRange})");
            
//             if (isForceValid)
//             {
//                 validForceTimer = 0f;
//             }
//         }
        
//         UpdateVisualFeedback();
//     }
    
//     void UpdateVisualFeedback()
//     {
//         if (!showInGameVisuals || isInserted) return;
        
//         if (connectionLine != null && currentPeg != null)
//         {
//             connectionLine.enabled = true;
//             Color lineColor = isForceValid ? readyColor : invalidForceColor;
//             connectionLine.startColor = lineColor;
//             connectionLine.endColor = lineColor;
//         }
        
//         if (ringRenderer != null && ringMaterial != null)
//         {
//             Color targetColor = isForceValid ? readyColor : invalidForceColor;
//             ringMaterial.color = targetColor;
            
//             if (ringMaterial.HasProperty("_EmissionColor"))
//             {
//                 ringMaterial.EnableKeyword("_EMISSION");
//                 ringMaterial.SetColor("_EmissionColor", targetColor * 1.5f);
//             }
//         }
//     }
    
//     void OnPegGrabbed(SelectEnterEventArgs args)
//     {
//         if (isInserted)
//         {
//             Debug.Log($"🔓 Peg {currentPeg.name} GRABBED from hole {gameObject.name}");
            
//             if (pegGrabInteractable != null)
//             {
//                 pegGrabInteractable.enabled = true;
//             }
            
//             if (pegRigidbody != null)
//             {
//                 pegRigidbody.isKinematic = false;
//                 pegRigidbody.useGravity = true;
//             }
            
//             isInserted = false;
//             validForceTimer = 0f;
//         }
//     }
    
//     void OnPegReleased(SelectExitEventArgs args)
//     {
//         if (pegRigidbody != null)
//         {
//             pegRigidbody.useGravity = true;
//         }
        
//         if (currentPeg != null && !isInserted)
//         {
//             ValidateForce();
            
//             if (!isForceValid)
//             {
//                 string requiredRange = ForceValidator.GetForceRangeText(currentPegIdentifier);
//                 Debug.Log($"❌ Peg {currentPeg.name} RELEASED with invalid force - will NOT snap (Current: {currentPegIdentifier.GetCurrentForce():F2}, Required: {requiredRange})");
//             }
//         }
        
//         validForceTimer = 0f;
//     }
    
//     void SnapPegToHole()
//     {
//         if (!isForceValid)
//         {
//             Debug.LogWarning($"❌ Cannot snap - force is invalid!");
//             return;
//         }
        
//         if (isInserted)
//         {
//             return;
//         }
        
//         isInserted = true;
        
//         if (pegGrabInteractable != null && pegGrabInteractable.isSelected)
//         {
//             XRInteractionManager interactionManager = pegGrabInteractable.interactionManager;
//             IXRSelectInteractor interactor = pegGrabInteractable.firstInteractorSelecting;
            
//             if (interactionManager != null && interactor != null)
//             {
//                 interactionManager.SelectExit(interactor, pegGrabInteractable);
//                 Debug.Log($"🔓 Force released peg from controller");
//             }
//         }
        
//         Vector3 pegOffset = Vector3.zero;
//         Collider pegCollider = currentPeg.GetComponent<Collider>();
//         if (pegCollider != null)
//         {
//             pegOffset = currentPeg.transform.position - pegCollider.bounds.center;
//         }
        
//         currentPeg.transform.position = attachPoint.position + pegOffset;
//         currentPeg.transform.rotation = attachPoint.rotation;
        
//         if (pegRigidbody != null)
//         {
//             pegRigidbody.velocity = Vector3.zero;
//             pegRigidbody.angularVelocity = Vector3.zero;
//             pegRigidbody.isKinematic = true;
//             pegRigidbody.useGravity = false;
//         }
        
//         if (pegGrabInteractable != null)
//         {
//             pegGrabInteractable.enabled = false;
//         }
        
//         if (showInGameVisuals && ringRenderer != null && ringMaterial != null)
//         {
//             ringMaterial.color = lockedColor;
//             if (ringMaterial.HasProperty("_EmissionColor"))
//             {
//                 ringMaterial.SetColor("_EmissionColor", lockedColor * 1.5f);
//             }
//         }
        
//         if (connectionLine != null)
//         {
//             connectionLine.enabled = false;
//         }
        
//         Debug.Log($"🔒 Peg {currentPeg.name} LOCKED into hole {gameObject.name} at {attachPoint.position}");
//     }

        
//     void OnDestroy()
//     {
//         if (connectionLine != null)
//         {
//             Destroy(connectionLine.gameObject);
//         }
//     }
// }


using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PegHoleInsertionForceValidated : MonoBehaviour
{
    [Header("Insertion Settings")]
    [SerializeField] private Transform attachPoint;
    [SerializeField] private float validForceHoldTime = 0.5f;
    
    [Header("In-Game Visual Feedback")]
    [SerializeField] private bool showInGameVisuals = true;
    [SerializeField] private Renderer ringRenderer;
    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color invalidForceColor = Color.red;
    [SerializeField] private Color lockedColor = new Color(0f, 0.5f, 1f);
    [SerializeField] private Color defaultColor = Color.blue;
    
    private GameObject currentPeg;
    private IPegIdentifier currentPegIdentifier;
    private XRGrabInteractable pegGrabInteractable;
    private Rigidbody pegRigidbody;
    private bool isInserted = false;
    private bool isForceValid = false;
    
    private float validForceTimer = 0f;
    
    private LineRenderer connectionLine;
    private Material ringMaterial;
    
    // Add this alongside the other private fields around line 588
    //public event System.Action OnPegSnapped;
    public event System.Action<string, PegColor> OnPegSnapped;


    
    void Start()
    {
        if (showInGameVisuals)
        {
            CreateVisualFeedback();
        }
        
        if (ringRenderer != null)
        {
            ringMaterial = ringRenderer.material;
        }
    }
    
    void CreateVisualFeedback()
    {
        GameObject lineObj = new GameObject("ConnectionLine");
        lineObj.transform.SetParent(transform);
        connectionLine = lineObj.AddComponent<LineRenderer>();
        connectionLine.material = new Material(Shader.Find("Sprites/Default"));
        connectionLine.startColor = readyColor;
        connectionLine.endColor = readyColor;
        connectionLine.startWidth = 0.002f;
        connectionLine.endWidth = 0.002f;
        connectionLine.positionCount = 2;
        connectionLine.enabled = false;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (currentPeg != null) return;
        
        if (other.CompareTag("Peg") || other.GetComponent<XRGrabInteractable>() != null)
        {
            currentPeg = other.gameObject;
            currentPegIdentifier = currentPeg.GetComponentInChildren<IPegIdentifier>();
            pegGrabInteractable = currentPeg.GetComponent<XRGrabInteractable>();
            pegRigidbody = currentPeg.GetComponent<Rigidbody>();
            
            validForceTimer = 0f;
            
            if (currentPegIdentifier == null)
            {
                Debug.LogWarning($"Peg {currentPeg.name} does not have PegIdentifier component in children!");
            }
            
            if (pegGrabInteractable != null)
            {
                pegGrabInteractable.selectEntered.AddListener(OnPegGrabbed);
                pegGrabInteractable.selectExited.AddListener(OnPegReleased);
            }
            
            ValidateForce();
            
            Debug.Log($"✅ Peg {currentPeg.name} ENTERED hole {gameObject.name}");
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (currentPeg == null || other.gameObject != currentPeg) return;
        if (attachPoint == null) return;
        if (isInserted) return;
        
        ValidateForce();
        
        if (isForceValid)
        {
            validForceTimer += Time.deltaTime;
            
            if (validForceTimer >= validForceHoldTime)
            {
                Debug.Log($"⏱️ Valid force held for {validForceHoldTime}s - auto-snapping!");
                SnapPegToHole();
                return;
            }
        }
        else
        {
            validForceTimer = 0f;
        }
        
        Vector3 pegCenter = currentPeg.transform.position;
        Collider pegCollider = currentPeg.GetComponent<Collider>();
        if (pegCollider != null)
        {
            pegCenter = pegCollider.bounds.center;
        }
        
        if (showInGameVisuals && connectionLine != null && connectionLine.enabled)
        {
            connectionLine.SetPosition(0, pegCenter);
            connectionLine.SetPosition(1, attachPoint.position);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentPeg && !isInserted)
        {
            Debug.Log($"⚠️ Peg {currentPeg.name} EXITED hole {gameObject.name}");
            
            if (pegGrabInteractable != null)
            {
                pegGrabInteractable.selectEntered.RemoveListener(OnPegGrabbed);
                pegGrabInteractable.selectExited.RemoveListener(OnPegReleased);
            }
            
            if (showInGameVisuals)
            {
                if (connectionLine != null)
                {
                    connectionLine.enabled = false;
                }
                
                if (ringRenderer != null && ringMaterial != null)
                {
                    ringMaterial.color = defaultColor;
                    if (ringMaterial.HasProperty("_EmissionColor"))
                    {
                        ringMaterial.SetColor("_EmissionColor", Color.black);
                    }
                }
            }
            
            currentPeg = null;
            currentPegIdentifier = null;
            pegGrabInteractable = null;
            pegRigidbody = null;
            isInserted = false;
            isForceValid = false;
            validForceTimer = 0f;
        }
    }
    
    void ValidateForce()
    {
        if (currentPegIdentifier == null || isInserted)
        {
            return;
        }
        
        bool wasValid = isForceValid;
        isForceValid = ForceValidator.IsForceValid(currentPegIdentifier);
        
        if (wasValid != isForceValid)
        {
            PegColor pegColor = currentPegIdentifier.GetPegColor();
            float currentForce = currentPegIdentifier.GetCurrentForce();
            string requiredRange = ForceValidator.GetForceRangeText(currentPegIdentifier);
            
            Debug.Log($"Force validation changed for {currentPeg.name} ({pegColor}): {(isForceValid ? "VALID ✅" : "INVALID ❌")} (Force: {currentForce:F2}, Required: {requiredRange})");
            
            if (isForceValid)
            {
                validForceTimer = 0f;
            }
        }
        
        UpdateVisualFeedback();
    }
    
    void UpdateVisualFeedback()
    {
        if (!showInGameVisuals || isInserted) return;
        
        if (connectionLine != null && currentPeg != null)
        {
            connectionLine.enabled = true;
            Color lineColor = isForceValid ? readyColor : invalidForceColor;
            connectionLine.startColor = lineColor;
            connectionLine.endColor = lineColor;
        }
        
        if (ringRenderer != null && ringMaterial != null)
        {
            Color targetColor = isForceValid ? readyColor : invalidForceColor;
            ringMaterial.color = targetColor;
            
            if (ringMaterial.HasProperty("_EmissionColor"))
            {
                ringMaterial.EnableKeyword("_EMISSION");
                ringMaterial.SetColor("_EmissionColor", targetColor * 1.5f);
            }
        }
    }
    
    void OnPegGrabbed(SelectEnterEventArgs args)
    {
        if (isInserted)
        {
            Debug.Log($"🔓 Peg {currentPeg.name} GRABBED from hole {gameObject.name}");
            
            if (pegGrabInteractable != null)
            {
                pegGrabInteractable.enabled = true;
            }
            
            if (pegRigidbody != null)
            {
                pegRigidbody.isKinematic = false;
                pegRigidbody.useGravity = true;
            }
            
            isInserted = false;
            validForceTimer = 0f;
        }
    }
    
    void OnPegReleased(SelectExitEventArgs args)
    {
        if (currentPeg != null && !isInserted)
        {
            ValidateForce();
            
            if (!isForceValid)
            {
                string requiredRange = ForceValidator.GetForceRangeText(currentPegIdentifier);
                Debug.Log($"❌ Peg {currentPeg.name} RELEASED with invalid force - will NOT snap (Current: {currentPegIdentifier.GetCurrentForce():F2}, Required: {requiredRange})");
            }
        }
        
        validForceTimer = 0f;
    }
    
    void SnapPegToHole()
    {
        if (!isForceValid)
        {
            Debug.LogWarning($"❌ Cannot snap - force is invalid!");
            return;
        }
        
        if (isInserted)
        {
            return;
        }
        
        isInserted = true;
        //OnPegSnapped?.Invoke(); 
        OnPegSnapped?.Invoke(gameObject.name, currentPegIdentifier.GetPegColor());


        
        if (pegGrabInteractable != null && pegGrabInteractable.isSelected)
        {
            XRInteractionManager interactionManager = pegGrabInteractable.interactionManager;
            IXRSelectInteractor interactor = pegGrabInteractable.firstInteractorSelecting;
            
            if (interactionManager != null && interactor != null)
            {
                interactionManager.SelectExit(interactor, pegGrabInteractable);
                Debug.Log($"🔓 Force released peg from controller");
            }
        }
        
        Vector3 pegOffset = Vector3.zero;
        Collider pegCollider = currentPeg.GetComponent<Collider>();
        if (pegCollider != null)
        {
            pegOffset = currentPeg.transform.position - pegCollider.bounds.center;
        }
        
        currentPeg.transform.position = attachPoint.position + pegOffset;
        currentPeg.transform.rotation = attachPoint.rotation;
        
        if (pegRigidbody != null)
        {
            pegRigidbody.velocity = Vector3.zero;
            pegRigidbody.angularVelocity = Vector3.zero;
            pegRigidbody.isKinematic = true;
            pegRigidbody.useGravity = false;
        }
        
        if (pegGrabInteractable != null)
        {
            pegGrabInteractable.enabled = false;
        }
        
        if (connectionLine != null)
        {
            connectionLine.enabled = false;
        }
        
        if (showInGameVisuals && ringRenderer != null)
        {
            if (ringMaterial == null)
            {
                ringMaterial = ringRenderer.material;
            }
            
            ringMaterial.color = lockedColor;
            
            if (ringMaterial.HasProperty("_EmissionColor"))
            {
                ringMaterial.EnableKeyword("_EMISSION");
                ringMaterial.SetColor("_EmissionColor", lockedColor * 1.5f);
            }
            
            Debug.Log($"🎨 Ring color set to LOCKED: {lockedColor}");
        }
        
        Debug.Log($"🔒 Peg {currentPeg.name} LOCKED into hole {gameObject.name} at {attachPoint.position}");
    }
    
    void OnDestroy()
    {
        if (connectionLine != null)
        {
            Destroy(connectionLine.gameObject);
        }
    }
}
