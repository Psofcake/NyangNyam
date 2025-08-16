using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cake : MonoBehaviour
{
    public GameObject particle;
    private SoundController Sound;
    
    private void Awake()
    {
        Sound =  GameObject.FindObjectOfType<SoundController>();
        if(!Sound) Debug.Log("Not Found SoundController");
        
        if(!particle) Debug.Log("Not Found Particle");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cat")||other.CompareTag("Squirrel"))
        {
            Sound.PlayEatingCake();
            GameObject particleObject = Instantiate(particle, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Cat")||other.gameObject.CompareTag("Squirrel"))
        {
            Sound.PlayEatingCake();
            GameObject particleObject = Instantiate(particle, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
