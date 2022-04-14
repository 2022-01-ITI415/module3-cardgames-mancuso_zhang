using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PokerHand 
{
    royalFlush,
    straightFlush,
    four,
    fullHouse,
    flush,
    straight,
    three,
    twoPair,
    onePair,
    gameFin
}

public class ScoreManager : MonoBehaviour
{
    static public int SCORE_FROM_PREVIOUS_ROUND = 0;
	static public int HIGH_SCORE = 0;



    static public string[] letters = new string[] {"C","D","H","S"};
    static public string[] numbers2 = new string[] {"1","2","3","4","5","6","7","8","9","10","11","12","13"};
    static public int[] numbers1 = new int[] {1,2,3,4,5,6,7,8,9,10,11,12,13};

    static public int[,] grid = { {0,1,2,3,4},
        {5,6,7,8,9},
        {10,11,12,13,14},
        {15,16,17,18,19},
        {20,21,22,23,24} };

    public Text row1;
    public Text row2;
    public Text row3;
    public Text row4;
    public Text row5;
    public Text col1;
    public Text col2;
    public Text col3;
    public Text col4;
    public Text col5;

    void Awake() 
    {
        SetText();

    }


    public void GameEnd (PokerHand pkh)
    {
        switch (pkh)
        {
            case PokerHand.gameFin:

                print("Game Over!");

                row1.text = ToString(CycleCheckHand(GetRow(grid,0)));
                row2.text = ToString(CycleCheckHand(GetRow(grid,1)));
                row3.text = ToString(CycleCheckHand(GetRow(grid,2)));
                row4.text = ToString(CycleCheckHand(GetRow(grid,3)));
                row5.text = ToString(CycleCheckHand(GetRow(grid,4)));
                col1.text = ToString(CycleCheckHand(GetColumn(grid,0)));
                col2.text = ToString(CycleCheckHand(GetColumn(grid,1)));
                col3.text = ToString(CycleCheckHand(GetColumn(grid,2)));
                col4.text = ToString(CycleCheckHand(GetColumn(grid,3)));
                col5.text = ToString(CycleCheckHand(GetColumn(grid,4)));

                break;
            default:

                break;
        }
    }

    void SetText ()
    {
        GameObject go = GameObject.Find("row1");
        if (go != null) { row1 = go.GetComponent<Text>(); }
        go = GameObject.Find("row2");
        if (go != null) { row2 = go.GetComponent<Text>(); }
        go = GameObject.Find("row3");
        if (go != null) { row3 = go.GetComponent<Text>(); }
        go = GameObject.Find("row4");
        if (go != null) { row4 = go.GetComponent<Text>(); }
        go = GameObject.Find("row5");
        if (go != null) { row5 = go.GetComponent<Text>(); }
        go = GameObject.Find("col1");
        if (go != null) { col1 = go.GetComponent<Text>(); }
        go = GameObject.Find("col2");
        if (go != null) { col2 = go.GetComponent<Text>(); }
        go = GameObject.Find("col3");
        if (go != null) { col3 = go.GetComponent<Text>(); }
        go = GameObject.Find("col4");
        if (go != null) { col4 = go.GetComponent<Text>(); }
        go = GameObject.Find("col5");
        if (go != null) { col5 = go.GetComponent<Text>(); }

        // Make them invisible
		ShowScore(false);
    }

    void ShowScore(bool show)
	{
		row1.gameObject.SetActive(show);
		row2.gameObject.SetActive(show);
        row3.gameObject.SetActive(show);
		row4.gameObject.SetActive(show);
        row5.gameObject.SetActive(show);
		col1.gameObject.SetActive(show);
        col2.gameObject.SetActive(show);
		col3.gameObject.SetActive(show);
        col4.gameObject.SetActive(show);
		col5.gameObject.SetActive(show);
	}

    int CycleCheckHand (int[] arr)
    {
        int score = 0;
        foreach(PokerHand hand in Enum.GetValues(typeof(PokerHand)))
        {
            if (CheckHand(hand, arr) > score)
            {
                score = CheckHand(hand, arr);
            }
        }
    }
 
