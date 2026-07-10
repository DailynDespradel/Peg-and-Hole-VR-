using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PokePointAttachment : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    public Transform dynamicAttachPoint;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnSelectEntered);
            grabInteractable.selectExited.AddListener(OnSelectExited);
        }
    }

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        IXRSelectInteractor interactor = args.interactorObject as IXRSelectInteractor;
        
        if (interactor != null)
        {
            Transform attachTransform = interactor.GetAttachTransform(this as IXRSelectInteractable);
            
            if (attachTransform != null)
            {
                CreateDynamicAttachPoint(attachTransform.position, attachTransform.rotation);
            }
        }
    }

    void OnSelectExited(SelectExitEventArgs args)
    {
        if (dynamicAttachPoint != null)
        {
            Destroy(dynamicAttachPoint.gameObject);
            dynamicAttachPoint = null;
        }
        
        grabInteractable.attachTransform = null;
    }

    void CreateDynamicAttachPoint(Vector3 worldPosition, Quaternion worldRotation)
    {
        if (dynamicAttachPoint == null)
        {
            GameObject attachPointObject = new GameObject("DynamicAttachPoint");
            dynamicAttachPoint = attachPointObject.transform;
            dynamicAttachPoint.SetParent(transform);
        }
        
        dynamicAttachPoint.position = worldPosition;
        dynamicAttachPoint.rotation = worldRotation;
        
        grabInteractable.attachTransform = dynamicAttachPoint;
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
            grabInteractable.selectExited.RemoveListener(OnSelectExited);
        }
        
        if (dynamicAttachPoint != null)
        {
            Destroy(dynamicAttachPoint.gameObject);
        }
    }
}
