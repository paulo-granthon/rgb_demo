using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DuelManager : MonoBehaviour {

	public Player Player0;
	public Player testEnemy;

// Singleton
	#region Singleton
	public static DuelManager instance;
	void Awake() {
		if (instance != null) {
			Debug.LogError("there's more than one DuelManager's instance found");
		}
		instance = this;
	}
	#endregion
//
// duel rules
	public class GameRules{
		public static int startingHP;	// how much hp players will start with
	}

	// back sprite of the cards 
	public Sprite cardBack;

	public ManaIcons manaIcons;

	[System.Serializable]
	public class ManaIcons {
		public Sprite manaCollorR;
		public Sprite manaCollorG;
		public Sprite manaCollorB;
		public Sprite manaCollorW;
	}

	// 
	public List<Player> players;		// players in the current duel
	public bool isDragging = false;		// for a global check of player's drag action
	public bool isTargeting = false;	// for a global check of player's target action
	public Card currentHoldingCard;		// what card is being drag

	// current turn
	public int currentTurn;

	// the current turn's player
	public Player currentTurnPlayer;
	public int numberOfPlayers;
	int _ctpID;
	public int ctpID {
		get {
			return _ctpID;
		} set {
			if (value >= players.Count) {
				 _ctpID = 0; 
			} else {
				 _ctpID = value; 
			}
		}
	} 

// StartDuel

	void Start() {
		List<Player> testDuelEnemy = new List<Player>();
		testDuelEnemy.Clear();
		testDuelEnemy.Add(testEnemy);
		StartDuel(testDuelEnemy, 2) ;
	}

	// DUEL BEGINS HERE, THIS METHOD WILL BE CALLED, WITH THE NECESSARY PROPERTIES SO THE DUEL CAN INICIATE, ALSO RESETING THE TABLE OBJECT, ACTIVATING IT 
	// AND DEACTIVATING THE GAME WORLD PLAYER. 
	public void StartDuel (List<Player> enemyPlayers, int whoHow) {		// who: case 0 = Player Starts; case 1 = Enemy Starts; case 2 = How: Heads or Tails;
		
		// add the players to the duel
		//players.Clear();																					// clear the list before the duel
		players[0] = Player0;																			// add THE PLAYER to the list in the 0 slot

		// add the other players to the list after THE PLAYER
		for (int i = 0; i < enemyPlayers.Count; i++) {
			players.Add(enemyPlayers[i]);
		}		
				
		// decide who starts the duel
		switch (whoHow) {
			//whoHow = 0
			case 0:
			// player 0 starts the duel (Player)
			currentTurnPlayer = players[0];
			break;

			// whoHow = 1
			case 1:
			// player 1 starts the duel (Enemy)
			currentTurnPlayer = players[1];
			break;

			case 2:
			//whoHow = 2		// coin flip / random
			currentTurnPlayer = players[FlipTheCoin(enemyPlayers.Count)];
			break;

			default:
			whoHow = 0;
			// player 0 starts the duel (Player)
			currentTurnPlayer = players[0];
			break;
		}

		// assembly table areas to their respectives owners
		for (int i = 0; i < players.Count; i++) {

			// reset hp
			players[i].ResetHP();

			// reset draw points 
			players[i].drawPoints = 0;

			// starting player begins with 1 less draw point
			players[i].drawPoints += 5;
			if (players[i].name != currentTurnPlayer.name) {
				players[i].drawPoints += 1;
			}

			// Player attach
			// TODO: add the player to it's GameObject in the field in addition to the game areas 

			// assembly table areas to their respectives owners
			players[i].field.minionsParent = GameObject.Find("Player " + i).transform.Find("MinionSlot").gameObject;
			players[i].field.minionsParent.GetComponent<DropZone>().owner = players[i];
			players[i].field.artifactsParent = GameObject.Find("Player " + i).transform.Find("ArtifactSlot").gameObject;
			players[i].field.artifactsParent.GetComponent<DropZone>().owner = players[i];
			players[i].graveyard.graveyard = GameObject.Find("Player " + i).transform.Find("Graveyard").gameObject;
			players[i].graveyard.graveyard.GetComponent<DropZone>().owner = players[i];
			players[i].hand.hand = GameObject.Find("Player " + i).transform.Find("Hand").gameObject; 
			players[i].hand.hand.GetComponent<DropZone>().owner = players[i];
			players[i].deck.deckSlot = GameObject.Find("Player " + i).transform.Find("DeckSlot").gameObject;
			players[i].deck.deckSlot.GetComponent<DropZone>().owner = players[i];

			// clear lists
			players[i].hand.cards.Clear();
			players[i].field.minions.Clear();
			players[i].field.artifacts.Clear();
			players[i].graveyard.cards.Clear();

			// clear AI trash
			if (players[i].intelligence != null) {
				players[i].intelligence.playable.Clear();
				players[i].intelligence.availableMana.Clear();
				players[i].intelligence.totalMana = new int[4];
			}

			// reset players mana pool
			players[i].manaPool.Clear();

			// remove empty list items from the deck
			for (int d = 0; d < players[i].deck.deck.Count; d++) {
				if (players[i].deck.deck[d] == null) {
					players[i].deck.deck.RemoveAt(d);
				}	
			}

			// shuffle the deck
			players[i].deck.Shuffle();
			players[i].deck.duelDeck.Clear();

			// populate the Decks
			for (int j = 0; j < players[i].deck.deck.Count; j++) {
				GameObject newCard = NewCardGM(players[i], players[i].deck.deckSlot.transform, players[i].deck.deck[j]);
				players[i].deck.duelDeck.Add(newCard.GetComponent<CardHolder>());
			}

		}

		// start the duel
		Debug.Log(currentTurnPlayer + " opens the duel");
		currentTurn = 0;
		NextTurn();

	}

// Turn Stuff

	public void NextTurn() {
		if (currentTurn > 0) {	ctpID++;	}
		currentTurn++;
		currentTurnPlayer = players[ctpID];
		currentTurnPlayer.drawPoints++;
		//Debug.Log(currentTurn + " " + currentTurnPlayer + " " + ctpID + " " + players.Count);

		// re-energise minions
		for (int i = 0; i < currentTurnPlayer.field.minions.Count; i++) {
			currentTurnPlayer.field.minions[i].curActTokens = currentTurnPlayer.field.minions[i].ActionTokens;
			currentTurnPlayer.field.minions[i].actStatus = ActStatus.Ready;
		}

		// re-energise artifacts
		for (int i = 0; i < currentTurnPlayer.field.artifacts.Count; i++) {
			currentTurnPlayer.field.artifacts[i].curActTokens = currentTurnPlayer.field.artifacts[i].ActionTokens;
			currentTurnPlayer.field.artifacts[i].actStatus = ActStatus.Ready;
		}

		// Take Turn if IA
		if (currentTurnPlayer.intelligence != null) { currentTurnPlayer.intelligence.TakeTurn(); }
	}

// FlipTheCoin (int maxValue)		// just a method that returns a random result among the options  
	public int FlipTheCoin(int maxValue) {
		if (maxValue == 0) {	maxValue = 1;	}
		int result = Random.Range(0, maxValue + 1);
		return result;
	}

// END DUEL // WIN DUEL //			will be called by Player.TakeDamage() when HP reaches 0, seting the damage source owner as Winner
	public void WinDuel(Player Winner) {
		Debug.Log(Winner.name + " Wins the duel");
		// end duel
		// delete all temp objects
		// clear duelDecks
		// give the winner a prize
		// SetActive(false) on TABLE object
		// return to Game World
	}

// =============================================================================================================================================

// Card Effects applier

	/* 
	public class EffectsQueue {
		// list of pending effects
		public List<Activation> activationQueue;
		public List<Activation> plannedEffects;

		// add a effect
		public void AddActivation(Activation activation)
		{
			activationQueue.Add(activation);
		}

		// add a future effect
		public void AddPlanned(Activation activation) 
		{
			// add to the planned effects.
			// it needs to hold information about when it will activate
			plannedEffects.Add(activation);

		}

		// apply effect
		public void Activate()
		{
			// reverse the effects list
			activationQueue.Reverse();
			// for each effect in the list, apply then 
			for (int i = 0; i < activationQueue.Count; i++)
			{
				ApplyEffect(activationQueue[i].tEffects);
			}
		}

		// apply effects
		public void ApplyEffect(List<CardEffect> effectsQueue)
		{
			// for each effect in the list, apply then 
			for (int i = 0; i < effectsQueue.Count; i++)
			{
				effectsQueue[i].EffectActivate();
			}
		}
	}
												*/


// PLAY CARD / SUMMON RELATED STUFF ================================================================================================================================================

	// check if the drop is possible in the current zone based on the card that is being playied, the zone it's being dropped and the turnphase / turn owner
	public bool CheckPlayable(CardHolder card, DropZone.DropZoneType from, DropZone.DropZoneType to) {
		if (currentTurnPlayer.name == card.owner.name) {
			if (from == DropZone.DropZoneType.Hand){
				if (card.card.cardType == CardType.Minion && to == DropZone.DropZoneType.MinionField) {
					if (card.CheckCost()){
						if (card.curStatus == Status.NonPlayed) {
							//Debug.Log("Playable 1, true - MINION from hand to field");
							return true;
						}
					}
				}
				if (card.card.cardType == CardType.Artifact && to == DropZone.DropZoneType.ArtifactField) {
					if (card.CheckCost()){
						if (card.curStatus == Status.NonPlayed) {
							//Debug.Log("Playable 2, true - ARTIFACT from hand to field");
							return true;
						}
					}
				}
				if (card.card.cardType == CardType.Magic) {
					if (to == DropZone.DropZoneType.MinionField || to == DropZone.DropZoneType.ArtifactField){
						if (card.CheckCost()){
							if (card.curStatus == Status.NonPlayed) {
								//Debug.Log("Playable 3, true - MAGIC from hand to 'any' field");
								return true;
							}
						}
					}
				}
				if (card.card.cardType == CardType.Mana) {
					if (to == DropZone.DropZoneType.MinionField || to == DropZone.DropZoneType.ArtifactField || to == DropZone.DropZoneType.GraveyardField) {
						if (card.curStatus == Status.NonPlayed) {
							//Debug.Log("Playable 4, true - MANA from hand to 'any' field");
							return true;
						}
					}
				}			
			} else if (from == DropZone.DropZoneType.Deck && card.owner.drawPoints > 0) {
				if (to == DropZone.DropZoneType.GraveyardField) {
					if (card.card.cardType == CardType.Mana){
						if (card.curStatus == Status.NonPlayed) {
							//Debug.Log("Playable 5, true - MANA from deck to grave");
							currentTurnPlayer.drawPoints--;
							return true;
						}
					}
				}
			}
		}
		//Debug.Log("Playable 5, false");
		return false;
	}

	public bool CheckDroppable(CardHolder card, DropZone.DropZoneType from, DropZone.DropZoneType to){
		if (currentTurnPlayer.name == card.owner.name) {
			if (from == DropZone.DropZoneType.Deck && currentTurnPlayer.drawPoints > 0){
				if (to == DropZone.DropZoneType.Hand){
					//Debug.Log("Droppable 1, true - DRAW from deck to hand");
					currentTurnPlayer.drawPoints--;
					return true;
				}
			}
			if (card.curStatus == Status.Dead && to == DropZone.DropZoneType.GraveyardField) {
				//Debug.Log("Droppable 2, true - DEAD already from field to grave");
				return true;
			}
		}
		//Debug.Log("Droppable 3, false");
		return false;
	}

// Change Zones 
	public void AreaChange(CardHolder card, DropZone from, DropZone to, bool mode) {
		for (int i = 0; i < players.Count; i++) {
			if (players[i].name == card.owner.name) {

				// remove from the previous zone
				switch (from.zoneType) {
					// from deck
					case DropZone.DropZoneType.Deck:
					players[i].deck.duelDeck.Remove(card);
					break;
					// from hand
					case DropZone.DropZoneType.Hand:
					players[i].hand.cards.Remove(card);
					break;
					// from the field (minions)
					case DropZone.DropZoneType.MinionField:
					players[i].field.minions.Remove(card);
					break;
					// from the field (artifacts)
					case DropZone.DropZoneType.ArtifactField:
					players[i].field.artifacts.Remove(card);
					break;
					// from the graveyard
					case DropZone.DropZoneType.GraveyardField:
					players[i].graveyard.cards.Remove(card);
					break;
				}

				// // play moving animation
				// if (mode) {
				// 	GetComponent<Duel_Interface>().MoveCard(card, from, to);
				// }

				// add to the current zone
				switch (to.zoneType) {
					// to deck
					case DropZone.DropZoneType.Deck:
					players[i].deck.duelDeck.Add(card);
					card.transform.SetParent(card.owner.deck.deckSlot.transform);
					break;
					//	to hand
					case DropZone.DropZoneType.Hand:
					players[i].hand.cards.Add(card);
					card.transform.SetParent(card.owner.hand.hand.transform);
					break;
					//	to the field (minions)
					case DropZone.DropZoneType.MinionField:
					players[i].field.minions.Add(card);
					card.transform.SetParent(card.owner.field.minionsParent.transform);
					break;
					//	to the field (artifacts)
					case DropZone.DropZoneType.ArtifactField:
					players[i].field.artifacts.Add(card);
					card.transform.SetParent(card.owner.field.artifactsParent.transform);
					break;
					//	to the graveyard
					case DropZone.DropZoneType.GraveyardField:
					players[i].graveyard.cards.Add(card);
					card.transform.SetParent(card.owner.graveyard.graveyard.transform);
					break;
				}

				if (card.cardUI.activeSelf == false && card.owner == Player0 
						&& to.zoneType != DropZone.DropZoneType.Deck
						&& to.zoneType != DropZone.DropZoneType.Hand) {
					if (card.card.cardSprite != cardBack) {
						card.cardUI.SetActive(true);
					}
				}

				Debug.Log(card.owner.name + "|| " + card.name + " changed from " + from.zoneType + " to " + to.zoneType);
			}
		}
	}

// Player
	[CreateAssetMenu(fileName = "New Player", menuName = "Player")]
	public class Player : ScriptableObject {
		public new string name;
		public Duel_AI intelligence;				// artificial intelligence that this player uses 
		public int hp = GameRules.startingHP;		// player's max hp (default = game rules starting hp)	
		public int curHP;									// player's current hp in the duel

		public ManaPool manaPool;					// the current amount of Mana the player currently have
		public Deck deck;							// player's Deck
		public Field field;							// player's field
		public Hand hand;						  // player's hand
		public Graveyard graveyard;				// cards on this player's graveyard 
		public int drawPoints = 0;			// 1 point = 1 draw	

		// card interface collors
		public CardTextCollors cardTextCollors;

		// draw the first card of the deck
		public void DrawCard(bool mode){
			CardHolder card = deck.duelDeck[0];
			hand.Add(card);
			DuelManager.instance.AreaChange(card, card.owner.deck.deckSlot.GetComponent<DropZone>(), 
				card.owner.hand.hand.GetComponent<DropZone>(), mode);

			//deck.duelDeck.Remove(card);
			card.transform.SetParent(hand.hand.transform);
			drawPoints--;
		}

		// Mana Pool
		[System.Serializable]
		public class ManaPool {
			int max = 99;
			public int _r; 
			public int _g;
			public int _b;
			public int _w;

			public int r { get { return _r; } set { if (value <= 0) { _r = 0; } else if (value >= max) { _r = max; } else { _r = value; } } }
			public int g { get { return _g; } set { if (value <= 0) { _g = 0; } else if (value >= max) { _g = max; } else { _g = value; } } }
			public int b { get { return _b; } set { if (value <= 0) { _b = 0; } else if (value >= max) { _b = max; } else { _b = value; } } }
			public int w { get { return _w; } set { if (value <= 0) { _w = 0; } else if (value >= max) { _w = max; } else { _w = value; } } }

			public void Clear(){
				r = 0;
				g = 0;
				b = 0;
				w = 0;
			}
			public void Add(ManaPool value){
				r += value.r;
				g += value.g;
				b += value.b;
				w += value.w;
			}
			public void Subtract(ManaPool value){
				r -= value.r;
				g -= value.g;
				b -= value.b;
				w -= value.w;
			}
			public bool Compare(ManaPool cost){
				bool rr = false; bool gg = false; bool bb = false; bool ww = false;
				if (r >= cost.r) { rr = true; }
				if (g >= cost.g) { gg = true; }
				if (b >= cost.b) { bb = true; }
				if (w >= cost.w) { ww = true; }
				if (rr && gg && bb && ww) {
					return true;
				} else {
					return false;
				}
			}
		}

		// Deck
		[System.Serializable]
		public class Deck {
			public List<Card> deck;
			public List<CardHolder> duelDeck;
			public GameObject deckSlot;

			// shuffle the cards in the deck
			public void Shuffle(){
				List<Card> previousDeck = deck;
				List<Card> newDeck = new List<Card>();
				int shuffleQueue = previousDeck.Count;
				for (int i = 0; i < shuffleQueue; i++) {					
					Card card = previousDeck[Random.Range(0,previousDeck.Count)];
					newDeck.Add(card);
					previousDeck.Remove(card);
				}
				deck = newDeck;
			}
			// TODO: a search method to look throught the deck and find one or more cards and then make something with them 
			// or maybe a List<Card> Find(rule) that return Cards that are possible to pick by the passing rule   
		}

		// player's hand
		[System.Serializable]
		public class Hand {
			// the hand 
			public GameObject hand;
			public List<CardHolder> cards;

			// refresh
			public void Refresh() {
				cards.Clear();
				for (int i = 0; i < hand.transform.childCount; i++) {
					if (hand.transform.GetChild(i).GetComponent<CardHolder>() != null) {
						cards.Add(hand.transform.GetChild(i).GetComponent<CardHolder>());
					}
				}
			}

			// player's card count on the hand
			public int handCount() {
				return hand.transform.childCount;
			}

			// add a card to the hand
			public void Add(CardHolder card) {
				cards.Add(card);
			}

			// find all cards on the player's hand, this will return a list of Card in the hand slots of the player's field
			public List<CardHolder> all() {
				List<CardHolder> handList = new List<CardHolder>();
				for (int i = 0; i < handCount(); i++){
					handList.Add(hand.transform.GetChild(i).GetComponent<CardHolder>());
				}

				return handList;
			}

			// target a minion, this will return a Card 
			public CardHolder targetCard(int target) {
				return hand.transform.GetChild(target).GetComponent<CardHolder>();
			}

		}

		// Field
		[System.Serializable]
		public class Field {
			// minions on the player's field
			public GameObject minionsParent;
			public List<CardHolder> minions;

			// artifacts on the player field
			public GameObject artifactsParent;
			public List<CardHolder> artifacts;

			public void Refresh() {
				minions.Clear();
				artifacts.Clear();
				for (int i = 0; i < minionsParent.transform.childCount; i++) {
					if (minionsParent.transform.GetChild(i).GetComponent<CardHolder>() != null) {
						minions.Add(minionsParent.transform.GetChild(i).GetComponent<CardHolder>());
					}
				}
				for (int i = 0; i < artifactsParent.transform.childCount; i++) {
					if (artifactsParent.transform.GetChild(i).GetComponent<CardHolder>() != null) {
						artifacts.Add(artifactsParent.transform.GetChild(i).GetComponent<CardHolder>());
					}
				}

			}

			public void AddMinion(CardHolder card) {
				minions.Add(card);
			}

			public void AddArtifact(CardHolder card) {
				artifacts.Add(card);
			}

			// player's minion count, this will return a int based on the child count of the minion slot
			public int minionCount() {
				return minionsParent.transform.childCount;
			}

			// player's artifact count, this will return a int based on the child count of the artifact slot
			public int artifactCount() {
				return artifactsParent.transform.childCount;
			}

			// find all minions, this will return a list of Card in the minion slots of the player's field
			public List<CardHolder> allMinion() {
				List<CardHolder> minionList = new List<CardHolder>();
				for (int i = 0; i < minionCount(); i++) {
					minionList.Add(minionsParent.transform.GetChild(i).GetComponent<CardHolder>());
				}
				return minionList;
			}

			// find all artifacts, this will return a list of Card in the artifact slots of the player's field
			public List<CardHolder> allArtifact() {
				List<CardHolder> artifactList = new List<CardHolder>();
				for (int i = 0; i < artifactCount(); i++){
					artifactList.Add(artifactsParent.transform.GetChild(i).GetComponent<CardHolder>());
				}
				return artifactList;
			}

			// target a minion, this will return a Card 
			public CardHolder targetMinion(int target) {
				return minionsParent.transform.GetChild(target).GetComponent<CardHolder>();
			}

			// target a artifact, this will return a Card
			public CardHolder targetArtifact(int target) {
				return artifactsParent.transform.GetChild(target).GetComponent<CardHolder>();
			}


		}

		// player's graveyard
		[System.Serializable]
		public class Graveyard {
			// graveyard gameobject
			public GameObject graveyard;
			public List<CardHolder> cards;

			public void Add(CardHolder card) {
				cards.Add(card);
			}

			// Refresh
			public void Refresh() {
				cards.Clear();
				for (int i = 0; i < graveyard.transform.childCount; i++) {
					if (graveyard.transform.GetChild(i).GetComponent<CardHolder>() != null) {
						cards.Add(graveyard.transform.GetChild(i).GetComponent<CardHolder>());
					}
				}
			}
			
			// player's card count on the graveyard
			public int Count() {
				return graveyard.transform.childCount;
			}

			// find all cards on the player's graveyard, this will return a list of Card in the graveyard of the player's field
			public List<CardHolder> all() {
				List<CardHolder> graveList = new List<CardHolder>();
				for (int i = 0; i < Count(); i++){
					graveList.Add(graveyard.transform.GetChild(i).GetComponent<CardHolder>());
				}
				return graveList;
			}

			// target a graveyard card, this will return a Card 
			public CardHolder targetCard(int target) {
				return graveyard.transform.GetChild(target).GetComponent<CardHolder>();
			}	
		}

		// Taking damage 
		public void TakeDamage(int amount, CardHolder source) {

		// check for effects
			// minions
			for (int i = 0; i < field.minions.Count; i++) {
				CardEffectDatabase.CheckforTrigger(field.minions[i], CardEffect.Trigger.BeforeDirectDef);
			}
			// artifacts
			for (int i = 0; i < field.artifacts.Count; i++) {
				CardEffectDatabase.CheckforTrigger(field.artifacts[i], CardEffect.Trigger.BeforeDirectDef);
			}


			curHP -= amount;					//

			// update visuals
			// CALLBACK() DUEL HUD // PLAYERS HP UPDATE (TO DO)
			//
		
			if (curHP <= 0) {					//
				DuelManager.instance.WinDuel(source.owner);
			}

		// check for effects
			// minions
			for (int i = 0; i < field.minions.Count; i++) {
				CardEffectDatabase.CheckforTrigger(field.minions[i], CardEffect.Trigger.AfterDirectDef);
			}
			// artifacts
			for (int i = 0; i < field.artifacts.Count; i++) {
				CardEffectDatabase.CheckforTrigger(field.artifacts[i], CardEffect.Trigger.AfterDirectDef);
			}


		}

		// reset currrent HP
		public void ResetHP() {
			curHP = hp;
		}

		// collors that show up in the cards' texts
		[System.Serializable]
		public class CardTextCollors {
			public Color upCollor;
			public Color defCollor;
			public Color downCollor;
		}

	}

// Create a new card
	public GameObject NewCardGM(Player owner, Transform where, Card card) {

		// create a new game object
		GameObject cardGM = new GameObject();
		// set the parent
		cardGM.transform.SetParent(where);

		// add the components and update the variables
		CardHolder c = cardGM.AddComponent<CardHolder>();
		c.card = card;
		cardGM.name = c.card.cardName;
		c.owner = owner;

		cardGM.AddComponent<CanvasRenderer>();

		// add text interface
		if (c.card.cardType == CardType.Mana) {
			GameObject card_UI = Instantiate(Resources.Load("CardUI_Mana")) as GameObject;
			card_UI.transform.SetParent(cardGM.transform);
			card_UI.GetComponent<Card_Interface>().card = c;
			c.cardUI = card_UI;
		} else if (c.card.cardType == CardType.Minion) {
			GameObject card_UI = Instantiate(Resources.Load("CardUI_Minion")) as GameObject;
			card_UI.transform.SetParent(cardGM.transform);
			card_UI.GetComponent<Card_Interface>().card = c;
			c.cardUI = card_UI;
		} else if (c.card.cardType == CardType.Artifact) {
			GameObject card_UI = Instantiate(Resources.Load("CardUI_Artifact")) as GameObject;
			card_UI.transform.SetParent(cardGM.transform);
			card_UI.GetComponent<Card_Interface>().card = c;
			c.cardUI = card_UI;
	//	} else if (c.card.cardType == CardType.Magic) {
	//		GameObject card_UI = Instantiate(Resources.Load("CardUI_Magic")) as GameObject;
	//		card_UI.transform.SetParent(cardGM.transform);
	//		card_UI.GetComponent<Card_Interface>().card = c;
	//		c.cardUI = card_UI;
		}

		// add sprite
		Image sprite = cardGM.AddComponent<Image>();
		// preserve card Identity
		if (where.GetComponent<DropZone>().zoneType == DropZone.DropZoneType.Deck) {
			sprite.sprite = DuelManager.instance.cardBack;
			cardGM.transform.GetChild(0).gameObject.SetActive(false);
		} else {
			sprite.sprite = c.card.cardSprite;
			cardGM.transform.GetChild(0).gameObject.SetActive(true);
		}
		
		sprite.type = Image.Type.Filled;
		sprite.preserveAspect = true;

		LayoutElement le = cardGM.AddComponent<LayoutElement>();
		cardGM.AddComponent<CanvasGroup>();
		Draggable d = cardGM.AddComponent<Draggable>();
		d.returnParent = where;

		le.minWidth = 100;
		le.minHeight = 133.3333f;
		le.preferredWidth = 100;
		le.preferredHeight = 133.3333f;

		cardGM.transform.localScale = new Vector3(1f, 1f, 1f);

		return cardGM;
	}

}

