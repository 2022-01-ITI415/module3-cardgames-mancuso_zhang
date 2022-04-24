using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Poker : MonoBehaviour 
{
	static public Poker 	S;
    static public int SCORE_FROM_PREVIOUS_ROUND = 0;
	static public int HIGH_SCORE = 0;

	public float reloadDelay = 5.0f; //The delay between rounds

	[Header("Set in Inspector")]
	public Deck			deck;
    public TextAsset    deckXML;
	public Layout       layout;	
    public TextAsset    layoutXML;

    public float        xOffset = 3; 
    public float        yOffset = -2.5f; 
    public Vector3      layoutCenter;
	public Transform    layoutAnchor; 

    public CardPoker		target; 
    public List<CardPoker>	drawPile;
    public List<CardPoker>	tableau; 
    public List<CardPoker>	discardPile;
	public List<CardPoker>	temporary;

    public int score = 0;

    public FloatingScore fsRun;
	public Text GTGameOver;
	public Text GTRoundResult;

	public GameObject prefabCard;

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
		S = this; // Poker singleton

        SetText();
        ShowScore(false);
        // Check for a high score in PlayerPrefs
		if (PlayerPrefs.HasKey("PokerHighScore"))
		{
			HIGH_SCORE = PlayerPrefs.GetInt("PokerHighScore");
		}

		// Add the score from the last round, which will be >0 if it was a win
		score = SCORE_FROM_PREVIOUS_ROUND;

		// And reset the SCORE_FROM_PREVIOUS_ROUND
		SCORE_FROM_PREVIOUS_ROUND = 0;

		// Set up the Texts that show at the end of the round. Set the Text Components
		GameObject go = GameObject.Find("GameOver");
		if (go != null)
		{
			GTGameOver = go.GetComponent<Text>();
		}

		go = GameObject.Find("RoundResult");
		if (go != null)
		{
			GTRoundResult = go.GetComponent<Text>();
		}

		// Make them invisible
		ShowResultsGTs(false);

		go = GameObject.Find("HighScore");
		string hScore = "High score: " + HIGH_SCORE;
		go.GetComponent<Text>().text = hScore;
	}

    void ShowResultsGTs(bool show)
	{
		GTGameOver.gameObject.SetActive(show);
		GTRoundResult.gameObject.SetActive(show);
	}


	void Start() 
    {
        // Scoreboard.S.score = score;

		deck = GetComponent<Deck> (); 		// gets the Deck
		deck.InitDeck (deckXML.text); 		// pass the DeckXML to it
        Deck.Shuffle(ref deck.cards); 		// shuffles the deck
		// The ref keyword passes a reference to deck.cards, which allows
		// deck.cards to be modified by Deck.Shuffle()

		layout = GetComponent<Layout>();  	// Get the Layout  
        layout.ReadLayout(layoutXML.text); 	// Pass LayoutXML to it

		drawPile = ConvertListCardsToListCardPokers(deck.cards);
		LayoutGame();
	}

	List<CardPoker> ConvertListCardsToListCardPokers(List<Card> lCD) 
    { 
        List<CardPoker> lCP = new List<CardPoker>(); 
        CardPoker tCP; 
        foreach(Card tCD in lCD) 
        { 
            tCP = tCD as CardPoker;
            lCP.Add(tCP); 
        } 
        return(lCP); 
    }

	CardPoker Draw() 
    { 
        CardPoker cp = drawPile[0]; 	// Pull the 0th CardPoker 
        drawPile.RemoveAt(0);           // Then remove it from List<> drawPile 
        return(cp);           
	}

    // Convert from the layoutID int to the CardPoker with that ID 
    CardPoker FindCardByLayoutID(int layoutID) 
    { 
        foreach (CardPoker tCP in tableau) 
        { 
            // Search through all cards in the tableau List<> 
            if (tCP.layoutID == layoutID) 
            { 
               // If the card has the same ID, return it 
               return(tCP); 
            } 
        } 
        // If it's not found, return null 
        return(null); 
    }

	void LayoutGame() 
    { 
        // Create an empty GameObject to serve as an anchor for the tableau
        if (layoutAnchor == null) 
        { 
            GameObject tGO = new GameObject("_LayoutAnchor"); 
            // ^ Create an empty GameObject named _LayoutAnchor in the Hierarchy 
            layoutAnchor = tGO.transform;                       // Grab its Transform
			layoutAnchor.transform.position = layoutCenter;     // Position it 
        }

		CardPoker cp; 

      	// Follow the layout 
      	foreach (SlotDef tSD in layout.slotDefs) 
        { 
			// GameObject cp = Instantiate(prefabCard) as GameObject;	// creates an instant of the prefabCard for the grid
			// cp.transform.parent = layoutAnchor;						// Make its parent layoutAnchor 

          	// ^ Iterate through all the SlotDefs in the layout.slotDefs as tSD 
          	cp = Draw();                            // Pull a card from the top (beginning) of the draw Pile 
          	cp.faceUp = tSD.faceUp;                 // Set its faceUp to the value in SlotDef 
          	cp.transform.parent = layoutAnchor;     // Make its parent layoutAnchor 
          	
            // This replaces the previous parent: deck.deckAnchor, which 
          	// appears as _Deck in the Hierarchy when the scene is playing. 
          	cp.transform.localPosition = new Vector3( 
                layout.multiplier.x * tSD.x, 
                layout.multiplier.y * tSD.y, 
                -tSD.layerID ); 
        	// ^ Set the localPosition of the card based on slotDef 
			
            cp.layoutID = tSD.id; 
        	cp.slotDef = tSD; 
    		cp.state = CardState.tableau; 

    		tableau.Add(cp); // Add this CardPoker to the List<> tableau     
	  	}
		// set up the initial target card
		MoveToTarget(Draw()); 
     	// Set up the Draw pile 
      	UpdateDrawPile();
	}
	 
    // This turns cards face-up (testing phase)
    void SetTableauFaces(CardPoker cp) 
    {
		cp.faceUp = true;
    }

	void MoveToDiscard(CardPoker cp) 
    { 
        // Set the state of the card to discard 
        cp.state = CardState.discard; 
        discardPile.Add(cp);                    // Add it to the discardPile List<> 
        cp.transform.parent = layoutAnchor;     // Update its transform parent 
        
        // Position this card on the discardPile 
        cp.transform.localPosition = new Vector3( 
            layout.multiplier.x * layout.discardPile.x, 
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + 0.5f ); 
        cp.faceUp = true; 

        // Place it on top of the pile for depth sorting 
        cp.SetSortingLayerName(layout.discardPile.layerName); 
        cp.SetSortOrder(-100 + discardPile.Count); 
    } 

    // Make cp the new target card (starting card location)
    void MoveToTarget(CardPoker cp) 
    { 
        // If there is currently a target card, move it to discardPile 
        if (target != null) MoveToDiscard(target); 
        target = cp;                            // cp is the new target 
        cp.state = CardState.target; 
        cp.transform.parent = layoutAnchor; 
        
        // Move to the target position 
        cp.transform.localPosition = new Vector3( 
            layout.multiplier.x * layout.discardPile.x, 
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID ); 
 		cp.faceUp = true; // Make it face-up 
    }

	public void MoveToTemporaryPile(CardPoker cp)
	{
		cp.state = CardState.temporary;

		cp.transform.parent = layoutAnchor;
		cp.transform.localPosition = new Vector3(
        	layout.multiplier.x* layout.temporaryPile.x,
        	layout.multiplier.y* layout.temporaryPile.y,
        	-layout.temporaryPile.layerID + 0.5f);
		cp.faceUp = true;
	}

	public void PlaceCard(CardPoker cp)
    {   
		temporary.Add(target);						// Add it to the temporary List<>
		
		target.state = CardState.tableau;			// Set the target to state of tableau 
		target.transform.parent = layoutAnchor;     // Update its transform parent 

		SlotDef tSD = cp.slotDef;
		// Position this card on the slotDef 
        target.transform.localPosition = new Vector3( 	// Find the location of the target
            layout.multiplier.x * tSD.x, 
            layout.multiplier.y * tSD.y, 
            -tSD.layerID); 
        // cp.faceUp = true; 								

		target.layoutID = tSD.id; 			// Change target.ID to the cp.ID
        target.slotDef = tSD; 				// Change target.slotDef to cp.slotDef (switch)
		target = cp;
    }

	void UpdateDrawPile() 
    { 
        CardPoker cp; 
        
        // Go through all the cards of the drawPile 
        for (int i = 0; i < drawPile.Count; i++) 
        { 
            cp = drawPile[i]; 
            cp.transform.parent = layoutAnchor; 

 			// Position it correctly with the layout.drawPile.stagger 
            Vector2 dpStagger = layout.drawPile.stagger; 
            cp.transform.localPosition = new Vector3( 
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x), 
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y), 
                -layout.drawPile.layerID+0.1f*i ); 
 			cp.faceUp = false; // Make them all face-down 
            cp.state = CardState.drawpile; 
            
            // Set depth sorting 
            cp.SetSortingLayerName(layout.drawPile.layerName); 
            cp.SetSortOrder(-10 * i); 
          } 
    }

	public void CardClicked(CardPoker cp) 
    {
        
        // The reaction is determined by the state of the clicked card 
      	switch (cp.state) 
        { 
        	case CardState.target: 
          		// Clicking the target card does nothing 
          		break; 

 			case CardState.drawpile: 
            	// Clicking any card in the drawPile does nothing
            	break; 

 			case CardState.tableau: 
            	// Clicking a card in the tableau will check if it's a valid play 
				bool validMatch = true; 
             	if (cp.faceUp) 
                { 
                // If the card is face-up, it's not valid 
                 	validMatch = false; 
             	}  
             	if (!validMatch) return;    // return if not valid 

                // If we got here, then: Yay! It's a valid card. 
                tableau.Remove(cp);         // Remove it from the tableau List 
				PlaceCard(cp);
				MoveToTemporaryPile(cp);
                
				// ScoreManager(ScoreEvent.mine);
            	break; 
      	}
		CheckForGameOver(); 
    }

    void CheckForGameOver() 
    { 
       // If the tableau is empty, the game is over 
       if (tableau.Count == 0) 
       { 
           // Call GameOver() with a win 
           GameOver(true); 
           return; 
        }
		GameOver(false);
    } 

    // Called when the game is over. Simple for now, but expandable 
    void GameOver(bool won) 
    { 
        if (won) 
        { 
            print("Game Over!");

            row1.text = CheckHand(GetRow(grid,0)).ToString();
            row2.text = CheckHand(GetRow(grid,1)).ToString();
            row3.text = CheckHand(GetRow(grid,2)).ToString();
            row4.text = CheckHand(GetRow(grid,3)).ToString();
            row5.text = CheckHand(GetRow(grid,4)).ToString();
            col1.text = CheckHand(GetColumn(grid,0)).ToString();
            col2.text = CheckHand(GetColumn(grid,1)).ToString();
            col3.text = CheckHand(GetColumn(grid,2)).ToString();
            col4.text = CheckHand(GetColumn(grid,3)).ToString();
            col5.text = CheckHand(GetColumn(grid,4)).ToString();
            
            for (int i = 0; i < 5; i++)
            {
                int temp = CheckHand(GetRow(grid,i));
                score += temp;
                temp = CheckHand(GetColumn(grid,i));
                score += temp;
            }

            GTGameOver.text = "Round Over!";

            // Poker.SCORE_FROM_PREVIOUS_ROUND = score;

            string sRR = "Round Score: " + score;
            GTRoundResult.text = sRR;
            if (Poker.HIGH_SCORE <= score)
            {
                Poker.HIGH_SCORE = score;
			    PlayerPrefs.SetInt("PokerHighScore", score);
            }

            ShowResultsGTs(true);
            ShowScore(true);
        } 
        else 
        { 
            return;
        } 
        // Reload the scene, resetting the game 
        Invoke("ReloadLevel", reloadDelay);
    }
	
	void ReloadLevel()
	{
		//Reload trhe scene, resetting the game
		SceneManager.LoadScene("Poker_Scene_2");
	}

