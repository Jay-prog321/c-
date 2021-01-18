using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory
{
    [System.Serializable]
    class EnemyConfig {
        public Enemy prefab = default;
        [FloatRangeSlider(0.5f, 2f)]
        public FloatRange scale = new FloatRange(1f);
        [FloatRangeSlider(0.2f, 5f)]
        public FloatRange speed = new FloatRange(1f);
        [FloatRangeSlider(-0.4f, 0.4f)]
        public FloatRange pathOffset = new FloatRange(0f);
        [FloatRangeSlider(10f, 1000f)]
        public FloatRange health = new FloatRange(100f);
    }
    [SerializeField]
    EnemyConfig small = default, medium = default, large = default;
    //[SerializeField]
    //Enemy prefab = default;
    //[SerializeField, FloatRangeSlider(-0.5f, 2f)]
    //FloatRange scale = new FloatRange(1f);
    //[SerializeField, FloatRangeSlider(-0.4f, 0.4f)]
    //FloatRange pathOffset = new FloatRange(0f);
    //[SerializeField, FloatRangeSlider(0.2f,5f)]
    //FloatRange speed = new FloatRange(1f);
    EnemyConfig GetConfig(EnemyType type){
        switch (type) {
            case EnemyType.Small:return small;
            case EnemyType.Medium:return medium;
            case EnemyType.Large:return large;
        }
        Debug.Assert(false, "Unsupport enemy type!");
        return null;
    }
    public bool toCreate = false;

    public Enemy Get(EnemyType type=EnemyType.Medium)
    {
        float GrenaierSize;
        float Grenaierlife;
        EnemyConfig config = GetConfig(type);
        Enemy enemy;
        if (pool.ContainsKey(config.prefab.name))
        {
            if (pool[config.prefab.name].Count > 0)
            {
                enemy = pool[config.prefab.name][0];
                enemy.gameObject.SetActive(true);
                pool[config.prefab.name].Remove(enemy);
                if (enemy.name.Contains("Grenaier"))
                {
                    GrenaierSize = 2f;
                    Grenaierlife = 5f;
                }
                else
                {
                    GrenaierSize = 1f;
                    Grenaierlife = 1f;
                }
                enemy.Initialize(config.scale.RandomValueInRange* GrenaierSize, config.speed.RandomValueInRange, config.pathOffset.RandomValueInRange, config.health.RandomValueInRange * Grenaierlife);
                return enemy;
            }
        }
        Enemy instance;
        if (prefabs.ContainsKey(config.prefab.name))
        {
            instance = prefabs[config.prefab.name];
        }
        else 
        {
            instance = Resources.Load<Enemy>("Prefabs/" + config.prefab.name);
            prefabs.Add(config.prefab.name, instance);
        }
        enemy = CreateGameObjectInstance(instance);
        enemy.OriginFactory = this;
        if (enemy.name.Contains("Grenaier"))
        {
            GrenaierSize = 2f;
            Grenaierlife = 5f;
        }
        else
        {
            GrenaierSize = 1f;
            Grenaierlife = 1f;
        }
        enemy.Initialize(config.scale.RandomValueInRange* GrenaierSize, config.speed.RandomValueInRange, config.pathOffset.RandomValueInRange, config.health.RandomValueInRange * Grenaierlife);
        enemy.name = config.prefab.name;
        return enemy;
    }
    public void Reclaim(Enemy enemy)
    {
     Debug.Assert(enemy.OriginFactory == this, "Wrong factory reclaimed!");
        //Destroy(enemy.gameObject);
        //Destroy(enemy.gameObject);
        RecycleEnemy(enemy);
    }
    private Dictionary<string, List<Enemy>> pool = new Dictionary<string, List<Enemy>>();
    private Dictionary<string, Enemy> prefabs = new Dictionary<string, Enemy>();
    void RecycleEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
        if (pool.ContainsKey(enemy.name))
        {
            pool[enemy.name].Add(enemy);
        }
        else
        {
            pool.Add(enemy.name, new List<Enemy>() { enemy }); 
        }
    }
}
