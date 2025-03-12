using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Leveled/Editor Content/Collectable Registry", order = 100)]
public class CollectableRegistry : GameService
{
    private List<Collectable> collectables;

    public event Action Collected;
    public event Action CountChanged;

    public IEnumerable<Collectable> Collectables => collectables;

    public int TotalCount => collectables.Count;
    public int CollectedCount => collectables.Count(collectable => collectable.IsCollected);
    public int UncollectedCount => collectables.Count - CollectedCount;

    public bool AllCollected => collectables.All(collectable => collectable.IsCollected);
    
    public void Add(Collectable collectable)
    {
        collectables.Add(collectable);
        collectable.Collected += OnCollectableCollected;
        CountChanged?.Invoke();
    }

    public void Remove(Collectable collectable)
    {
        collectables.Remove(collectable);
        collectable.Collected -= OnCollectableCollected;
        CountChanged?.Invoke();
    }

    private void OnCollectableCollected()
    {
        Collected?.Invoke();
    }

    protected override void Initialize()
    {
        collectables = new();
    }
}
