using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AcceptibleWaysToDie
{
    murder,
    killBox
}

public enum AcceptableWaysForReward
{
    none,
    destroyPillar,
    destroyBridge,
    activateTotem,
    ragdollSmack
}

public class PointsManager : MonoBehaviour
{

    public GameObject pointBubblePrefab;
    public int numberOfPointsMessages = 40;

    PointsBubble[] pointsMessages = new PointsBubble[0];
    int bubbleIndex;
    List<EnemyController> RecentlyKilledEnemies = new List<EnemyController>();
    List<MomentFeature> UnqueuedMoments = new List<MomentFeature>();
    List<MomentFeature> QueuedMoments = new List<MomentFeature>();
    MomentFeature CurrentFeature
    {
        get
        {
            if (pointsMessages.Length == 0)
            {
                GenerateMessageBubbles();
            }
            return UnqueuedMoments[0];
        }
    }

    public UnityEngine.UI.Text scoreUIText;
    Color scoreStartColor;
    float colorLerpTracker;

    int totalPoints;
    public int TotalPoints
    {
        get { return totalPoints; }
        set
        {
            totalPoints = Mathf.Max(0, value);
            scoreUIText.text = totalPoints.ToString() + " POINTS";
            scoreUIText.color = Color.white;
            colorLerpTracker = 0;
        }
    }
    [SerializeField]
    float pauseBetweenMoments = 1.1f;
    float showMomentTimestamp, clearMomentAfter = 2, clearMomentTimeStamp;

    #region setup
    private void Start()
    {
        scoreStartColor = scoreUIText.color;
        TotalPoints = 0;
        if (scoreUIText == null)
        {
            Debug.LogWarning("Points UI text not assigned in inspector", gameObject);
        }
    }

    public void OnRestart()
    {
        totalPoints = 0;
        scoreUIText.text = totalPoints.ToString();
    }

    void GenerateMessageBubbles()
    {
        pointsMessages = new PointsBubble[numberOfPointsMessages];
        for (int i = 0; i < numberOfPointsMessages; i++)
        {
            pointsMessages[i] = Instantiate(pointBubblePrefab, transform.position, Quaternion.identity, transform).GetComponent<PointsBubble>();
            pointsMessages[i].transform.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
            UnqueuedMoments.Add(new MomentFeature());
        }
    }
    #endregion

    #region moment calls
    public void AwardPointsForCircumstance(AcceptableWaysForReward circumstance)
    {
        switch (circumstance)
        {
            case (AcceptableWaysForReward.activateTotem):
                CurrentFeature.pointValue = 500;
                CurrentFeature.description = "Pillar activated";
                break;
            case (AcceptableWaysForReward.destroyPillar):
                CurrentFeature.pointValue = 250;
                CurrentFeature.description = "Destroyed Pillar";
                break;
            case (AcceptableWaysForReward.destroyBridge):
                CurrentFeature.pointValue = 250;
                CurrentFeature.description = "Destroyed Bridge";
                break;
            case (AcceptableWaysForReward.ragdollSmack):
                CurrentFeature.pointValue = 20;
                CurrentFeature.description = "Pinata";
                break;
            default:
                return;
        }
        //add to queued messages
        if (CurrentFeature.pointValue == 0)
            return;

        QueuedMoments.Add(CurrentFeature);
        UnqueuedMoments.Remove(CurrentFeature);
    }

    public void AwardPointsForBodyKilled(Vector3 eventPosition, EnemyController corpse, AcceptibleWaysToDie circumstance)
    {
        if (corpse is EnemyController && !RecentlyKilledEnemies.Contains(corpse))
        {
            //t_feature is whatever kill info we want
            MomentFeature_AIState(circumstance, corpse.AIMentalState, corpse.lastDamageSource is CharacterBody ? corpse.lastDamageSource.bodyType : BodyType.None);
            RecentlyKilledEnemies.Add(corpse);

            //add to queued messages
            if (CurrentFeature.pointValue == 0)
                return;

            QueuedMoments.Add(CurrentFeature);
            UnqueuedMoments.Remove(CurrentFeature);
        }
    }

