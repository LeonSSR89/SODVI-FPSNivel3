using UnityEngine;
using UnityEngine.AI;

public class RangedEnemyAi : MonoBehaviour
{
    public GameObject Player;
    [SerializeField] private GameObject _trail;
    [SerializeField] private Transform _originTransform;

    private NavMeshAgent _agent;
    private AudioSource _audioSource;
    private float _distance, _curShootCooldown;
    [SerializeField] LayerMask _layermask;
    [SerializeField] AudioClip _shootSound;
    [SerializeField] private float _detectionRadius ,_damage, _shootCooldown, _spreadDist;
    [SerializeField] private Animator _enemyAnimator;

    private bool _hasSpottedPlayer = false;


    private RaycastHit _rayInfo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _agent = gameObject.GetComponent<NavMeshAgent>();
        _audioSource = GetComponent<AudioSource>();
        if (_originTransform == null) _originTransform = transform;

        _curShootCooldown = _shootCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if (Player != null)
        {
            _distance = (Player.transform.position - transform.position).magnitude;

            //Si el jugador está detrás de una pared, el enemigo no lo detectará
            if ((!_hasSpottedPlayer) && Physics.Raycast(transform.position, Player.transform.position - transform.position, out _rayInfo,_detectionRadius) && _rayInfo.collider.gameObject==Player)
            {
                Debug.Log("Spotted first time by " + gameObject.name);
                _hasSpottedPlayer = true;
            }

            //Condiciones para que el enemigo se mueva
            if (_detectionRadius <_distance || !_hasSpottedPlayer) {
                _agent.SetDestination(transform.position);
                if (_enemyAnimator!=null) _enemyAnimator.SetBool("Chasing", false);
                return;
            }

            //Condiciones para animación idle y de persecución
            if (_enemyAnimator!=null) _enemyAnimator.SetBool("Chasing", true);
            if (_agent.velocity.magnitude<=0.01)
            {
                if (_enemyAnimator!=null) {
                    _enemyAnimator.SetBool("Stopped", true);
                    //_enemyAnimator.SetBool("Chasing", false);
                }
            }
            else if (_enemyAnimator!=null) {
                    //_enemyAnimator.SetBool("Chasing", true);
                    _enemyAnimator.SetBool("Stopped", false);
                }


            //Enemigo se hace para atrás cuando jugador esta muy cerca
            if (_distance < _agent.stoppingDistance*2/3)
            {
                _agent.SetDestination(transform.position + (transform.position- Player.transform.position).normalized * _agent.stoppingDistance*2);
                if (_enemyAnimator!=null) _enemyAnimator.SetBool("Backing", true);

            }
            else {
                _agent.SetDestination(Player.transform.position);
                if (_enemyAnimator!=null) _enemyAnimator.SetBool("Backing", false);

            }

            //Condiciones para disparar al jugador
            if ((_distance <= _agent.stoppingDistance * 3/2) && !Physics.Raycast(transform.position, Player.transform.position - transform.position, _distance, LayerMask.GetMask("Default"))) {
                transform.LookAt( new Vector3 (Player.transform.position.x, transform.position.y, Player.transform.position.z), Vector3.up);
                Attack(Player);
                
            }
            
        }
        if (0 < _curShootCooldown) _curShootCooldown -= Time.deltaTime;
    }

    void Attack (GameObject target)
    {
        if (0 < _curShootCooldown) return;
        _curShootCooldown = _shootCooldown;
        if (_enemyAnimator!=null) {
            _enemyAnimator.SetTrigger("Fire");
        }

        Vector3 chosenAim = target.transform.position + target.transform.TransformVector((Random.value-0.5f)*_spreadDist, (Random.value-0.5f)*_spreadDist, 0);

        GameObject trail = Instantiate(_trail, _originTransform.position,  _originTransform.rotation);
        
        trail.transform.LookAt(chosenAim);
        trail.transform.Rotate(Vector3.right*90, Space.Self);
        trail.transform.localScale = trail.transform.localScale + new Vector3 (_distance*0.1f,0,0);

        StartCoroutine(DamageEntCoroutine(chosenAim, 0.5f));        
    }

    //Sirve para darle al jugador un momento para esquivar los disparos
    System.Collections.IEnumerator DamageEntCoroutine(Vector3 chosenAim, float damageDelay)
    {
        yield return new WaitForSeconds(damageDelay); 
        if(_audioSource!=null) _audioSource.PlayOneShot(_shootSound);
        if (Physics.Raycast(_originTransform.position, (chosenAim - _originTransform.position).normalized , out _rayInfo, _distance*2, _layermask))
        {
            if (_rayInfo.collider.gameObject.GetComponent<Health>()!= null) _rayInfo.collider.gameObject.GetComponent<Health>().TakeDamage(_damage);
        }
    }
}
