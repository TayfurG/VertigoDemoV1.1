using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    static public GameManager GM { get; set; }
    // Start is called before the first frame update

    [SerializeField]
    private GameObject GameOverPanel = null;

    [System.NonSerialized]
    public bool isGameOver = false;

  
    private void Awake()
    {
        GM = this;
    }

    private void Start()
    {


        if (PlayerPrefs.GetInt("FirstTime") == 0)
        {
            PlayerPrefs.SetInt("FirstTime", 1);
            PlayerPrefs.SetString("Username", " ");
        }
    }

    public void RePlayButton()
    {
      
            SceneManager.LoadScene(1, LoadSceneMode.Single);
        
        
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(0.65f);

        LeanTween.value(0, 225, 0.5f).setIgnoreTimeScale(true).setOnUpdate((float val) => {

            if (GameOverPanel != null)
                GameOverPanel.GetComponent<Image>().color = new Color32(100, 100, 100, (byte)val);
        });

        GameOverPanel.transform.GetChild(0).gameObject.SetActive(true);
        GameOverPanel.transform.GetChild(1).gameObject.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
