using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    private Board board;
    public Text ScoreText;
    public int Score;
    public Image scoreBar;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    // Update is called once per frame
    void Update()
    {
        ScoreText.text = "" + Score;
        UpdateBar();

    }
    public void IncreaseScore(int amountToIncrease)
    {
        Score += amountToIncrease;
    } 
    private void UpdateBar()
    {
        if(board!=null && scoreBar!=null)
        {
            int length = board.scoreGoals.Length;
            scoreBar.fillAmount = (float)Score / (float)board.scoreGoals[length - 1];

        }
    }
}
