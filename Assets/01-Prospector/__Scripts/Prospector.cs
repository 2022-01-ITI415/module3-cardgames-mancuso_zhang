using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

//An enum to handle all the possible scoring events
public enum ScoreEvent
{
	draw,
	mine,
	mineGold,
	gameWin,
	gameLoss
}

public class Prospector : MonoBehaviour 
{
	static public Prospector 	S;
    static public int SCORE_FROM_PREVIOUS_ROUND = 0;
	static public int HIGH_SCORE = 0;

	public float reloadDelay = 5.0f; //The delay between rounds

    [Header("Bezier Curve Management")]
	public Transform fsPosMidObject;
	public Transform fsPosRunObject;
	public Transform fsPosMid2Object;
	public Transform fsPosEndObject;

	public Vector3 fsPosMid;
	public Vector3 fsPosRun;
	public Vector3 fsPosMid2;
	public Vector3 fsPosEnd;

	[Header("Set in Inspector")]
	public Deck			deck;
    public TextAsset    deckXML;
	public Layout       layout;	
    public TextAsset    layoutXML;

    public float        xOffset = 3; 
    public float        yOffset = -2.5f; 
    public Vector3      layoutCenter;
	public Transform    layoutAnchor; 

    public CardProspector           target; 
    public List<CardProspector>     drawPile;
    public List<CardProspector>     tableau; 
    public List<CardProspector>     discardPile;

    //Fields to track score info
	public int chain = 0;
	public int scoreRun = 0;
	public int score = 0;

    public FloatingScore fsRun;
	public Text GTGameOver;
	public Text GTRoundResult;

	void Awake()
    {
		S = this; // Prospector singleton

        // Check for a high score in PlayerPrefs
		if (PlayerPrefs.HasKey("ProspectorHighScore"))
		{
			HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
		}

		// Add the score from the last round, which will be >0 if it was a win
		score += SCORE_FROM_PREVIOUS_ROUND;

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
		string hScore = "High score: " + Utils.AddCommasToNumber(HIGH_SCORE);
		go.GetComponent<Text>().text = hScore;
	}

    void ShowResultsGTs(bool show)
	{
		GTGameOver.gameObject.SetActive(show);
		GTRoundResult.gameObject.SetActive(show);
	}

	void Start() 
    {
        Scoreboard.S.score = score;

		deck = GetComponent<Deck> (); 		// gets the Deck
		deck.InitDeck (deckXML.text); 		// pass the DeckXML to it
        Deck.Shuffle(ref deck.cards); 		// shuffles the deck
		// The ref keyword passes a reference to deck.cards, which allows
		// deck.cards to be modified by Deck.Shuffle()

		layout = GetComponent<Layout>();  	// Get the Layout  
        layout.ReadLayout(layoutXML.text); 	// Pass LayoutXML to it

		drawPile = ConvertListCardsToListCardProspectors(deck.cards);
		LayoutGame();

		//Get Bezier curve positions
		fsPosMid = fsPosMidObject.position;
		fsPosRun = fsPosRunObject.position;
		fsPosMid2 = fsPosMid2Object.position;
		fsPosEnd = fsPosEndObject.position;
	}

