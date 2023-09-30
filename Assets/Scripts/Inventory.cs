using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
    private int _unitsPerInventorySlot = 1;

    public int rows,columns;

    private List<Item> items;

    public IReadOnlyList<Item> Items => items;

    private Item outOfInventoryItem;
    private Dictionary<Item, (int,int)> itemPositions;

    UnityEvent<Item> E_ItemAdded = new ();
    UnityEvent<Item> E_ItemRemoved = new UnityEvent<Item>();

    [SerializeField]
    private GameObject _inventoryTopLeft;
    private GameObject _highlightsGO;

    [SerializeField]
    private Sprite _highlightSprite;
    [SerializeField]
    private Color _highlightColorGood, _highlightColorBad;
    private SpriteRenderer[,] _highlightsSprites;

    bool[,] slots;

    private void PrepareInventory()
    {
        slots = new bool[rows,columns];
        items = new();
        itemPositions = new();

        //highlights
        _highlightsGO = new GameObject("highlights");
        _highlightsGO.transform.parent = transform;
        _highlightsSprites = new SpriteRenderer[rows, columns];
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                var _highlight = new GameObject($"hl ({y},{x})");
                _highlight.transform.parent = _highlightsGO.transform;
                var sr = _highlight.AddComponent<SpriteRenderer>();
                sr.sprite = _highlightSprite;
                _highlight.transform.position = _inventoryTopLeft.transform.position + new Vector3(x, -y, 0) * _unitsPerInventorySlot;
                sr.renderingLayerMask = 1;
                _highlight.transform.localScale= new Vector3(4.601659f, 4.5317f, 1);
                _highlight.SetActive(false);
                _highlightsSprites[y, x] = sr;
            }
        }
    }


    public (int,int) GetRowColumnFromCoords(Vector2 coords)
    {
        var pos = coords - (Vector2)_inventoryTopLeft.transform.position  +new Vector2(1,-1)*0.5f*_unitsPerInventorySlot;
        var row = Mathf.FloorToInt(-pos.y / _unitsPerInventorySlot);
        var column = Mathf.FloorToInt(pos.x / _unitsPerInventorySlot);
        return (row, column);
    }

    public bool CanBePlaced(Item item, int row, int column)
    {
        if (row < 0 || row >= rows || column < 0 || column >= columns)
            return false;
        var spots = item.OccupiedSpotsAfterTransform();
        var maxRow = spots.Max((x) => x.Item1);
        var maxColumn = spots.Max((x) => x.Item2);
        if (row + maxRow >= rows || column + maxColumn >= columns)
            return false;
        foreach (var spot in spots)
            if (slots[row + spot.Item1, column + spot.Item2])
                return false;
        return true;
    }

    private void DrawHighlights()
    {
        if(!gameObject.activeSelf)
            return;

        if (slots == null)
            return;

        if (GameManager.Instance.HeldItem == null)
        {
            _highlightsGO.SetActive(false);
            return;
        }
        _highlightsGO.SetActive(true);

        var (x0, y0) = GetRowColumnFromCoords(GameManager.Instance.HeldItem.transform.position + new Vector3(0.5f, -0.5f) * _unitsPerInventorySlot);

        bool canBePlaced = CanBePlaced(GameManager.Instance.HeldItem,x0,y0);

        for (int x = 0; x < rows; x++)
            for (int y = 0; y < columns; y++)
                _highlightsSprites[x, y].gameObject.SetActive(false);

        foreach (var (x1,y1) in GameManager.Instance.HeldItem.OccupiedSpotsAfterTransform())
        {
            var x = x0 + x1;
            var y = y0 + y1;
            if (x < 0 || x >= rows || y < 0 || y >= columns)
                continue;
            _highlightsSprites[x, y].color = canBePlaced ? _highlightColorGood : _highlightColorBad;
            _highlightsSprites[x,y].gameObject.SetActive(true);
        }

    }

    public Item GetItemAtMousePos()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var (row, column) = GetRowColumnFromCoords(mousePos);
        return GetItem(row, column);
    }

    public Item GetItem(int x,int y)
    {
        if(x < 0 || x >= rows || y < 0 || y >= columns)
            return null;
        if (!slots[x,y])
            return null;
        foreach (var item in items)
        {
            var (x1, y1) = itemPositions[item];
            foreach (var (x2, y2) in item.OccupiedSpotsAfterTransform())
                if (x1 + x2 == x && y1 + y2 == y)
                {
                    Debug.Log($"Found item at {x},{y}");
                    return item;
                }
        }
        return null;
    }

    public bool AddItem(Item item, int x, int y)
    {
        if(!CanBePlaced(item, x, y))
            return false;
        Debug.Log($"Placing item at {x},{y}");

        item.transform.position = _inventoryTopLeft.transform.position + (new Vector3(y, -x, 0) + new Vector3(-0.5f,0.5f,0)) * _unitsPerInventorySlot;

        items.Add(item);
        itemPositions.Add(item, (x, y));

        var spots = item.OccupiedSpotsAfterTransform();
        foreach (var spot in spots)
            slots[x + spot.Item1, y + spot.Item2] = true;
        item.Locked = true;

        E_ItemAdded.Invoke(item);

        return true;
    }

    public void RemoveItem(Item item)
    {
        if (!items.Contains(item))
        {
            Debug.LogError("Item not in inventory");
            return;
        }
        items.Remove(item);

        var (x, y) = itemPositions[item];
        var spots = item.OccupiedSpotsAfterTransform();
        foreach (var spot in spots)
            slots[x + spot.Item1, y + spot.Item2] = false;
        item.Locked = false;
        itemPositions.Remove(item);

        E_ItemRemoved.Invoke(item);
    }

    private void Start()
    {
        _unitsPerInventorySlot = GameManager.Instance.unitsPerInventorySlot;

        PrepareInventory();
    }

    private void Update()
    {
        DrawHighlights();
    }
}
