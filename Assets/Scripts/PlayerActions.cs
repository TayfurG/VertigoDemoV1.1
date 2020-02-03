using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class PlayerActions : MonoBehaviour
{
    public Transform HexGrid = null;

    GridScript GS = null;

    bool DestroyedAnyHexagonsThisTurn = false;

    public bool GameInProgress = false;

    public GameObject HexagonExplosionAnim = null;

    [SerializeField]
    private AudioClip HexagonDestroySound = null;
    [SerializeField]
    private AudioClip HexagonTurnSound = null;
    // Start is called before the first frame update
    void Awake()
    {
        GS = GetComponent<GridScript>();
        
    }


    // Update is called once per frame
    void Update()
    {
       
        if (!GameInProgress && !GameManager.GM.isGameOver)
        {
#if UNITY_EDITOR
            if(Input.GetMouseButtonDown(0))
            {
#else
                if (Input.touches.Any(x => x.phase == TouchPhase.Began))
            {
#endif
#if UNITY_EDITOR
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
#else
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position), Vector2.zero);
#endif

                if (hit.collider != null)
                {
                    DestroyedAnyHexagonsThisTurn = false;
#if UNITY_EDITOR
                    Vector2 CurrentMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
#else
                    Vector2 CurrentMousePos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
#endif


                    //DestroyHexagons();


                    StartCoroutine(Rotate3Times(CurrentMousePos));
                }
            }
     
        }

    }

    //To make 3 adjacent hexagon rotate three times.
    IEnumerator Rotate3Times(Vector2 MousePos)
    {
        GameInProgress = true;

        //Create a new game object and set it parent to 3 adjacent hexagons.
        //Then rotate the parent.
        GameObject ThreeClosest = new GameObject("ThreeClosest");

        List<Transform> Hex = new List<Transform>();
        Hex = GetThreeClosestHexagons(MousePos);
        List<Vector2> HexPositions = new List<Vector2>();

        for (int i = 0; i < 3; i++)
        {
            HexPositions.Add(Hex[i].position);
        }

        ThreeClosest.transform.position = calculateCentroid(Hex);

        for (int i = 0; i < 3; i++)
        {
            Hex[i].SetParent(ThreeClosest.transform);
        }

        for (int i = 0; i < 3; i++)
        {
            //Check if any matched hexagon, after the turn.
            if (DestroyedAnyHexagonsThisTurn)
            {
                for (int H = 0; H < Hex.Count; H++)
                {
                    if(Hex[H] != null)
                    Hex[H].SetParent(HexGrid);
                }
                Destroy(ThreeClosest);
                while (DestroyedAnyHexagonsThisTurn)
                {
                    yield return new WaitForSeconds(0.42f);
                        Hex = new List<Transform>();
                    CheckAdjacentHexagonsAndDestroy();
                    GS.HexagonFall(GS.Hexagons, Hex);
                }
                break;
            }

            //Now actually rotate the hexagons.
            RotateSelectedHexagons(ThreeClosest, Hex, GS.Hexagons);
            yield return new WaitForSeconds(0.28f);
        }
        for (int H = 0; H < Hex.Count; H++)
        {
            if (Hex[H] != null)
                Hex[H].SetParent(HexGrid);
        }
        Destroy(ThreeClosest);

        while(DestroyedAnyHexagonsThisTurn)
        {
            yield return new WaitForSeconds(0.42f);

            Hex = new List<Transform>();
            CheckAdjacentHexagonsAndDestroy();
            GS.HexagonFall(GS.Hexagons, Hex);
        }

        //Wait for hexagons to finish their movement.
        yield return new WaitForSeconds(0.21f);
        //All hexagons is in place and player now can make his next move.
        GameInProgress = false;

        //If there is a bomb/bombs in the game, decrease its/their duration.
        for (int y = 0; y < GS.GridSizeY; y++)
        {
            for (int x = 0; x < GS.GridSizeX; x++)
            {
                if (GS.Hexagons[x, y].GetComponent<HexScript>().isBomb)
                    GS.Hexagons[x, y].transform.GetChild(0).GetComponent<BombScript>().UpdateBombDuration();
            }
        }

        //TODO: Check if there is a move available or not.


    }




    void RotateSelectedHexagons(GameObject ThreeClosest ,List<Transform> Hex, GameObject[,] Hexagons)
    {
        SoundManager.SM.PlayTheSound(HexagonTurnSound, 1f);
        //Set 3 closest hexagons to vectors
        Vector2Int hexagonACoord = Hex[0].GetComponent<HexScript>().Coordinates;
        Vector2Int hexagonBCoord = Hex[1].GetComponent<HexScript>().Coordinates;
        Vector2Int hexagonCCoord = Hex[2].GetComponent<HexScript>().Coordinates;

        //To determine the lone hexagon
        Vector2Int loneHexagonCoord;
        Vector2Int leftHexagonCoord;
        Vector2Int rightHexagonCoord;

        if (hexagonACoord.y != hexagonBCoord.y && hexagonACoord.y != hexagonCCoord.y)
        {
            loneHexagonCoord = hexagonACoord;

            leftHexagonCoord = hexagonBCoord;
            rightHexagonCoord = hexagonCCoord;
        }
        else if (hexagonBCoord.y != hexagonCCoord.y && hexagonBCoord.y != hexagonACoord.y)
        {
            loneHexagonCoord = hexagonBCoord;

            leftHexagonCoord = hexagonACoord;
            rightHexagonCoord = hexagonCCoord;
        }
        else
        {
            loneHexagonCoord = hexagonCCoord;

            leftHexagonCoord = hexagonACoord;
            rightHexagonCoord = hexagonBCoord;
        }

        if (leftHexagonCoord.x > rightHexagonCoord.x)
        {
            Vector2Int tempCoord = leftHexagonCoord;
            leftHexagonCoord = rightHexagonCoord;
            rightHexagonCoord = tempCoord;
        }

        if (loneHexagonCoord.y > leftHexagonCoord.y)
        {

            GameObject tempHex = Hexagons[loneHexagonCoord.x, loneHexagonCoord.y];

            Hexagons[loneHexagonCoord.x, loneHexagonCoord.y] = Hexagons[rightHexagonCoord.x, rightHexagonCoord.y];

            Hexagons[rightHexagonCoord.x, rightHexagonCoord.y] = Hexagons[leftHexagonCoord.x, leftHexagonCoord.y];

            Hexagons[leftHexagonCoord.x, leftHexagonCoord.y] = tempHex;
        }
        else
        {
            GameObject tempHex = Hexagons[loneHexagonCoord.x, loneHexagonCoord.y];

            Hexagons[loneHexagonCoord.x, loneHexagonCoord.y] = Hexagons[leftHexagonCoord.x, leftHexagonCoord.y];

            Hexagons[leftHexagonCoord.x, leftHexagonCoord.y] = Hexagons[rightHexagonCoord.x, rightHexagonCoord.y];

            Hexagons[rightHexagonCoord.x, rightHexagonCoord.y] = tempHex;
        }

        //Set new hexagon coordinates after rotating.
        Hexagons[loneHexagonCoord.x, loneHexagonCoord.y]
            .GetComponent<HexScript>().Coordinates = new Vector2Int(
                loneHexagonCoord.x, loneHexagonCoord.y);

        Hexagons[leftHexagonCoord.x, leftHexagonCoord.y]
                    .GetComponent<HexScript>().Coordinates = new Vector2Int(
                        leftHexagonCoord.x, leftHexagonCoord.y);

        Hexagons[rightHexagonCoord.x, rightHexagonCoord.y]
                    .GetComponent<HexScript>().Coordinates = new Vector2Int(
                        rightHexagonCoord.x, rightHexagonCoord.y);

        //Now physically rotate the 3 hexagons and call MakeActionAfterRotation function.
    LeanTween.rotateZ(ThreeClosest,
    (ThreeClosest.transform.rotation.eulerAngles.z) - 120,
    0.2f).setOnComplete(x => {
        MakeActionAfterRotation(GS.Hexagons, Hex);
    });

    }


    

 

   

    void MakeActionAfterRotation(GameObject[,]Hexagons,List<Transform> Hex)
    {
        //After rotation, check if any 3 adjacent hexagon with same color matched. If so, destroy.
        CheckAdjacentHexagonsAndDestroy();

        //If at least one hexagon got destroyed, make sure we do not move our 3 selected hexagons in HexagonFall function.
            if (DestroyedAnyHexagonsThisTurn)
                Hex = new List<Transform>();

        //Find missing hexagons and replace with new ones.
            GS.HexagonFall(Hexagons, Hex);
    }

    //To find the center point of 3 adjacent hexagons
    Vector2 calculateCentroid(List<Transform> Hex)
    {
        
        Vector3 centroid = Vector3.zero;

        foreach (Transform T in Hex)
        {
            centroid += T.transform.position;
        }

        centroid /= Hex.Count;

        return centroid;
    }

    //To find 3 hexagons that are closest to user click
    List<Transform> GetThreeClosestHexagons(Vector2 MousePos)
    {
        List<Transform> Hex = new List<Transform>();

        for (int y = 0; y < GS.GridSizeY; y++)
        {
            for (int x = 0; x < GS.GridSizeX; x++)
            {
                Hex.Add(GS.Hexagons[x, y].transform);
            }
        }

        Hex = Hex.OrderBy(point => Vector3.Distance(MousePos, point.transform.position)).ToList();

        return new List<Transform> { Hex[0], Hex[1], Hex[2] };
    }

    //To see if any 3 adjacent hexagons with same color
    void CheckAdjacentHexagonsAndDestroy()
    {
        GameObject[,] Hexagons = GS.Hexagons;

        //For storing hexagons to be destroyed
        List<Vector2Int> HexToBeDestroyed = new List<Vector2Int>();

        for (int y = 0; y < GS.GridSizeY; y++)
        {
            for (int x = 0; x < GS.GridSizeX; x++)
            {
                Transform CurrentHex = Hexagons[x, y].transform;
                Color CurrentColor = CurrentHex.GetComponent<SpriteRenderer>().color;
                int MatchedColors = 0;

                if (y % 2 == 0)
                {
                    if (x - 1 >= 0 && y + 1 <= GS.GridSizeY - 1)
                    {

                        if (CurrentColor == Hexagons[x - 1, y].GetComponent<SpriteRenderer>().color) //LeftOne
                        {
                            MatchedColors++;
                        }

                        if (CurrentColor == Hexagons[x - 1, y + 1].GetComponent<SpriteRenderer>().color) //TopLeftOne
                        {
                            MatchedColors++;
                        }


                        if (MatchedColors == 2)
                        {
                            if (!HexToBeDestroyed.Contains(new Vector2Int(x, y)))
                                HexToBeDestroyed.Add(new Vector2Int(x, y));
                            if (!HexToBeDestroyed.Contains(new Vector2Int(x - 1, y)))
                                HexToBeDestroyed.Add(new Vector2Int(x - 1, y));
                            if (!HexToBeDestroyed.Contains(new Vector2Int(x - 1, y + 1)))
                                HexToBeDestroyed.Add(new Vector2Int(x - 1, y + 1));
                            /*
                            if (!HexToBeDestroyed.Contains(Hexagons[x - 1, y].gameObject))
                                HexToBeDestroyed.Add(Hexagons[x - 1, y].gameObject);
                            if (!HexToBeDestroyed.Contains(Hexagons[x - 1, y + 1].gameObject))
                                HexToBeDestroyed.Add(Hexagons[x - 1, y + 1].gameObject);
                                */
                        }

                        MatchedColors = 0;


                        if (CurrentColor == Hexagons[x - 1, y + 1].GetComponent<SpriteRenderer>().color) //TopLeftOne
                        {
                            MatchedColors++;
                        }
                        if (CurrentColor == Hexagons[x, y + 1].GetComponent<SpriteRenderer>().color) //TopRightOne
                        {
                            MatchedColors++;
                        }

                        if (MatchedColors == 2)
                        {

                            if (!HexToBeDestroyed.Contains(new Vector2Int(x, y)))
                                HexToBeDestroyed.Add(new Vector2Int(x, y));
                            if (!HexToBeDestroyed.Contains(new Vector2Int(x, y + 1)))
                                HexToBeDestroyed.Add(new Vector2Int(x, y + 1));
                            if (!HexToBeDestroyed.Contains(new Vector2Int(x - 1, y + 1)))
                                HexToBeDestroyed.Add(new Vector2Int(x - 1, y + 1));

                        }

                        MatchedColors = 0;

                    }

                    if (x + 1 <= GS.GridSizeX - 1 && y + 1 <= GS.GridSizeY - 1)
                    {
                        if (CurrentColor == Hexagons[x, y + 1].GetComponent<SpriteRenderer>().color) //TopRightOne
                        {
                            MatchedColors++;
                        }
                        if (CurrentColor == Hexagons[x + 1, y].GetComponent<SpriteRenderer>().color) //RightOne
                        {
                            MatchedColors++;
                        }

                        if (MatchedColors == 2)
                        {
                            if (!HexToBeDestroyed.Contains(new Vector2Int(x, y)))
                                HexToBeDestroyed.Add(new Vector2Int(x, y));
                            if (!HexToBeDestroyed.Contains(new Vector2Int(x, y + 1)))
                                HexToBeDestroyed.Add(new Vector2Int(x, y + 1));
                            if (!HexToBeDestroyed.Contains(new Vector2Int(x + 1, y)))
                                HexToBeDestroyed.Add(new Vector2Int(x + 1, y));

                      
                        }

                        MatchedColors = 0;

                    }

                    if (x + 1 <= GS.GridSizeX - 1 && y - 1 >= 0)
                    {
                        if (CurrentColor == Hexagons[x + 1, y].GetComponent<SpriteRenderer>().color) //RightOne
                        {
                            MatchedColors++;
                        }
                        if (CurrentColor == Hexagons[x, y - 1].GetComponent<SpriteRenderer>().color) //BottomRightOne
                        {
                            MatchedColors++;
                        }

                        if (MatchedColors == 2)
                        {
                            if (!HexToBeDestroyed.Contains(new Vector2Int(x, y)))
                                HexToBeDestroyed.Add(new Vector2Int(x, y));
                            if (!HexToBeDestroyed.Contains(new Vector2Int(x, y - 1)))
                                HexToBeDestroyed.Add(new Vector2Int(x, y - 1));
                            if (!HexToBeDestroyed.Contains(new Vector2Int(x + 1, y)))
                                HexToBeDestroyed.Add(new Vector2Int(x + 1, y));

                        }

                        MatchedColors = 0;
                    }

                    if (x - 1 >= 0 && y - 1 >= 0)
                    {
                        if (CurrentColor == Hexagons[x, y - 1].GetComponent<SpriteRenderer>().color) //BottomRightOne
                        {
                            MatchedColors++;
                        }
                        if (CurrentColor == Hexagons[x - 1, y - 1].GetComponent<SpriteRenderer>().color) //BottomLeftOne
                        {
                            MatchedColors++;
                        }

                        if (MatchedColors == 2)
                        {

                            if (!HexToBeDestroyed.Contains(new Vector2Int(x, y)))
                                HexToBeDestroyed.Add(new Vector2Int(x, y));
                            if (!HexToBeDestroyed.Contains(new Vector2Int(x, y - 1)))
                                HexToBeDestroyed.Add(new Vector2Int(x, y - 1));
                            if (!HexToBeDestroyed.Contains(new Vector2Int(x - 1, y - 1)))
                                HexToBeDestroyed.Add(new Vector2Int(x - 1, y - 1));
                        }

                        MatchedColors = 0;

                        if (CurrentColor == Hexagons[x - 1, y - 1].GetComponent<SpriteRenderer>().color) //BottomLeftOne
                        {
                            MatchedColors++;
                        }
                        if (CurrentColor == Hexagons[x - 1, y].GetComponent<SpriteRenderer>().color) //LeftOne
                        {
                            MatchedColors++;
                        }

                        if (MatchedColors == 2)
                        {

                            if (!HexToBeDestroyed.Contains(new Vector2Int(x, y)))
                                HexToBeDestroyed.Add(new Vector2Int(x, y));
                            if (!HexToBeDestroyed.Contains(new Vector2Int(x - 1, y)))
                                HexToBeDestroyed.Add(new Vector2Int(x - 1, y));
                            if (!HexToBeDestroyed.Contains(new Vector2Int(x - 1, y - 1)))
                                HexToBeDestroyed.Add(new Vector2Int(x - 1, y - 1));
                        }

                        MatchedColors = 0;
                    }

                }

            }
        }

        //Check if any hexagons will be destroyed
        if(HexToBeDestroyed.Count > 0)
        {
            DestroyedAnyHexagonsThisTurn = true;
            SoundManager.SM.PlayTheSound(HexagonDestroySound,0.7f);
        }
        else
        {
            DestroyedAnyHexagonsThisTurn = false;
        }

        //Destroyed Hexagons * 5
        ScoreManager.SM.AddScore(HexToBeDestroyed.Count * 5);


        //Destroy hexagons now
        foreach (Vector2Int V in HexToBeDestroyed)
        {
            if (GS.Hexagons[V.x, V.y] != null)
            {
                ParticleSystem PS = HexagonExplosionAnim.transform.GetChild(2).GetComponent<ParticleSystem>();
                ParticleSystem PS1 = HexagonExplosionAnim.transform.GetChild(3).GetComponent<ParticleSystem>();

                var main = PS.main;
                var main2 = PS1.main;

                Color HexColor = GS.Hexagons[V.x, V.y].GetComponent<SpriteRenderer>().color;

                main.startColor = HexColor;
                main2.startColor = HexColor;

                GameObject HexAnim = Instantiate(HexagonExplosionAnim, GS.Hexagons[V.x, V.y].transform.position, Quaternion.identity);



                
                Destroy(GS.Hexagons[V.x, V.y]);
                GS.Hexagons[V.x, V.y] = null;
            }
        }

    }




}
