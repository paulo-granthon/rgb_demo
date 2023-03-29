using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New AI", menuName = "AI")]
public class Duel_AI : ScriptableObject {

	// the player this AI is referencing to 
	DuelManager.Player player;

	// current playable cards on hand 
	public List<CardHolder> playable = new List<CardHolder>();

	// current available mana cards on hand 
	public List<CardHolder> availableMana = new List<CardHolder>();

	// pseudo-ManaPool for counting available mana based on hand's Mana cards
	public int[] totalMana = new int [4];

	// if true, calls Refresh() on the main turn cycle
	public bool needsRefresh = true;

	//

	// minimum act value for minions to attack and artifacts to act
	public int MinActValue = 10;

	//

	int[] minionsReady = new int[0];
	int[] artifactsReady = new int[0];

	// INITIATE TURN CYCLE
	public void TakeTurn() {
		// update the player 
		player = DuelManager.instance.currentTurnPlayer;
		Debug.LogWarning(player.name + " Takes the turn");

		bool playDone = false;
		bool ActAble = false;
		int cycle = new int();

		while (player.drawPoints > 0 && player.deck.duelDeck.Count > 0) {
			player.DrawCard(player.intelligence?true:false);
		}

		// cycle: play cards until theres no cards left to play
		for (;!playDone;) {

			Refresh();

			//Debug.LogWarning("Play Cycle: " + cycle);
			cycle++;
			if (playable.Count > 0) {

				Debug.LogWarning("Play a Card()");
				PlayACard();

			} else {

				if (playable.Count <= 0){
					Debug.Log("play done, checking attack");
					ActAble = true;
					playDone = true;
				}
			}	
		}

		for (;ActAble;) {
			RefreshReadyToAct();
			if (minionsReady.Length + artifactsReady.Length > 0) {
				if (DoFieldActions()) {
				} else {
					ActAble = false;
				}
			} else {
				ActAble = false;
			}
		}
		
		// Cycle: attack with minions until there's no ready minions left 

		DuelManager.instance.NextTurn();

	}

	// update the playable list
	public void Refresh() {

		player.field.Refresh();
		player.hand.Refresh();
		player.graveyard.Refresh();
		playable.Clear();
		availableMana.Clear();
		totalMana = new int [4];

		//Debug.LogWarning("refresh()");

		// update the amount of mana available
		for (int j = 0; j < player.hand.cards.Count; j++) {
			if (player.hand.cards[j].card.cardType == CardType.Mana) {
				totalMana[0] += player.hand.cards[j].card.manaCost.r;
				totalMana[1] += player.hand.cards[j].card.manaCost.g;
				totalMana[2] += player.hand.cards[j].card.manaCost.b;
				totalMana[3] += player.hand.cards[j].card.manaCost.w;
			}
		}
		
		// check if playable 
		for (int i = 0; i < player.hand.cards.Count; i++) {
			// if mana, add to available mana 
			if (player.hand.cards[i].card.cardType == CardType.Mana) {
				availableMana.Add(player.hand.cards[i]);
			} 
			// if not mana, check if playable 
			else {
				if (CheckCost(player.hand.cards[i].card)) {
					playable.Add(player.hand.cards[i]);
				}
			}
		}

		needsRefresh = false;
		
	}


// Play cards actions
	void PlayACard() {
		// create a value array of each playable card
		int[] playValue = new int[playable.Count];
		for (int i = 0; i < playable.Count; i++) {
			playValue[i] = CheckPlayValue(playable[i].card);
			//Debug.Log(playable[i].card.cardName + ": " + playValue[i]);
		}
		// search the most valuable playable card 
		int maxValueID = new int();
		for (int i = 1; i < playValue.Length; i++) {
			if (playValue[i] > playValue[i-1]) {
				//Debug.Log("most valuable: " + playable[i].card.cardName + ": " + playValue[i]);
				maxValueID = i;
			}
		}

		Debug.Log("Playing " + playable[maxValueID].card.cardName);

		// play the mana required for the card
		PlayMana(playable[maxValueID]);

		// play the card 
		playable[maxValueID].PlayCard();
		//Debug.Log(playable[maxValueID].card.cardName + " Playied");
		playable.RemoveAt(maxValueID);

		// refresh() callback
		needsRefresh = true;

	}

