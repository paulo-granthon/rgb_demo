using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New CardEffect", menuName = "CardEffect")]
public class CardEffect : ScriptableObject {

	public enum Trigger {
		// when the card first enters the field
		WhenSummoned, 
		// before declaring an attack against a minion
		BeforeAttacking, 
		// after declaring an attack against a minion
		AfterAttacking, 
		// before declaring a direct attack against a player
		BeforeDirectAtk,
		// after declaring a direct attack against a player
		AfterDirectAtk,
		// before defending an attack
		BeforeDefending,
		// after defending an attack
		AfterDefending,
		// before it's owner take a direct attack
		BeforeDirectDef,
		// after it's owner take a direct attack
		AfterDirectDef, 
		// before being destroyed
		BeforeDie,
		// after being destroyed
		AfterDie
	};

	public int id;
	public string Name;
	public Trigger trigger;

	public void EffectActivate() {
		
		// CardEffectDatabase.Activate(id);
	}
}
