using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundObject : MonoBehaviour
{
    public Vector2 offset, size;
    void Start()
    {
        bool stones = Random.Range(0f, 1f) < 0.5f;
        var sprites = !stones ?
            BeautySalonManager.Instance.GenerateFromLayers(BeautySalonManager.Instance.backgroundLayersTrees) :
            BeautySalonManager.Instance.GenerateFromLayers(BeautySalonManager.Instance.backgroundLayersStones);

        if(stones)
        {
            var bc = GetComponent<BoxCollider2D>();
            bc.offset = offset;
            bc.size = size;
        }

        sprites.transform.SetParent(this.transform);
        sprites.transform.localPosition = Vector3.zero;
    }
}
