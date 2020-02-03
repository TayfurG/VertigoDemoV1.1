using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreSettings : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        DontDestroyOnLoad(this);
        Application.targetFrameRate = 60;
    }

    void Start()
    {

        //  SceneManager.LoadScene(1, LoadSceneMode.Single);
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
    }

}
