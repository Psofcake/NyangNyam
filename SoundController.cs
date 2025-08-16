using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
    
    public AudioClip eatingCakeSound;
    public AudioClip jumpingSound;
    public AudioClip destroySound;
    public AudioClip clickSound;
    public AudioClip mouseOverSound;
    
    private AudioSource _audioSource;
    private AudioClip _audioClip;

    private void Awake()
    {
        if(!eatingCakeSound) Debug.Log("Not Found AudioClip - Cake");
        if(!jumpingSound) Debug.Log("Not Found AudioClip - Jump");
        if(!mouseOverSound) Debug.Log("Not Found AudioClip - Destroy");
        if(!clickSound) Debug.Log("Not Found AudioClip - ButtonClick");
        if(!mouseOverSound) Debug.Log("Not Found AudioClip - MouseOver");
        
        _audioSource = GetComponent<AudioSource>();
        if (!_audioSource)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("New AudioSource Added");
        }
    }

    public void PlayEatingCake()
    {
        _audioSource?.PlayOneShot(eatingCakeSound);
    }
    public void PlayJumping()
    {
        _audioSource?.PlayOneShot(jumpingSound);
    }
    public void PlayDestroy()
    {
        _audioSource?.PlayOneShot(destroySound);
    }
    public void PlayButtonClick()
    {
        _audioSource?.PlayOneShot(clickSound);
    }
    public void PlayMouseOver()
    {
        _audioSource?.PlayOneShot(mouseOverSound);
    }
}
