using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{

    public LayerMask clickableLayers;
    InteractionObject objectBeingPointedAt, objectBeingHeld;
    VisualizeVia InteractionVisualizer;
    public PlayerMovement playerMovement;
    public Inventory playerInventory;

    TeleVisualizer TeleportVisualizer;
    Ray shooter;
    RaycastHit t_rh;
    Vector3 subtract = new Vector3(0, 0.7f, 0);
    Vector3 teleportEndSpot;
    float rayLength = 2.5f, wallBuffer = 0.5f;

    private void Awake()
    {
        objectBeingPointedAt = new InteractionObject();
        objectBeingHeld = new InteractionObject();

        //if mouse input
        InteractionVisualizer = GetComponent<VisualizeViaCanvas>();
        TeleportVisualizer = GetComponentInChildren<TeleVisualizer>();

        if (playerInventory == null)
            Debug.LogError("no inventory attached to interaction manager");
    }

    public void EngageWithObjectStart(Transform objectToEngage)
    {
        if (objectBeingPointedAt.Exists && objectToEngage == objectBeingPointedAt.m_Transform)
        {
            //InteractWithObjectBeingPointedAt();
            switch (objectBeingPointedAt.m_PuzzleElement.InteractionType)
            {
                case (ObjectInteractions.drag):
                    objectBeingHeld = objectBeingPointedAt;
                    objectBeingHeld.m_PuzzleElement.OnInteractStart(InteractionVisualizer.HeldObjectHolder);
                    break;
                case (ObjectInteractions.take):
                    playerInventory.StoreItem(objectBeingPointedAt);
                    InteractionVisualizer.AddItemToHand(objectBeingPointedAt);
                    //playerInventory.VisualizeItem(InteractionVisualizer.HeldObjectHolder);
                    break;
                case (ObjectInteractions.use):
                    if (objectBeingPointedAt.m_PuzzleElement.RequiredKey != null)
                        if (objectBeingPointedAt.m_PuzzleElement.RequiredKey == playerInventory.ActiveStoredItem.m_PuzzleElement)
                        {
                            objectBeingPointedAt.m_PuzzleElement.OnInteractStart();
                            playerInventory.UsedActiveHeldItem();

                            InteractionVisualizer.NarrateInteraction(true);
                        }
                        else
                        {
                            objectBeingPointedAt.m_PuzzleElement.OnInteractFailed();
                            InteractionVisualizer.NarrateInteraction(false);
                        }
                    else
                        objectBeingPointedAt.m_PuzzleElement.OnInteractStart();
                    break;
                default:
                    objectBeingPointedAt.m_PuzzleElement.OnInteractStart(this);
                    break;
            }
        }
    }
    public void EngageWithObjectContinued(Transform objectToEngage)
    {
        // if (objectBeingHeld.Exists)
        //     return;
    }

    public void MightEngageWithObjectStart(Transform objectToEngage)
    {
        if (objectBeingHeld.Exists)
        {
            //switch (objectBeingHeld.m_PuzzleElement.InteractionType)
            {
                //    case (ObjectInteractions.drag):
                objectBeingHeld.m_PuzzleElement.OnInteractEnd();
                objectBeingHeld.ClearInfo();
                //         break;
            }
            //StopInteractingWithHeldObject();
        }
    }
    public void MightEngageWithObjectContinued(Transform objectToEngage)
    {
        //if (objectBeingHeld.Exists)
        //   return;
        if (objectToEngage is Transform && objectToEngage.CompareTag("Interact"))
        {
            objectBeingPointedAt.PopulateInfo(objectToEngage);
            SetInteractionIndicators(objectToEngage.GetComponent<PuzzleElement>().InteractionType);
        }
        else
        {
            objectBeingPointedAt.ClearInfo();
            SetInteractionIndicators(ObjectInteractions.none);
        }


        /*if (objectToEngage != objectBeingPointedAt.m_Transform)
        {
            if (objectToEngage is Transform && objectToEngage.CompareTag("Interact"))
            {
                objectBeingPointedAt.PopulateInfo(objectToEngage);
                SetInteractionIndicators(objectToEngage.GetComponent<PuzzleElement>().InteractionType);
            }
            else
            {
                objectBeingPointedAt.ClearInfo();
                SetInteractionIndicators(ObjectInteractions.none);
            }
        }*/
    }

    void InteractWithObjectBeingPointedAt()
    {

        //if holding something, ignore
        //if not holding something, 

        {
            //objectBeingPointedAt.Interact();
            //interact
            //if this interaction is a grab, pickup object handle
            //if this interaction is a take, add object to inventory
        }
    }
    void StopInteractingWithHeldObject()
    {

    }

    public void AimNavPointerStart()
    {
        //show
        TeleportVisualizer.ShowTeleportFX();
    }

    public void AimNavPointerContinued(Vector3 angle)
    {
        //aim 
        //angle
        //45 degree down
        //down

        if (angle.y > 0.99f)
            return;

        shooter.origin = InteractionVisualizer.TeleportObjectHolder.transform.position;
        shooter.direction = InteractionVisualizer.TeleportObjectHolder.transform.forward;

        Physics.Raycast(shooter, out t_rh, rayLength);
        teleportEndSpot = shooter.GetPoint(rayLength);

        Debug.DrawLine(shooter.origin, teleportEndSpot, Color.green);
        if (t_rh.transform is Transform)
        {
            teleportEndSpot = t_rh.point;
        }
        else
        {
            for (int i = 0; i < 6; i++)
            {
                if (t_rh.transform is Transform)
                {
                    teleportEndSpot = t_rh.point;
                    break;
                }
                else
                {
                    shooter.origin = shooter.GetPoint(rayLength);
                    shooter.direction = shooter.direction - subtract;
                    Physics.Raycast(shooter, out t_rh, rayLength);
                    Debug.DrawLine(shooter.origin, shooter.GetPoint(rayLength), Color.green);
                }
            }
            if (t_rh.transform is Transform)
            {
                teleportEndSpot = t_rh.point;
            }
            else
            {
                shooter.origin = shooter.GetPoint(rayLength);
                shooter.direction = -Vector3.up;
                Physics.Raycast(shooter, out t_rh);
                Debug.DrawLine(shooter.origin, shooter.GetPoint(20), Color.green);
                teleportEndSpot = t_rh.point;
            }
        }

        if (t_rh.normal != Vector3.up)
        {
            shooter.origin = t_rh.point;
            shooter.direction = t_rh.normal;
            Physics.Raycast(shooter, out t_rh, wallBuffer);
            Debug.DrawLine(shooter.origin, shooter.GetPoint(wallBuffer), Color.green);
            teleportEndSpot = shooter.GetPoint(wallBuffer);

            shooter.origin = teleportEndSpot;
            shooter.direction = -Vector3.up;
            Physics.Raycast(shooter, out t_rh);
            Debug.DrawLine(shooter.origin, shooter.GetPoint(20), Color.green);
            teleportEndSpot = t_rh.point;
        }
        else
        {
            shooter.origin = t_rh.point;
            shooter.direction = new Vector3(shooter.direction.x,0,shooter.direction.z);

            Physics.Raycast(shooter, out t_rh, wallBuffer);
            Debug.DrawLine(shooter.origin, shooter.GetPoint(wallBuffer), Color.green);
            if (t_rh.transform is Transform)
            {
                shooter.origin = t_rh.point;
                shooter.direction = t_rh.normal;
                Physics.Raycast(shooter, out t_rh, wallBuffer);
                Debug.DrawLine(shooter.origin, shooter.GetPoint(wallBuffer), Color.green);
                teleportEndSpot = shooter.GetPoint(wallBuffer);
            }
        }

        TeleportVisualizer.SetTeleport(InteractionVisualizer.TeleportObjectHolder.position, teleportEndSpot);
    }

    public void ReleasedNavPointer()
    {
        //teleport
        playerMovement.Move(teleportEndSpot);

        TeleportVisualizer.HideTeleportFX();
    }

    public void SetScrollInput(float amt)
    {
        if (amt < 0)
            playerInventory.ScrollThroughItems(Inventory.ScrollDir.next);
        else
            playerInventory.ScrollThroughItems(Inventory.ScrollDir.prev);
    }
    public void SetInteractionIndicators(ObjectInteractions type)
    {
        InteractionVisualizer.VisualizeInteraction(type);
    }

}