//
//
//
//
// The start of ScoreManager
//
//
//
//

        public void SetText ()
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

        public void ShowScore(bool show)
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
 
    int CheckHand (int[] arr)
    {
        string[] str = ConvertToSuit(arr);
        arr = ConvertToRank(arr);
        
        if (Royal(arr) && Flush(str)) // royal flush
        {
            return 100;
        }
        else if (Straight(arr) && Flush(str)) // straight flush
        {
            return 75;
        }
        else if (Quads(arr)) // four of a kind
        { 
            return 50; 
        }
        else if (TwoPair(arr) && Trips(arr)) // full house
        {
            return 25;
        }
        else if (Flush(str)) // flush
        {
            return 20;
        }
        else if (Straight(arr)) // straight
        {
            return 15;
        }
        else if (Trips(arr)) // triple
        { 
            return 10; 
        }
        else if (TwoPair(arr)) // two pair
        { 
            return 5; 
        }
        else if (Pair(arr)) // one pair
        { 
            return 2; 
        }
        return 0;
    }

    int[] ConvertToRank (int[] arr)
    {
        for (int i = 0; i < 5; i++)
        {
            arr[i] = GetRank(arr[i]);
        }
        return arr;
    }

    string[] ConvertToSuit (int[] arr)
    {
        string[] str = new string[5];
        for (int i = 0; i < 5; i++)
        {
            str[i] = GetSuit(arr[i]);
        }
        return str;
    }

    int GetRank (int layoutID)
    {
        foreach (CardPoker tCP in temporary) 
        { 
            // Search through all cards in the tableau List<> 
            if (tCP.layoutID == layoutID) 
            { 
               // If the card has the same ID, return the rank
               return(tCP.rank); 
            } 
        } 
        // If it's not found, return null 
        return(0); 
    }

    string GetSuit (int layoutID) 
    {
        foreach (CardPoker tCP in temporary) 
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

    Dictionary<int, int> Counting (int[] arr)
    {
        var dict = new Dictionary <int, int> ();

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
        return dict;
    }

    bool Pair (int[] arr)
    {	
        var dict = Counting(arr);

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
        var dict = Counting(arr);
        
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
        var dict = Counting(arr);
        
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
        var dict = Counting(arr);
        
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

        for (int i = 0; i < 4; i++)
        {
            if (Mathf.Abs(arr[i] - arr[i+1]) != 1)
            {
                return false;
            }
        }
        return true;
    }

    bool Flush (string[] arr)
    {
        for (int i = 0; i < 5; i++)
        {
            if (arr[i] != arr[1])
            {
                return false;
            }
        }
        return true;
    }

    bool Royal (int[] arr)
    {
        Array.Sort(arr);
        int[] royals = {1,10,11,12,13};

        for (int i = 0; i < 5; i++)
        {
            if (arr[i] != royals[i])
            {
                return false;
            }
        }
        return true;
    }

    public int[] GetRow(int[,] mat, int row)
	{
		int[] temp = new int[5];
		for (int i = 0; i < 5; i++)
		{
    		temp[i] = mat[row, i];
		}
		return temp;
	}
	
	public int[] GetColumn(int[,] mat, int col)
	{
		int[] temp = new int[5];
		for (int i = 0; i < 5; i++)
		{
    		temp[i] = mat[i, col];
		}
		return temp;
	}
}
