 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

public enum TileKind
{
    Breakable,
    Blank,
    Normal
}
 
[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public TileKind tileKind;
}

public class Board : MonoBehaviour
{

    internal GameState currentState = GameState.move;
    public int width;
    public int height;
    public int offSet; 
    public GameObject tileprefab;
    public GameObject breakableTilePrefab; 
    public GameObject[] dots; 
    public GameObject destroyEffect;
    public TileType[] boardLayout;
    private bool[,] blankSpaces;
    private BackgroundTile[,] breakableTiles;
    public GameObject[,] alldots;
    public Dot currentDot;
    private FindMatches findMatches;
    private int basePieceValue=20;
    private int streakValue=1;
    private ScoreManager scoreManager;
    private SoundManager soundManager;
    public float refillDelay = 0.5f;
    public int[] scoreGoals;

    void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();
        scoreManager = FindObjectOfType<ScoreManager>();
        breakableTiles = new BackgroundTile[width, height];
        findMatches = FindObjectOfType<FindMatches>();
        blankSpaces = new bool[width, height];
        alldots = new GameObject[width, height];
        Setup();
    }

    public void GenerateBlankSpaces()
    {
        for(int i=0;i<boardLayout.Length;i++)
        {
            if(boardLayout[i].tileKind==TileKind.Blank)
            {
               blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
        }
    }

    public void GenerateBreakableTiles()
    {
        //Look at all the tiles in the layout
        for(int i=0;i<boardLayout.Length;i++)
        {
            //if a tile is a "jelly"tile
            if(boardLayout[i].tileKind==TileKind.Breakable)
            {
                //create a "jelly " tile at that position
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile  = Instantiate(breakableTilePrefab, tempPosition, Quaternion.identity);
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
                //tile.layer= LayerMask.NameToLayer("Gellys");
            } 
        }
    }

    private void Setup() 
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();

      // yield return new WaitForSeconds(.0f);

        for(int i=0;i<width;i++)
        {
            for(int j=0;j<height;j++)
            {
                if (!blankSpaces[i, j])
                {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    Vector2 tilePosition= new Vector2(i,j);
                    GameObject backgroundTile = Instantiate(tileprefab, tilePosition, Quaternion.identity) as GameObject;
                    backgroundTile.transform.parent = this.transform;
                    backgroundTile.name = "( " + i + " , " + j + " )";
                    int dotToUse = Random.Range(0, dots.Length);

                    int maxIterations = 0;
                    while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                    {
                        dotToUse = Random.Range(0, dots.Length);
                        maxIterations++;
                        Debug.Log(maxIterations);
                    }
                    maxIterations = 0;
                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    dot.GetComponent<Dot>().row = j;
                    dot.GetComponent<Dot>().column = i;
                    dot.transform.parent = this.transform;
                    dot.name = "( " + i + " , " + j + " )";
                    alldots[i, j] = dot;
                }
            }
        }

        //GenerateBlankSpaces();
        //GenerateBreakableTiles();
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {   
        if (column > 1 && row > 1)
        {
            if (alldots[column - 1, row] != null && alldots[column - 2, row] != null)
            {
                if (alldots[column - 1, row].tag == piece.tag && alldots[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
            if (alldots[column, row - 1] != null && alldots[column, row - 2] != null)
            {
                if (alldots[column, row - 1].tag == piece.tag && alldots[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }

        } 
        else if(column <=1  || row <= 1)
        {
            if (row >1)
            {
                if (alldots[column, row - 1] != null && alldots[column, row - 2] != null)
                {
                    if (alldots[column, row - 1].tag == piece.tag && alldots[column, row - 2].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }
            if (column > 1)
            {
                if (alldots[column - 1, row] != null && alldots[column - 2, row] != null)
                {
                    if (alldots[column - 1, row].tag == piece.tag && alldots[column - 2, row].tag == piece.tag)
                    {
                        return true;
                    }
                }   
            } 
        }
        return false; 
    }

    private bool ColumnOrRow()
    {
        int numberHorizontal = 0;
        int numberVertical = 0;
        Dot firstPiece = findMatches.CurrentMatches[0].GetComponent<Dot>();
        if (firstPiece != null)
        {
            foreach (GameObject currentPiece in findMatches.CurrentMatches)
            {
                Dot dot = currentPiece.GetComponent<Dot>();
                if (dot.row == firstPiece.row)
                {
                    numberHorizontal++;
                }
                if (dot.column == firstPiece.column)
                {
                    numberVertical++;
                }

            }
        }
        return (numberVertical == 5 || numberHorizontal == 5);
    }

    private void CheckToMakeBombs()
    {
        if(findMatches.CurrentMatches.Count==4 || findMatches.CurrentMatches.Count==7)
        {
            findMatches.checkBombs();
        }
        if(findMatches.CurrentMatches.Count==5  || findMatches.CurrentMatches.Count==8)
        {
            if (ColumnOrRow())
            {
                //make a color bomb
                //is the current dot matched?
                if(currentDot != null)
                {
                    if(currentDot.isMatched)
                    {
                        if(!currentDot.isColorBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeColorBomb();
                        }
                        else
                        {
                            if(currentDot.otherDot!=null)
                            {
                                Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                                if(otherDot.isMatched)
                                {
                                    if(!otherDot.isColorBomb)
                                    {
                                        otherDot.isMatched = false;
                                        otherDot.MakeColorBomb();
                                    }
                                }
                            }
                        }
                    }
                }
                    
            }
            else
            {
                //make a adjacent bomb
                //is the current dot matched?
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isAdjacentBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeAdjacentBomb();
                        }
                        else
                        {
                            if (currentDot.otherDot != null)
                            {
                                Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                                if (otherDot.isMatched)
                                {
                                    if (!otherDot.isAdjacentBomb)
                                    {
                                        otherDot.isMatched = false;
                                        otherDot.MakeAdjacentBomb();
                                    }
                                }
                            }
                        }
                    }
                }

            }



        }
    }
      
    public  void DestroyMatchesAt(int column,int row)
    {
        if (alldots[column,row].GetComponent<Dot>().isMatched)
        {
            //How many elements are in the matched pieces list from findmatches?
            if(findMatches.CurrentMatches.Count>=4 )
            {
                CheckToMakeBombs();
            }
            //Does a tile need to break?
            if (breakableTiles[column, row] != null)
            {
                //if it does,give one damage
                breakableTiles[column, row].TakeDamage(1);
                if (breakableTiles[column, row].hitPoints <= 0)
                {
                    breakableTiles[column, row] = null;
                }
            }
            //Does the sound manager exist?
            if(soundManager!=null)
            {
                soundManager.playRandomDestroyNoise();

            }
           
            GameObject particle = Instantiate(destroyEffect , alldots[column, row].transform.position, Quaternion.identity);
            Destroy(particle, .5f);
            Destroy(alldots[column, row]);
            scoreManager.IncreaseScore(basePieceValue * streakValue);
                
            alldots[column, row] = null; 
        }
    }

    public  void DestroyMatches() 
    {
        for (int i=0;i<width;i++)
        {
            for(int j=0;j<height;j++)
            { 
                if(alldots[i,j] !=null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        findMatches.CurrentMatches.Clear();
        StartCoroutine(DecreaseRowCo2());
    }
 
    private IEnumerator DecreaseRowCo2()
    {
        for(int i=0;i<width;i++)
        {
            for (int j=0;j<height;j++)
            {
                //if the current spot is not blank and is empty
                if(!blankSpaces[i,j] && alldots[i,j]==null)
                {
                    //loop from the space above top the top of the column
                    for(int k=j+1;k<height;k++)
                    {
                        //if a dot is found...
                        if(alldots[i,k]!=null)
                        {
                            //move that dot to this empty space
                            alldots[i, k ].GetComponent<Dot>().row = j;
                            //see that spot to be null
                            alldots[i, k] = null;
                            //break out of the loop
                            break;
                        }
                    
                    }
                }
            }
        }
        yield return new WaitForSeconds(refillDelay*0.5f);
        StartCoroutine(FillBoardCo());

    }

    private IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;
        for (int i =0;i < width;i++)
        {
            for(int j=0;j < height;j++)
            {
                if(alldots[i,j]==null)
                {
                    nullCount++;
                }
                else if(nullCount>0)
                {
                    alldots[i, j].GetComponent<Dot>().row-= nullCount;
                    alldots[i, j] = null;
                }
                   
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(refillDelay*0.5f);
        StartCoroutine(FillBoardCo());

    }

    private void RefillBoard()
    {
        for (int i=0;i<width;i++)
        {
            for(int j=0;j<height;j++)
            {
                if(alldots[i,j]==null &&  !blankSpaces[i,j])
                {
                    Vector2 tempPosition = new Vector2(i, j+offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIterations = 0;

                    while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                    {
                        maxIterations++;
                        dotToUse = Random.Range(0, dots.Length);
                    }

                    maxIterations = 0;
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    alldots[i, j] = piece;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;
                }
            } 
        }
    }

    private bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (alldots[i, j] != null)
                {
                    if (alldots[i, j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(refillDelay);
        while (MatchesOnBoard())
        {
            streakValue++;
            DestroyMatches();
            yield return new WaitForSeconds(2*refillDelay);
      
        }
        findMatches.CurrentMatches.Clear();
        currentDot = null;
        yield return new WaitForSeconds(.5f);
        if(IsDeadlocked())
        {
            ShuffleBoard();
           

            Debug.Log("Deadloced!!!");
        }
        yield return new WaitForSeconds(refillDelay);

        
        currentState = GameState.move;
        streakValue = 1;
    }

    private void SwitchPieces(int column,int row,Vector2 direction)
    {
        //Take the second piece and save it in a holder
        GameObject holder = alldots[column+(int )direction.x, row+(int)direction.y] as GameObject;
        //Switching the first dot to be the second position
        alldots[column + (int)direction.x, row + (int)direction.y] = alldots[column, row];
        //Set the first dot to be the second dot
        alldots[column, row] = holder;
    }

    private bool CheckForMatches()
    {
        for(int i=0;i<width;i++)
        {
            for(int j=0;j<width;j++)
            {
                if(alldots[i,j]!=null)
                {
                    //make sure that one and two to the right are in the board
                    if (i < width - 2)
                    {
                        //Check if the dots to the right and two to the right exist
                        if (alldots[i + 1, j] != null && alldots[i + 2, j] != null)
                        {
                            if (alldots[i + 1, j].tag == alldots[i, j].tag && alldots[i + 2, j].tag == alldots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                    if (j < height - 2)
                    {
                        //check if the dots above exists
                        if (alldots[i, j + 1] != null && alldots[i, j + 2] != null)
                        {
                            if (alldots[i, j + 1].tag == alldots[i, j].tag && alldots[i, j + 2].tag == alldots[i, j].tag)
                            {
                                return true;
                            }
                        }

                    }
                }
            }
                
        }
        return false;
    }

    public bool SwitchAndCheck(int column, int row, Vector2 direction)
    {
        SwitchPieces(column, row, direction);
        if (CheckForMatches())
        {
            SwitchPieces(column, row, direction);
            return true;
        }
        SwitchPieces(column, row, direction);
        return false; 

    }

    private bool IsDeadlocked()
    {
        for(int i=0;i<width;i++)
        {
            for(int j=0;j<height;j++)
            {
                if(alldots[i,j]!=null)
                {
                    if(i<width-1)
                    {
                        if(SwitchAndCheck(i,j,Vector2.right))
                        {
                            return false;
                        }
                    }
                    if(j<height-1)
                    {
                        if(SwitchAndCheck(i,j,Vector2.up))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private void ShuffleBoard()
    {
        //create a list of gameobjects
        List<GameObject> newBoard = new List<GameObject>();
        //Add every piece to this list
        for(int i=0;i<width;i++)
        {
            for(int j = 0; j < height;j++)
            {
                if(alldots[i,j]!=null)
                {
                    newBoard.Add(alldots[i, j]);
                }
            }


        }
        //for every spot on the board
        for(int i=0;i<width;i++)
        {
            for(int j=0;j<height;j++)
            {
                //if this spot should not be blank
                if(!blankSpaces[i,j])
                {
                    //pick a random number
                    int pieceToUse = Random.Range(0, newBoard.Count);
                    
                    //Assign the column to the piece
                    int maxIterations = 0;
                    while (MatchesAt(i, j, newBoard[pieceToUse]) && maxIterations < 100)
                    {
                        pieceToUse = Random.Range(0, newBoard.Count);
                        maxIterations++;
                        Debug.Log(maxIterations);
                    }
                    //make a container for the piece
                    Dot piece = newBoard[pieceToUse].GetComponent<Dot>();
                    maxIterations = 0;
                    piece.column = i;
                    piece.row = j;
                    //Fill in the dots array with this new piece
                    alldots[i, j] = newBoard[pieceToUse];
                    //Remove it from the list
                    newBoard.Remove(newBoard[pieceToUse]);
                }
            }
        }
        //check if it is still deadlocked
        if(IsDeadlocked())
        {
            ShuffleBoard();
            
        }
    }

}
