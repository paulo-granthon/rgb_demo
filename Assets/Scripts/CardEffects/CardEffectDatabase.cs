using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardEffectDatabase {

	public static void CheckforTrigger (CardHolder card, CardEffect.Trigger trigger) {
		
	}

	public static void CheckForTrigger (CardHolder card, CardEffect.Trigger trigger) {
		for (int i = 0; i < card.card.effects.Count; i++) {
			if (card.card.effects[i].trigger == trigger) {
				card.card.effects[i].EffectActivate();
			}
		}
	}

	// public static void Activate (int id) {
	// 	CardEffectDatabase.Effect0 + "id";
	// }

	// public List<var> AskConditions () {
	// 	list<var> conditions = new list<var>();

		
		
	// 	return conditions;
	// }

	static void Effect0 () {
		// activate the effect 0 
	}
}
