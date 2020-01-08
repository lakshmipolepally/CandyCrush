using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{

    private Board board;

    public List<GameObject> CurrentMatches = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }

    private List<GameObject> IsAdjacentBomb(Dot dot1,Dot dot2,Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isAdjacentBomb)
        {
            CurrentMatches.Union(GetAdjacentPieces( dot1.column,dot1.row));
        }
        if (dot2.isAdjacentBomb)
        {
            CurrentMatches.Union(GetAdjacentPieces(dot1.column, dot1.row));
        }
        if (dot3.isAdjacentBomb)
        {
            CurrentMatches.Union(GetAdjacentPieces(dot1.column, dot1.row));
        }
        return currentDots;
    }

    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3)
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isRowBomb)
        {
            CurrentMatches.Union(GetRowPieces(dot1.row));
        }
        if (dot2.GetComponent<Dot>().isRowBomb)
        {
            CurrentMatches.Union(GetRowPieces(dot2.row));
        }
        if (dot3.GetComponent<Dot>().isRowBomb)
        {
            CurrentMatches.Union(GetRowPieces(dot3.row));
        }
        return currentDots;
    }

    private List<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3)
        {
            List<GameObject> currentDots = new List<GameObject>();
            if (dot1.isColumnBomb)
            {
                CurrentMatches.Union(GetColumnPieces(dot1.row));
            }
            if (dot2.isColumnBomb)
            {
                CurrentMatches.Union(GetColumnPieces(dot2.row));
            }
            if (dot3.isColumnBomb)
            {
                CurrentMatches.Union(GetColumnPieces(dot3.row));
            }
            return currentDots;

        }

    private void AddToListAndMatch(GameObject dot)
    {
        if (!CurrentMatches.Contains(dot))
        {
            CurrentMatches.Add(dot);
        }
        dot.GetComponent<Dot>().isMatched = true;
    }

    private void GetNearbyPieces(GameObject dot1,GameObject dot2,GameObject dot3)
    {

        AddToListAndMatch(dot1);
        AddToListAndMatch(dot2);
        AddToListAndMatch(dot3);
    }

    private IEnumerator FindAllMatchesCo()
    {
        yield return new WaitForSeconds(.2f);
        for (int i = 0; i < board.width; i++)
        {
            for (int j = 0; j < board.height; j++)
            {
                GameObject currentDot = board.alldots[i, j];
              
                if (currentDot != null)
                {
                    Dot currentDotDot = currentDot.GetComponent<Dot>();
                    if (i > 0 && i < board.width - 1)
                    {
                        GameObject leftDot = board.alldots[i - 1, j];

                        GameObject rightDot = board.alldots[i + 1, j];
                        if (leftDot != null && rightDot != null)
                        {
                            Dot rightDotDot = rightDot.GetComponent<Dot>();
                            Dot leftDotDot = leftDot.GetComponent<Dot>();
                            if (leftDot != null && rightDot != null)
                            {
                                //Dot rightDotDot = rightDot.GetComponent<Dot>();
                                //Dot leftDotDot = leftDot.GetComponent<Dot>();
                                if (leftDot != null && rightDot != null)
                                {
                                    if (leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag)
                                    {
                                        CurrentMatches.Union(IsRowBomb(leftDotDot, currentDotDot, rightDotDot));

                                        CurrentMatches.Union(IsColumnBomb(leftDotDot, currentDotDot, rightDotDot));

                                        CurrentMatches.Union(IsAdjacentBomb(leftDotDot, currentDotDot, rightDotDot));

                                        GetNearbyPieces(leftDot, currentDot, rightDot);

                                    }
                                }
                            }
                        }

                        if (j > 0 && j < board.height - 1)
                        {
                            GameObject upDot = board.alldots[i, j + 1];

                            GameObject downDot = board.alldots[i, j - 1];
                            if (upDot != null && downDot != null)
                            {
                                Dot downDotDot = downDot.GetComponent<Dot>();
                                Dot upDotDot = upDot.GetComponent<Dot>();



                                if (upDot != null && downDot != null)
                                {
                                    if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                                    {
                                        CurrentMatches.Union(IsRowBomb(upDotDot, currentDotDot, downDotDot));

                                        CurrentMatches.Union(IsColumnBomb(upDotDot, currentDotDot, downDotDot));

                                        CurrentMatches.Union(IsAdjacentBomb(upDotDot, currentDotDot, downDotDot));

                                        GetNearbyPieces(upDot, currentDot, downDot);

                                    }

                                }
                            }
                        }
                    }

                }
            }
        }
    }

    public void MatchPiecesofColor(string color)
    {
        for(int i=0;i<board.width;i++)
        {
            for(int j=0;j<board.height;j++)
            {
                //check if that piece exists
                if(board.alldots[i,j]!=null)
                {
                    //check the tag on that dot
                    if(board.alldots[i,j].tag==color)
                    {
                        //set that dot to be matched
                        board.alldots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
    }
     
    List<GameObject> GetAdjacentPieces(int column,int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for(int i=column-1;i<=column+1;i++)
        {
            for(int j=row-1;j<=row+1;j++)
            {
                //check if the pieces is inside the board
                if (i >= 0 && i<board.width && j>=0 && j<board.height)

                {
                    if (board.alldots[i, j] != null)
                    {
                        dots.Add(board.alldots[i, j]);
                        board.alldots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
        return dots;
    }
       
    List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> dots = new List<GameObject>();
        for(int i=0;i<board.height;i++)
        {
            if(board.alldots[column,i]!=null)
            {
                Dot dot = board.alldots[column, i].GetComponent<Dot>();
                if(dot .isRowBomb)
                {
                    dots.Union(GetRowPieces(i)).ToList();
                }
                dots.Add(board.alldots[column, i]);
                dot.isMatched = true;
            }
        }
        return dots;
    }

    List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.width; i++)
        {
            if (board.alldots[i,row] != null)
            {
                Dot dot = board.alldots[row, i].GetComponent<Dot>();
                if (dot.isColumnBomb)
                {
                    dots.Union(GetColumnPieces(i)).ToList();
                }
                dots.Add(board.alldots[i,row]);
                dot.isMatched = true;
            }
        }
        return dots;
    }

    public void checkBombs()
    {
        //Did the Player move something?
        if (board.currentDot !=null)
        {
            //Is the pieces they moved matched?
            if (board.currentDot.isMatched)
            {
                //make it unmatched
                board.currentDot.isMatched = false;
                //Deside what kind of bomb to make 
                /*
                int typeofBomb = Random.Range(0, 100);
                if (typeofBomb<50)
                {
                    //make a row bomb
                    board.currentDot.MakeRowBomb();
                }
                else if(typeofBomb>=50) 
                {
                    //make a column bomb
                    board.currentDot.MakeColumnBomb();
                }
                */
                if((board.currentDot.swipeAngle>-45 && board.currentDot.swipeAngle<=45)
                    ||(board.currentDot.swipeAngle<-135 ||board.currentDot.swipeAngle>=135))
                {
                    board.currentDot.MakeRowBomb();
                }
                else
                {
                    board.currentDot.MakeColumnBomb();
                }
            }
            //Is the other piece matched?
            else if (board.currentDot!=null)
            {
                Dot otherDot = board.currentDot .otherDot.GetComponent<Dot>();
                //Is the other Dot Matched?.
                if(otherDot.isMatched)
                {
                    //make it unmatched
                    otherDot.isMatched = false;
                    /*
                    //Deside what kind of bomb to make
                    int typeofBomb = Random.Range(0, 100);
                    if (typeofBomb < 50)
                    {
                        //make a row bomb
                        otherDot.MakeRowBomb();
                    }
                    else if (typeofBomb >= 50)
                    {
                        //make a column bomb
                        otherDot.MakeColumnBomb();
                    }
                    */
                    if ((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45)
                    || (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135))
                    {
                        otherDot.MakeRowBomb();
                    }
                    else
                    {
                        otherDot.MakeColumnBomb();
                    }

                }
            }
            
               
                    

         
            
        }
    }


}  



    


    

