using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PLib.Pooling;

public class SceneTest : MonoBehaviour {

    public bool createPrefab = false;
    public bool getItems = false;
    public bool putItems = false;
    public bool expirePool = false;
    public bool prewarm = false;
    public int count = 5;

    private GameObject prefab;
    List<GameObject> list;

    private void Start()
    {
        Debug.Log("Ready");
    } 

    private void Update()
    {
        if(createPrefab)
        {
            createPrefab = false;
            prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefab.name = "prefab";
            PPool.SetLimit(prefab, staleDuration: 2);
        }

        if (getItems)
        {
            getItems = false;
            list = new List<GameObject>();
            for (int i = 0; i < count; i++) list.Add(PPool.Get(prefab));
        }

        if (putItems)
        {
            putItems = false;
            for (int i = 0; i < count; i++) PPool.Put(list[i]);
            list.Clear();
        }

        if (expirePool)
        {
            expirePool = false;
            PPool.Expire(prefab);
        }

        if (prewarm)
        {
            prewarm = false;
            PPool.Prewarm(prefab, count, 4);
        }
    }
}