    int CheckHand (PokerHand pkh, int[] arr)
    {
        string[] str;
        int check = 0;
        switch (pkh)
        {
            case PokerHand.royalFlush:
                check = 100;
                break;

            case PokerHand.straightFlush:
                check = 75;
                break;

            case PokerHand.four:
                if (Quadruple(ConvertToRank(arr))) 
                { 
                    check = 50; 
                }
                break;

            case PokerHand.fullHouse:
                if (Double(ConvertToRank(arr)) && Triple(ConvertToRank(arr)))
                {
                    check = 25;
                }
                break;

            case PokerHand.flush:
                check = 20;
                break;

            case PokerHand.straight:
                check = 15;
                break;

            case PokerHand.three:
                if (Trips(ConvertToRank(arr))) 
                { 
                    check = 10; 
                }
                break;

            case PokerHand.twoPair:
                if (Double(ConvertToRank(arr))) 
                { 
                    check = 5; 
                }
                break;

            case PokerHand.onePair:
                if (Pair(ConvertToRank(arr))) 
                { 
                    check = 2; 
                }
                break;

            default: 
                break;
        }
        return check;
    }

    int[] ConvertToRank (int[] arr)
    {
        foreach (int count in arr)
        {
            count = GetRank(count);
        }
        return arr;
    }

    string[] ConvertToSuit (int[] arr)
    {
        string[] str = new string[5];
        for (i = 0; i < arr.Length(); i++)
        {
            str[i] = GetSuit(arr[i]);
        }
        return str;
    }

    int GetRank (int layoutID)
    {
        foreach (CardProspector tCP in tableau) 
        { 
            // Search through all cards in the tableau List<> 
            if (tCP.layoutID == layoutID) 
            { 
               // If the card has the same ID, return the rank
               return(tCP.rank); 
            } 
        } 
        // If it's not found, return null 
        return(null); 
    }

    string GetSuit (int layoutID) 
    {
        foreach (CardProspector tCP in tableau) 
        { 
            // Search through all cards in the tableau List<> 
            if (tCP.layoutID == layoutID) 
            { 
               // If the card has the same ID, return the rank
               return(tCP.suit); 
            } 
        } 
        // If it's not found, return null 
        return(null);
    }

    bool Pair (int[] arr)
    {	
        var dict = new Dictionary < int, int > ();

        foreach (var count in arr) 
        {
	        if (dict.ContainsKey(count))
            {
                dict[count]++;
            }
            else
            {
                dict[count] = 1;
            }
        }
        foreach (var val in dict)
        {
            if (val.Value == 2)
            {
                return true;
            }
        }
        return false;
    }
	
	bool TwoPair (int[] arr)
    {	
        var dict = new Dictionary < int, int > ();
        foreach (var count in arr) 
        {
	        if (dict.ContainsKey(count))
            {
                dict[count]++;
            }
            else
            {
                dict[count] = 1;
            }
        }
		int n = 0;
        foreach (var val in dict)
		{
		 	if (val.Value >= 2)
			{
				n++;
			}
		}
		if (n == 2)
		{
            return true;
		}
        return false;
    }

    bool Trips (int[] arr)
    {	
        var dict = new Dictionary < int, int > ();
        foreach (var count in arr) 
        {
	        if (dict.ContainsKey(count))
            {
                dict[count]++;
            }
            else
            {
                dict[count] = 1;
            }
        }
        foreach (var val in dict)
        {
            if (val.Value == 3)
            {
                return true;
            }
        }
        return false;
    }
    bool Quads (int[] arr)
    {
        var dict = new Dictionary < int, int > ();
        foreach (var count in arr) 
        {
	        if (dict.ContainsKey(count))
            {
                dict[count]++;
            }
            else
            {
                dict[count] = 1;
            }
        }
        foreach (var val in dict)
		{
		 	if (val.Value == 4)
			{
                return true;
			}
		}
        return false;
    }

    bool Straight (int[] arr)
    {
        Array.Sort(arr);
        bool check = false;
        for(int i = 0; i < arr.Length()-1; i++)
        {
            if (Mathf.Abs(arr[i] - arr[i+1] == 1))
            {
                check = true;
            }
            else 
            {
                check = false;
            }
            if (!check) { return false; }
        }
        return check;

    }

    public bool AdjacentRank(CardProspector c0, CardProspector c1) 
    { 
        // If they are 1 apart, they are adjacent 
        if (Mathf.Abs(c0.rank - c1.rank) == 1) return(true); 


        
        // Otherwise, return false 
        return(false); 
    }

    static public int[] GetRow(int[,] mat, int row)
	{
		int[] temp = new int[5];
		for (int i = 0; i < mat.GetLength(row); i++)
		{
    		temp[i] = mat[row, i];
		}
		return temp;
	}
	
	static public int[] GetColumn(int[,] mat, int col)
	{
		int[] temp = new int[5];
		for (int i = 0; i < mat.GetLength(col); i++)
		{
    		temp[i] = mat[i, col];
		}
		return temp;
	}
}
