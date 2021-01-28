using UnityEngine;
//using UnityEngine.SceneManagement;

[CreateAssetMenu]
public class GameTileContentFactory : GameObjectFactory
{
	public void Reclaim(GameTileContent content)
	{
		Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
		Destroy(content.gameObject);
	}
	GameTileContent Get(GameTileContent prefab)
	{
		GameTileContent instance = CreateGameObjectInstance(prefab);
		instance.OriginFactory = this;
		return instance;
	}
	[SerializeField]
	GameTileContent destinationPrefab = default;
	[SerializeField]
	GameTileContent emptyPrefab = default;
	[SerializeField]
	GameTileContent wallPrefab = default;
	[SerializeField]
	GameTileContent spawnPointPrefab = default;
	[SerializeField]
	Tower[] towerPrefabs = default;
	/// <summary>
	/// 实例化一个物体Empty或Destination
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public GameTileContent Get(GameTileContentType type)
	{
		switch (type)
		{
			case GameTileContentType.Destination: return Get(destinationPrefab);
			case GameTileContentType.Empty: return Get(emptyPrefab);
			case GameTileContentType.wall: return Get(wallPrefab);
			case GameTileContentType.SpawnPoint:return Get(spawnPointPrefab);
			//case GameTileContentType.Tower:return Get(towerPrefab);
		}
		Debug.Assert(false, "Unsupported non-tower type!" + type);
		return null;
	}
	public GameTileContent Get(TowerType type) {
		Debug.Assert((int)type < towerPrefabs.Length, "Unsupported tower types!");
		Tower prefab = towerPrefabs[(int)type];
		Debug.Assert(type== prefab.TowerType,"Tower prefab at wrong index!");
		return Get(prefab);
	}
	T Get<T>(T prefab) where T : GameTileContent {
		T instance = CreateGameObjectInstance(prefab);
		instance.OriginFactory = this;
		return instance;
	}
}
