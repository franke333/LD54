using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundObject : MonoBehaviour
{
    enum DecorType{
        Trees, Stones, Grass
    }

    public Vector2 offset, size;
    void Start()
    {
        float r = Random.Range(0, 6);
        DecorType decor = r < 4 ? DecorType.Grass : (r == 4 ? DecorType.Stones : DecorType.Trees);

        GameObject sprite;

        switch (decor)
        {
            case DecorType.Trees:
                sprite = BeautySalonManager.Instance.GenerateFromLayers(BeautySalonManager.Instance.backgroundLayersTrees);
                break;
            case DecorType.Stones:
                sprite = BeautySalonManager.Instance.GenerateFromLayers(BeautySalonManager.Instance.backgroundLayersStones);
                break;
            default:
                sprite = BeautySalonManager.Instance.GenerateFromLayers(BeautySalonManager.Instance.backgroundLayersGrass);
                break;
        }

        if(decor == DecorType.Stones)
        {
            var bc = GetComponent<BoxCollider2D>();
            bc.offset = offset;
            bc.size = size;
        }
        if(decor == DecorType.Grass)
            Destroy(GetComponent<BoxCollider2D>());

        sprite.transform.SetParent(this.transform);
        sprite.transform.localPosition = Vector3.zero;
    }
}
