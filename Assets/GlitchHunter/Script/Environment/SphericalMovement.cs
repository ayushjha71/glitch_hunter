using UnityEngine;

public class SphericalMovement : MonoBehaviour
{
    [Header("Object Array")]
    public GameObject[] objectsToMove;
    public Transform centerPoint;

    [Header("Movement Settings")]
    public float moveDuration = 3f;
    public float maxDistance = 5f;
    public bool moveOnStart = true;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Arrangement Settings")]
    public float initialRadius = 1f;
    [Range(0, 90)] public float elevationAngle = 30f;
    public bool randomizeInitialAngles = false;

    [Header("Debug")]
    [SerializeField] private bool isMoving = false;
    [SerializeField] private float moveProgress = 0f;
    private Vector3[] initialSphericalCoords;
    private Vector3[] targetSphericalCoords;

    void Start()
    {
        if (centerPoint == null) centerPoint = transform;

        InitializeObjectPositions();
    }

    void InitializeObjectPositions()
    {
        initialSphericalCoords = new Vector3[objectsToMove.Length];
        targetSphericalCoords = new Vector3[objectsToMove.Length];

        float angleIncrement = 360f / objectsToMove.Length;
        System.Random rand = new System.Random(randomizeInitialAngles ? System.DateTime.Now.Millisecond : 0);

        for (int i = 0; i < objectsToMove.Length; i++)
        {
            if (objectsToMove[i] == null) continue;

            // Calculate initial angles
            float azimuth = randomizeInitialAngles ?
                (float)rand.NextDouble() * 360f :
                i * angleIncrement;

            float elevation = randomizeInitialAngles ?
                (float)rand.NextDouble() * elevationAngle :
                elevationAngle;

            // Set spherical coordinates
            initialSphericalCoords[i] = new Vector3(
                initialRadius,
                azimuth * Mathf.Deg2Rad,
                elevation * Mathf.Deg2Rad
            );

            targetSphericalCoords[i] = new Vector3(
                maxDistance,
                initialSphericalCoords[i].y,
                initialSphericalCoords[i].z
            );
        }
    }

    void Update()
    {
        if (!isMoving) return;

        moveProgress = Mathf.Clamp01(moveProgress + Time.deltaTime / moveDuration);
        float easedProgress = movementCurve.Evaluate(moveProgress);

        for (int i = 0; i < objectsToMove.Length; i++)
        {
            if (objectsToMove[i] == null) continue;

            // Interpolate between initial and target positions
            Vector3 currentSpherical = Vector3.Lerp(
                initialSphericalCoords[i],
                targetSphericalCoords[i],
                easedProgress
            );

            objectsToMove[i].transform.position = GetSphericalPosition(currentSpherical);
        }

        if (moveProgress >= 1f)
        {
            StopMoving();
        }
    }

    Vector3 GetSphericalPosition(Vector3 sphericalCoords)
    {
        return centerPoint.position + SphericalToCartesian(sphericalCoords);
    }

    public void StartMoving()
    {
        if (isMoving) return;
        isMoving = true;
        moveProgress = 0f;
    }

    public void StopMoving()
    {
        isMoving = false;
        Debug.Log("Movement completed");
    }

    public void ResetPositions()
    {
        moveProgress = 0f;
        for (int i = 0; i < objectsToMove.Length; i++)
        {
            if (objectsToMove[i] == null) continue;
            objectsToMove[i].transform.position = GetSphericalPosition(initialSphericalCoords[i]);
        }
    }

    // Spherical to Cartesian conversion with better handling of edge cases
    Vector3 SphericalToCartesian(Vector3 spherical)
    {
        float r = spherical.x;
        float theta = spherical.y; // azimuthal angle
        float phi = spherical.z; // polar angle

        // Handle potential division by zero
        if (Mathf.Approximately(r, 0f)) return Vector3.zero;

        float sinPhi = Mathf.Sin(phi);
        return new Vector3(
            r * sinPhi * Mathf.Cos(theta),
            r * Mathf.Cos(phi),
            r * sinPhi * Mathf.Sin(theta)
        );
    }

    // Visual debugging
    void OnDrawGizmosSelected()
    {
        if (centerPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerPoint.position, maxDistance);

        if (Application.isPlaying && objectsToMove != null && isMoving)
        {
            Gizmos.color = Color.cyan;
            foreach (var obj in objectsToMove)
            {
                if (obj != null)
                {
                    Gizmos.DrawLine(centerPoint.position, obj.transform.position);
                }
            }
        }
    }
}