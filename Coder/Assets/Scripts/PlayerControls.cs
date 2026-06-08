using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class PlayerControls : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private CharacterController _characterController;
    private PlayerInput _playerInput;
    [Header("Movement")]
    [SerializeField] private GameObject _camera;
    [SerializeField] private float _speed, _speedMult, _jumpPower, _lookSpeed;
    

    [Header("Shooting")]
    [SerializeField] private GameObject _starterGun;
    [SerializeField] private List<GameObject> _gunList;
    private List<int> _gunClips = new List<int>();
    [SerializeField] private List<int> _ammoCount; //0:Light, 1:Medium, 2:Heavy
    [SerializeField] private GameObject _WeaponLabel;

    [Header("Effects")]
    [SerializeField] private GameObject _hitParticleObject;
    [SerializeField] private GameObject _hitEnemyParticle;
    [SerializeField] private Animator _gunholderAnimator;
    [SerializeField] private AudioClip _emptySound;
    [SerializeField] private GameObject _pauseMenu;
    private GameObject _currentGun;
    private float _curSwitchCool;
    private int _curGunID = 0;
    private GunStats _gunstats;
    private float _curShootCooldown = 0;
    private RaycastHit _rayInfo;

    private bool _reloading = false;
    

    private float _lookY = 0;
    private float _newSpeed;
    //private bool _isJumping;
    private Vector2 _input;

    private bool _canJump =true;
    private float _jumpCooldown = 0.1f;
    private float _jumpCurCool;
    private float _curJumpPower;
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
        _gunholderAnimator = GameObject.Find("/---- CORE ----/PlayerCamera/GunHolder").GetComponent<Animator>();

        _jumpCurCool = _jumpCooldown;

        //Starter Gun
        if (_starterGun != null)
        {
            for (int i = 0; i < _gunList.Count; i++)
            {
                if (_gunList[i] == _starterGun) _curGunID = i;
                _gunClips.Add(_gunList[i].GetComponent<GunStats>().clipSize);
            }
            SwitchGun(_starterGun);
        }

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }

    void FixedUpdate()
    {
        //Movimiento
        _input = _playerInput.actions["Move"].ReadValue<Vector2>();
        if (_playerInput.actions["Sprint"].IsPressed()) _newSpeed = _speed*_speedMult;
        else _newSpeed = _speed;

        _characterController.Move(transform.TransformVector(new Vector3 (_input.x, 0, _input.y)) * _newSpeed * Time.fixedDeltaTime);
        _characterController.Move(new Vector3(0,-9.8f*Time.fixedDeltaTime + _curJumpPower*Time.fixedDeltaTime,0));
        _curJumpPower *= 1-Time.fixedDeltaTime*5;        

        //Cooldowns
        if (_curShootCooldown> 0) _curShootCooldown -= Time.fixedDeltaTime;
        if (_curSwitchCool > 0) _curSwitchCool -= Time.fixedDeltaTime;
        if (_characterController.isGrounded)
        {
            if ( _jumpCurCool > 0 ) _jumpCooldown-=Time.fixedDeltaTime;
            else _canJump = true; 
        }
    }

    void Update ()
    {
        if (Time.timeScale != 1) return;
        //Camara
        _input = _playerInput.actions["Look"].ReadValue<Vector2>();
        gameObject.transform.Rotate(new Vector3 (0,_input.x*_lookSpeed*Time.fixedDeltaTime,0), Space.Self);

        _camera.transform.rotation = transform.rotation;

        if (!( ( (_input.y < 0) && (_lookY >= 75) )||( (_input.y > 0) && (_lookY<=-75) ) ))
        {
            _lookY += -_input.y * _lookSpeed * Time.fixedDeltaTime;
            if (_lookY < -75) _lookY = -75;
            if (_lookY > 75) _lookY = 75;
        } 
        _camera.transform.Rotate(new Vector3(_lookY,0,0), Space.Self);

        //Cambiar armas
        if ((_playerInput.actions["SwitchGun"].ReadValue<Vector2>().y != 0.0) && (_curSwitchCool<= 0))
        {
            _curSwitchCool = 0.5f;
            if (_input.y < 0) _curGunID --;
            else _curGunID++;
            if (_curGunID < 0) _curGunID = _gunList.Count-1;
            _curGunID = _curGunID%_gunList.Count;
            SwitchGun(_gunList[_curGunID]);
        }

        //Disparar
        if (_reloading) {
            if (_curShootCooldown <= 0) _reloading = false;
            _WeaponLabel.transform.Find("AmmoLabel").GetComponent<TMP_Text>().SetText("... / " + _ammoCount[_gunstats.ammoType]);
        }
        if (_playerInput.actions["Shoot"].IsPressed() && (_curShootCooldown <= 0) && (_currentGun!=null))
        {
            _curShootCooldown = _gunstats.shootCooldown;



            if (_gunClips[_curGunID] <= 0) //Sin munición
            {
                Debug.Log("empty");
                _camera.GetComponent<AudioSource>().PlayOneShot(_emptySound);
            }
            else {                         //Con munición (disparo)
                Debug.Log("Dispara");

                _gunClips[_curGunID] --;
                _camera.GetComponent<AudioSource>().PlayOneShot(_gunstats.shootSound);
                _gunholderAnimator.ResetTrigger("Fire");
                _gunholderAnimator.ResetTrigger("SmallFire");
                if (_gunstats.shootCooldown <= 0.2) {
                    _gunholderAnimator.SetTrigger("SmallFire");
                }
                else _gunholderAnimator.SetTrigger("Fire");

                if (_currentGun.transform.Find("ParticleHolder") != null) _currentGun.transform.Find("ParticleHolder").GetComponent<ParticleSystem>().Play();
                
                //Hit Enemy
                if (Physics.Raycast(_camera.transform.position, _camera.transform.TransformVector(Vector3.forward), out _rayInfo, _gunstats.range, LayerMask.GetMask("Enemy")))
                {
                    if (_hitEnemyParticle != null){
                        _hitEnemyParticle.transform.position = _rayInfo.point;
                        _hitEnemyParticle.GetComponent<ParticleSystem>().Play();
                        _hitEnemyParticle.GetComponent<AudioSource>().Play();
                    }

                    if ((_rayInfo.collider.gameObject.GetComponent<Health>() != null) && (_rayInfo.collider.gameObject!=gameObject)) {
                        _rayInfo.collider.gameObject.GetComponent<Health>().TakeDamage(_gunstats.damage);
                    }
                }

                //Hit Environment
                else if (Physics.Raycast(_camera.transform.position, _camera.transform.TransformVector(Vector3.forward), out _rayInfo, _gunstats.range, LayerMask.GetMask("Default")))
                {
                    _hitParticleObject.transform.position = _rayInfo.point;
                    _hitParticleObject.GetComponent<ParticleSystem>().Play();
                    _hitParticleObject.GetComponent<AudioSource>().Play();
                }

                
            }
        } 

        //Reloading
        if (_playerInput.actions["Reload"].IsPressed() && (_curShootCooldown <= 0) && (_currentGun!=null) && (_gunClips[_curGunID]<_gunstats.clipSize))
        {
            Reload();
        }

        
        
        if (!_reloading) _WeaponLabel.transform.Find("AmmoLabel").GetComponent<TMP_Text>().SetText(_gunClips[_curGunID] + " / " + _ammoCount[_gunstats.ammoType] );
    }

    //Salto
    void OnJump()
    {
        if (!_canJump || !_characterController.isGrounded) return;
        _canJump = false;
        _curJumpPower = _jumpPower;
        _jumpCurCool = _jumpCooldown;
    }

    public void OnPause()
    {
        if (_pauseMenu != null)
        {
            _pauseMenu.SetActive(!_pauseMenu.activeSelf);
        }
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        } 
        else
        {
            Time.timeScale = 0;
            UnityEngine.Cursor.lockState = CursorLockMode.None;
        }

    }

    //Recarga de arma
    void Reload()
    {
        if ( (!(_curShootCooldown <= 0 && _currentGun!=null)) || _ammoCount[_gunstats.ammoType]<=0) return;
        _curShootCooldown = 3;
        _curSwitchCool = 3;
        _camera.GetComponent<AudioSource>().PlayOneShot(_gunstats.reloadSound);

        _gunholderAnimator.ResetTrigger("Reload");
        _gunholderAnimator.SetTrigger("Reload");

        _reloading = true;
        if ((_gunstats.clipSize - _gunClips[_curGunID]) <= _ammoCount[_gunstats.ammoType])
        {
            _ammoCount[_gunstats.ammoType] -= _gunstats.clipSize - _gunClips[_curGunID];
            _gunClips[_curGunID] = _gunstats.clipSize;
        }
        else
        {
            _gunClips[_curGunID] += _ammoCount[_gunstats.ammoType];
            _ammoCount[_gunstats.ammoType] = 0;
        }
    }

    //Cambiar arma física
    void SwitchGun (GameObject gun)
    {
        Destroy(_currentGun);
        _currentGun = Instantiate(gun, Vector3.zero, Quaternion.identity);
        _currentGun.transform.SetParent(GameObject.Find("/---- CORE ----/PlayerCamera/GunHolder").transform);
        _currentGun.transform.localPosition = Vector3.zero;
        _currentGun.transform.localRotation = Quaternion.identity;
        _currentGun.transform.localScale = Vector3.one;
        if (gun.name != "Pistol") _currentGun.transform.Rotate(Vector3.up * 90, Space.Self); //offset for guns
        _gunstats = _currentGun.GetComponent<GunStats>();
        _WeaponLabel.transform.Find("GunImage").GetComponent<Image>().sprite = _gunstats.UIImage;
        _WeaponLabel.transform.Find("TypeLabel").GetComponent<Image>().sprite = _WeaponLabel.transform.Find("TypeLabel").GetComponent<TypeImages>().AmmoImages[_gunstats.ammoType];

        for (int i = 0; i < _gunList.Count; i++)
        {
            if (_gunList[i] == gun) _curGunID = i;
        }
    }

    public void PickupAmmo (int ammoNum, int ammoType)
    {
        _ammoCount[ammoType] += ammoNum;
    }
    public void PickupGun(GameObject gun)
    {
        for (int i = 0; i < _gunList.Count; i++)
        {
            if (_gunList[i] == gun)
            {
                PickupAmmo(gun.GetComponent<GunStats>().clipSize * 2, gun.GetComponent<GunStats>().ammoType);
                return;
            }
        }
        _gunList.Add(gun);
        _gunClips.Add(gun.GetComponent<GunStats>().clipSize);
        SwitchGun(gun);
    }
}
