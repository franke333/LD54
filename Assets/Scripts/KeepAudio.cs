using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepAudio : MonoBehaviour
{
    public float Volume = -1;
    // Start is called before the first frame update
    void Start()
    {
        var arr = GameObject.FindObjectsOfType<KeepAudio>();
        if (arr.Length > 1)
        {
            
            AudioManager.Instance.Volume = arr[0].Volume == -1 ? arr[1].Volume : Volume;
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }

    private void Update()
    {
        Volume = AudioManager.Instance.Volume;
    }
}