	// check if a card is playable in the player's hand 
	bool CheckCost(Card card) {

		int[] cardCost = CardManaCost(card);

		for (int i = 0; i < cardCost.Length; i++) {
			if (totalMana[i] < cardCost[i]) {
				//Debug.Log(DuelManager.instance.currentTurnPlayer.name + " could not play " + card.name);
				return false;
			}
		}

		return true;
	}

	// check the value of the play, the most valuable play = the better card to play in that moment
	int CheckPlayValue(Card card) {
		int value = new int();

		switch (card.cardType) {
			
			// if minion
			case CardType.Minion:
			// if high hp
			if (DuelManager.instance.currentTurnPlayer.hp >= 15) {
				value += 10;
			}
			// if low hp
			else if (DuelManager.instance.currentTurnPlayer.hp <= 10) {
				value += 20;
			}
			// if few minions
			if (DuelManager.instance.currentTurnPlayer.field.minions.Count < 2) {
				value += 10;
			}
			// plus minion power 
			value += card.damage;
			value += card.health;
			// TODO: for each card.effects[i], +2 value if a positive effect and -1 value if its a negative effect

			break;

			// if artifact
			case CardType.Artifact:
			// artifacts generally are worth using
			value += 15;
			// TODO: for each card.effects[i], +5 value if a positive effect and -1 value if its a negative effect (same as minion but more valuable)
			break;

			case CardType.Magic:
			// magics generally are worth using 
			value += 18;
			// TODO: for each card.effects[i], +8 value if a positive effect and -1 value if its a negative effect (same as minion but more valuable)
			break;

			//TODO: also, check each effect, if it targets something in the field or if it buffs a minion, and the effect's condition in general

			default:
			break;
		}

		// less value if costs a lot of mana 
		value -= (card.manaCost.r + card.manaCost.g + card.manaCost.b + card.manaCost.w);

		return value;

	}

	// select Mana to play
	void PlayMana(CardHolder card) {
		// check the most valuable mana to play the card and then play them 

		int[] cardCost = CardManaCost(card.card);
		int totalCost = (cardCost[0] + cardCost[1] + cardCost[2] + cardCost[3]) * 10;
		int[] manaValue = new int[availableMana.Count];
		int totalManaValue = new int();

		//Debug.LogWarning("totalcost: " + totalCost + " totalMana: " + totalMana);

		//while (totalCost > totalMana) {
			for (int i = 0; i < availableMana.Count && totalCost > totalManaValue; i++) {
				if (cardCost[0] > 0) {	// if card needs R mana
					if (availableMana[i].card.manaCost.r > 0) {	// if the mana card provides R mana
						if (availableMana[i].card.manaCost.r > cardCost[0]) {	// if it provides excess
							manaValue[i] += 1;
						} 
						else {	// no excess
							manaValue[i] += 10;
						}
					}
				}
				if (cardCost[1] > 0) {	// if card needs G mana
					if (availableMana[i].card.manaCost.g > 0) {	// if the mana card provides G mana
						if (availableMana[i].card.manaCost.g > cardCost[1]) {	// if it provides excess
							manaValue[i] += 1;
						} 
						else {	// no excess
							manaValue[i] += 10;
						}
					}
				}
				if (cardCost[2] > 0) {	// if card needs B mana
					if (availableMana[i].card.manaCost.b > 0) {	// if the mana card provides B mana
						if (availableMana[i].card.manaCost.b > cardCost[2]) {	// if it provides excess
							manaValue[i] += 1;
						} 
						else {	// no excess
							manaValue[i] += 10;
						}
					}
				}
				if (cardCost[3] > 0) {	// if card needs W mana
					if (availableMana[i].card.manaCost.w > 0) {	// if the mana card provides W mana
						if (availableMana[i].card.manaCost.w > cardCost[3]) {	// if it provides excess
							manaValue[i] += 1;
						} 
						else {	// no excess
							manaValue[i] += 10;
						}
					}
				}

				//Debug.Log("manaValue: " + manaValue[i]);
				totalManaValue += manaValue[i];
				//Debug.Log("totalCost: " + totalCost + " totalMana: " + totalMana);

			}
		//}

		ManaPlayCycle(totalCost / 10, manaValue);
	}

