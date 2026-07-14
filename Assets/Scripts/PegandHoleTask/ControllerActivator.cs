using UnityEngine;

public class ControllerActivator : MonoBehaviour
{
    public enum InputMode
    {
        HandsOnly,
        ControllersOnly,
        Both
    }

    [Header("Input Mode Selection")]
    [SerializeField] private InputMode inputMode = InputMode.HandsOnly;

    [Header("References")]
    [SerializeField] private GameObject leftControllerObject;
    [SerializeField] private GameObject rightControllerObject;
    [SerializeField] private GameObject leftHandObject;
    [SerializeField] private GameObject rightHandObject;

    private InputMode currentMode;

    void Start()
    {
        currentMode = inputMode;
        ApplyInputMode();
    }

    void Update()
    {
        if (inputMode != currentMode)
        {
            currentMode = inputMode;
            ApplyInputMode();
        }
    }

    void ApplyInputMode()
    {
        switch (currentMode)
        {
            case InputMode.HandsOnly:
                SetHandsOnly();
                break;

            case InputMode.ControllersOnly:
                SetControllersOnly();
                break;

            case InputMode.Both:
                SetBothActive();
                break;
        }
    }

    void SetHandsOnly()
    {
        if (leftControllerObject != null)
            leftControllerObject.SetActive(false);

        if (rightControllerObject != null)
            rightControllerObject.SetActive(false);

        if (leftHandObject != null)
            leftHandObject.SetActive(true);

        if (rightHandObject != null)
            rightHandObject.SetActive(true);
    }

    void SetControllersOnly()
    {
        if (leftControllerObject != null)
            leftControllerObject.SetActive(true);

        if (rightControllerObject != null)
            rightControllerObject.SetActive(true);

        if (leftHandObject != null)
            leftHandObject.SetActive(false);

        if (rightHandObject != null)
            rightHandObject.SetActive(false);
    }

    void SetBothActive()
    {
        if (leftControllerObject != null)
            leftControllerObject.SetActive(true);

        if (rightControllerObject != null)
            rightControllerObject.SetActive(true);

        if (leftHandObject != null)
            leftHandObject.SetActive(true);

        if (rightHandObject != null)
            rightHandObject.SetActive(true);
    }

    public void SetInputMode(InputMode mode)
    {
        inputMode = mode;
    }

    public void SetHandsOnlyMode()
    {
        SetInputMode(InputMode.HandsOnly);
    }

    public void SetControllersOnlyMode()
    {
        SetInputMode(InputMode.ControllersOnly);
    }

    public void SetBothMode()
    {
        SetInputMode(InputMode.Both);
    }
}
