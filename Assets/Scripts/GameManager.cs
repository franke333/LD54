using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GameManager : SingletonClass<GameManager>
{
    public readonly int pixelsPerUnit = 25;
    public readonly int unitsPerInventorySlot = 5;

    public Item HeldItem { get; set; }

    public List<Item> ItemPrefabs;

    public Item scythe, lantern;

    public bool IsHoldingItem => HeldItem != null;

    public Inventory MainInventory, PickUpInventory;
    public bool IsPickupInvOpen => PickUpInventory.gameObject.activeSelf;
    private Item _itemLockedToPickUp;
    public Item QuestItem { get => _itemLockedToPickUp; }

    public GameObject WorldGO;

    public GameObject WorldObjectPrefab;
    public GameObject NPCPrefab;

    [SerializeField]
    private int _NxNSpawnPoints;
    [SerializeField]
    float _spawnPointRadius;
    [SerializeField]
    private int _npcs;
    [SerializeField]
    private float _chanceOnWorldObject;

    public Light2D globalLight;

    public Material LitMaterial;


    public float TimeLeft = 240f;

    [Header("UI")]
    public GameObject MenuGO;
    public List<GameObject> EndGameGO;
    public TMP_Text pauseText, timeText, scoreText;

    public GameObject Intro;

    public bool Paused, Ended;
    public bool AllowInput => !Paused && !Ended;

    public int Score { get; set; }

    public void Start()
    {
        Score = 0;
        CreateWorld();

        var scytheGO = Instantiate(scythe);
        var lanternGO = Instantiate(lantern);

        scytheGO.gameObject.SetActive(true);
        lanternGO.gameObject.SetActive(true);

        MainInventory.AddItem(lanternGO, 5, 0);
        
        OpenPickUpInventory(null, false);
        PickUpInventory.AddItem(scytheGO, 0, 0);
    }

    public void OpenPickUpInventory(Item item,bool activateItem)
    {
        PickUpInventory.gameObject.SetActive(true);
        _itemLockedToPickUp = item;
        if (activateItem)
        {
            item.gameObject.SetActive(true);
            PickUpInventory.AddItem(item, 0, 0);
        }
    }

    public bool ClosePickUpInventory(out bool containsItem)
    {
        containsItem = false;
        if (!IsPickupInvOpen || IsHoldingItem)
            return false;

        if(PickUpInventory.Items.Count == 0)
        {
            Intro.SetActive(false);
            PickUpInventory.gameObject.SetActive(false);
            _itemLockedToPickUp = null;
            return true;
        }

        if(PickUpInventory.Items.Count == 1 && PickUpInventory.Items[0] == _itemLockedToPickUp)
        {
            containsItem = true;
            _itemLockedToPickUp.gameObject.SetActive(false);
            PickUpInventory.gameObject.SetActive(false);
            PickUpInventory.RemoveItem(_itemLockedToPickUp);
            //Destroy(_itemLockedToPickUp.gameObject);
            _itemLockedToPickUp = null;
            return true;
        }
        return false;
    }

    private void MoveItem()
    {
        if (HeldItem == null)
            return;
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var (itemWidth,itemHeight) = HeldItem.GetSizeInSlots();
        Vector3 offset = new Vector3(-itemWidth / 2f, itemHeight / 2f, 0) * unitsPerInventorySlot;
        HeldItem.transform.position = new Vector3(mousePos.x, mousePos.y, 0) + offset;
    }

    private bool PickUpItem()
    {
        if (IsHoldingItem)
            return false;
        if (Input.GetMouseButtonDown(0))
        {
            foreach (var inventory in new[] {MainInventory, PickUpInventory})
            {
                var item = inventory.GetItemAtMousePos();
                if (item == null)
                    continue;
                HeldItem = item;
                inventory.RemoveItem(item);
                HeldItem.GetComponentInChildren<SpriteRenderer>().sortingOrder = 10;
                return true;
            }
        }
        return false;
    }

    private bool PlaceItem()
    {
        if (!IsHoldingItem)
            return false;
        if (Input.GetMouseButtonDown(0))
        {
            var itemPos = HeldItem.transform.position + new Vector3(0.5f,-0.5f)*unitsPerInventorySlot;
            foreach (var inventory in new[] { MainInventory, PickUpInventory })
            {
                if(inventory.gameObject.activeSelf == false)
                    continue;
                var (row, column) = inventory.GetRowColumnFromCoords(itemPos);
                if (inventory.AddItem(HeldItem, row, column))
                {
                    HeldItem.GetComponentInChildren<SpriteRenderer>().sortingOrder = 5;
                    HeldItem = null;
                    return true;
                }
            }

        }
        return false;
    }

    private void ProcessInput() 
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if(!AllowInput)
            return;
        if (!PlaceItem())
            PickUpItem();
        MoveItem();


        
    }

    private void CreateWorld()
    {
        HashSet<Vector2Int> npcPositions = new HashSet<Vector2Int>();
        for (int i = 0; i < _npcs; i++)
        {
            var pos = new Vector2Int(Random.Range(0, _NxNSpawnPoints), Random.Range(0, _NxNSpawnPoints));
            while (npcPositions.Contains(pos))
            {
                pos = new Vector2Int(Random.Range(0, _NxNSpawnPoints), Random.Range(0, _NxNSpawnPoints));
            }
            npcPositions.Add(pos);
        }

        float squareRadius = (_spawnPointRadius*2.0f)/_NxNSpawnPoints;

        for (int x = 0; x < _NxNSpawnPoints; x++)
        {
            for (int y = 0; y < _NxNSpawnPoints; y++)
            {
                Vector2 pos = new Vector2(x*squareRadius - _spawnPointRadius, y * squareRadius - _spawnPointRadius);
                pos += new Vector2(Random.Range(-squareRadius/2.2f, squareRadius/2.2f), Random.Range(-squareRadius / 2.2f, squareRadius / 2.2f));
                if (npcPositions.Contains(new Vector2Int(x, y)))
                {
                    var qg = Instantiate(NPCPrefab, pos, Quaternion.identity);
                    var comp = qg.GetComponent<QuestGiver>();
                    QuestManager.Instance.QuestGivers.Add(comp);
                    qg.transform.SetParent(WorldGO.transform);
                }
                else if (Random.Range(0f, 1f) < _chanceOnWorldObject)
                {
                    var go = Instantiate(WorldObjectPrefab, pos, Quaternion.identity);
                    go.transform.SetParent(WorldGO.transform);
                }
                
            }
        }
    }

    private void Update()
    {
        
        globalLight.intensity = Mathf.Lerp(0.01f, 0.25f, (240f-TimeLeft) / 240f);
        ProcessInput();
        if(Input.GetKeyDown(KeyCode.P))
            if(!IsPickupInvOpen)
                OpenPickUpInventory(Instantiate(ItemPrefabs[Random.Range(0,ItemPrefabs.Count)]),true);
            else
                ClosePickUpInventory(out _);

        if (Paused || Ended)
            return;

        if (!Intro.activeSelf)
            TimeLeft -= Time.deltaTime;
        if (TimeLeft <= 0)
        {
            TimeLeft = 0;
            EndGame();
        }
        timeText.text = $"Time until sunrise: {((int)TimeLeft) / 60}:{(TimeLeft % 60).ToString("00.00")}";
    
    }

    // --- UI ---

    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void TogglePause()
    {
        if (Ended)
            return;
        Paused = !Paused;
        MenuGO.SetActive(Paused);
    }

    public void EndGame()
    {
        Ended = true;
        MenuGO.SetActive(true);
        foreach (var go in EndGameGO)
            go.SetActive(true);

        scoreText.text = $"Score: {GameManager.Instance.Score}";
        pauseText.text = $"Game over";
    }
}
