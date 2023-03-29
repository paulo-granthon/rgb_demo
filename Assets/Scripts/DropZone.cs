using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler {
	
	public DuelManager.Player owner;
	public DropZoneType zoneType; // the type of zone is this  
	public bool playZone;
	public virtual void OnPointerEnter(PointerEventData eventData)
	{

	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		
	}

	public void OnDrop(PointerEventData eventData)
	{
		Draggable d = eventData.pointerDrag.GetComponent<Draggable>();
		CardHolder c = eventData.pointerDrag.GetComponent<CardHolder>();

		if (c !=null && d != null && DuelManager.instance.isDragging) {

			DropZone from = d.returnParent.GetComponent<DropZone>();
			
			bool possiblePlay = DuelManager.instance.CheckPlayable(c, from.zoneType, zoneType);
			if (possiblePlay) {
				d.returnParent = transform;
				DuelManager.instance.AreaChange(c, from, this, false);
				d.GetComponent<CardHolder>().PlayCard();
			} 
			else {
			bool possibleDrop = DuelManager.instance.CheckDroppable(c, from.zoneType, zoneType);
				if (possibleDrop) {
					d.returnParent = transform;
					DuelManager.instance.AreaChange(c, from, this, c.owner.intelligence?true:false);
					if (from.zoneType == DropZoneType.Deck) {
						c.GetComponent<Image>().sprite = c.card.cardSprite;
					}
				}	
			}
			
		}

	}

	public enum DropZoneType {Deck, Hand, MinionField, ArtifactField, GraveyardField};

}
