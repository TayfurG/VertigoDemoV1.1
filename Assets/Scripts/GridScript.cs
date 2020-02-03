    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridScript : MonoBehaviour
{
    [Header("Hexagon Settings")]
    public int GridSizeX = 8;
    public int GridSizeY = 9;
    [Header("Increase the size and choose a color")]
    public Color[] HexagonColors = null;
    [SerializeField]
    private int BombThresold = 700;

    [Header(" ")]
    public Transform HexGrid = null;

    [SerializeField]
    GameObject HexagonPre = null;

    public GameObject[,] Hexagons;

    float hexWidth = 1f;
    float hexHeight = 1f;

    float offsetSpaceForGrid = 0.05f;

    float NewHexStartPosY = 0;

    Vector2 startPos = Vector2.zero;

    [SerializeField]
    GameObject BombPre = null;
   
    int InitialBombThresold = 0;



    // Start is called before the first frame update
    void Start()
    {
        HexagonPre.transform.localScale = new Vector2(((ScreenManager.SM.getScreenWidth() / (GridSizeX + 0.5f))) - offsetSpaceForGrid,
                                                      ((ScreenManager.SM.getScreenWidth() / (GridSizeX + 0.5f))) - offsetSpaceForGrid);
        hexWidth = HexagonPre.GetComponent<SpriteRenderer>().bounds.size.x;
        hexHeight = HexagonPre.GetComponent<SpriteRenderer>().bounds.size.y;

        CalcStartPos();

        Hexagons = new GameObject[GridSizeX, GridSizeY];
        CreateGrid(GridSizeX, GridSizeY);

        NewHexStartPosY = CalcWorldPos(Vector2.zero).y + HexagonPre.transform.localScale.y;

        InitialBombThresold = BombThresold;

    }

    void CalcStartPos()
    {
        float offset = HexagonPre.GetComponent<SpriteRenderer>().bounds.size.x / 2f;

        float x = (-ScreenManager.SM.getScreenWidth() / 2f) + // Left side of the screen + 
                                                     offset + // half size of a hexagon +
                                                     ((offsetSpaceForGrid /2f ) * (GridSizeX + 0.5f)); //Desired empty space on the sides.
        float y = ScreenManager.SM.getScreenHeight() / 6f;

        startPos = new Vector2(x, y);
    }

   public Vector2 CalcWorldPos(Vector2 gridPos)
    {
        float offset = 0;
        if(gridPos.y %2 != 0)
            offset = hexWidth / 2f;

        float x = startPos.x + gridPos.x * hexWidth + offset;
        float y = startPos.y - gridPos.y * hexHeight * 0.75f;

        return new Vector2(x, y);

    }

   public void HexagonFall(GameObject[,] hexArray, List<Transform> ThreeHexagons)
    {

        
        // Handle fall for base columns and for offset columns
        for (int offset = 0; offset < 2; offset++)
        {
            
            // Handle fall for each column at current offset
            for (int x = 0; x < hexArray.GetLength(0); x++)
            {
                int bottomYIndex = hexArray.GetLength(1) - offset - 1;

                // List of indices of where each hexagon in that column will come from.
                // We will fill from bottom to top.
                List<Vector2Int> sourceIndices = new List<Vector2Int>();

                for (int y = bottomYIndex; y >= 0; y -= 2)
                {
                    // HexExists returns true if the hex isn't empty. 
                    // Something along the lines of ` return input!=null; `
                    // depending on what "empty" hexes look like in the array

                    if (hexArray[x,y] != null)
                    {
                        sourceIndices.Add(new Vector2Int(x, y));
                    }
                }

                // We have a list of where to get each bottom hexes from, now do the move/create
                for (int y = bottomYIndex; y >= 0; y -= 2)
                {
                    if (sourceIndices.Count > 0)
                    {
                        // If we have any available hexes in column,
                        // use the bottommost one (at index 0)
                        hexArray[x, y] = hexArray[sourceIndices[0].x, sourceIndices[0].y];

                        //hexArray[x, y].transform.position = CalcWorldPos(new Vector2(x, y));

                        if(!ThreeHexagons.Contains(hexArray[x,y].transform))
                        LeanTween.move(hexArray[x, y], CalcWorldPos(new Vector2(x, y)), 0.2f);

                        // We have now found a home for hex previously at sourceIndices[0].
                        // Remove that index from list so hex will stay put.
                        sourceIndices.RemoveAt(0);
                    }
                    else
                    {
                        // Otherwise, we need to generate a new hex

                        Vector2 NewHexPosition = CalcWorldPos(new Vector2(x, y));

                        hexArray[x, y] = Instantiate(HexagonPre, 
                            new Vector2(NewHexPosition.x,NewHexStartPosY), 
                            Quaternion.identity,HexGrid.transform);

                        LeanTween.move(hexArray[x, y], NewHexPosition, 0.2f);

                        Hexagons[x, y].GetComponent<SpriteRenderer>().color = HexagonColors[Random.Range(0, HexagonColors.Length)];

                        if(BombThresold <= ScoreManager.SM.CurrentScore)
                        {
                            GameObject Bomb = Instantiate(BombPre, 
                                Hexagons[x, y].transform.position, 
                                Quaternion.identity, 
                                Hexagons[x, y].transform);

                            Hexagons[x, y].GetComponent<HexScript>().isBomb = true;

                            if(InitialBombThresold != 100 && InitialBombThresold != 150)
                            {
                                InitialBombThresold -= 100;
                            }
                            else
                            {
                                InitialBombThresold = 150;
                            }
                            
                            BombThresold += InitialBombThresold;
                        }
                    }


                    hexArray[x, y].GetComponent<HexScript>().Coordinates = new Vector2Int(x, y);
                    hexArray[x, y].transform.name = "X: " + x + " | Y: " + y;

                }
            }
        }
    }




    public void CreateGrid(int gridWidth, int gridHeight)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {


                GameObject Hexagon = Instantiate(HexagonPre, Vector2.zero, Quaternion.identity, HexGrid);
                Hexagons[x, y] = Hexagon;
                
                if(x > 0)
                {
                    do
                    {
                        Hexagons[x, y].GetComponent<SpriteRenderer>().color = HexagonColors[Random.Range(0, HexagonColors.Length)];

                    } while (Hexagons[x, y].GetComponent<SpriteRenderer>().color == Hexagons[x - 1, y].GetComponent<SpriteRenderer>().color);

                }
                else
                {
                    Hexagons[x, y].GetComponent<SpriteRenderer>().color = HexagonColors[Random.Range(0, HexagonColors.Length)];
                }        
                
                Vector2 gridPos = new Vector2(x, y);
                Hexagon.transform.position = CalcWorldPos(gridPos);
                Hexagon.GetComponent<HexScript>().Coordinates = new Vector2Int(x, y);
                Hexagon.transform.name = "X: " + x + " | Y: " + y;


            }

        }

    }

}
