using UnityEngine;

public class Pickup : MonoBehaviour
{
     private Collider _collider;
     private PlayerControls player;

    [Header("1:Ammo 2:Gun")]
    [SerializeField] private int _pickupType; 
    
    [Header("Ammo Pickup: ")]
    [SerializeField] private int _ammoCount;
    [SerializeField] private int _ammoType;
    
    [Header("Gun Pickup: ")]
    [SerializeField] private GameObject _gun;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _collider = GetComponent<Collider>();
    }
    void OnTriggerEnter(Collider otherCollider)
    {
        Debug.Log(otherCollider.gameObject);
        player = otherCollider.gameObject.GetComponent<PlayerControls>();
        if (player != null)
        {
            switch (_pickupType)
            {
                case 1:
                player.PickupAmmo(_ammoCount, _ammoType);
                break;
                case 2:
                player.PickupGun(_gun);
                break;
            }

            Destroy(gameObject);
        }
    }
}
