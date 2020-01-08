using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Board Variables")]
    public int column;
    public int row;
    public int previouscolumn;
    public int previousRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;

    private HintManager hintManager; 
    private FindMatches findMatches;
    public  GameObject otherDot;
    private Board board;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;

    [Header(("Swipe Stuff"))]
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    [Header("Powerup Stuff")]
    public bool isColorBomb;
    public bool isColumnBomb;
    public bool isRowBomb;
    public bool isAdjacentBomb;
    public GameObject adjacentMarker;
    public GameObject rowArrow;
    public GameObject columArrow;
    public GameObject colorBomb;


    // Start is called before the first frame update
    void Start()
    {
        isColumnBomb = false;
        isRowBomb = false;
        isColorBomb = false;
        isAdjacentBomb = false;

        hintManager = FindObjectOfType<HintManager>();
        board = FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();
        //targetX = (int)transform.position.x;
        //targetY = (int)transform.position.y;
        //row =targetY;
        //column = targetX;
        //previousRow = row;
        //previouscolumn = column; 

    }

    //This is for Testing and Debug  only.

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isAdjacentBomb = true;
            GameObject marker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
            marker.transform.parent = this.transform;
        }
    }

    // Update is called once per frame  

    void Update() 
    {
        /*
        if (isMatched)
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            Color currenColor = mySprite.color; 
            mySprite.color = new Color(currenColor.r,currenColor.g,currenColor.b,.5f);
        }
        */  
        targetX = column;
        targetY = row;
        if(Mathf.Abs(targetX-transform.position.x)>.1)
        {
            //move Towards the target
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
             if (board.alldots[column,row] !=this .gameObject)
            {
                board.alldots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }



        else
        {
            //Directly set the position
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
            //board.alldots[column, row] = this.gameObject;
        }
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            //move Towards the target
            tempPosition = new Vector2(transform.position.x,targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .4f);
            if (board.alldots[column, row] != this.gameObject)
            {
                board.alldots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }


        else
        {
            //Directly set the position
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
            //board.alldots[column, row] = this.gameObject;
        }
    }

    public IEnumerator checkMoveCo()
    {   
        if(isColorBomb)
        {
            //This piece is color bomb,and the other piece is color to destroy
            findMatches.MatchPiecesofColor(otherDot.tag);
            isMatched = true;

        }
        else if(otherDot.GetComponent<Dot>().isColumnBomb)
        {
            //The other piece is a color bomb,and this piece has the color to destroy
            findMatches.MatchPiecesofColor(this.gameObject.tag);
            otherDot.GetComponent<Dot>().isMatched = true; 
        }

        yield return new WaitForSeconds(.5f);
        if (otherDot != null)
        {
            if (!isMatched && !otherDot.GetComponent<Dot>().isMatched)
            {
                otherDot.GetComponent<Dot>().row = row;
                otherDot.GetComponent<Dot>().column= column;
                row = previousRow;
                column = previouscolumn;
                yield return new WaitForSeconds(.5f);
                board.currentDot = null;
                board.currentState = GameState.move;
                
            }
            else
            {
                board.DestroyMatches();
               
            }
           //  otherDot = null;  
            
        }
         
    }

    private void OnMouseDown()
    {
        //Destroy the hint
        if (hintManager != null)
        {
            hintManager.DestroyHint();
        }
        if (board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        } 
    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }

    } 

    void CalculateAngle()
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            board.currentState = GameState.wait;
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();
           
            board.currentDot = this;
        } 
        else
        {
            board.currentState = GameState.move;
           
        }
    }

    void MovePiecesActual(Vector2 direction)
    {
        otherDot = board.alldots[column + (int)direction.x, row + (int)direction.y];
        previousRow = row;
        previouscolumn = column;
        if (otherDot != null)
        {
            otherDot.GetComponent<Dot>().column += -1 * (int)direction.x;
            otherDot.GetComponent<Dot>().row += -1 * (int)direction.y;
            column += (int)direction.x;
            row += (int)direction.y;
            StartCoroutine(checkMoveCo());
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    void MovePieces()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            //RightSwipe
            /*
            otherDot = board.alldots[column + 1, row];
            previousRow = row;
            previouscolumn = column; 
            otherDot.GetComponent<Dot>().column -= 1;
            column += 1;
            StartCoroutine(checkMoveCo());
            */
            MovePiecesActual(Vector2.right);
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {

            //UpSwipe
            /*
            otherDot = board.alldots[column , row+1];
            previousRow = row;
             previouscolumn = column; 
            otherDot.GetComponent<Dot>().row -= 1;
            row += 1;
            StartCoroutine(checkMoveCo());
            */
            MovePiecesActual(Vector2.up);
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {

            //LeftSwipe
            /*
            otherDot = board.alldots[column - 1, row];
            previousRow = row;
            previouscolumn = column; 
            otherDot.GetComponent<Dot>().column += 1;
            column -= 1;
            StartCoroutine(checkMoveCo());
            */
            MovePiecesActual(Vector2.left);
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {

            //DownSwipe 
            /*
            otherDot = board.alldots[column , row -1];
            previousRow = row;
            previouscolumn = column; 
            otherDot.GetComponent<Dot>().row += 1;
            row-= 1;
            StartCoroutine(checkMoveCo());
            */
            MovePiecesActual(Vector2.down);
        }
        else
        {
            board.currentState = GameState.move;

        }
       
    }

    void FindMatches()
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftDot1 = board.alldots[column - 1, row];
            GameObject rightDot1 = board.alldots[column + 1, row];
            if (leftDot1 != null && rightDot1 != null)
            {
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
                {
                    leftDot1.GetComponent<Dot>().isMatched = true;
                    rightDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }

        }
        if (row > 0 && row < board.height - 1)
        {
            GameObject upDot1 = board.alldots[column, row + 1];
            GameObject downDot1 = board.alldots[column, row - 1];
            if (upDot1 != null && downDot1 != null)
            {
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
                {
                    upDot1.GetComponent<Dot>().isMatched = true;
                    downDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }

    }
     
    public void MakeRowBomb()
    {
        isRowBomb = true;
        GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColumnBomb()
    {
        isColumnBomb = true;
        GameObject arrow = Instantiate(columArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
        this.gameObject.tag = "color";
    }

    public void MakeColorBomb()
    {
        isColorBomb = true;
        GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
        color.transform.parent = this.transform;
    }

    public void MakeAdjacentBomb()
    {
        isAdjacentBomb = true;
        GameObject maker = Instantiate(adjacentMarker, transform.position, Quaternion.identity);
        maker.transform.parent = this.transform;
    }
}
  