using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class Card : ScriptableObject {

	// card properties
	public int cardId;							// id of the card
	public string cardName;						// name of the card
	public CardType cardType;					// the type of the card (minion, magic, artifact, energy)

	public Collor cardCollor;					// collor of the card 
	public DuelManager.Player.ManaPool manaCost; // the total mana this card needs to be summoned or if CardType = mana, the amount of mana this card offers
	public List<CardEffect> effects;		 	// effects activated by triggers during the duel 

	public string description;
	public int damage;							// damage
	public int health;							// health

	public Sprite cardSprite;					// sprite of the card 

}

// enums 
public enum CardType {Mana, Minion, Magic, Artifact};		// CARD TYPE
public enum Collor {Red, Green, Blue, White, Black};		// CARD COLLOR
public enum Status {NonPlayed, Moving, Field, Dead};				// how / where the card is 
public enum ActStatus {Ready, Cooldown};				// Whether or not the minion can attack or the artifact can activate a manual effect
