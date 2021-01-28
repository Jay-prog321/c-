using UnityEngine;

public class GameLevel : PersistableObject
{
    [SerializeField]
    SpawnZone spawnZone;
    [SerializeField]
    PersistableObject[] persistableObjects;
    public static GameLevel Current { get; private set; }
    private void OnEnable()
    {
        Current = this;
        if (persistableObjects == null) {
            persistableObjects = new PersistableObject[0];
        }
    }
    //public Vector3 SpawnPoint {
    //    get {
    //        return spawnZone.SpawnPoint;
    //    }
    //}
    public void ConfigureSpawn(CreatingShape shape) {
        spawnZone.ConfigureSpawn(shape);
    }
    public override void Save(GameDataWriter writer)
    {
        writer.Write(persistableObjects.Length);
        for (int i = 0; i < persistableObjects.Length; i++)
        {
            persistableObjects[i].Save(writer);
        }
    }
    public override void Load(GameDataReader reader)
    {
        int saveCount = reader.ReadInt();
        for (int i = 0; i < saveCount; i++)
        {
            persistableObjects[i].Load(reader);
        }
    }
}
