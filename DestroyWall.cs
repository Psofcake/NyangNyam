using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DestroyWall
    : MonoBehaviour
{
    public GameObject particle;
    private SoundController Sound;
    
    private Quaternion obstacleRotation;
    private void Awake()
    {
        Sound =  GameObject.FindObjectOfType<SoundController>();
        if(!Sound) Debug.Log("Not Found SoundController");
        
        if(!particle) Debug.Log("Not Found Particle");
        
        obstacleRotation = transform.rotation;
    }

    private void OnCollisionEnter(Collision other)
    {
        foreach (ContactPoint contact in other.contacts)
        {
            Vector3 impulsePower = contact.impulse;
            Debug.Log($"{other.gameObject.name}가 {name}에 충돌하는 힘의 크기: {impulsePower.magnitude}");
            if (impulsePower.magnitude > 1.5f && !other.gameObject.CompareTag("Cat"))
                Invoke("DestroyThis", 0.8f);
        }
        
        Quaternion currentRotation = transform.rotation;
        float angle = Quaternion.Angle(obstacleRotation, currentRotation);
        
        //Debug.Log($"obstacleAngle of {name} : {angle}");
        
        if (angle > 75f)
        {
            DestroyThis();
        }
    }

    private void DestroyThis()
    {
        Sound.PlayDestroy();
        GameObject particleObject = Instantiate(particle, transform.position, transform.rotation);
        Destroy(gameObject);
    }
    
}
