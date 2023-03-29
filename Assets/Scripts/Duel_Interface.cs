using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Duel_Interface : MonoBehaviour {

	public float cardDragSpeed;
	public float cardDragDelay;

	List<MovingObj> movingList = new List<MovingObj>();


	public void MoveCard(CardHolder card, DropZone from, DropZone to) {
		movingList.Add(new MovingObj (card, card.transform.position, to));

	}

	void Update() {
		if (movingList.Count > 0) {
			for (int i = 0; i < movingList.Count; i++) {
				if (movingList[i].card.transform.position != movingList[i].to.transform.position) {
					movingList[i].card.curStatus = Status.Moving;
					movingList[i].card.transform.Translate(movingList[i].to.transform.position * Time.deltaTime * cardDragSpeed);
					Debug.Log(movingList[i].card.card.cardName + " " + movingList[i].card.transform.position.ToString() + ", from: " + movingList[i].from + ", to: " + movingList[i].to);

				} else {
					Debug.Log(movingList[i] + " reached it's destination");
					movingList[i].card.curStatus = movingList[i].nextStatus;
					movingList.RemoveAt(i);
				}
			}
		}
	}

	public void OnDrawGizmos() {
		Gizmos.color = Color.white;

		for (int i = 0; i < movingList.Count; i++) {
			Gizmos.DrawIcon(movingList[i].to.transform.position, "dest");
			Gizmos.DrawIcon(movingList[i].from, "orign");

		}
	}

	public class MovingObj {
		public CardHolder card;
		public Vector3 from;
		public DropZone to;
		public Status nextStatus;

		public MovingObj (CardHolder _card, Vector3 _from, DropZone _to) {
			card = _card;
			from = _from;
			to = _to;
			
			switch (to.zoneType) {
				case DropZone.DropZoneType.Deck:
				nextStatus = Status.NonPlayed;
				break;

				case DropZone.DropZoneType.Hand:
				nextStatus = Status.NonPlayed;
				break;

				case DropZone.DropZoneType.MinionField:
				nextStatus = Status.Field;
				break;

				case DropZone.DropZoneType.ArtifactField:
				nextStatus = Status.Field;
				break;

				case DropZone.DropZoneType.GraveyardField:
				nextStatus = Status.Dead;
				break;
			}

		}
	}
	
}
