using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingTextScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(Anim());
	}
	
	
    IEnumerator Anim()
    {
        while(true)
        {
            for (int i = 1; i < 4; i++)
            {
                GetComponent<TextMeshProUGUI>().text = "Loading" + HowManyDots(i);
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
        
    }

    string HowManyDots(int Dot)
    {
        string dots = "";
        for (int i = 0; i < Dot; i++)
        {
            dots += ".";
        }

        return dots;
    }
}
