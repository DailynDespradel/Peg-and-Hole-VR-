// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.XR;
// using UnityEngine.XR.Interaction.Toolkit;
// using UnityEngine.InputSystem;

// public class TriggerLoadingIndicator : MonoBehaviour
// {
//     [SerializeField]
//     private FloatUnityEvent m_onLoad;
    
//     private bool isGrabbed = false;
//     private ActionBasedController controller;

//     private XRGrabInteractable grabInteractable;

//     void Start()
//     {
//         grabInteractable = GetComponentInParent<XRGrabInteractable>();
        
//         if (grabInteractable != null)
//         {
//             Debug.Log($"TriggerLoadingIndicator: Found XRGrabInteractable on {grabInteractable.name}");
//             grabInteractable.selectEntered.AddListener(OnGrabbed);
//             grabInteractable.selectExited.AddListener(OnReleased);
//         }
//         else
//         {
//             Debug.LogError($"TriggerLoadingIndicator: No XRGrabInteractable found in parent of {gameObject.name}!");
//         }
//     }

//     void OnGrabbed(SelectEnterEventArgs args)
//     {
//         isGrabbed = true;
//         Debug.Log($"TriggerLoadingIndicator: Peg GRABBED!");
        
//         IXRSelectInteractor interactor = args.interactorObject as IXRSelectInteractor;
//         if (interactor != null)
//         {
//             controller = interactor.transform.GetComponentInParent<ActionBasedController>();
//             if (controller != null)
//             {
//                 Debug.Log($"TriggerLoadingIndicator: Found ActionBasedController on {controller.name}");
//             }
//             else
//             {
//                 Debug.LogWarning("TriggerLoadingIndicator: No ActionBasedController found!");
//             }
//         }
//     }

//     void OnReleased(SelectExitEventArgs args)
//     {
//         isGrabbed = false;
//         controller = null;
//         m_onLoad.Invoke(0f);
//         Debug.Log($"TriggerLoadingIndicator: Peg RELEASED!");
//     }

//     void Update()
//     {
//         if (!isGrabbed || controller == null)
//         {
//             m_onLoad.Invoke(0f);
//             return;
//         }
        
//         float triggerValue = controller.activateActionValue.action.ReadValue<float>();
        
//         Debug.Log($"Update: Trigger from ActionBasedController = {triggerValue:F3}");
        
//         m_onLoad.Invoke(triggerValue);
//     }

//     void OnDestroy()
//     {
//         if (grabInteractable != null)
//         {
//             grabInteractable.selectEntered.RemoveListener(OnGrabbed);
//             grabInteractable.selectExited.RemoveListener(OnReleased);
//         }
//     }
// }
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class TriggerLoadingIndicator : MonoBehaviour
{
    [SerializeField]
    private FloatUnityEvent m_onLoad;
    
    private bool isGrabbed = false;
    private ActionBasedController controller;
    private XRGrabInteractable grabInteractable;
    private IPegIdentifier pegIdentifier;


    void Start()
    {
        grabInteractable = GetComponentInParent<XRGrabInteractable>();
        pegIdentifier = GetComponentInParent<IPegIdentifier>();

        
        if (grabInteractable != null)
        {
            Debug.Log($"TriggerLoadingIndicator: Found XRGrabInteractable on {grabInteractable.name}");
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
        else
        {
            Debug.LogError($"TriggerLoadingIndicator: No XRGrabInteractable found in parent of {gameObject.name}!");
        }
        
        if (pegIdentifier != null)
        {
            Debug.Log($"TriggerLoadingIndicator: Found PegIdentifier, will update force values");
        }
        else
        {
            Debug.LogWarning($"TriggerLoadingIndicator: No PegIdentifier found on {gameObject.name}");
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        Debug.Log($"TriggerLoadingIndicator: Peg GRABBED!");
        
        IXRSelectInteractor interactor = args.interactorObject as IXRSelectInteractor;
        if (interactor != null)
        {
            controller = interactor.transform.GetComponentInParent<ActionBasedController>();
            if (controller != null)
            {
                Debug.Log($"TriggerLoadingIndicator: Found ActionBasedController on {controller.name}");
            }
            else
            {
                Debug.LogWarning("TriggerLoadingIndicator: No ActionBasedController found!");
            }
        }
    }

    void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        controller = null;
        float releaseValue = 0f;
        
        m_onLoad.Invoke(releaseValue);
        
        if (pegIdentifier != null)
        {
            pegIdentifier.UpdateForceFromTrigger(releaseValue);
        }
        
        Debug.Log($"TriggerLoadingIndicator: Peg RELEASED!");
    }

    void Update()
    {
        float triggerValue = 0f;
        
        if (isGrabbed && controller != null)
        {
            triggerValue = controller.activateActionValue.action.ReadValue<float>();
            Debug.Log($"Update: Trigger from ActionBasedController = {triggerValue:F3}");
        }
        
        m_onLoad.Invoke(triggerValue);
        
        if (pegIdentifier != null)
        {
            pegIdentifier.UpdateForceFromTrigger(triggerValue);
        }
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}
