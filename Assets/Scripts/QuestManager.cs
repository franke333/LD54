using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class QuestManager : SingletonClass<QuestManager>
{
    public List<Quest> ActiveQuests = new List<Quest>();
    public List<Quest> AvailableQuests = new List<Quest>();
    public List<QuestGiver> QuestGivers = new List<QuestGiver>();

    public UnityEvent E_ReceiveQuest = new UnityEvent(), E_CompleteQuest = new UnityEvent();

    public Sprite QuestIndicatorSprite;
    public List<Sprite> ItemSizeSprite;


    public Gradient gradientPickUp, gradientDeliver;


    //top left and then clockwise
    public GameObject[] IndicatorCorners;
    public Vector3 IndicatorOffset;
    public Dictionary<Quest, GameObject> questIndicatorDict = new Dictionary<Quest, GameObject>();


    public Color PickUpColor, DeliverColor;


    private bool IsRightTurn(Vector2 a, Vector2 b, Vector2 c)
    {
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) < 0;
    }

    public void GenerateQuestIndicator(Quest quest)
    {
        var indicator = new GameObject();
        var srCircle = indicator.AddComponent<SpriteRenderer>();
        srCircle.sortingOrder = -4;
        var goItem = new GameObject();
        goItem.transform.SetParent(indicator.transform);
        var srItem = goItem.AddComponent<SpriteRenderer>();
        srItem.sortingOrder = -3;
        indicator.transform.SetParent(transform);
        indicator.transform.localPosition = Vector3.zero;
        indicator.transform.localScale = Vector3.one;
        indicator.transform.localRotation = Quaternion.identity;

        int size = quest.item.OccupiedSpotsBeforeTransform().Count;
        if (size < 4)
            size = 0;
        else 
            size = size < 7 ? 1 : 2;

        srItem.sprite = ItemSizeSprite[size];
        srCircle.sprite = QuestIndicatorSprite;
        srCircle.color = PickUpColor;

        questIndicatorDict.Add(quest, indicator);
    }

    private float MapRange(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        // Calculate the ratio of how far 'value' is between 'fromMin' and 'fromMax'
        float fromRange = fromMax - fromMin;
        float toRange = toMax - toMin;

        // Make sure not to divide by zero
        if (fromRange == 0)
        {
            throw new ArgumentException("Input range 'fromMin' and 'fromMax' cannot be the same");
        }

        // Calculate the scaled value
        float scaledValue = (value - fromMin) / fromRange;

        // Map the scaled value to the new range
        float mappedValue = toMin + (scaledValue * toRange);

        // Make sure the result is within the specified 'toMin' and 'toMax' range
        return Mathf.Min(Mathf.Max(mappedValue, toMin), toMax);
    }

    public void UpdateIndicatorTransformAndColor(Quest quest)
    {
        Vector2 tl, tr, dr, dl;
        tl = IndicatorCorners[0].transform.position;
        tr = IndicatorCorners[1].transform.position;
        dr = IndicatorCorners[2].transform.position;
        dl = IndicatorCorners[3].transform.position;

        Vector3 qp = quest.progress == Quest.QuestProgress.NotStarted ? quest.QuestGivenBy.transform.position : quest.QuestDeliverTo.transform.position;

        if(IsRightTurn(tl,tr,qp) && IsRightTurn(tr,dr,qp) && IsRightTurn(dr,dl,qp) && IsRightTurn(dl,tl,qp))
        {
            questIndicatorDict[quest].transform.position = new Vector3(qp.x,qp.y,0) + IndicatorOffset;
            questIndicatorDict[quest].transform.rotation = Quaternion.Euler(0,0,-45f);
            questIndicatorDict[quest].transform.GetChild(0).rotation = Quaternion.identity;
            return;
        }

        //check quadrants

        if (IsRightTurn(dr, tl, qp))
        {
            //Top right
            if (IsRightTurn(dl, tr, qp))
            {
                //rigt
                float d = qp.x - dr.x;
                float newY = MapRange(qp.y, dr.y - d, tr.y+d, dr.y, tr.y);
                questIndicatorDict[quest].transform.position = new Vector3(dr.x, newY, 0);
            }
            else
            {
                //top
                float d = qp.y - tl.y;
                float newX = MapRange(qp.x, tl.x - d, tr.x + d, tl.x, tr.x);
                questIndicatorDict[quest].transform.position = new Vector3(newX, tl.y, 0);
            }
        }
        else
        {
            //Down left
            if (IsRightTurn(dl, tr, qp))
            {
                //down
                float d = dl.y - qp.y;
                float newX = MapRange(qp.x, dl.x - d, dr.x + d, dl.x, dr.x);
                questIndicatorDict[quest].transform.position = new Vector3(newX, dl.y, 0);
            }
            else
            {
                //left
                float d = tl.x - qp.x;
                float newY = MapRange(qp.y, dl.y - d, tl.y + d, dl.y, tl.y);
                questIndicatorDict[quest].transform.position = new Vector3(tl.x, newY, 0);
                
            }
        }

        //TODO: rotate
        questIndicatorDict[quest].transform.right = -qp + questIndicatorDict[quest].transform.position;
        questIndicatorDict[quest].transform.Rotate(0,0,45f);
        questIndicatorDict[quest].transform.GetChild(0).rotation = Quaternion.identity;

        float t = Vector2.Distance(qp, PlayerScript.Instance.transform.position);
        t = (Mathf.Clamp(t, 40f, 400f) - 40f)/(400f-40f);

        questIndicatorDict[quest].GetComponent<SpriteRenderer>().color = 
            (quest.progress == Quest.QuestProgress.NotStarted ? gradientPickUp : gradientDeliver).Evaluate(t);
    }

    public void UpdateIndicatorStatus(Quest quest)
    {
        if(quest.progress == Quest.QuestProgress.Delivered)
        {
            var go = questIndicatorDict[quest];
            questIndicatorDict.Remove(quest);
            Destroy(go.gameObject);
            return;
        }

        var srs = questIndicatorDict[quest].GetComponents<SpriteRenderer>();
        var srIndicator = srs.Where(srs => srs.sortingOrder == -4).First();
        srIndicator.color = DeliverColor;
        
    }

    public int CalculateScore(Quest quest)
    {
        var item = quest.item;

        var distance = Vector2.Distance(quest.QuestGivenBy.transform.position, quest.QuestDeliverTo.transform.position);
        var distanceMult = distance / 120f;
        distanceMult = Mathf.Clamp(distanceMult, 1f, 3f);

        var score = (int)(distanceMult * (3 + item.columns * item.rows + item.OccupiedSpotsBeforeTransform().Count()));
        Debug.Log($"Score given for quest: {score} (distance multiplier was {distanceMult})");

        return score;
    }

    public void CompleteQuest(Quest quest)
    {
        ActiveQuests.Remove(quest);
        GameManager.Instance.Score += CalculateScore(quest);
        quest.progress = Quest.QuestProgress.Delivered;
        UpdateIndicatorStatus(quest);
        quest.QuestGivenBy.quest = null;
        quest.QuestDeliverTo.quest = null;
        Destroy(quest.item.gameObject);
        E_CompleteQuest.Invoke();
    }

    public void StartQuest(Quest quest)
    {
        AvailableQuests.Remove(quest);

        quest.QuestDeliverTo.quest = quest;
        quest.progress = Quest.QuestProgress.Started;

        ActiveQuests.Add(quest);

        UpdateIndicatorStatus(quest);

        E_ReceiveQuest.Invoke();
    }

    public void LoadQuestGivers()
    {
        QuestGivers = FindObjectsOfType<QuestGiver>().ToList();
        Debug.Log($"Loaded {QuestGivers.Count} quest givers");
    }

    private void Start()
    {
        LoadQuestGivers();
    }

    public void Update()
    {


        if (AvailableQuests.Count < 3)
        {
            var q = Quest.Generate();
            if (q != null)
            {
                GenerateQuestIndicator(q);
                q.QuestGivenBy.quest = q;
                q.QuestDeliverTo.quest = q;
                AvailableQuests.Add(q);
            }
        }
        foreach (var quest in ActiveQuests)
        {
            UpdateIndicatorTransformAndColor(quest);
        }
        foreach (var quest in AvailableQuests)
        {
            UpdateIndicatorTransformAndColor(quest);
        }
    }




}
