using UnityEngine;

public class CameraPivot : MonoBehaviour
{
    [SerializeField] private Transform pivot;
    [SerializeField] private float cameraRotSpeed;
    private bool canPivot = true;
    void Update()
    { 
        if(canPivot)
            transform.RotateAround(pivot.position, Vector3.up, cameraRotSpeed * Time.deltaTime);
    }

    public void TogglePivot()
    {
        canPivot = !canPivot;
    }
}
