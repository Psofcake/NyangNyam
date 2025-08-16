using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour,IPointerEnterHandler
{
    private SoundController Sound;
    
    private void Start()
    {
        //SoundController 스크립트를 가진 게임오브젝트 찾아오기
        Sound = GameObject.FindObjectOfType<SoundController>();
        if (!Sound) Debug.Log("Not Found SoundManager");
    }
    // ㅡㅡㅡㅡㅡㅡㅡ씬 로드하기ㅡㅡㅡㅡㅡㅡㅡ

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void LoadNextSceneInvoke()
    {
        Sound.PlayButtonClick();
        Invoke("LoadNextScene", 0.2f);
    }
    
    public void LoadScene(int index)
    {
        Sound.PlayButtonClick();

        switch (index)
        {
            case 0:
                Invoke("LoadStage", 0.1f);
                break;
            case 1:
                Invoke("LoadLevel1", 0.1f);
                break;
            case 2:
                Invoke("LoadLevel2", 0.1f);
                break;
            case 3:
                Invoke("LoadLevel3", 0.1f);
                break;
            case 4:
                Invoke("LoadLevel4", 0.1f);
                break;
            case 5:
                Invoke("LoadLevel5", 0.1f);
                break;
            case 99:
                Invoke("QuitGame", 0.1f);
                break;
            default: 
                Invoke("LoadLobby", 0.2f);
                break;
        }
        
        
    }
    private void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        SceneManager.LoadScene(currentSceneIndex+1);
    }
    public void LoadLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
    private void LoadStage()
    {
        SceneManager.LoadScene(1);
    }
    private void LoadLevel1()
    {
        SceneManager.LoadScene(2);
    }
    private void LoadLevel2()
    {
        SceneManager.LoadScene(3);
    }
    private void LoadLevel3()
    {
        SceneManager.LoadScene(4);
    }
    private void LoadLevel4()
    {
        SceneManager.LoadScene(5);
    }
    private void LoadLevel5()
    {
        SceneManager.LoadScene(6);
    }
    private void QuitGame()
    {
        Application.Quit();
    }

    // public void LoadSceneByIndex(int sceneIndex)
    // {
    //     SceneManager.LoadScene(sceneIndex);
    // }
    
    // public void LoadSceneByName(string sceneName)
    // {
    //     SceneManager.LoadScene(sceneName);
    // }
    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
    
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        //Output to console the clicked GameObject's name and the following message. You can replace this with your own actions for when clicking the GameObject.
        Debug.Log(name + " OnPointEnter!");
        Sound.PlayMouseOver();
    }
}