	List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD) 
    { 
        List<CardProspector> lCP = new List<CardProspector>(); 
        CardProspector tCP; 
        foreach(Card tCD in lCD) 
        { 
            tCP = tCD as CardProspector;
            lCP.Add(tCP); 
        } 
        return(lCP); 
    }

	CardProspector Draw() 
    { 
        CardProspector cp = drawPile[0]; 	// Pull the 0th CardProspector 
        drawPile.RemoveAt(0);            	// Then remove it from List<> drawPile 
        return(cp);           
	}

    // Convert from the layoutID int to the CardProspector with that ID 
    CardProspector FindCardByLayoutID(int layoutID) 
    { 
        foreach (CardProspector tCP in tableau) 
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
        // Create an empty GameObject to serve as an anchor for the tableau // a 
        if (layoutAnchor == null) 
        { 
            GameObject tGO = new GameObject("_LayoutAnchor"); 
            // ^ Create an empty GameObject named _LayoutAnchor in the Hierarchy 
            layoutAnchor = tGO.transform;                       // Grab its Transform
			layoutAnchor.transform.position = layoutCenter;     // Position it 
        }

		CardProspector cp; 
      	// Follow the layout 
      	foreach (SlotDef tSD in layout.slotDefs) 
          { 
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
    		cp.state = eCardState.tableau; 

            // CardProspectors in the tableau have the state CardState.tableau 
			cp.SetSortingLayerName(tSD.layerName);
    		tableau.Add(cp); // Add this CardProspector to the List<> tableau     
	  	}

		// set which cards are hiding others
		foreach (CardProspector tCP in tableau) 
        { 
            foreach( int hid in tCP.slotDef.hiddenBy ) 
            { 
                cp = FindCardByLayoutID(hid); 
                tCP.hiddenBy.Add(cp); 
            } 
     	}

		// set up the initial target card
		MoveToTarget(Draw()); 
     	// Set up the Draw pile 
      	UpdateDrawPile();
	}
	 
    // This turns cards in the Mine face-up or face-down 
    void SetTableauFaces() 
    { 
        foreach( CardProspector cp in tableau ) 
        { 
            bool faceUp = true;         // Assume the card will be face-up 
            foreach( CardProspector cover in cp.hiddenBy ) 
            { 
                // If either of the covering cards are in the tableau 
                if (cover.state == eCardState.tableau) 
                { 
                    faceUp = false;     // then this card is face-down 
                } 
            } 
            cp.faceUp = faceUp;         // Set the value on the card 
        } 
    }

	void MoveToDiscard(CardProspector cp) 
    { 
        // Set the state of the card to discard 
        cp.state = eCardState.discard; 
        discardPile.Add(cp);                    // Add it to the discardPile List<> 
        cp.transform.parent = layoutAnchor;     // Update its transform parent 
        
        // Position this card on the discardPile 
        cp.transform.localPosition = new Vector3( 
            layout.multiplier.x * layout.discardPile.x, 
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID+0.5f ); 
        cp.faceUp = true; 

        // Place it on top of the pile for depth sorting 
        cp.SetSortingLayerName(layout.discardPile.layerName); 
        cp.SetSortOrder(-100 + discardPile.Count); 
    } 

    // Make cp the new target card 
    void MoveToTarget(CardProspector cp) 
    { 
        // If there is currently a target card, move it to discardPile 
        if (target != null) MoveToDiscard(target); 
        target = cp;                            // cp is the new target 
        cp.state = eCardState.target; 
        cp.transform.parent = layoutAnchor; 
        
        // Move to the target position 
        cp.transform.localPosition = new Vector3( 
            layout.multiplier.x * layout.discardPile.x, 
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID ); 
 		cp.faceUp = true; // Make it face-up 
        
        // Set the depth sorting 
        cp.SetSortingLayerName(layout.discardPile.layerName); 
        cp.SetSortOrder(0); 
    }

	void UpdateDrawPile() 
    { 
        CardProspector cp; 
        
        // Go through all the cards of the drawPile 
        for (int i=0; i<drawPile.Count; i++) 
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
            cp.state = eCardState.drawpile; 
            
            // Set depth sorting 
            cp.SetSortingLayerName(layout.drawPile.layerName); 
            cp.SetSortOrder(-10 * i); 
          } 
    }

	public void CardClicked(CardProspector cp) 
    { 
        // The reaction is determined by the state of the clicked card 
      	switch (cp.state) 
        { 
        	case eCardState.target: 
          		// Clicking the target card does nothing 
          		break; 

 			case eCardState.drawpile: 
            	// Clicking any card in the drawPile will draw the next card 
            	MoveToDiscard(target);	// Moves the target to the discardPile 
            	MoveToTarget(Draw());   // Moves the next drawn card to the target 
            	UpdateDrawPile();     	// Restacks the drawPile
				ScoreManager(ScoreEvent.draw);
            	break; 

 			case eCardState.tableau: 
            	// Clicking a card in the tableau will check if it's a valid play 
				bool validMatch = true; 
             	if (!cp.faceUp) 
                { 
                // If the card is face-down, it's not valid 
                 	validMatch = false; 
             	} 
             	if (!AdjacentRank(cp, target)) 
                { 
                // If it's not an adjacent rank, it's not valid 
                 	validMatch = false; 
             	} 
             	if (!validMatch) return;    // return if not valid 
        		
                // If we got here, then: Yay! It's a valid card. 
             	tableau.Remove(cp);         // Remove it from the tableau List 
             	MoveToTarget(cp); 
				SetTableauFaces();
				ScoreManager(ScoreEvent.mine);
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
        // If there are still cards in the draw pile, the game's not over
	    if (drawPile.Count > 0) return; 
     
        // Check for remaining valid plays 
        foreach (CardProspector cp in tableau) 
        { 
            // If there is a valid play, the game's not over 
            if (AdjacentRank(cp, target)) return; 
        } 
        // Since there are no valid plays, the game is over 
        // Call GameOver with a loss 
        GameOver (false); 
    } 

    // Called when the game is over. Simple for now, but expandable 
    void GameOver(bool won) 
    { 
        if (won) 
        { 
            // print ("Game Over. You won! :)");
			ScoreManager(ScoreEvent.gameWin);
        } 
        else 
        { 
            // print ("Game Over. You Lost. :("); 
			ScoreManager(ScoreEvent.gameLoss);
        } 
        // Reload the scene, resetting the game 
        Invoke("ReloadLevel", reloadDelay);
    }
	
	void ReloadLevel()
	{
		//Reload trhe scene, resetting the game
		SceneManager.LoadScene("_Prospector_Scene_1");
	}

	public bool AdjacentRank(CardProspector c0, CardProspector c1) 
    { 
        // If either card is face-down, it's not adjacent. 
        if (!c0.faceUp || !c1.faceUp) return(false); 

        // If they are 1 apart, they are adjacent 
        if (Mathf.Abs(c0.rank - c1.rank) == 1) return(true); 

        // If one is Ace and the other King, they are adjacent 
        if (c0.rank == 1 && c1.rank == 13) return(true); 
        
        if (c0.rank == 13 && c1.rank == 1) return(true); 
        
        // Otherwise, return false 
        return(false); 
    }

    // ScoreManager handles all the scoring
	void ScoreManager(ScoreEvent sEvt)
	{

		List<Vector3> fsPts;
		switch (sEvt)
		{
		// Same things need to happen whether it's a draw, a win, or a loss
		case ScoreEvent.draw: 		// Drawing a card
		case ScoreEvent.gameWin: 	// Won the round
		case ScoreEvent.gameLoss: 	// Lost the round
			chain = 0; 				// resets the score chain
			score += scoreRun; 		// Add scoreRun to the total score
			scoreRun = 0; 			// reset scoreRun

			// Add fsRun to the _Scoreboard score
			if (fsRun != null)
			{
				// Create points for the Bezier curve
				fsPts = new List<Vector3>();
				fsPts.Add(fsPosRun);
				fsPts.Add(fsPosMid2);
				fsPts.Add(fsPosEnd);
				fsRun.reportFinishTo = Scoreboard.S.gameObject;
				fsRun.Init(fsPts, 0, 1);

				// Also adjust the fontSize
				fsRun.fontSizes = new List<float>(new float[] {28, 36, 4});
				fsRun = null; 		// Clear fsRun so it's created again
			}

			break;

		case ScoreEvent.mine: 		// Remove a mine card
			chain++; 				// Increase the score chain
			scoreRun += chain; 		// add score for this card to run

			// Create a FloatingScore for this score
			FloatingScore fs;

			// Move it from the mousePosition to fsPosRun
			Vector3 p0 = Input.mousePosition;
			// p0.x /= Screen.width;
			// p0.y /= Screen.height;
			fsPts = new List<Vector3>();
			fsPts.Add(p0);
			fsPts.Add(fsPosMid);
			fsPts.Add(fsPosRun);
			fs = Scoreboard.S.CreateFloatingScore(chain, fsPts);
			fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
			if (fsRun == null)
			{
				fsRun = fs;
				fsRun.reportFinishTo = null;
			}
			else
			{
				fs.reportFinishTo = fsRun.gameObject;
			}

			break;
		}

		// This second switch statement handles round wins and losses
		switch (sEvt)
		{
		case ScoreEvent.gameWin:
			GTGameOver.text = "Round Over";
			// If it's a win, add the score to the next round. static fields are NOT reset by reloading the level
			Prospector.SCORE_FROM_PREVIOUS_ROUND = score;
			// print("You won this round! Round score: " + score);
			GTRoundResult.text = "You won this round! Play another to add to your score!\nRound Score: " + score;
			ShowResultsGTs(true);
			break;
		case ScoreEvent.gameLoss:
			GTGameOver.text = "Game Over";
			// If it's a loss, check against the high score
			if (Prospector.HIGH_SCORE <= score)
			{
				// print("You got the high score! High score: " + score);
				string sRR = "You got the high score!\nHigh score: " + score;
				GTRoundResult.text = sRR;
				Prospector.HIGH_SCORE = score;
				PlayerPrefs.SetInt("ProspectorHighScore", score);
			}
			else
			{
				//print("Your final score for the game was:" + score);
				GTRoundResult.text = "Your final score was: " + score;
			}
			ShowResultsGTs(true);
			break;
		default: 
			// print("score: " + score + " scoreRun: " + scoreRun + " chain: " + chain);
			break;
		}
	}
}
