using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DestroyObstacle : MonoBehaviour
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
        Quaternion currentRotation = transform.rotation;
        float angle = Quaternion.Angle(obstacleRotation, currentRotation);
        
        //Debug.Log($"obstacleAngle of {name} : {angle}");
        
        if (other.gameObject.CompareTag("Ground"))
        {
            Invoke("DestroyThis", 0.8f);
        }
        else if (angle > 75f)
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
