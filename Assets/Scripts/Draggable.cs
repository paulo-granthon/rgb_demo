using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

	CardHolder card;
	public Transform returnParent;
	GameObject placeholder = null;
	LineRenderer targetAttack = null;
	bool targetingAttack = false;

	public void OnBeginDrag(PointerEventData eventData) {

		if (card == null) { card = GetComponent<CardHolder>(); }

		if (card != null && card.curStatus == Status.Field 
		&& card.card.cardType == CardType.Minion 
		&& card.actStatus == ActStatus.Ready 
		&& card.curActTokens > 0) {
			
			// target an attack
			if (targetAttack == null) { targetAttack = PlayerInterface.instance.GetComponent<LineRenderer>(); }
			targetAttack.enabled = true;
			targetAttack.SetPosition(0, transform.position);
			targetingAttack = true;
			// duel manager start target
			DuelManager.instance.isTargeting = true;
			DuelManager.instance.currentHoldingCard = card.card;

		} else if (card.owner == DuelManager.instance.Player0) {	// if owner is Player
			targetingAttack = false;
			returnParent = transform.parent;

			// placeholder
			placeholder = new GameObject();
			placeholder.transform.SetParent(returnParent);
			LayoutElement le = placeholder.AddComponent<LayoutElement>();
			LayoutElement tle = GetComponent<LayoutElement>();
			le.preferredHeight = tle.preferredHeight;
			le.preferredWidth = tle.preferredWidth;
			placeholder.transform.SetSiblingIndex(transform.GetSiblingIndex());

			// start card drag
			transform.SetParent(returnParent.parent);
			// Duel manager start drag
			DuelManager.instance.isDragging = true;
			DuelManager.instance.currentHoldingCard = card.card;

		}

		GetComponent<CanvasGroup>().blocksRaycasts = false;

	}

	public void OnDrag(PointerEventData eventData) {
		
		if (!targetingAttack && DuelManager.instance.isDragging) {
			// move the card along the cursor
			transform.position = eventData.position;

			if (returnParent.GetComponent<DropZone>().zoneType != DropZone.DropZoneType.Deck 
				&& returnParent.GetComponent<DropZone>().zoneType != DropZone.DropZoneType.GraveyardField) {

				int newIndex = returnParent.childCount;

				for (int i = 0; i < returnParent.childCount; i++) {
					if(transform.position.x < returnParent.GetChild(i).position.x) {

						newIndex = i;
						if (placeholder.transform.GetSiblingIndex() < newIndex)
						newIndex--;
						break;
					}
				}
				placeholder.transform.SetSiblingIndex(newIndex);
			}
		} else if (DuelManager.instance.isTargeting) {
			targetAttack.SetPosition(1, eventData.position);
		}
	}

	public void OnEndDrag(PointerEventData eventData) {
		
		if (!targetingAttack && DuelManager.instance.isDragging) {

			transform.SetParent(returnParent);
			if (placeholder != null) {
				transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
				Destroy(placeholder);
			}
			
			// duel manager end drag
			DuelManager.instance.isDragging = false;


		} else if (DuelManager.instance.isTargeting) {

			targetAttack.enabled = false;

         	PointerEventData pointerData = new PointerEventData (EventSystem.current) { pointerId = -1, };
        	pointerData.position = Input.mousePosition;
        	List<RaycastResult> results = new List<RaycastResult>();
        	EventSystem.current.RaycastAll(pointerData, results);
			if (TargetCard(results)) { }
			else { TargetPlayer(results); } 

			targetingAttack = false;
			DuelManager.instance.isTargeting = false;
		}

		GetComponent<CanvasGroup>().blocksRaycasts = true;
	}

	bool TargetCard(List<RaycastResult> hitList){
		for (int i = 0; i < hitList.Count; i++) {
			CardHolder target = hitList[i].gameObject.GetComponent<CardHolder>();
			if (target != null) {
				if (target.card != null) {
					if (target.curStatus == Status.Field && target.card.health > 0) {
						card.Attack(target);
						return true;
					}
				}
			}
		}
		return false;
	}

	void TargetPlayer(List<RaycastResult> hitList){
		for (int i = 0; i < hitList.Count; i++) {
			DropZone target = hitList[i].gameObject.GetComponent<DropZone>();
			if (target != null) {
				if (target.owner != null) {
					if (target.owner != card.owner && target.owner.field.minions.Count <= 0) {
						card.DirectAttack(target.owner);
					}
				}
			}
		}
	}
}