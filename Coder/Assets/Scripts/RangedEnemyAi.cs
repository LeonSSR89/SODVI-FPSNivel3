using UnityEngine;
using UnityEngine.AI;

public class RangedEnemyAi : MonoBehaviour
{
    public GameObject Player;
    [SerializeField] private GameObject _trail;

    private NavMeshAgent _agent;
    private float _distance, _curShootCooldown;
    [SerializeField] LayerMask _layermask;
    [SerializeField] private float _detectionRadius ,_damage, _shootCooldown, _spreadDist;

    private RaycastHit _rayInfo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _agent = gameObject.GetComponent<NavMeshAgent>();

        _curShootCooldown = _shootCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if (Player != null)
        {
            _distance = (Player.transform.position - transform.position).magnitude;
            if (_detectionRadius <_distance) {
                _agent.SetDestination(transform.position);
                return;
            }

            if (_distance < _agent.stoppingDistance*2/3)
            {
                _agent.SetDestination(transform.position + (transform.position- Player.transform.position).normalized * _agent.stoppingDistance*2);
                //transform.LookAt( new Vector3 (Player.transform.position.x, transform.position.y, Player.transform.position.z), Vector3.up);
            }
            else _agent.SetDestination(Player.transform.position);

            if ((_distance <= _agent.stoppingDistance * 3/2) && !Physics.Raycast(transform.position, Player.transform.position - transform.position, _distance)) {
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
        
        Vector3 chosenAim = target.transform.position + target.transform.TransformVector((Random.value-0.5f)*_spreadDist, (Random.value-0.5f)*_spreadDist, 0);

        if (Physics.Raycast(transform.position, (chosenAim - transform.position).normalized , out _rayInfo, _distance*2, _layermask))
        {
            if (_rayInfo.collider.gameObject.GetComponent<Health>()!= null) _rayInfo.collider.gameObject.GetComponent<Health>().TakeDamage(_damage);
        }

        GameObject trail = Instantiate(_trail, transform.position + (chosenAim - transform.position)/2,  Quaternion.identity);
        trail.transform.LookAt(chosenAim);
        trail.transform.localScale = trail.transform.localScale + new Vector3 (0,0,_distance*1.5f);
    }

}
