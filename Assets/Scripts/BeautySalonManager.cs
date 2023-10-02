using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeautySalonManager : SingletonClass<BeautySalonManager>
{
    [Serializable]
    public class TwinSprite
    {
        public Sprite solidSprite, addColorSprite;
    }

    [Serializable]
    public class SpriteLayer
    { 
        public string layerName;
        public int layerOrder;
        public List<Color> colorList;
        public List<TwinSprite> twinSprites;
    }

    public List<SpriteLayer> spriteLayers;

    public List<SpriteLayer> backgroundLayersStones, backgroundLayersTrees, backgroundLayersGrass;

    public GameObject GenerateFromLayers(List<SpriteLayer> layers)
    {
        GameObject npc = new GameObject("NPC");

        SpriteRenderer[] spriteRenderers = new SpriteRenderer[layers.Count*2];
        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                spriteRenderers[2 * i + j] = new GameObject(layers[i ].layerName + (j%2==1 ? " Colored" : "")).AddComponent<SpriteRenderer>();
                spriteRenderers[2 * i + j].transform.parent = npc.transform;
                spriteRenderers[2 * i + j].transform.localPosition = Vector3.zero;
                spriteRenderers[2 * i + j].transform.localRotation = Quaternion.identity;
                spriteRenderers[2 * i + j].transform.localScale = Vector3.one;
                spriteRenderers[2 * i + j].sortingOrder = layers[i].layerOrder + (1-j);

                spriteRenderers[2 * i + j].material = GameManager.Instance.LitMaterial;
            }

            var twinSprite = layers[i].twinSprites[UnityEngine.Random.Range(0, layers[i].twinSprites.Count)];
            spriteRenderers[2*i].sprite = twinSprite.solidSprite;
            spriteRenderers[2*i + 1].sprite = twinSprite.addColorSprite;
            spriteRenderers[2*i + 1].color = layers[i].colorList[UnityEngine.Random.Range(0, layers[i].colorList.Count)];

        }

        return npc;
    }
}
