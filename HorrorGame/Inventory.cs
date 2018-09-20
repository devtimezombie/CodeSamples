using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    InteractionObject activeStoredItem = new InteractionObject();
    public InteractionObject ActiveStoredItem
    {
        get
        {
            return activeStoredItem;
        }
        set
        {
            if (activeStoredItem.Exists)
                activeStoredItem.m_Transform.gameObject.SetActive(false);
            activeStoredItem = value;
            activeStoredItem.m_Transform.gameObject.SetActive(true);
        }
    }
    int numInStoredItems;
    List<InteractionObject> StoredItems = new List<InteractionObject>();
    public enum ScrollDir
    {
        prev,
        next
    }

    void Start()
    {

    }

    public void StoreItem(InteractionObject item)
    {
        if (StoredItems.Count > 0)
            foreach (InteractionObject io in StoredItems)
            {
                if (io.m_PuzzleElement == item.m_PuzzleElement)
                    return;
            }

        InteractionObject t_newItem = new InteractionObject();
        t_newItem.PopulateInfo(item.m_PuzzleElement);
        StoredItems.Add(t_newItem);
        SetActiveHeldItem(-1);
    }

    public void ScrollThroughItems(ScrollDir direction)
    {
        if (StoredItems.Count == 0) return;

        if (direction == ScrollDir.next)
            numInStoredItems += 1;
        else
            numInStoredItems -= 1;
        numInStoredItems = mod(numInStoredItems, StoredItems.Count);

        ActiveStoredItem = StoredItems[numInStoredItems];
    }

    int mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public void SetActiveHeldItem(int index)
    {
        if (StoredItems.Count == 0) return;

        if (index == -1)
            numInStoredItems = StoredItems.Count - 1;
        else if (index > -1 && index < StoredItems.Count)
            numInStoredItems = index;

        ActiveStoredItem = StoredItems[numInStoredItems];
    }

    public void UsedActiveHeldItem()
    {
        if (ActiveStoredItem.m_PuzzleElement.InteractionType == ObjectInteractions.take)
        {
            ActiveStoredItem.m_PuzzleElement.OnInteractEnd();
            StoredItems.Remove(ActiveStoredItem);
            ScrollThroughItems(ScrollDir.prev);
        }
    }

    public bool ContainsKey(PuzzleElement key)
    {
        foreach (InteractionObject io in StoredItems)
        {
            if (io.m_PuzzleElement == key)
                return true;
        }
        return false;
    }
}
