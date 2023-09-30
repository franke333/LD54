using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Item : MonoBehaviour
{
    [Multiline(5)]
    public string shape;
    public string itemName;
    //rows, cols
    public int rows,columns;

    private bool[,] _bitMask;
    private bool[,] BitMask {
        get
        {
            if (_bitMask == null)
                LoadBitMask();
            return _bitMask;
        }
    }

    public int rotated = 0;
    public bool flipped = false;

    private int _unitsPerInventorySlot = 1;
    private int _pixelsPerUnit = 1;

    private SpriteRenderer _sr;

    public bool Locked = false;

    public bool IsSpotOccupied(int x,int y) {
        
        var (xx, yy) = InverseTransformCoords(x, y);
        if (xx < 0 || xx >= rows || yy < 0 || yy >= columns)
            return false;
        return BitMask[xx, yy];
    }

    public List<(int,int)> OccupiedSpotsBeforeTransform()
    {
        List<(int, int)> spots = new List<(int, int)>();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                if (BitMask[i, j])
                    spots.Add((i, j));
            }
        }
        return spots;
    }

    public List<(int, int)> OccupiedSpotsAfterTransform()
    {
        return OccupiedSpotsBeforeTransform().Select((x) => TransformCoords(x.Item1, x.Item2)).ToList();
    }
    
    private (int,int) InverseTransformCoords(int x,int y)
    {
        if (flipped)
        {
            if(rotated%2==1)
                x = columns - x - 1;
            else
                y = columns - y - 1;
        }
        switch (rotated)
        {
            case 0:
                return (x, y);
            case 1:
                return (rows - y - 1, x);
            case 2:
                return (rows - x - 1, columns - y - 1);
            case 3:
                return (y, columns - x - 1);
        }

        throw new System.Exception("Invalid rotation");
    }

    private (int,int) TransformCoords(int x, int y)
    {
        if(flipped)
            y = columns - y - 1;
        switch (rotated)
        {
            case 0:
                return (x, y);
            case 1:
                return (y, rows - x - 1);
            case 2:
                return (rows - x - 1, columns - y - 1);
            case 3:
                return (columns - y - 1, x);
        }
        throw new System.Exception("Invalid rotation");
    }


    private void GetItemSize()
    {
        rows = 0;
        columns = 0;
        while (shape[columns] != '\n')
        {
            columns++;
        }
        rows = shape.Where(c => c=='X' || c=='O').Count() / columns;
        Debug.Log($"Shape of {itemName} is ({columns};{rows})");
    }

    private void CorrectSpriteTransform()
    {

        Vector3 center = new Vector3(columns / 2f, -rows / 2f, 0) * _unitsPerInventorySlot;
        if(rotated%2==1)
            center = new Vector3(rows / 2f, -columns / 2f, 0) * _unitsPerInventorySlot;
        _sr.transform.localPosition = center;
        _sr.flipX = flipped;
        _sr.transform.rotation = Quaternion.Euler(0, 0, -rotated * 90);
    }

    private void LoadBitMask()
    {
        if(_bitMask!=null)
            return;
        GetItemSize();
        _bitMask = new bool[rows, columns];
        int x = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                BitMask[i, j] = shape[x++] == 'X' ? true : false;
            }
            x++;
        }
    }

    private void OnDrawGizmos()
    {
        if (BitMask == null)
            return;

        Gizmos.color = Color.red;
        foreach (var (r, c) in OccupiedSpotsAfterTransform())
        {
            Gizmos.DrawWireCube(transform.position + new Vector3(c+0.5f, -r -0.5f, 0) * _unitsPerInventorySlot, Vector3.one * _unitsPerInventorySlot);
        }
    }

    private void Awake()
    {
       LoadBitMask();

        _sr = GetComponentInChildren<SpriteRenderer>();
        
        _pixelsPerUnit = GameManager.Instance.pixelsPerUnit;
        _unitsPerInventorySlot = GameManager.Instance.unitsPerInventorySlot;

        CorrectSpriteTransform();
    }

    public void Rotate()
    {
        if(!Locked)
            rotated = (rotated + 1) % 4;

        CorrectSpriteTransform();
    }

    public void Flip()
    {
        if (Locked)
            return;
        flipped = !flipped;
        if(rotated%2 == 1)
            rotated = (rotated + 2) % 4;

        CorrectSpriteTransform();
    }

    /// <summary>
    /// After Transform
    /// </summary>
    /// <returns>(width,height)</returns>
    public (int,int) GetSizeInSlots()
    {
        if (rotated % 2 == 0)
            return (columns, rows);
        else
            return (rows, columns);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Rotate();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            Flip();
        }
    }





}
