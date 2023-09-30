using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private int _unitsPerInventorySlot = 1;

    public int rows,columns;

    private List<Item> items;
    private Item outOfInventoryItem;
    private Dictionary<Item, (int,int)> itemPositions;

    [SerializeField]
    private GameObject _inventoryTopLeft;

    bool[,] slots;

    private void PrepareInventory()
    {
        slots = new bool[rows,columns];
        items = new();
        itemPositions = new();
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

    private void OnDrawGizmos()
    {
        if (slots == null)
            return;
        
        for (int column = 0; column < columns; column++)
        {
            for (int row = 0; row < rows; row++)
            {
                if (slots[row, column])
                    continue;

                if (GameManager.Instance.IsHoldingItem) {
                    var heldItem = GameManager.Instance.HeldItem;
                    var (x, y) = GetRowColumnFromCoords(heldItem.transform.position + new Vector3(0.5f, -0.5f) * _unitsPerInventorySlot);
                    if (heldItem.IsSpotOccupied(row-x,column-y))
                        Gizmos.color = Color.yellow;
                    else
                        Gizmos.color = Color.magenta;
                }
                else
                    Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(_inventoryTopLeft.transform.position + new Vector3(column, -row , 0) * _unitsPerInventorySlot, _unitsPerInventorySlot / 2f);
            }
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
    }

    private void Start()
    {
        _unitsPerInventorySlot = GameManager.Instance.unitsPerInventorySlot;

        PrepareInventory();
    }
}
