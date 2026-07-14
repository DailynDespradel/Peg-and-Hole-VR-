// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;

// public class PegHoleInsertion : MonoBehaviour
// {
//     [Header("Insertion Settings")]
//     [SerializeField] private Transform attachPoint;
//     [SerializeField] private bool snapImmediately = true;
    
//     [Header("In-Game Visual Feedback")]
//     [SerializeField] private bool showInGameVisuals = true;
//     [SerializeField] private Renderer ringRenderer;
//     [SerializeField] private Color readyColor = Color.yellow;
//     [SerializeField] private Color lockedColor = Color.green;
//     [SerializeField] private Color defaultColor = Color.blue;
    
//     private GameObject currentPeg;
//     private XRGrabInteractable pegGrabInteractable;
//     private Rigidbody pegRigidbody;
//     private bool isInserted = false;
    
//     private LineRenderer connectionLine;
//     private Material ringMaterial;
//     private FixedJoint lockJoint;
    
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
//             pegGrabInteractable = currentPeg.GetComponent<XRGrabInteractable>();
//             pegRigidbody = currentPeg.GetComponent<Rigidbody>();
            
//             if (pegGrabInteractable != null)
//             {
//                 pegGrabInteractable.selectEntered.AddListener(OnPegGrabbed);
//                 pegGrabInteractable.selectExited.AddListener(OnPegReleased);
//             }
            
//             if (showInGameVisuals)
//             {
//                 if (connectionLine != null)
//                 {
//                     connectionLine.enabled = true;
//                 }
                
//                 if (ringRenderer != null && ringMaterial != null)
//                 {
//                     ringMaterial.color = readyColor;
//                     if (ringMaterial.HasProperty("_EmissionColor"))
//                     {
//                         ringMaterial.EnableKeyword("_EMISSION");
//                         ringMaterial.SetColor("_EmissionColor", readyColor * 1.5f);
//                     }
//                 }
//             }
            
//             Debug.Log($"✅ Peg {currentPeg.name} ENTERED hole {gameObject.name}");
            
//             // Snap immediately when peg enters if not being held
//             if (snapImmediately && !isInserted && pegGrabInteractable != null && !pegGrabInteractable.isSelected)
//             {
//                 SnapPegToHole();
//             }
//         }
//     }
    
//     void OnTriggerStay(Collider other)
//     {
//         if (currentPeg == null || other.gameObject != currentPeg) return;
//         if (attachPoint == null) return;
        
//         // Get the peg's collider center in world space
//         Vector3 pegCenter = currentPeg.transform.position;
//         Collider pegCollider = currentPeg.GetComponent<Collider>();
//         if (pegCollider != null)
//         {
//             pegCenter = pegCollider.bounds.center;
//         }
        
//         // Update line renderer visual - draw from peg center to attach point
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
//             pegGrabInteractable = null;
//             pegRigidbody = null;
//             isInserted = false;
//         }
//     }
    
//     void OnPegGrabbed(SelectEnterEventArgs args)
//     {
//         if (isInserted)
//         {
//             Debug.Log($"🔓 Peg {currentPeg.name} GRABBED from hole {gameObject.name}");
            
//             // Remove the joint to unlock
//             if (lockJoint != null)
//             {
//                 Destroy(lockJoint);
//                 lockJoint = null;
//             }
            
//             isInserted = false;
//         }
//     }
    
//     void OnPegReleased(SelectExitEventArgs args)
//     {
//         // When peg is released while inside the hole trigger, snap it
//         if (currentPeg != null && !isInserted && snapImmediately)
//         {
//             Debug.Log($"📍 Peg {currentPeg.name} RELEASED in hole {gameObject.name} - snapping!");
//             SnapPegToHole();
//         }
//     }
    
//     void SnapPegToHole()
//     {
//         isInserted = true;
        
//         // Calculate offset between peg transform and its collider center
//         Vector3 pegOffset = Vector3.zero;
//         Collider pegCollider = currentPeg.GetComponent<Collider>();
//         if (pegCollider != null)
//         {
//             pegOffset = currentPeg.transform.position - pegCollider.bounds.center;
//         }
        
//         // Snap peg transform, accounting for collider offset
//         currentPeg.transform.position = attachPoint.position + pegOffset;
//         currentPeg.transform.rotation = attachPoint.rotation;
        
//         if (pegRigidbody != null)
//         {
//             pegRigidbody.velocity = Vector3.zero;
//             pegRigidbody.angularVelocity = Vector3.zero;
            
//             // Use a joint instead of kinematic
//             lockJoint = currentPeg.AddComponent<FixedJoint>();
            
//             // Try to find a rigidbody on the pegboard to connect to
//             Rigidbody parentRB = GetComponentInParent<Rigidbody>();
//             if (parentRB != null)
//             {
//                 lockJoint.connectedBody = parentRB;
//             }
//             else
//             {
//                 // If no rigidbody, the joint will be fixed to world space at this position
//                 lockJoint.connectedAnchor = attachPoint.position;
//             }
//         }
        
//         // Change to green when locked
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
        
