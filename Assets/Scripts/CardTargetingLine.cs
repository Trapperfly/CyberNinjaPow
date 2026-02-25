using UnityEngine;

/// <summary>
/// Attach this script to a GameObject that has a LineRenderer component.
/// The line starts at this object's position, arches upward, and ends at the mouse cursor's world position.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class CardTargetingLine : MonoBehaviour
{
    public Vector2 startPos;
    public Vector3 endPos;
    [Header("Line Settings")]
    [Tooltip("Number of points along the arc. More = smoother curve.")]
    [Range(2, 64)]
    public int pointCount = 20;

    [Tooltip("How high the arc peaks relative to the midpoint between start and mouse.")]
    public float archHeight = 2f;

    [Tooltip("Number of points along the arc. More = smoother curve.")]
    [Range(0.2f, 0.8f)]
    public float archTopPosition = 0.5f;

    [Header("Mouse Depth")]
    [Tooltip("World-space Z depth at which the mouse position is calculated (for 2D/top-down use 0).")]
    public float mouseWorldDepth = 10f;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = pointCount;
    }

    private void Update()
    {
        // Ensure positionCount stays in sync with the inspector value at runtime
        if (_lineRenderer.positionCount != pointCount)
            _lineRenderer.positionCount = pointCount;

        if (!Manager.Instance.boardManager.inCardAction)
        endPos = GetMouseWorldPosition();

        DrawArch(startPos, endPos);
    }

    private void DrawArch(Vector3 start, Vector3 end)
    {
        Vector3 midPoint = (start + end) * archTopPosition;

        // Lift the control point upward (or in the local "up" direction) to create the arch
        Vector3 controlPoint = midPoint + Vector3.up * archHeight;

        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1); // 0 to 1
            Vector3 point = QuadraticBezier(start, controlPoint, end, t);
            _lineRenderer.SetPosition(i, point);
        }
    }

    /// <summary>
    /// Evaluates a quadratic Bezier curve at parameter t (0–1).
    /// </summary>
    private Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return (u * u * p0) + (2f * u * t * p1) + (t * t * p2);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = mouseWorldDepth;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}