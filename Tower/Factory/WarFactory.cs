using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu]
public class WarFactory : GameObjectFactory
{
    [SerializeField]
    public Shell shellPrefab = default;
    [SerializeField]
    public Explosion explosionPrefab = default;
    public Shell Shell => Get(shellPrefab);
    public Explosion Explosion => Get(explosionPrefab);

    T Get<T>(T prefab)where T:WarEntity
    {
        T instance;
      if(pool[prefab.name].Count>0)
      {
        instance = (T)pool[prefab.name][0];
        instance.gameObject.SetActive(true);
        pool[prefab.name].Remove(instance);
        //Debug.Log(prefab.name + "last:" + pool[prefab.name].Count);
        return instance;
      }
         instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
         instance.name = prefab.name;
         pool[prefab.name].Add(instance);
         //Debug.Log(prefab.name + "(after add)last:" + pool[prefab.name].Count);
         return instance;
        //instance.OriginFactory = this;
    }
    public void Reclaim(WarEntity entity)
    {
        Debug.Assert(entity.OriginFactory==this,"Wrong factory reclaimed!");
        //Destroy(entity.gameObject);
        RecycleObj(entity); 
    }
    public Dictionary<string, List<WarEntity>> pool = new Dictionary<string, List<WarEntity>>();
    public Dictionary<string, WarEntity> prefabs = new Dictionary<string, WarEntity>();
    public void Create()
    {
        pool.Add("shell", new List<WarEntity>());
        pool.Add("explosion", new List<WarEntity>());
        for (int i = 0; i < 50; i++)
        {
            Shell shell = CreateGameObjectInstance(shellPrefab);
            pool["shell"].Add(shell);
            shell.gameObject.SetActive(false);
            shell.name = shellPrefab.name;
            shell.OriginFactory = this;
            Explosion explosion = CreateGameObjectInstance(explosionPrefab);
            pool["explosion"].Add(explosion);
            explosion.gameObject.SetActive(false);
            explosion.name = explosionPrefab.name;
            explosion.OriginFactory = this;
        }
    }
    void RecycleObj(WarEntity obj)
    {
        obj.gameObject.SetActive(false);
        obj.gameObject.transform.position = new Vector3(0,0,10);
        pool[obj.name].Add(obj);
        //else 
        //{
        //    pool.Add(obj.name, new List<WarEntity>() { obj });
        //}
    }
}
