using UnityEngine;
using TMPro;

public class BombScript : MonoBehaviour
{
    public GameObject BombAnim = null;

    [SerializeField]
    private AudioClip BombSound = null;
        
    int BombDuration = 6;
        
    bool FirstTimeAppearance = true;

    Quaternion BombRotation;

    private void Start()
    {
        transform.GetChild(0).GetComponent<TextMeshPro>().text = BombDuration.ToString();
    }

    private void Awake()
    {
        BombRotation = transform.rotation;
        
    }
 
    void Update()
    {
        transform.rotation = BombRotation;
    }

    public void UpdateBombDuration()
    {
        if(!FirstTimeAppearance)
        {
            BombDuration--;
            transform.GetChild(0).GetComponent<TextMeshPro>().text = BombDuration.ToString();

            if (BombDuration <= 0)
            {
                //Game Over
                SoundManager.SM.PlayTheSound(BombSound,1f);
                Instantiate(BombAnim, transform.position, Quaternion.identity);
                GameManager.GM.isGameOver = true;
                GameManager.GM.GameOver();
                Destroy(this.gameObject);
            }
        }
        else
        {
            FirstTimeAppearance = false;
        }
        
    }
}
