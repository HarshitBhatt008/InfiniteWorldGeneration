using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkItem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "chunkitem")
        {
            Destroy(collision.gameObject);
        }
    }
}
