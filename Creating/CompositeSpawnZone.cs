using UnityEngine;

public class CompositeSpawnZone : SpawnZone
{
    [SerializeField]
    SpawnZone[] spawnZones;
    [SerializeField]
    bool sequential;
    [SerializeField]
    bool overrideConfig;
    int nextSeuqentialIndex;
    public override Vector3 SpawnPoint {
        get {
            int index;
            if (sequential)
            {
                index = nextSeuqentialIndex++;
                if (nextSeuqentialIndex >= spawnZones.Length) {
                    nextSeuqentialIndex = 0;
                }
            }
            else {
                index = Random.Range(0, spawnZones.Length);
            }           
            return spawnZones[index].SpawnPoint;
        }
    }
    public override void ConfigureSpawn(CreatingShape shape)
    {
        if (overrideConfig)
        {
            base.ConfigureSpawn(shape);
        }
        else
        {
            int index;
            if (sequential)
            {
                index = nextSeuqentialIndex++;
                if (nextSeuqentialIndex >= spawnZones.Length)
                {
                    nextSeuqentialIndex = 0;
                }
            }
            else
            {
                index = Random.Range(0, spawnZones.Length);
            }
            spawnZones[index].ConfigureSpawn(shape);
        }
    }
}
