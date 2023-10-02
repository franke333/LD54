using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSpriteOrder : MonoBehaviour
{
    List<SpriteRenderer> renderers;
    List<int> originalOrders;
    
    private bool loaded = false;

    private void Update()
    {
        if (!loaded)
        {
            renderers = new List<SpriteRenderer>();
            originalOrders = new List<int>();
            foreach (var r in GetComponentsInChildren<SpriteRenderer>())
            {
                renderers.Add(r);
                originalOrders.Add(r.sortingOrder);
            }
            loaded = true;
        }
        for (int i = 0; i < renderers.Count; i++)
            if (renderers[i].transform.position.y < PlayerScript.Instance.transform.position.y)
                renderers[i].sortingOrder = originalOrders[i] + 20;
            else
                    renderers[i].sortingOrder = originalOrders[i] - 20;


    }

}
