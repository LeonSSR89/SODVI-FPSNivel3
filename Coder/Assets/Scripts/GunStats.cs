using UnityEngine;

public class GunStats : MonoBehaviour
{
    public float range, damage, shootCooldown;
    [Header("0:Light 1:Medium, 2:Heavy")]
    public int ammoType;
    public int clipSize;
    public AudioClip shootSound, reloadSound;
    public Sprite UIImage;
}
