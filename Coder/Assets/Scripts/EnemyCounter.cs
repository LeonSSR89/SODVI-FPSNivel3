using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class EnemyCounter : MonoBehaviour
{
    [SerializeField] GameObject _enemyParent, _winMenu;
    private int _childCount, _aliveEnemyCount;
    private List<GameObject> EnemyList = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _childCount = _enemyParent.transform.childCount;
        for (int i = 0; i<_childCount; i++)
        {
            GameObject child = _enemyParent.transform.GetChild(i).gameObject;
            if (child.GetComponent<RangedEnemyAi>() != null) EnemyList.Add(child);
        }
        _childCount = EnemyList.Count;
        //Debug.Log(EnemyList.Count);
    }

    // Update is called once per frame
    void Update()
    {
        _aliveEnemyCount = 0;
        for (int i = 0; i<EnemyList.Count; i++)
        {
            if (EnemyList[i]!=null && EnemyList[i].GetComponent<Health>()._health > 0) _aliveEnemyCount++;
        }
        GetComponent<TMP_Text>().SetText(_aliveEnemyCount + " / " + _childCount );
        if (_aliveEnemyCount == 0) {
            _winMenu.SetActive(true);
            UnityEngine.Cursor.lockState = CursorLockMode.None;
        }
    }
}
