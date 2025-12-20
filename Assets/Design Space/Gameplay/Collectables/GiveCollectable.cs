using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GiveCollectable : MonoBehaviour
{
    [SerializeField] private Collectable giveCollectable;
    [SerializeField] private int count;
    [SerializeField] private UnityEvent allCollectablesGiven;

    private List<Collectable> collectables;
    private int given;

    private void Awake()
    {
        collectables = new();

        given = 0;

        for (int i = 0; i < count; i++)
        {
            var collectable = Instantiate(giveCollectable, transform);
            collectable.gameObject.SetActive(false);
            
            collectables.Add(collectable);
        }
    }

    public void Give()
    {
        if (given >= count) return;

        collectables[given].Collect();
        
        given++;

        if (given >= count)
        {
            allCollectablesGiven.Invoke();
        }
    }
}
