using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : SingletonClass<GameManager>
{
    public readonly int pixelsPerUnit = 25;
    public readonly int unitsPerInventorySlot = 5;

    public Item HeldItem { get; set; }

    public List<Item> ItemPrefabs;

    public bool IsHoldingItem => HeldItem != null;

    public Inventory MainInventory, PickUpInventory;
    private bool IsPickingUpItem => PickUpInventory.gameObject.activeSelf;
    private Item _itemLockedToPickUp;

    public void Start()
    {
        //TODO scythe?
        PickUpInventory.gameObject.SetActive(false);
    }

    public void OpenPickUpInventory(Item item)
    {
        PickUpInventory.gameObject.SetActive(true);
        _itemLockedToPickUp = item;
        PickUpInventory.AddItem(item, 0,0);
    }

    public bool ClosePickUpInventory(out bool itemPickedUp)
    {
        itemPickedUp = false;
        if (!IsPickingUpItem)
            return false;

        if(PickUpInventory.Items.Count == 0)
        {
            itemPickedUp = _itemLockedToPickUp != null;
            PickUpInventory.gameObject.SetActive(false);
            _itemLockedToPickUp = null;
            return true;
        }

        if(PickUpInventory.Items.Count == 1 && PickUpInventory.Items[0] == _itemLockedToPickUp)
        {
            PickUpInventory.gameObject.SetActive(false);
            PickUpInventory.RemoveItem(_itemLockedToPickUp);
            Destroy(_itemLockedToPickUp.gameObject);
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
        if (!PlaceItem())
            PickUpItem();
        MoveItem();
        
    }

    

    private void Update()
    {
        ProcessInput();
        if(Input.GetKeyDown(KeyCode.P))
            if(!IsPickingUpItem)
                OpenPickUpInventory(Instantiate(ItemPrefabs[Random.Range(0,ItemPrefabs.Count)]));
            else
                ClosePickUpInventory(out _);
    }
}
