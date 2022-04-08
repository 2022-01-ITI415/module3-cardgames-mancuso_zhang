using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    none,
    gameFin
}

public class ScoreManager : MonoBehaviour
{
    static public int SCORE_FROM_PREVIOUS_ROUND = 0;
	static public int HIGH_SCORE = 0;

    static public string[] letters = new string[] {"C","D","H","S"};
    static public string[] numbers2 = new string[] {"1","2","3","4","5","6","7","8","9","10","11","12","13"};
    static public int[] numbers1 = new int[] {1,2,3,4,5,6,7,8,9,10,11,12,13};

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Score(PokerHand pkh)
    {
        switch (pkh)
        {
            case PokerHand.royalFlush:

                break;

            case PokerHand.straightFlush:

                break;

            case PokerHand.four:

                break;

            case PokerHand.fullHouse:

                break;

            case PokerHand.flush:

                break;

            case PokerHand.straight:

                break;

            case PokerHand.three:

                break;

            case PokerHand.twoPair:

                break;

            case PokerHand.onePair:

                break;

            case PokerHand.none:

                break;
        }

        switch (pkh)
        {
            case PokerHand.gameFin:

                print("Game Over!");

                break;
            default:
                break;
        }
    }
}
