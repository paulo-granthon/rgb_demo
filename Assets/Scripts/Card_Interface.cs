using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Script managing a lot of Card's visual stuff 
public class Card_Interface : MonoBehaviour {

	public CardHolder card;
	public Text nameText;
	public Text damageText;
	public Text healthText;

	int i;

	public Color upCollor;
	public Color defCollor;
	public Color downCollor;

	void Start () {

		Set();

		Callback();
	}

	void OnEnable() {
		if (nameText != null) {
			Callback();
		} else {
			Set();
		}
	}

	void Set() {
		// Assign components
		if (card == null) {card = transform.parent.GetComponent<CardHolder>();}
		if (card.cardUI == null) {card.cardUI = gameObject;}
		nameText = transform.Find("Name").Find("Text").GetComponent<Text>();

		if (card.card.cardType == CardType.Minion) {
			damageText = transform.Find("Damage").Find("Text").GetComponent<Text>();
			healthText = transform.Find("Health").Find("Text").GetComponent<Text>();
		}

		if (card.card.cardType == CardType.Mana) {
			// clear grid children
			for (int i = 0; i < transform.Find("ManaCost").Find("Grid").transform.childCount; i++) {
				Destroy(transform.Find("ManaCost").Find("Grid").transform.GetChild(i).gameObject);
			}

			// populate icons R
			if (card.card.manaCost.r > 0) {
				for (int i = 0; i < card.card.manaCost.r; i++) {
					GameObject icon = Instantiate(Resources.Load("ManaIcon")) as GameObject;
					icon.transform.SetParent(transform.Find("ManaCost").Find("Grid").transform);
					icon.transform.Find("Sprite").GetComponent<Image>().sprite = DuelManager.instance.manaIcons.manaCollorR;
					icon.transform.localScale = new Vector3(1,1,1);
				}
			}
			// populate icons G
			if (card.card.manaCost.g > 0) {
				for (int i = 0; i < card.card.manaCost.g; i++) {
					GameObject icon = Instantiate(Resources.Load("ManaIcon")) as GameObject;
					icon.transform.SetParent(transform.Find("ManaCost").Find("Grid").transform);
					icon.transform.Find("Sprite").GetComponent<Image>().sprite = DuelManager.instance.manaIcons.manaCollorG;
					icon.transform.localScale = new Vector3(1,1,1);
				}
			}
			// populate icons B
			if (card.card.manaCost.b > 0) {
				for (int i = 0; i < card.card.manaCost.b; i++) {
					GameObject icon = Instantiate(Resources.Load("ManaIcon")) as GameObject;
					icon.transform.SetParent(transform.Find("ManaCost").Find("Grid").transform);
					icon.transform.Find("Sprite").GetComponent<Image>().sprite = DuelManager.instance.manaIcons.manaCollorB;
					icon.transform.localScale = new Vector3(1,1,1);
				}
			}
			// populate icons W
			if (card.card.manaCost.w > 0) {
				for (int i = 0; i < card.card.manaCost.b; i++) {
					GameObject icon = Instantiate(Resources.Load("ManaIcon")) as GameObject;
					icon.transform.SetParent(transform.Find("ManaCost").Find("Grid").transform);
					icon.transform.Find("Sprite").GetComponent<Image>().sprite = DuelManager.instance.manaIcons.manaCollorW;
					icon.transform.localScale = new Vector3(1,1,1);
				}
			}

			
		} else {
			if (card.card.cardCollor == Collor.Red) {
				transform.Find("ManaCost").Find("Icon").Find("Sprite").GetComponent<Image>().sprite = DuelManager.instance.manaIcons.manaCollorR;
				transform.Find("ManaCost").Find("Icon").Find("Text").GetComponent<Text>().text = card.card.manaCost.r.ToString();
			} else if (card.card.cardCollor == Collor.Green) {
				transform.Find("ManaCost").Find("Icon").Find("Sprite").GetComponent<Image>().sprite = DuelManager.instance.manaIcons.manaCollorG;
				transform.Find("ManaCost").Find("Icon").Find("Text").GetComponent<Text>().text = card.card.manaCost.g.ToString();
			} else if (card.card.cardCollor == Collor.Blue) {
				transform.Find("ManaCost").Find("Icon").Find("Sprite").GetComponent<Image>().sprite = DuelManager.instance.manaIcons.manaCollorB;
				transform.Find("ManaCost").Find("Icon").Find("Text").GetComponent<Text>().text = card.card.manaCost.b.ToString();
			} else if (card.card.cardCollor == Collor.White) {
				transform.Find("ManaCost").Find("Icon").Find("Sprite").GetComponent<Image>().sprite = DuelManager.instance.manaIcons.manaCollorW;
				transform.Find("ManaCost").Find("Icon").Find("Text").GetComponent<Text>().text = card.card.manaCost.w.ToString();
			}
		}

	}
	
	public void Callback () {

		upCollor = card.owner.cardTextCollors.upCollor;
		defCollor = card.owner.cardTextCollors.defCollor;
		downCollor = card.owner.cardTextCollors.downCollor;

		// set the update information
		nameText.text = card.card.cardName;

		if (card.card.cardType != CardType.Mana) {
			damageText.text = card.curDamage.ToString();
			healthText.text = card.curHealth.ToString();
			if (card.ActionTokens >= 0 && card.actStatus == ActStatus.Cooldown) {
				//card.GetComponent<Image>().color // TODO make it more grey when in cooldown;
			}
		}

		// update collors if that's not the first update (when it becomes visible for the first time)
		if (i > 0) {
			// compare previous and current name
			if (nameText.text != card.card.cardName) {
				nameText.color = downCollor;
			}
			
			// Mana cards don't have damage and health status
			if (card.card.cardType == CardType.Minion) {
				// compare previous and current damage
				if (damageText.text != card.card.damage.ToString()) {
					int compare = Compare(card.card.damage, damageText.text);
					if (compare > 0) {
						damageText.color = upCollor;
					} else if (compare == 0) {
						damageText.color = defCollor;
					} else if (compare < 0) {
						damageText.color = downCollor;
					}
				}
				// compare previous and current health
				if (healthText.text != card.card.health.ToString()) {
					int compare = Compare(card.card.health, healthText.text);
					if (compare > 0) {
						healthText.color = upCollor;
					} else if (compare == 0) {
						healthText.color = defCollor;
					} else if (compare < 0) {
						healthText.color = downCollor;
					}
				}
			} else if (card.card.cardType == CardType.Mana) {
				if (card.curStatus == Status.Dead) {
					for (int i = 0; i < transform.Find("ManaCost").Find("Grid").transform.childCount; i++) {
						transform.Find("ManaCost").Find("Grid").transform.GetChild(i).gameObject.SetActive(false);
					}

				}
			}
		} else {
			i++;
			nameText.color = defCollor;

			if (card.card.cardType != CardType.Mana) {
				damageText.color = defCollor;
				healthText.color = defCollor;
			}
			
		}
	}

	// transform string into int
	int Compare(int prev, string cur) {

		int curInt = int.Parse(cur);

		return System.Math.Sign(curInt - prev);
	}
}
