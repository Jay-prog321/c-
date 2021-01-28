using UnityEngine;
using System.Collections.Generic;
public class GameBoard : MonoBehaviour
{

    [SerializeField]
    Transform ground = default;
    [SerializeField]
    GameTile tilePrefab = default;
    [SerializeField]
    Texture2D gridTexture = default;
    Vector2Int size;//场景初始尺寸
    GameTile[] tiles;
    Queue<GameTile> searchFrontier = new Queue<GameTile>();
    GameTileContentFactory contentFactory;
    public float PlayerCoin = 0;
    const int UILayerMask = 5;
    /// <summary>
    /// 实例化一个场景
    /// </summary>
    /// <param name="size">尺寸</param>
    /// <param name="contentFactory"></param>
    public void Initialize(Vector2Int size, GameTileContentFactory contentFactory)
    {
        this.size = size;
        this.contentFactory = contentFactory;
        ground.localScale = new Vector3(size.x, size.y, 1f);
        Vector2 offset = new Vector2((size.x - 1) * 0.5f, (size.y - 1) * 0.5f);
        tiles = new GameTile[size.x * size.y];
        for (int i = 0,y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++,i++)
            {
                GameTile tile = tiles[i] = Instantiate(tilePrefab);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(x - offset.x, 0f, y - offset.y);
                if (x > 0)
                {
                    GameTile.MakeEastWestNeighbors(tile, tiles[i - 1]);
                }
                if (y > 0)
                {
                    GameTile.MakeNorthSouthNeighbors(tile, tiles[i - size.x]);
                }
                tile.IsAlternative = (x & 1) == 0;//偶数位IsAlternative为TRUE，奇数位IsAlternative为FALSE
                if ((y & 1) == 0)//偶数行偶数位IsAlternative为FALSE，奇数位IsAlternative为TRUE
                {
                    tile.IsAlternative = !tile.IsAlternative;
                }
                //tile.Content = contentFactory.Get(GameTileContentType.Empty);
            }
        }
        Clear();
    }
    public void Clear()
    {
        foreach (GameTile tile in tiles) {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
        }
        SpawnPoints.Clear();
        updatingContent.Clear();
        ToggleDestination(tiles[tiles.Length / 2]);
        ToggleSpawnPoint(tiles[0]);
    }
    /// <summary>
    /// 寻找路径,如果不存在Destination返回false
    /// </summary>
    /// <returns></returns>
    bool FindPaths()
    {
        foreach (GameTile tile in tiles)
        {
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                tile.BecomeDestination();
                searchFrontier.Enqueue(tile);
            }
            else
            {
                tile.ClearPath();
            }
        }
        if (searchFrontier.Count == 0)
        {
            return false;
        }
        while (searchFrontier.Count > 0)//为所有tiles的distance赋值1，并且nextOnPath赋值为自身
        {
            GameTile tile = searchFrontier.Dequeue();//初始为tiles[tiles.Length / 2]
            if (tile != null)
            {
                if (tile.IsAlternative){
                searchFrontier.Enqueue(tile.GrowPathNorth());
                searchFrontier.Enqueue(tile.GrowPathSouth());
                searchFrontier.Enqueue(tile.GrowPathEast());
                searchFrontier.Enqueue(tile.GrowPathWest());
                }
                else
                {
                    searchFrontier.Enqueue(tile.GrowPathWest());
                    searchFrontier.Enqueue(tile.GrowPathEast());
                    searchFrontier.Enqueue(tile.GrowPathSouth());
                    searchFrontier.Enqueue(tile.GrowPathNorth());
                }
            }
        }
        foreach (GameTile tile in tiles)
        {
            if (!tile.HasPath)
                return false;
        }
        if (showPaths) {
            foreach (GameTile tile in tiles)
            {
                tile.ShowPath();
            }
        }
        return true;
    }
    public GameTile GetTile(Ray ray) {
        if (Physics.Raycast(ray,out RaycastHit hit,float.MaxValue,1)) {
            if (hit.transform.gameObject.layer == UILayerMask) { Debug.Log("UI!"); return null; } 
            int x = (int)(hit.point.x+size.x*0.5f);
            int y = (int)(hit.point.z + size.y * 0.5f);
            if(x>=0&&x<size.x&&y>=0&&y<size.y)return tiles[x+y*size.x];
        }
        return null;
    }
    /// <summary>
    /// click Destination 转换成Empty,click Empty转换成Destination
    /// </summary>
    /// <param name="tile"></param>
    public void ToggleDestination(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Destination)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            //移除Destination有可能会寻找路径失败，需要二次确认
            if (!FindPaths())
            {
                tile.Content = contentFactory.Get(GameTileContentType.Destination);
                FindPaths();
            }
        }
        else if((tile.Content.Type == GameTileContentType.Empty))
        {
            tile.Content = contentFactory.Get(GameTileContentType.Destination);
            FindPaths();
        }
    }
    
    /// <summary>
    /// 将Empty转换成Wall或将Wall转换成Empty
    /// </summary>
    /// <param name="tile"></param>
    public void ToggleWall(GameTile tile) {
        //Wall 5金币  laser tower 10金币  mortar tower 50金币
        if (tile.Content.Type == GameTileContentType.wall)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            PlayerCoin += 5;
            FindPaths();
            return;
        }
        if (PlayerCoin < 5) { return; }
        else {
            tile.Content = contentFactory.Get(GameTileContentType.wall);
            PlayerCoin -= 5;
            FindPaths();
        }

    }
    List<GameTile> SpawnPoints = new List<GameTile>();
    /// <summary>
    /// 将Empty转换成SpawnPoint或将SpawnPoint转换成Empty
    /// </summary>
    /// <param name="tile"></param>
    public void ToggleSpawnPoint(GameTile tile) {
        if (tile.Content.Type == GameTileContentType.SpawnPoint)
        {
            //SpawnPoint至少要保留一个，这样敌人才能出现
            if (SpawnPoints.Count > 1) {
                SpawnPoints.Remove(tile);
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty) {
            tile.Content = contentFactory.Get(GameTileContentType.SpawnPoint);
            SpawnPoints.Add(tile);
        }
    }
    public void ToggleTower(GameTile tile,TowerType towerType) {
        if (tile.Content.Type == GameTileContentType.Tower)
        {
            updatingContent.Remove(tile.Content);
            if (((Tower)tile.Content).TowerType == towerType)
            {
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                if (towerType == TowerType.Laser) { PlayerCoin += 10; }
                if (towerType == TowerType.Mortar){ PlayerCoin += 50; }
                FindPaths();
            }
            else {
                tile.Content = contentFactory.Get(towerType);
                if (towerType == TowerType.Laser) { PlayerCoin += 40; }
                if (towerType == TowerType.Mortar&&PlayerCoin>=40) { PlayerCoin -= 40; }
                updatingContent.Add(tile.Content);
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            if (towerType == TowerType.Laser&&PlayerCoin < 10) { return; }
            if (towerType == TowerType.Mortar && PlayerCoin < 50) { return; }
            tile.Content = contentFactory.Get(towerType);
            //if (!FindPaths())
            if (FindPaths())
            {
                updatingContent.Add(tile.Content);
            }
            else { 
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
                return;
            }
            if (towerType == TowerType.Laser && PlayerCoin >= 10) { PlayerCoin -= 10; }
            if (towerType == TowerType.Mortar && PlayerCoin >= 50) { PlayerCoin -= 50; }
        }
        else if (tile.Content.Type == GameTileContentType.wall) {
            if (towerType == TowerType.Laser && PlayerCoin < 5) { return; }
            if (towerType == TowerType.Mortar && PlayerCoin < 45) { return; }
            tile.Content = contentFactory.Get(towerType);
            updatingContent.Add(tile.Content);
            if (towerType == TowerType.Laser && PlayerCoin >= 5) { PlayerCoin -= 5; }
            if (towerType == TowerType.Mortar && PlayerCoin >= 45) { PlayerCoin -= 45; }
        }
    }
    bool showPaths,showGrid;
    public bool ShowPaths {
        get => showPaths;
        set {
            showPaths = value;
            if (showPaths)
            {
                foreach (GameTile tile in tiles)
                {
                    tile.ShowPath();
                }
            }
            else {
                foreach (GameTile tile in tiles) {
                    tile.HidePath();
                }
            }
        }
    }
    public bool ShowGrid {
        get => showGrid;
        set {
            showGrid = value;
            Material m = ground.GetComponent<MeshRenderer>().material;
            if (showGrid)
            {
                m.mainTexture = gridTexture;
                m.SetTextureScale("_MainTex",size);
            }
            else {
                m.mainTexture = null;
            }
        }
    }
    public int SpawnPointCount => SpawnPoints.Count;
    public GameTile GetSpawnPoint(int index)
    {
        return SpawnPoints[index];
    }
    List<GameTileContent> updatingContent = new List<GameTileContent>();
    public void GameUpdate() {
        for (int i = 0; i < updatingContent.Count; i++)
        {
            updatingContent[i].GameUpdate();
        }
    }
}
