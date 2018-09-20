using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualizeVia : MonoBehaviour
{
    public Transform HeldObjectHolder, TeleportObjectHolder;

    public virtual void VisualizeInteraction(ObjectInteractions action) { }

    public virtual void AddItemToHand(InteractionObject item)
    {
        item.m_Transform.SetParent(HeldObjectHolder);
        item.m_Transform.localPosition = Vector3.zero;
    }

    public virtual void NarrateInteraction(bool succeeded) { }
}
