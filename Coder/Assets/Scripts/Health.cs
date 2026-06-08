using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class Health : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float _health;
    [SerializeField] private float _maxHealth, _disappearTime;
    [SerializeField] private GameObject _camera, _hitGuiItem, _guiCanvas, _healthbar, _loseMenu;
    private CinemachineBasicMultiChannelPerlin _perlinNoise;
    private bool _dying = false;

    void Start ()
    {
        _health = _maxHealth;
        if (_camera == null) return;
        _perlinNoise = _camera.GetComponent<CinemachineCamera>().GetComponent<CinemachineBasicMultiChannelPerlin>();
    }
    public void TakeDamage(float damage)
    {
        if (_dying) return;
        if ((_camera!=null) && (damage > 0))
        {
            StartCoroutine(HitCamEffect());
        }
        
        _health -= damage;
        if (_health <= 0)
        {
            _dying = true;
            _health = 0;
            
            Destroy(gameObject.GetComponent<RangedEnemyAi>());
            Destroy(gameObject.GetComponent<Collider>());
            Destroy(gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>());
            //Debug.Log(GetComponent<Animator>());
            if (tag == "Player") {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0.3f;
                _loseMenu.SetActive(true);
            }
            if (GetComponent<Animator>()!=null)
            {
                GetComponent<Animator>().SetTrigger("Death");
                Destroy(gameObject, _disappearTime);
            }
            else Destroy(gameObject, _disappearTime);
            
        }

        if (_healthbar != null) _healthbar.transform.localScale = new Vector3(_health/_maxHealth,1,1);
    }

    IEnumerator HitCamEffect()
    {
        GameObject _guiItem = Instantiate(_hitGuiItem, _guiCanvas.transform);
        _perlinNoise.AmplitudeGain = 1;
        yield return new WaitForSeconds(0.1f);
        Destroy(_guiItem);
        _perlinNoise.AmplitudeGain = 0;
    }
}
