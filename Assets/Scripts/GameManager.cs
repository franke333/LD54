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

    public void Start()
    {
        OpenPickUpInventory();
    }

    public void OpenPickUpInventory()
    {
        PickUpInventory.gameObject.SetActive(true);
        Item item = Instantiate(ItemPrefabs[Random.Range(0,ItemPrefabs.Count)]);
        PickUpInventory.AddItem(item, 0,0);
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
            OpenPickUpInventory();
    }
}