	// play the mana selected
	void ManaPlayCycle(int totalCost, int[] manaValue) {

		//Debug.LogWarning	("totalCost: " + totalCost + " totalMana: " + totalMana);

		for (int c = 0; c < totalCost; c++) {
			
			//Debug.Log("manaValue.Lenght: " + manaValue.Length + " | availableMana.Count: " + availableMana.Count);
			int maxValueID = 0;
			if (availableMana.Count > 1) {
				for (int i = 1; i < manaValue.Length; i++) {
					if (manaValue[i] > manaValue[i-1]) {
						maxValueID = i;
					}
				}
			}

			//Debug.Log("Playing index " + maxValueID + " out of Lenght: " + manaValue.Length);
			//Debug.Log("Playing mana: " + availableMana[maxValueID].card.cardName);
			availableMana[maxValueID].PlayCard();
			availableMana.RemoveAt(maxValueID);
			//Debug.Log("totalManaQ: " + totalMana + " maxValue: " + manaValue[maxValueID]);
			manaValue[maxValueID] = 0;
			
		}

	}

	// Convert ManaCost to Int[]
	int[] CardManaCost(Card card) {
		int[] cardCost = new int[4];
		cardCost[0] = card.manaCost.r;
		cardCost[1] = card.manaCost.g;
		cardCost[2] = card.manaCost.b;
		cardCost[3] = card.manaCost.w;
		return cardCost;
	}
//

// Field Actions (Minion attacks && Artifact activation)
	bool DoFieldActions() {
		// check every minion and artifact on the field and choose the most valuable current ready action
		// then keep checking again until there's no action worth using it 

		// create a int array of action values with the minimum value to actually "act" with a card being int MinActValue
		// and unusable cards getting a full value of 0
		RefreshReadyToAct();

		CardType maxValueType;
		int maxMinionValueID = 0;
		int maxArtifactValueID = 0;
		// choose the most valuable one, attack with / activate it 
		// and return true if something has been used, else, return false to interrupt cycle
		if (player.field.minionCount() > 1) {
			for (int i = 1; i < minionsReady.Length; i++) {
				if (minionsReady[i] > minionsReady[i-1]) {
					Debug.Log("maxValueID: " + maxMinionValueID + " actual index: " + minionsReady[i] + 
							" minionsReady[]: " + minionsReady.Length + " minionCount(): " + player.field.minionCount() +
							"minions.count: " + player.field.minions.Count);
					maxMinionValueID = i;
				}
			}
		}
		if (player.field.artifactCount() > 1) {
			for (int i = 0; i < artifactsReady.Length; i++) {
				if (artifactsReady[i] > artifactsReady[i-1]) {
					maxArtifactValueID = i;
				}
			}
		}

		if (player.field.minionCount() > 0 && player.field.artifactCount() > 0) {
			if (minionsReady[maxMinionValueID] > artifactsReady[maxArtifactValueID]) {
				maxValueType = CardType.Minion;
			} else {
			maxValueType = CardType.Artifact;
			}
		} else if (player.field.minionCount() > 0 && player.field.artifactCount() <= 0) {
			maxValueType = CardType.Minion;
		} else if (player.field.minionCount() <= 0 && player.field.artifactCount() > 0) {
			maxValueType = CardType.Artifact;
		} else return false;
		
		// if the value is enought, Act! else, return false
		switch (maxValueType) {
			case CardType.Minion:
			if (minionsReady[maxMinionValueID] > MinActValue) {
				if (ChooseValidAttackTarget(player.field.minions[maxMinionValueID]) != null) {
					// attack with it to the new target
					player.field.minions[maxMinionValueID].Attack(ChooseValidAttackTarget(player.field.minions[maxMinionValueID]));
					return true;
				} else {
					player.field.minions[maxMinionValueID].DirectAttack(DuelManager.instance.Player0);
					return true;
				}
			}
			return false;

			case CardType.Artifact:
			if (artifactsReady[maxArtifactValueID] > MinActValue) {
				// i don't know exactly right now how this will be evaluated but surelly will be a way
				Debug.Log("Artifact " + player.field.artifacts[maxArtifactValueID].card.cardName + " could Act");
				return true;
			}
			return false;

			default:
			return false;
		}

	}

