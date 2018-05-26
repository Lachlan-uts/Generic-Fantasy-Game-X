using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public Transform Target;
    public Rigidbody2D Body;
    public Vector3 Offset;

    public float SmoothTime = 0.3F;
    private Vector3 Vel;
    Vector3 Last;

    void Awake()
    {
        Last = transform.position;
    }

    void LateUpdate()
    {
        var newPos = Vector3.SmoothDamp(Last, (Body == null) ? Target.position : (Vector3)Body.position, ref Vel, SmoothTime);
        Last = newPos;
        transform.position = newPos + Offset;
    }
}
