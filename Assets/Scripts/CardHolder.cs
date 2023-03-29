using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardHolder : MonoBehaviour {

	//
	public new string name;

	Card _card;
	[SerializeField]
	public Card card { get { return _card; } set { _card = value; name = _card.name; } }

	public int curDamage;
	public int curHealth;

	//
	public GameObject cardUI;

	//
	public DuelManager.Player owner;			// card's owner
	public Status curStatus = Status.NonPlayed;		// current status of the card
	public int ActionTokens = 1;
	public int curActTokens;
	public ActStatus actStatus = ActStatus.Cooldown; 	// Minion: able to attack	// Artifact: able to activate effect

	void Start() {
		ResetCurrent();
	}

	public void PlayCard()	// this will happen when the card enter the field (the card is played)
	{
		// WHAT'S THE TYPE OF THE CARD // IF IT'S NOT MANA, THEN PLAY IT THIS WAY --
		if (card.cardType != CardType.Mana){
		// check mana cost and then continue with the PlayCard() if CheckCost() is true 
			if (CheckCost() && curStatus == Status.NonPlayed) {

				if (GetComponent<Image>().sprite == DuelManager.instance.cardBack) {
					GetComponent<Image>().sprite = card.cardSprite;
					cardUI.SetActive(true);
				} else {
					cardUI.GetComponent<Card_Interface>().Callback();
				}

				// subtract the mana cost
				owner.manaPool.Subtract(card.manaCost);
				actStatus = ActStatus.Ready;
				curActTokens = 0;

				// check if playied successfully, if playied, then ok, if not for whatever reason, return this card to its place 
				switch (card.cardType) {

					// minion
					case CardType.Minion: 
					curStatus = Status.Field;
					curDamage = card.damage;	curHealth = card.health;
					DuelManager.instance.AreaChange(this, Where(), owner.field.minionsParent.GetComponent<DropZone>(), owner.intelligence?true:false);
					UpdateReturnParent();

					// check for effects
					CardEffectDatabase.CheckforTrigger(this, CardEffect.Trigger.WhenSummoned);

					break;

					// artifact
					case CardType.Artifact: 
					curStatus = Status.Field;
					DuelManager.instance.AreaChange(this, Where(), owner.field.artifactsParent.GetComponent<DropZone>(), owner.intelligence?true:false);
					UpdateReturnParent();

					// check for effects
					CardEffectDatabase.CheckforTrigger(this, CardEffect.Trigger.WhenSummoned);

					break;

					// magic
					case CardType.Magic:
					// activate magic effects 
					Destroy();
					break;
					
				}

				owner.field.Refresh(); owner.hand.Refresh();
				curActTokens = ActionTokens;


			} else {
				// card's owner don't have enought mana to play the card 
				curStatus = Status.NonPlayed;
				//return;	//idk if i need this line 
			}

		} else {		// IF THE CARD IS MANA, THEN PLAY IT THIS OTHER WAY 

			if (GetComponent<Image>().sprite == DuelManager.instance.cardBack) {
				GetComponent<Image>().sprite = card.cardSprite;
				cardUI.SetActive(true);
			}

			DuelManager.instance.AreaChange(this, Where(), owner.field.minionsParent.GetComponent<DropZone>(), owner.intelligence?true:false);
			UpdateReturnParent();
			owner.manaPool.Add(card.manaCost);

			// check for effects
			CardEffectDatabase.CheckforTrigger(this, CardEffect.Trigger.WhenSummoned);

			// discard the card after mana is added
			Destroy();
		}
	}

	// check if this card's player manapool contains the amount necessary to play this card 
	public bool CheckCost() {
		if (card.cardType != CardType.Mana){
			if (owner.manaPool.Compare(card.manaCost)){
				return true;
			} else {
				return false;
			}
		} else {
			return true;
		}
	}

	// Destroy Card
	public void Destroy() {

		// check for effects
		CardEffectDatabase.CheckforTrigger(this, CardEffect.Trigger.BeforeDie);
		
		// reset
		ResetCurrent();					//
		curStatus = Status.Dead;
		// update card visuals
		cardUI.GetComponent<Card_Interface>().Callback();
		// Destroy
		DropZone from = GetComponent<Draggable>().returnParent.GetComponent<DropZone>();
		GetComponent<Draggable>().returnParent = owner.graveyard.graveyard.transform;
		DuelManager.instance.AreaChange(this, from, owner.graveyard.graveyard.GetComponent<DropZone>(), owner.intelligence?true:false);

		// check for effects
		CardEffectDatabase.CheckforTrigger(this, CardEffect.Trigger.AfterDie);
	}

	public DropZone Where() {
		return transform.parent.GetComponent<DropZone>();
	}

	public void UpdateReturnParent() {
		GetComponent<Draggable>().returnParent = transform.parent;
	}

	// Attack
	public void Attack(CardHolder target) {

		// check for effects
		CardEffectDatabase.CheckforTrigger(this, CardEffect.Trigger.BeforeAttacking);

		Debug.Log(card.cardName + " Attacks enemy's minion " + target.card.cardName);
		target.TakeDamage(curDamage, this);					//

		// check for effects
		CardEffectDatabase.CheckforTrigger(this, CardEffect.Trigger.AfterAttacking);

		ActCount();

	}

	public void DirectAttack(DuelManager.Player target) {

		// check for effects
		CardEffectDatabase.CheckforTrigger(this, CardEffect.Trigger.BeforeDirectAtk);

		Debug.Log(card.cardName + " Attacks enemy " + target.name + " directly!");
		target.TakeDamage(curDamage, this);					//

		// check for effects
		CardEffectDatabase.CheckforTrigger(this, CardEffect.Trigger.AfterDirectAtk);

		ActCount();

	}

	// Take Damage
	public void TakeDamage(int amount, CardHolder source) {
		
		// check for effects
		CardEffectDatabase.CheckforTrigger(this, CardEffect.Trigger.BeforeDefending);

		curHealth -= amount;					//

		// update visuals
		cardUI.GetComponent<Card_Interface>().Callback();
		
		// check for effects
		CardEffectDatabase.CheckforTrigger(this, CardEffect.Trigger.AfterDefending);

		if (curHealth <= 0) {					//
			Destroy();
		}

	}

	void ActCount() {
		curActTokens--;
		if (curActTokens >= 0) {
			actStatus = ActStatus.Cooldown;
		}
	}

	// replace current data with the original card's data
	public void ResetCurrent() {
		curDamage = card.damage;
		curHealth = card.health;
	}

}

