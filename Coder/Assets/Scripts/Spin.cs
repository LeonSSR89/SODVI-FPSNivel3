using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField] private float _spinSpeed;
    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime* _spinSpeed, Space.Self);
    }
}
