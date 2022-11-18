using System;
using System.Collections.Generic;
using UnityEngine;

namespace SDG.Unturned;

public class PackInfo
{
    private Vector3 wanderNormal;

    private float _wanderAngle;

    private Vector3? avgSpawnPoint;

    private int avgAnimalPointRecalculation;

    private Vector3 avgAnimalPoint;

    public List<AnimalSpawnpoint> spawns { get; private set; }

    public List<Animal> animals { get; private set; }

    public float wanderAngle
    {
        get
        {
            return _wanderAngle;
        }
        set
        {
            _wanderAngle = value;
            wanderNormal = new Vector3(Mathf.Cos((float)Math.PI / 180f * wanderAngle), 0f, Mathf.Sin((float)Math.PI / 180f * wanderAngle));
        }
    }

    public Vector3 getWanderDirection()
    {
        return wanderNormal;
    }

    public Vector3 getAverageSpawnPoint()
    {
        if (!avgSpawnPoint.HasValue)
        {
            avgSpawnPoint = Vector3.zero;
            for (int i = 0; i < spawns.Count; i++)
            {
                AnimalSpawnpoint animalSpawnpoint = spawns[i];
                if (animalSpawnpoint != null)
                {
                    avgSpawnPoint += animalSpawnpoint.point;
                }
            }
            avgSpawnPoint /= spawns.Count;
        }
        return avgSpawnPoint.Value;
    }

    public Vector3 getAverageAnimalPoint()
    {
        if (Time.frameCount > avgAnimalPointRecalculation)
        {
            avgAnimalPoint = Vector3.zero;
            for (int i = 0; i < animals.Count; i++)
            {
                Animal animal = animals[i];
                if (!(animal == null))
                {
                    avgAnimalPoint += animal.transform.position;
                }
            }
            avgAnimalPoint /= (float)animals.Count;
            avgAnimalPointRecalculation = Time.frameCount;
        }
        return avgAnimalPoint;
    }

    public PackInfo()
    {
        spawns = new List<AnimalSpawnpoint>();
        animals = new List<Animal>();
        wanderAngle = UnityEngine.Random.Range(0f, 360f);
    }
}
