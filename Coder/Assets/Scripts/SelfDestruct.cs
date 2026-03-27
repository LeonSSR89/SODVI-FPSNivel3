using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [SerializeField] private float _timer;
    void FixedUpdate()
    {
        if (_timer <= 0) Destroy(gameObject);
        _timer -= Time.fixedDeltaTime;
    }
}
