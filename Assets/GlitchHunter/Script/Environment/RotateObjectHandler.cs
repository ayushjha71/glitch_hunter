using UnityEngine;

public class RotateObjectHandler : MonoBehaviour
{
    [SerializeField]
    private float xAxis = 50;
    [SerializeField]
    private float yAxis = 50;
    [SerializeField]
    private float zAxis = 50;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(xAxis * Time.deltaTime, yAxis * Time.deltaTime, zAxis * Time.deltaTime);  
    }
}
