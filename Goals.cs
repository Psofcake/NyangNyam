using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Goals : MonoBehaviour
{
    private int goalCount;
    private int cakeCount;
    public GameObject _levelClear;
    public TextMeshProUGUI _score;
    
    void Start()
    {
        goalCount = transform.childCount;
        cakeCount = goalCount;
        
        if (!_levelClear)
            Debug.Log("Not Found LevelClearUI");
    }

    // Update is called once per frame
    void Update()
    {
        cakeCount = transform.childCount;
        
        if (cakeCount == 0)
        {
            _levelClear.SetActive(true);
            
            _score.text = "Cake " + (goalCount-cakeCount) + " / " + goalCount;
        }
    }
}
