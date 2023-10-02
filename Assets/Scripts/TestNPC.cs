using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNPC : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       var sprites =  BeautySalonManager.Instance.GenerateFromLayers(BeautySalonManager.Instance.spriteLayers);

        sprites.transform.SetParent(this.transform);
        sprites.transform.localPosition = Vector3.zero;
    }

}