//         if (lockJoint != null)
//         {
//             Destroy(lockJoint);
//         }
//     }
// }



// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;

// public class PegHoleInsertion : MonoBehaviour
// {
//     [Header("Insertion Settings")]
//     [SerializeField] private Transform attachPoint;
//     [SerializeField] private bool snapImmediately = true;
    
//     [Header("In-Game Visual Feedback")]
//     [SerializeField] private bool showInGameVisuals = true;
//     [SerializeField] private Renderer ringRenderer;
//     [SerializeField] private Color readyColor = Color.yellow;
//     [SerializeField] private Color lockedColor = Color.green;
//     [SerializeField] private Color defaultColor = Color.blue;
    
//     private GameObject currentPeg;
//     private XRGrabInteractable pegGrabInteractable;
//     private Rigidbody pegRigidbody;
//     private bool isInserted = false;
    
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
//             pegGrabInteractable = currentPeg.GetComponent<XRGrabInteractable>();
//             pegRigidbody = currentPeg.GetComponent<Rigidbody>();
            
//             if (pegGrabInteractable != null)
//             {
//                 pegGrabInteractable.selectEntered.AddListener(OnPegGrabbed);
//                 pegGrabInteractable.selectExited.AddListener(OnPegReleased);
//             }
            
//             if (showInGameVisuals)
//             {
//                 if (connectionLine != null)
//                 {
//                     connectionLine.enabled = true;
//                 }
                
//                 if (ringRenderer != null && ringMaterial != null)
//                 {
//                     ringMaterial.color = readyColor;
//                     if (ringMaterial.HasProperty("_EmissionColor"))
//                     {
//                         ringMaterial.EnableKeyword("_EMISSION");
//                         ringMaterial.SetColor("_EmissionColor", readyColor * 1.5f);
//                     }
//                 }
//             }
            
//             Debug.Log($"✅ Peg {currentPeg.name} ENTERED hole {gameObject.name}");
            
//             // Snap immediately when peg enters if not being held
//             if (snapImmediately && !isInserted && pegGrabInteractable != null && !pegGrabInteractable.isSelected)
//             {
//                 SnapPegToHole();
//             }
//         }
//     }
    
//     void OnTriggerStay(Collider other)
//     {
//         if (currentPeg == null || other.gameObject != currentPeg) return;
//         if (attachPoint == null) return;
        
//         // Get the peg's collider center in world space
//         Vector3 pegCenter = currentPeg.transform.position;
//         Collider pegCollider = currentPeg.GetComponent<Collider>();
//         if (pegCollider != null)
//         {
//             pegCenter = pegCollider.bounds.center;
//         }
        
//         // Update line renderer visual - draw from peg center to attach point
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
//             pegGrabInteractable = null;
//             pegRigidbody = null;
//             isInserted = false;
//         }
//     }
    
//     void OnPegGrabbed(SelectEnterEventArgs args)
//     {
//         if (isInserted)
//         {
//             Debug.Log($"🔓 Peg {currentPeg.name} GRABBED from hole {gameObject.name}");
            
//             // Re-enable physics when grabbed
//             if (pegRigidbody != null)
//             {
//                 pegRigidbody.isKinematic = false;
//                 pegRigidbody.useGravity = false;
//             }
            
//             isInserted = false;
//         }
//     }
    
//     void OnPegReleased(SelectExitEventArgs args)
//     {
//         // Re-enable gravity when released
//         if (pegRigidbody != null && !isInserted)
//         {
//             pegRigidbody.useGravity = true;
//         }
        
//         // When peg is released while inside the hole trigger, snap it
//         if (currentPeg != null && !isInserted && snapImmediately)
//         {
//             Debug.Log($"📍 Peg {currentPeg.name} RELEASED in hole {gameObject.name} - snapping!");
//             SnapPegToHole();
//         }
//     }
    
//     void SnapPegToHole()
//     {
//         isInserted = true;
        
//         // Calculate offset between peg transform and its collider center
//         Vector3 pegOffset = Vector3.zero;
//         Collider pegCollider = currentPeg.GetComponent<Collider>();
//         if (pegCollider != null)
//         {
//             pegOffset = currentPeg.transform.position - pegCollider.bounds.center;
//         }
        
//         // Snap peg transform, accounting for collider offset
//         currentPeg.transform.position = attachPoint.position + pegOffset;
//         currentPeg.transform.rotation = attachPoint.rotation;
        
//         if (pegRigidbody != null)
//         {
//             pegRigidbody.velocity = Vector3.zero;
//             pegRigidbody.angularVelocity = Vector3.zero;
            
//             // Make kinematic to completely lock it in place (no vibration)
//             pegRigidbody.isKinematic = true;
//             pegRigidbody.useGravity = false;
//         }
        
//         // Change to green when locked
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

public class PegHoleInsertion : MonoBehaviour
{
    [Header("Insertion Settings")]
    [SerializeField] private Transform attachPoint;
    [SerializeField] private bool snapImmediately = true;
    