[System.Serializable]
public class InteractionObject
{
    public bool Exists { get { return m_Transform != null; } }
    public bool IsPuzzle { get { return m_PuzzleElement is PuzzleElement; } }
    public Transform m_Transform { get; private set; }
    public PuzzleElement m_PuzzleElement { get; private set; }
    Vector3 normalSize;

    public void PopulateInfo(Transform startingInfo)
    {
        m_Transform = startingInfo;
        m_PuzzleElement = m_Transform.GetComponent<PuzzleElement>();
    }
    public void PopulateInfo(PuzzleElement startingInfo)
    {
        m_PuzzleElement = startingInfo;
        m_Transform = m_PuzzleElement.transform;
    }

    public void ClearInfo()
    {
        if (m_Transform is Transform)
        {
            m_Transform = null;
            m_PuzzleElement = null;
        }
    }

    public Transform ReturnRelevantItem()
    {
        if (m_PuzzleElement is PuzzleElement)
            return m_PuzzleElement.ReturnTransform();
        else
            return null;
    }

    public void ShrinkItem(float size)
    {
        normalSize = m_Transform.localScale;
        m_Transform.localScale *= size;
    }

    public void ResetItemScale()
    {
        m_Transform.localScale = normalSize;
    }
}

public enum ObjectInteractions
{
    none,
    look,
    take,
    use,
    leave,
    drag,
    UI
}