	// search for ready minions and artifacts
	void RefreshReadyToAct() {
		minionsReady = new int[player.field.minions.Count];
		artifactsReady = new int [player.field.artifacts.Count];

		// browse for every minion currently able to attack
		if (player.field.minionCount() > 0) {
			for (int i = 0; i < player.field.minions.Count; i++) {
				if (player.field.minions[i].ActionTokens > 0 && player.field.minions[i].actStatus == ActStatus.Ready) {
					minionsReady[i] = CheckActValue(player.field.minions[i]);
				}
			}
		}
		// browse for every minion currently able to attack
		if (player.field.artifactCount() > 0) {
			for (int i = 0; i < player.field.artifacts.Count; i++) {
				if (player.field.artifacts[i].ActionTokens > 0 && player.field.artifacts[i].actStatus == ActStatus.Ready) {
					artifactsReady[i] = CheckActValue(player.field.artifacts[i]);
				}
			}
		}

	}

	// after evaluation, check the most valuable target for the most valuable attacker
	CardHolder ChooseValidAttackTarget(CardHolder attacker) {
		if (DuelManager.instance.Player0.field.minionCount() > 0) {
			int[] targetValue = new int[DuelManager.instance.Player0.field.minionCount()];
		// evaluate all target
		for (int i = 0; i < DuelManager.instance.Player0.field.minionCount(); i++) {
			if (attacker.curDamage >= DuelManager.instance.Player0.field.minions[i].curHealth) {
				targetValue[i] += DuelManager.instance.Player0.field.minions[i].curHealth * 2;
				targetValue[i] += Mathf.CeilToInt(DuelManager.instance.Player0.field.minions[i].curDamage * 1.5f);

			} else {
				targetValue[i] += DuelManager.instance.Player0.field.minions[i].curHealth;
				targetValue[i] += DuelManager.instance.Player0.field.minions[i].curDamage;

			}
		}
		// choose the greater
		int maxValueID = 0;
		for (int i = 1; i < targetValue.Length; i++) {
			if (targetValue[i] > targetValue[i-1]) {
				maxValueID = i;
			}
		}

		return DuelManager.instance.Player0.field.minions[maxValueID];
		} else return null;

	}

	// evaluate minion attack value 
	int CheckActValue(CardHolder evaluated) {
		int actValue = 0;

		switch (evaluated.card.cardType) {
			case CardType.Minion:
			if (DuelManager.instance.Player0.field.minionCount() > 0) {
				// non-direct attack, search the damage that kill the most powerfull 
				actValue += EvaluateDamageMxM(evaluated);
				// also give actValue for the amount of minions the Player has
				actValue += DuelManager.instance.Player0.field.minionCount();
			} else {
				// DIRECT ATTACK
				actValue += 25;
			}
			// the less the player hp, the most likely is the AI to attack		
			actValue += (DuelManager.instance.Player0.hp - DuelManager.instance.Player0.curHP);
			// also give some actValue for the amount of damage the card has
			actValue += evaluated.curDamage;

			break;

			case CardType.Artifact:
			// need to finish CardEffect stuff before goint into details on this
			actValue += 20;

			break;
			
			default:
			actValue = 0;
			break;
		}

		return actValue;
	}

	// if there's minions on the enemy's field
	int EvaluateDamageMxM(CardHolder damageSource) {
		int damageValue = 0;
		int matchDxH = 0;
		bool match = false;
		// check possible targets
		for (int i = 0; i < DuelManager.instance.Player0.field.minionCount(); i++) {
			// check if this one attack will kill something
			if (damageSource.curDamage >= DuelManager.instance.Player0.field.minions[i].curHealth) {
				if (!match) {
					matchDxH = i;
					match = true;
				} else {
					if (DuelManager.instance.Player0.field.minions[i].curDamage > DuelManager.instance.Player0.field.minions[matchDxH].curDamage) {
						matchDxH = i;
					}
				}
			}
		}
		if (!match) {
			damageValue = 5;
			for (int i = 0; i < DuelManager.instance.Player0.field.minionCount(); i++) {
				damageValue += DuelManager.instance.Player0.field.minions[i].curDamage;
			}
		} else {
			damageValue = 10 + DuelManager.instance.Player0.field.minions[matchDxH].curDamage;
		}
		return damageValue;

	}

}


