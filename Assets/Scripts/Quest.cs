using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Quest
{
    public enum QuestProgress
    {
        NotStarted,
        Started,
        Delivered
    }

    public QuestGiver QuestGivenBy, QuestDeliverTo;
    public Item item;
    public QuestProgress progress;

    public static Quest Generate()
    {
        var busyQuestGivers = QuestManager.Instance.ActiveQuests.Select(q => q.QuestGivenBy).
                                Concat(QuestManager.Instance.ActiveQuests.Select(q => q.QuestDeliverTo)).
                                Concat(QuestManager.Instance.AvailableQuests.Select(q => q.QuestGivenBy)).
                                Concat(QuestManager.Instance.AvailableQuests.Select(q => q.QuestDeliverTo)).ToList();
        var freeQuestGivers = QuestManager.Instance.QuestGivers.Except(busyQuestGivers).ToList();
        if (freeQuestGivers.Count < 2)
            return null;

        var qgFrom = Random.Range(0, freeQuestGivers.Count);
        var qgTo = (Random.Range(1, freeQuestGivers.Count) + qgFrom)% freeQuestGivers.Count;

        var q = new Quest()
        {
            QuestGivenBy = freeQuestGivers[qgFrom],
            QuestDeliverTo = freeQuestGivers[qgTo],
            item = GameObject.Instantiate(GameManager.Instance.ItemPrefabs[Random.Range(0, GameManager.Instance.ItemPrefabs.Count)]),
            progress = QuestProgress.NotStarted
        };

        q.item.gameObject.SetActive(false);
        return q;
    }
}
