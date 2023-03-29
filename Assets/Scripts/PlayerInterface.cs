using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInterface : MonoBehaviour {
// Singleton
	#region Singleton
	public static PlayerInterface instance;
	void Awake() {
		if (instance != null) {
			Debug.LogError("there's more than one DuelManager's instance found");
		}
		instance = this;
	}
	#endregion 
// Singleton */ 

	DuelManager.Player.ManaPool manaPool;

	public Text player0HP;
	public Text player1HP;

	public Text textR;
	public Text textG;
	public Text textB;
	public Text textW;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		// player manaPool
		if (DuelManager.instance.players[0] != null) {
			manaPool = DuelManager.instance.players[0].manaPool;
			textR.text = manaPool.r.ToString();
			textG.text = manaPool.g.ToString();
			textB.text = manaPool.b.ToString();
			textW.text = manaPool.w.ToString();
		}

		// players HP
		if (DuelManager.instance.players[0] != null && DuelManager.instance.players[1] != null) {
			player0HP.text = DuelManager.instance.players[0].curHP.ToString();
			player1HP.text = DuelManager.instance.players[1].curHP.ToString();
		}
	}
}
