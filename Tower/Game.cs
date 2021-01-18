using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour
{

	[SerializeField]
	Vector2Int boardSize = new Vector2Int(11, 11);
	[SerializeField]
	public GameBoard board = default;
	[SerializeField]
	GameTileContentFactory tileContentFactory = default;
	//[SerializeField]
	//EnemyFactory enemyFactory = default;
	//[Tooltip("产生敌人的速度")]
	//[SerializeField, Range(0.1f, 10f)]
	//float spawnSpeed = 1f;
	//float spawnProgress;
	[SerializeField]
	GameScenario scenario = default;
	GameScenario.State activeScenario;
	GameBehaviorCollection enemies = new GameBehaviorCollection();
	GameBehaviorCollection nonEnemies = new GameBehaviorCollection();
	public TowerType selectedTowerType;//选择Tower的类型
	[SerializeField]
	WarFactory warFactory = default;
	[SerializeField, Range(0, 100)]
	int statingPlayerHealth = 10;
	[SerializeField, Range(0f, 10f)]
	public float playSpeed = 1f;
	static Game instance;
	public int playerHealth;
	public const float pauseTimeScale = 0f;
	float tempTime=0f;
	TowerUI towerUI;
	void Awake()
	{
		towerUI = FindObjectOfType<TowerUI>();
		//Time.timeScale = pauseTimeScale;
		playerHealth = statingPlayerHealth;
		board.Initialize(boardSize, tileContentFactory);
		board.ShowGrid = true;
		activeScenario = scenario.Begin();
		warFactory.Create();
	}

	void OnValidate()
	{
		if (boardSize.x < 2)
		{
			boardSize.x = 2;
		}
		if (boardSize.y < 2)
		{
			boardSize.y = 2;
		}
	}

	Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);
	public bool IsStartCount=false;
    private void Update()
    {
		if (!IsGameStart) return;
		if (!IsStartCount) return;
		float RefleshTime = 1f;//金币刷新时间
		if(Time.time> tempTime + RefleshTime&&IsStartCount)
        {
			board.PlayerCoin += 5;
			tempTime = Time.time;
		}

		if (Input.GetMouseButtonDown(0))
		{
			HandleTouch();
		}
		else if (Input.GetMouseButtonDown(1)) {
			HandleAlternativeTouch();
		}
		if (Input.GetKeyDown(KeyCode.V)) {
			board.ShowPaths = !board.ShowPaths;
		}
		if (Input.GetKeyDown(KeyCode.G))
		{
			board.ShowGrid = !board.ShowGrid;
		}
        if (Input.GetKeyDown(KeyCode.Space)) {
			Time.timeScale = Time.timeScale > pauseTimeScale ? pauseTimeScale : 1f;
		}
		else if (Time.timeScale > pauseTimeScale) {
			Time.timeScale = playSpeed;
		}
		if (Input.GetKeyDown(KeyCode.B))
		{
			BeginNewGame();
		}
		if (Input.GetKeyDown(KeyCode.Alpha1)) {
			selectedTowerType = TowerType.Laser;
		}
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
			selectedTowerType = TowerType.Mortar;
		}
        //spawnProgress += spawnSpeed * Time.deltaTime;
        //while (spawnProgress >= 1f) {
        //	spawnProgress -= 1f;
        //	SpawnEnemy();
        //}
        if (playerHealth <= 0 && statingPlayerHealth > 0&& IsGameStart) {
			Debug.Log("defeat!");
			Time.timeScale = pauseTimeScale;
			//BeginNewGame();
			towerUI.Defeated.Play();
			towerUI.Defeated.SetHook("stop", ()=>towerUI.Defeated.SetPaused(true));;
			IsGameStart = false;
		}
        if (!activeScenario.Progress()&&enemies.IsEmpty&& IsGameStart) {
			Debug.Log("Victory!");
			towerUI.Victory.Play();
			towerUI.Victory.SetHook("stop", () => towerUI.Victory.SetPaused(true)); ;
			//towerUI.Victory.Play();
			activeScenario.Progress();
			IsGameStart = false;
		}
		activeScenario.Progress();
		enemies.GameUpdate();//计算敌人数量
		nonEnemies.GameUpdate();//计算shell和explosion数量
		Physics.SyncTransforms();
		board.GameUpdate();
	}
	/// <summary>
	/// 射线创建spawnPoint,同时按住shift创建Destination
	/// </summary>
	void HandleAlternativeTouch() {
		GameTile tile = board.GetTile(TouchRay);
		if (tile != null) {
			if (Input.GetKey(KeyCode.LeftShift))
			{
				board.ToggleDestination(tile);
			}
			else {
				board.ToggleSpawnPoint(tile);
			}
		}
	}
	public bool IsShaftToTower = false;
	public bool IsGameStart=false;
	/// <summary>
	/// 点击鼠标左键创建Wall，再次点击转换成Empty,加shift创建Tower(同时按1为镭射塔，2为投石塔)
	/// </summary>
	void HandleTouch() {
		if (IsGameStart)
        {
			GameTile tile = board.GetTile(TouchRay);
			if (tile != null)
			{
				if (Input.GetKey(KeyCode.LeftShift) || IsShaftToTower)
				{
					board.ToggleTower(tile, selectedTowerType);
				}
				else
				{
					board.ToggleWall(tile);
				}
				//tile.Content =tileContentFactory.Get( GameTileContentType.Destination);
			}
		}		
	}
	/// <summary>
	/// 在随机SpawnPoint产生敌人
	/// </summary>
	public static void SpawnEnemy(EnemyFactory factory,EnemyType type) {
		GameTile spawnPoint =instance.board.GetSpawnPoint(Random.Range(0, instance.board.SpawnPointCount));
		Enemy enemy = factory.Get(type);
		enemy.SpawnOn(spawnPoint);
		instance.enemies.Add(enemy);
	}
	public static Shell SpawnShell()
    {
		Shell shell = instance.warFactory.Shell;
		instance.nonEnemies.Add(shell);
		return shell;
    }
    private void OnEnable()
    {
		instance = this;
	}
	public static Explosion SpawnExplosion()
    {
		Explosion explosion = instance.warFactory.Explosion;
		instance.nonEnemies.Add(explosion);
		return explosion;
    }
	public void BeginNewGame() {
		Time.timeScale = playSpeed;
		IsGameStart = true;
		playerHealth = statingPlayerHealth;
		enemies.Clear();
		nonEnemies.Clear();
		board.Clear();
		activeScenario = scenario.Begin();
		board.PlayerCoin = 0;
	}
	public static void EnemyReachedDestination() {
		instance.playerHealth -= 1;
	}
}