    public void RemovePointsForDeath(bool noBody)
    {
        if (noBody)
        {
            CurrentFeature.pointValue = -TotalPoints / 2;
            CurrentFeature.description = "ANNIHILATION";
        }
        else
        {
            CurrentFeature.pointValue = -250;
            CurrentFeature.description = "died in a body";
        }

        QueuedMoments.Add(CurrentFeature);
        UnqueuedMoments.Remove(CurrentFeature);
    }

    #endregion

    #region identifying moment info
    void MomentFeature_AIState(AcceptibleWaysToDie circumstance, AIStates aIStates, BodyType bodyType = BodyType.None)
    {
        if (circumstance == AcceptibleWaysToDie.killBox)
        {
            switch (aIStates)
            {
                case (AIStates.Stunned):
                    if (bodyType == BodyType.Player)
                    {
                        CurrentFeature.pointValue = 50;
                        CurrentFeature.description = "Depossession";
                    }
                    else
                    {
                        CurrentFeature.pointValue = 100;
                        CurrentFeature.description = "DUNK";
                    }
                    break;
                case (AIStates.Chasing):
                    CurrentFeature.pointValue = 50;
                    CurrentFeature.description = "Bait";
                    break;
            }
        }
        else
        {
            switch (aIStates)
            {
                case (AIStates.Stunned):
                    CurrentFeature.pointValue = 100;
                    CurrentFeature.description = "Kick 'em while they're down";
                    break;
                case (AIStates.Chasing):
                case (AIStates.Positioning):
                    CurrentFeature.pointValue = 50;
                    CurrentFeature.description = "Killing Blow";
                    break;
                case (AIStates.Attacking):
                case (AIStates.ImmediateAttack):
                    CurrentFeature.pointValue = 50;
                    CurrentFeature.description = "Parry";
                    break;
                case (AIStates.Following):
                    CurrentFeature.pointValue = 100;
                    CurrentFeature.description = "Bullying";
                    break;
                case (AIStates.Roaming):
                    CurrentFeature.pointValue = 80;
                    CurrentFeature.description = "Cold blooded murder";
                    break;
                default:
                    CurrentFeature.pointValue = 10;
                    CurrentFeature.description = "Something cool";
                    break;
            }
        }
    }
    #endregion

    #region putting the info on screen
    private void Update()
    {
        if (QueuedMoments.Count > 0 && Time.time > showMomentTimestamp + pauseBetweenMoments)
        {
            DisplayMoment(FormatMomentDescription());
            TotalPoints += QueuedMoments[0].pointValue;

            UnqueuedMoments.Add(QueuedMoments[0]);
            QueuedMoments.RemoveAt(0);
            showMomentTimestamp = Time.time;
        }

        if (RecentlyKilledEnemies.Count > 0 && Time.time > clearMomentTimeStamp + clearMomentAfter)
        {
            RecentlyKilledEnemies.RemoveAt(0);
            clearMomentTimeStamp = Time.time;
        }

        if (scoreUIText.color.b > scoreStartColor.b)
        {
            scoreUIText.color = Color.Lerp(Color.white, scoreStartColor, colorLerpTracker);
            colorLerpTracker += Time.deltaTime;
        }
    }

    string FormatMomentDescription()
    {
        return (QueuedMoments[0].pointValue > 0 ? "+" : "") + QueuedMoments[0].pointValue + "   " + QueuedMoments[0].description;
    }

    void DisplayMoment(string momentText)
    {
        if (pointsMessages.Length == 0)
        {
            GenerateMessageBubbles();
        }
        bubbleIndex = bubbleIndex % numberOfPointsMessages;
        pointsMessages[bubbleIndex].SetText(momentText);
        bubbleIndex++;
    }
    #endregion

    #region naming
    string FormatBodyTypeForDescription(BodyType type)
    {
        if (type == BodyType.None || type == BodyType.all)
        {
            return "fate";
        }
        else
        {
            return EnemyManager.CreatureName(type);
        }
    }
    #endregion
}

[System.Serializable]
public class MomentFeature
{
    public int pointValue;
    public string description;
}
