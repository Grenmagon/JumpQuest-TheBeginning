using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class Lever : MonoBehaviour
{
    private bool on = false;
    private bool interpolating = false;
    private float currentInterpolationTime = 0.0f;
    private InputAction interactAction;
    public bool playerInRange;
    [SerializeField] private float switchTime;
    [SerializeField] private Transform onPosition;
    [SerializeField] private Transform offPosition;
    [SerializeField] private GameObject leverHandle;

    [SerializeField] private LayerMask layerMask;

    [SerializeField] private UnityEvent onLeverActivated;
    [SerializeField] private UnityEvent onLeverDeactivated;
    void Start()
    {
        this.interactAction = InputSystem.actions.FindAction("Interact");
    }
    IEnumerator InterpolateLeverCoroutine()
    {
        this.interpolating = true;
        Vector3 startPosition, targetPosition;
        Quaternion startRotation, targetRotation;
        if (this.on)
        {
            startPosition = this.offPosition.position;
            startRotation = this.offPosition.rotation;
            targetPosition = this.onPosition.position;
            targetRotation = this.onPosition.rotation;
        }
        else
        {
            startPosition = this.onPosition.position;
            startRotation = this.onPosition.rotation;
            targetPosition = this.offPosition.position;
            targetRotation = this.offPosition.rotation;
        }

        this.currentInterpolationTime = 0.0f;
        while (this.currentInterpolationTime < this.switchTime)
        {
            float percentage = this.currentInterpolationTime / this.switchTime;
            var currentPosition = Vector3.Lerp(startPosition, targetPosition, percentage);
            var currentRotation = Quaternion.Slerp(startRotation, targetRotation, percentage);
            this.leverHandle.transform.SetPositionAndRotation(currentPosition, currentRotation);
            yield return null;
            this.currentInterpolationTime += Time.deltaTime;
        }
        this.leverHandle.transform.SetPositionAndRotation(targetPosition, targetRotation);
        this.interpolating = false;
        doStuff();
    }

    private void doStuff()
    {
        if (this.on)
            onLeverActivated.Invoke();
        else
            onLeverDeactivated.Invoke();
    }
    void ToggleLever()
    {
        if (!playerInRange)
            { return; }

        this.on = !this.on;
        this.StartCoroutine(this.InterpolateLeverCoroutine());


    }

    void FixedUpdate()
    {
        if (this.interactAction.WasPressedThisFrame() && !this.interpolating)
        {
            this.ToggleLever();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((layerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((layerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            playerInRange = false;
        }
    }
}