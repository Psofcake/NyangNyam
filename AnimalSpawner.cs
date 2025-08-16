using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    [SerializeField]private GameObject AngryBird_Prefab;
    [SerializeField]private GameObject AngryBird_GameObject;
    
    private Vector3 initPosiiton;
    private Vector3 initRotation;
    
    void Start()
    {
        AngryBird_GameObject = Instantiate(AngryBird_Prefab, AngryBird_Prefab.transform.position, Quaternion.LookRotation(AngryBird_Prefab.transform.forward));
        AngryBird_GameObject.SetActive(true);
        AngryBird_GameObject.GetComponent<ABController>().AngryBirdNotification += OnDie;
    }
    
    void OnDie()
    {
        //죽기 전에 호출
        Destroy(AngryBird_GameObject);
        
        AngryBird_GameObject = Instantiate(AngryBird_Prefab, AngryBird_Prefab.transform.position, Quaternion.LookRotation(AngryBird_Prefab.transform.forward));
        AngryBird_GameObject.SetActive(true);
        AngryBird_GameObject.GetComponent<ABController>().AngryBirdNotification += OnDie;
    }
}
