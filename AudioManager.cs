using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // 싱글톤 패턴을 사용하여 오디오 매니저를 관리할 수 있습니다.
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = "AudioManager";
                    instance = obj.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 설정

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true; // 배경음악을 반복 재생합니다.
    }

    // 배경음악 재생 함수
    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    // 배경음악 일시 정지 함수
    public void PauseBackgroundMusic()
    {
        audioSource.Pause();
    }

    // 배경음악 재개 함수
    public void ResumeBackgroundMusic()
    {
        audioSource.UnPause();
    }

    // 필요에 따라 추가적인 기능을 구현할 수 있습니다.
}