using System.Collections.Generic;
using UnityEngine;

public class AfterImagePool : MonoBehaviour
{
    [SerializeField] private AfterImageGhost prefab;
    [SerializeField] private int prewarm = 24;

    private readonly Queue<AfterImageGhost> pool = new Queue<AfterImageGhost>();

    private void Awake()
    {
        for (int i = 0; i < prewarm; i++)
            CreateOne();
    }

    private AfterImageGhost CreateOne()
    {
        var g = Instantiate(prefab, transform);
        g.gameObject.SetActive(false);
        pool.Enqueue(g);
        return g;
    }

    public AfterImageGhost Get()
    {
        if (pool.Count == 0) CreateOne();
        return pool.Dequeue();
    }

    public void Release(AfterImageGhost ghost)
    {
        pool.Enqueue(ghost);
    }
}
