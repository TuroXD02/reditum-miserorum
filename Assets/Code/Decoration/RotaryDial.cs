using UnityEngine;
using UnityEngine.Events;

public class RotaryDial : MonoBehaviour
{
    [Header("Dial Setup")]
    public Transform dialVisual;            // Assign the visible part of the dial (child object)
    public float minAngle = 0f;
    public float maxAngle = 270f;
    public bool snapBack = true;
    public float rotationSpeed = 10f;       // Lerp speed

    [Header("Events")]
    public UnityEvent OnFullRotation;

    [SerializeField] private float dragThreshold = 1f; // Degrees threshold to register drag
    [SerializeField] private LayerMask dialLayerMask;

    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private float startDragAngle;
    private float initialMouseAngle;
    private bool isDragging = false;
    private bool isSnappingBack = false;

    void Update()
    {
        HandleInput();
        UpdateDialRotation();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseOverDial())
            {
                // Stop snap-back if it's winding
                isDragging = true;
                isSnappingBack = false;

                initialMouseAngle = GetMouseAngleRelativeToPivot();
                startDragAngle = currentAngle;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                if (Mathf.Abs(targetAngle - maxAngle) < 5f)
                {
                    OnFullRotation?.Invoke();
                }

                isDragging = false;

                if (snapBack)
                {
                    isSnappingBack = true;
                    targetAngle = minAngle;
                }
            }
        }

        if (isDragging)
        {
            float currentMouseAngle = GetMouseAngleRelativeToPivot();
            float angleDelta = Mathf.DeltaAngle(initialMouseAngle, currentMouseAngle);

            if (Mathf.Abs(angleDelta) > dragThreshold)
            {
                targetAngle = Mathf.Clamp(startDragAngle + angleDelta, minAngle, maxAngle);
            }
        }
    }

    private void UpdateDialRotation()
    {
        if (!isDragging && isSnappingBack)
        {
            currentAngle = Mathf.LerpAngle(currentAngle, minAngle, Time.deltaTime * rotationSpeed);

            if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, minAngle)) < 0.5f)
            {
                currentAngle = minAngle;
                isSnappingBack = false;
            }
        }
        else
        {
            currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);
        }

        dialVisual.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }

    private float GetMouseAngleRelativeToPivot()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 pivotScreenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 dir = mousePos - pivotScreenPos;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    private bool IsMouseOverDial()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, dialLayerMask))
        {
            return hit.transform == dialVisual;
        }

        // Default true for simplicity if no collider is used
        return true;
    }
}
