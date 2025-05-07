using UnityEngine;

public class CameraPivot : MonoBehaviour
{
    [SerializeField] private Transform pivot;
    [SerializeField] private float cameraRotSpeed;
    private bool canPivot = true;
    Vector3 initialPos;
    Quaternion initialRot;

    private void Awake()
    {
        initialPos = transform.position;
        initialRot = transform.rotation;
    }

    void Update()
    { 
        if(canPivot)
            transform.RotateAround(pivot.position, Vector3.up, cameraRotSpeed * Time.deltaTime);
    }

    public void TogglePivot()
    {
        canPivot = !canPivot;
    }

    public void SetToStartPos()
    {
        transform.position = initialPos;
        transform.rotation = initialRot;
    }
}
