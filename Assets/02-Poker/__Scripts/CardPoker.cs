using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 

// An enum defines a variable type with a few prenamed values
public enum CardState 
{ 
    drawpile, 
    tableau, 
    target, 
    discard 
} 

public class CardPoker : Card // Make sure CardPoker extends Card  
{  
    [Header("Set Dynamically: CardPoker")] 
    // This is how you use the enum CardState 
    public CardState            state = CardState.drawpile; 
    // The hiddenBy list stores which other cards will keep this one face down 
    public List<CardPoker>      hiddenBy = new List<CardPoker>(); 
    // The layoutID matches this card to the tableau XML if it's a tableau card 
    public int                  layoutID; 
    // The SlotDef class stores information pulled in from the LayoutXML <slot> 
    public SlotDef              slotDef; 

    override public void OnMouseUpAsButton() 
    { 
        // Call the CardClicked method on the Prospector singleton 
        Poker.S.CardClicked(this); 
        // Also call the base class (Card.cs) version of this method 
        base.OnMouseUpAsButton();
    }
}
