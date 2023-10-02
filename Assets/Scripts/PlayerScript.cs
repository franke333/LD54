using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerScript : SingletonClass<PlayerScript>
{
    public GameObject World;

    private Rigidbody2D _worldRB;

    public float Speed = 1f;
    public float magnitude = 1f;
    public float InteractRange = 5f;

    [SerializeField]
    float _animSpeed = 1f;
    public GameObject spriteScythe,spriteLantern;

    private Vector3 lightOffset;
    public GameObject[] lights;
    List<SpriteRenderer> spriteRenderers;

    private void Start()
    {
        _worldRB = World.GetComponent<Rigidbody2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList();
        lightOffset = lights[0].transform.localPosition;
    }

    private void Animate()
    {
        float p = Mathf.PerlinNoise(Time.time * _animSpeed, Time.time * _animSpeed) * Mathf.PI * 2;
        Vector2 offset = new Vector2(Mathf.Cos(p),Mathf.Sin(p))* magnitude;
        offset -= Vector2.one * 0.1f;
        spriteScythe.transform.localPosition = offset;
        spriteLantern.transform.localPosition = offset;
    }

    private void Interact()
    {
        if (!Input.GetKeyDown(KeyCode.Space))
            return;

        var closestQG = QuestManager.Instance.QuestGivers
            .Where(qg => Vector2.Distance(qg.transform.position, transform.position) < InteractRange)
            .OrderBy(qg => Vector2.Distance(qg.transform.position, transform.position))
            .FirstOrDefault();
        if (closestQG == null)
        {
            if (GameManager.Instance.IsPickupInvOpen)
                GameManager.Instance.ClosePickUpInventory(out bool _);
            return;
        }


        if (GameManager.Instance.IsPickupInvOpen)
        {
            var result = GameManager.Instance.ClosePickUpInventory(out bool containsItem);
            if (!result)
                return;

            if(closestQG.quest == null)
            {
                return;
            }

            if (containsItem)
            {
                if(closestQG.quest.QuestDeliverTo == closestQG)
                {
                    QuestManager.Instance.CompleteQuest(closestQG.quest);
                }
            }
            else // no item in pickup inventory
            {
                if (closestQG.quest.QuestGivenBy == closestQG)
                {
                    QuestManager.Instance.StartQuest(closestQG.quest);
                }
            }

            return;
        }

        if (closestQG.quest == null)
        {
            GameManager.Instance.OpenPickUpInventory(null, false);
            return;
        }

        if(closestQG.quest.QuestGivenBy == closestQG)
        {
            if (closestQG.quest.progress == Quest.QuestProgress.NotStarted)
            {
                GameManager.Instance.OpenPickUpInventory(closestQG.quest.item, true);
                return;
            }
            else if (closestQG.quest.progress == Quest.QuestProgress.Started)
            {
                GameManager.Instance.OpenPickUpInventory(null, false);
                return;
            }
        }
        else //deliver to
        {
            if (closestQG.quest.progress == Quest.QuestProgress.NotStarted)
            {
                GameManager.Instance.OpenPickUpInventory(null, false);
                return;
            }
            else if (closestQG.quest.progress == Quest.QuestProgress.Started)
            {
                GameManager.Instance.OpenPickUpInventory(closestQG.quest.item, false);
                return;
            }
        }

    }

    private void ProcessInput()
    {
        AudioManager.Instance.Walking = false;
        _worldRB.velocity = Vector2.zero;
        if (!GameManager.Instance.AllowInput)
            return;

        Interact();

        if (GameManager.Instance.PickUpInventory.gameObject.activeSelf)
            return;

        Vector2 direction = Vector2.zero;

        if(Input.GetKey(KeyCode.W))
            direction += Vector2.up;
        if(Input.GetKey(KeyCode.S))
            direction += Vector2.down;
        if(Input.GetKey(KeyCode.A))
            direction += Vector2.left;
        if(Input.GetKey(KeyCode.D))
            direction += Vector2.right;

        if(direction.x < 0)
            foreach (var spriteRenderer in spriteRenderers)
                spriteRenderer.flipX = false;
        else if(direction.x > 0)
            foreach (var spriteRenderer in spriteRenderers)
                spriteRenderer.flipX = true;

        foreach (var light in lights)
            if(direction.x < 0)
                light.transform.localPosition = lightOffset;
            else if(direction.x > 0)
                light.transform.localPosition = new(-lightOffset.x,lightOffset.y);

       
        AudioManager.Instance.Walking = direction.magnitude > 0;
        _worldRB.velocity = -direction.normalized * Speed;
    }

    private void Update()
    {
        ProcessInput();
        Animate();
    }
}
