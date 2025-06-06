using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class RotaryDial : MonoBehaviour
{
    [Header("Dial Setup")]
    public Transform dialVisual;               // The rotating visual
    public float rewindSpeed;           // Max degrees per second during rewind
    public float slowDownThreshold = 0.2f;     // Slow down when under 20% of total rewind
    public AnimationCurve slowDownCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // Eases out

    [Header("Events")]
    public UnityEvent OnFullRotation;

    [SerializeField] private LayerMask dialLayerMask;

    private float currentAngle = 0f;
    private float totalDraggedRotation = 0f;
    private float rewindRemainingRotation = 0f;
    private float rewindTotalRotation = 0f;

    private bool isDragging = false;
    private bool isRewinding = false;

    private float lastMouseAngle = 0f;

    void Update()
    {
        HandleInput();
        UpdateDial();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) && IsMouseOverDial())
        {
            if (isRewinding)
            {
                // Interrupt rewind
                isRewinding = false;
                rewindRemainingRotation = 0f;
                totalDraggedRotation = 0f;
            }

            isDragging = true;
            lastMouseAngle = GetMouseAngle();
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            rewindRemainingRotation = totalDraggedRotation;
            rewindTotalRotation = totalDraggedRotation;
            isRewinding = true;

            if (Mathf.Abs(totalDraggedRotation) >= 360f)
                OnFullRotation?.Invoke();
        }

        if (isDragging)
        {
            float currentMouseAngle = GetMouseAngle();
            float delta = Mathf.DeltaAngle(lastMouseAngle, currentMouseAngle);
            totalDraggedRotation += delta;
            currentAngle += delta;
            lastMouseAngle = currentMouseAngle;
        }
    }

    private void UpdateDial()
    {
        if (isRewinding)
        {
            float rewindProgress = 1f - Mathf.Clamp01(Mathf.Abs(rewindRemainingRotation / rewindTotalRotation));
            float slowDownFactor = 1f;

            if (rewindProgress > 1f - slowDownThreshold)
            {
                float t = (rewindProgress - (1f - slowDownThreshold)) / slowDownThreshold;
                slowDownFactor = slowDownCurve.Evaluate(t);
            }

            float direction = Mathf.Sign(rewindRemainingRotation);
            float delta = rewindSpeed * slowDownFactor * Time.deltaTime * direction;

            if (Mathf.Abs(delta) > Mathf.Abs(rewindRemainingRotation))
                delta = rewindRemainingRotation;

            currentAngle -= delta;
            rewindRemainingRotation -= delta;

            if (Mathf.Approximately(rewindRemainingRotation, 0f))
            {
                isRewinding = false;
                totalDraggedRotation = 0f;
            }
        }

        dialVisual.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }

    private float GetMouseAngle()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mouseWorld - transform.position;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    private bool IsMouseOverDial()
    {
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero, 0f, dialLayerMask);
        return hit.collider != null && hit.collider.transform == dialVisual;
    }
}
