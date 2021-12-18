using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float Z = -10;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        transform.position = new Vector3(0, MinY, Z);
    }

    void Update()
    {
        float transY = transform.position.y;
        if(Input.GetKey(KeyCode.LeftShift))
        {
            transY += Input.mouseScrollDelta.y * 5;
        }
        else
        {
            transY += Input.mouseScrollDelta.y;
        }
        transform.position = new Vector3(0, Mathf.Max(MinY, transY), Z);
    }

    private float MinY { get { return cam.orthographicSize; } }
}
