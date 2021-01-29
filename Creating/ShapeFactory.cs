using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
[CreateAssetMenu]
public class ShapeFactory :ScriptableObject
{
    [SerializeField]
    CreatingShape[] prefabs;
    [SerializeField]
    Material[] materials;
    [SerializeField]
    bool recycle;
    Scene poolScence;
    List<CreatingShape>[] pools;
    [System.NonSerialized]
    int factoryId = int.MinValue;
    public int FactoryId { get {
            return factoryId;
        }
        set {
            if (factoryId == int.MinValue && value != int.MaxValue)
            {
                factoryId = value;
            }
            else {
                Debug.Log("Not allowed to change factoryId");
            }
        }
    }
    public CreatingShape Get(int shapeID,int materialID=0) {
        CreatingShape instance;
        if (recycle)
        {
            if (pools == null)
            {
                CreatePools();
            }
            List<CreatingShape> pool = pools[shapeID];
            int lastIndex = pool.Count - 1;
            if (lastIndex >= 0)
            {
                instance = pool[lastIndex];
                instance.gameObject.SetActive(true);
                pool.RemoveAt(lastIndex);
            }
            else {
                instance = Instantiate(prefabs[shapeID]);
                instance.OriginalFactory = this;
                instance.ShapeID = shapeID;
                SceneManager.MoveGameObjectToScene(instance.gameObject,poolScence);
            }
        }
        else {
            instance = Instantiate(prefabs[shapeID]);
            instance.ShapeID = shapeID;
        }
        instance.SetMaterial(materials[materialID],materialID);
        return instance;
    }
    public CreatingShape GetRandom() {
        return Get(Random.Range(0,prefabs.Length),Random.Range(0,materials.Length));
    }
    void CreatePools() {
        pools = new List<CreatingShape>[prefabs.Length];
        for (int i = 0; i < pools.Length; i++)
        {
            pools[i] = new List<CreatingShape>();
        }
        if (Application.isEditor)
        {
            poolScence = SceneManager.GetSceneByName(name);
            if (poolScence.isLoaded)
            {
                GameObject[] rootObjects = poolScence.GetRootGameObjects();
                for (int i = 0; i < rootObjects.Length; i++)
                {
                    CreatingShape pooledShape = rootObjects[i].GetComponent<CreatingShape>();
                    if (!pooledShape.gameObject.activeSelf)
                    {
                        pools[pooledShape.ShapeID].Add(pooledShape);
                    }
                }
                return;
            }
        }
        poolScence = SceneManager.CreateScene(name);
    }
    public void Reclaim(CreatingShape shapeToRecycle) {
        if (shapeToRecycle.OriginalFactory != this) {
            Debug.LogError("Tried to reclaim shape with wrong factory");
            return;
        }
        if (recycle)
        {
            if (pools == null)
            {
                CreatePools();
            }
            pools[shapeToRecycle.ShapeID].Add(shapeToRecycle);
            shapeToRecycle.gameObject.SetActive(false);

        }
        else {
            Destroy(shapeToRecycle.gameObject);
        }
    }
}
