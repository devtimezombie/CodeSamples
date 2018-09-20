using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleElement : MonoBehaviour
{

    public ObjectInteractions InteractionType = ObjectInteractions.take;
    public PuzzleElement RequiredKey;
    public UseEvent OnFailedUse, OnSuccessfulUse;

    // Use this for initialization
    void Start()
    {

    }

    public virtual void OnInteractStart()
    {
        OnSuccessfulUse.Invoke();

        /* switch (InteractionType)
         {
             case (ObjectInteractions.look):
                 break;
             case (ObjectInteractions.take):
                 //add item to inventory
                 //hide
                 break;
             case (ObjectInteractions.use):
             //OnFailedUse
                 break;
             case (ObjectInteractions.leave):
                 break;
             case (ObjectInteractions.drag):

                 break;
             default:
                 //Debug.Log("Clicked!");
                 break;
         }*/
    }
    public virtual void OnInteractStart(Transform specialInfo) { }
    public virtual void OnInteractStart(InteractionManager callback)
    {
        //if (InteractionType == ObjectInteractions.take)
        Debug.Log(InteractionType);
        OnSuccessfulUse.Invoke();
    }

    public virtual void OnInteractFailed() {
        OnFailedUse.Invoke();
    }

    public virtual void OnInteractEnd()
    {
        gameObject.SetActive(false);
    }

    public virtual Transform ReturnTransform()
    {
        return this.transform;
    }
}

[System.Serializable]
public class UseEvent : UnityEngine.Events.UnityEvent { }