    [Header("In-Game Visual Feedback")]
    [SerializeField] private bool showInGameVisuals = true;
    [SerializeField] private Renderer ringRenderer;
    [SerializeField] private Color readyColor = Color.yellow;
    [SerializeField] private Color lockedColor = Color.green;
    [SerializeField] private Color defaultColor = Color.blue;
    
    private GameObject currentPeg;
    private XRGrabInteractable pegGrabInteractable;
    private Rigidbody pegRigidbody;
    private bool isInserted = false;
    
    private LineRenderer connectionLine;
    private Material ringMaterial;
    
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
            pegGrabInteractable = currentPeg.GetComponent<XRGrabInteractable>();
            pegRigidbody = currentPeg.GetComponent<Rigidbody>();
            
            if (pegGrabInteractable != null)
            {
                pegGrabInteractable.selectEntered.AddListener(OnPegGrabbed);
                pegGrabInteractable.selectExited.AddListener(OnPegReleased);
            }
            
            if (showInGameVisuals)
            {
                if (connectionLine != null)
                {
                    connectionLine.enabled = true;
                }
                
                if (ringRenderer != null && ringMaterial != null)
                {
                    ringMaterial.color = readyColor;
                    if (ringMaterial.HasProperty("_EmissionColor"))
                    {
                        ringMaterial.EnableKeyword("_EMISSION");
                        ringMaterial.SetColor("_EmissionColor", readyColor * 1.5f);
                    }
                }
            }
            
            Debug.Log($"✅ Peg {currentPeg.name} ENTERED hole {gameObject.name}");
            
            // Snap immediately when peg enters if not being held
            if (snapImmediately && !isInserted && pegGrabInteractable != null && !pegGrabInteractable.isSelected)
            {
                SnapPegToHole();
            }
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (currentPeg == null || other.gameObject != currentPeg) return;
        if (attachPoint == null) return;
        
        // Get the peg's collider center in world space
        Vector3 pegCenter = currentPeg.transform.position;
        Collider pegCollider = currentPeg.GetComponent<Collider>();
        if (pegCollider != null)
        {
            pegCenter = pegCollider.bounds.center;
        }
        
        // Update line renderer visual - draw from peg center to attach point
        if (showInGameVisuals && connectionLine != null && connectionLine.enabled)
        {
            connectionLine.SetPosition(0, pegCenter);
            connectionLine.SetPosition(1, attachPoint.position);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentPeg)
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
                
                if (ringRenderer != null && ringMaterial != null && !isInserted)
                {
                    ringMaterial.color = defaultColor;
                    if (ringMaterial.HasProperty("_EmissionColor"))
                    {
                        ringMaterial.SetColor("_EmissionColor", Color.black);
                    }
                }
            }
            
            currentPeg = null;
            pegGrabInteractable = null;
            pegRigidbody = null;
            isInserted = false;
        }
    }
    
    void OnPegGrabbed(SelectEnterEventArgs args)
    {
        if (isInserted)
        {
            Debug.Log($"🔓 Peg {currentPeg.name} GRABBED from hole {gameObject.name}");
            
            // Re-enable physics when grabbed
            if (pegRigidbody != null)
            {
                pegRigidbody.isKinematic = false;
                pegRigidbody.useGravity = true;
            }
            
            isInserted = false;
        }
    }
    
    void OnPegReleased(SelectExitEventArgs args)
    {
        // Always ensure gravity is on when released (unless being snapped)
        if (pegRigidbody != null)
        {
            pegRigidbody.useGravity = true;
        }
        
        // When peg is released while inside the hole trigger, snap it
        if (currentPeg != null && !isInserted && snapImmediately)
        {
            Debug.Log($"📍 Peg {currentPeg.name} RELEASED in hole {gameObject.name} - snapping!");
            SnapPegToHole();
        }
    }
    
    void SnapPegToHole()
    {
        isInserted = true;
        
        // Calculate offset between peg transform and its collider center
        Vector3 pegOffset = Vector3.zero;
        Collider pegCollider = currentPeg.GetComponent<Collider>();
        if (pegCollider != null)
        {
            pegOffset = currentPeg.transform.position - pegCollider.bounds.center;
        }
        
        // Snap peg transform, accounting for collider offset
        currentPeg.transform.position = attachPoint.position + pegOffset;
        currentPeg.transform.rotation = attachPoint.rotation;
        
        if (pegRigidbody != null)
        {
            pegRigidbody.velocity = Vector3.zero;
            pegRigidbody.angularVelocity = Vector3.zero;
            
            // Make kinematic to completely lock it in place (no vibration)
            pegRigidbody.isKinematic = true;
            pegRigidbody.useGravity = false;
        }
        
        // Change to green when locked
        if (showInGameVisuals && ringRenderer != null && ringMaterial != null)
        {
            ringMaterial.color = lockedColor;
            if (ringMaterial.HasProperty("_EmissionColor"))
            {
                ringMaterial.SetColor("_EmissionColor", lockedColor * 1.5f);
            }
        }
        
        if (connectionLine != null)
        {
            connectionLine.enabled = false;
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
