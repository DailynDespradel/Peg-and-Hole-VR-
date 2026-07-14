using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HeadGazeController : MonoBehaviour
{
    [Header("Gaze Settings")]
    [SerializeField] private Camera gazeCamera;
    [SerializeField] private float maxGazeDistance = 10f;
    [SerializeField] private LayerMask uiLayerMask = -1;
    
    [Header("Reticle")]
    [SerializeField] private GameObject reticle;
    [SerializeField] private float reticleDistance = 2f;
    
    private XRInteractionManager interactionManager;
    
    void Start()
    {
        if (gazeCamera == null)
        {
            gazeCamera = Camera.main;
        }
        
        interactionManager = FindObjectOfType<XRInteractionManager>();
        
        if (reticle != null)
        {
            reticle.SetActive(true);
        }
    }
    
    void Update()
    {
        PerformGazeRaycast();
        UpdateReticlePosition();
    }
    
    void PerformGazeRaycast()
    {
        if (gazeCamera == null) return;
        
        Ray gazeRay = new Ray(gazeCamera.transform.position, gazeCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(gazeRay, out hit, maxGazeDistance, uiLayerMask))
        {
            Debug.DrawRay(gazeCamera.transform.position, gazeCamera.transform.forward * hit.distance, Color.green);
        }
        else
        {
            Debug.DrawRay(gazeCamera.transform.position, gazeCamera.transform.forward * maxGazeDistance, Color.red);
        }
    }
    
    void UpdateReticlePosition()
    {
        if (reticle == null || gazeCamera == null) return;
        
        Vector3 reticlePosition = gazeCamera.transform.position + gazeCamera.transform.forward * reticleDistance;
        reticle.transform.position = reticlePosition;
        reticle.transform.rotation = Quaternion.LookRotation(reticle.transform.position - gazeCamera.transform.position);
    }
}
