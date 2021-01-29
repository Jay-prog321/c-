using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class CreatingGame : PersistableObject
{
    #region [SerializeField]
    [SerializeField]
    KeyCode createKey = KeyCode.C;
    [SerializeField]
    KeyCode newGameKey = KeyCode.N;
    [SerializeField]
    KeyCode saveKey = KeyCode.S;
    [SerializeField]
    KeyCode loadKey = KeyCode.L;
    [SerializeField]
    KeyCode destroyKey = KeyCode.X;
    List<CreatingShape> creatingShapes;
    [SerializeField]
    PersistentStorage storage;
    [SerializeField]
    int levelCount;
    [SerializeField]
    bool reseedOnLoad;
    [SerializeField]
    Slider creationSpeedSlider;
    [SerializeField]
    Slider destructionSpeedSlider;
    [SerializeField]
    ShapeFactory[] shapeFactories;
    #endregion
    #region public
    public float CreationSpeed { get; set; }
    public float DestructionSpeed { get; set; }
    //public SpawnZone spawnZoneOfLevel { get; set; }
    //public static CreatingGame Instance { get; private set; }
    #endregion
    const int saveVersion = 5;
    float creationProgress, destructionProgress;
    int loadedLevelBuildIndex;
    Random.State mainRandomState;
    private void OnEnable()
    {
        if (shapeFactories[0].FactoryId != 0)
        {
            for (int i = 0; i < shapeFactories.Length; i++)
            {
                shapeFactories[i].FactoryId = i;
            }
        }
    }
    private void Start()
    {
        mainRandomState = Random.state;
        creatingShapes = new List<CreatingShape>();
        if (Application.isEditor)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScence = SceneManager.GetSceneAt(i);
                if (loadedScence.name.Contains("Level")) {
                    SceneManager.SetActiveScene(loadedScence);
                    loadedLevelBuildIndex = loadedScence.buildIndex;
                    return;
                }
            }
        }
        BeginNewGame();
        StartCoroutine(LoadLevel(levelBuildIndex:1));
    }
    private void Update()
    {
        if (Input.GetKeyDown(createKey))
        {
            CreateShape();
        }
        else if (Input.GetKeyDown(newGameKey))
        {
            BeginNewGame();
            StartCoroutine(LoadLevel(loadedLevelBuildIndex));
        }
        else if (Input.GetKeyDown(saveKey))
        {
            storage.Save(this, saveVersion);
        }
        else if (Input.GetKeyDown(destroyKey))
        {
            DestroyShape();
        }
        else if (Input.GetKeyDown(loadKey))
        {
            //load();
            BeginNewGame();
            storage.Load(this);
        }
        else
        {
            for (int i = 0; i <= levelCount; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i)) {
                    BeginNewGame();
                    StartCoroutine(LoadLevel(i));
                    return;
                }
            }
        }
    }
    private void FixedUpdate()
    {
        for (int i = 0; i < creatingShapes.Count; i++)
        {
            creatingShapes[i].GameUpdate();
        }
        creationProgress += Time.deltaTime * CreationSpeed;
        while (creationProgress >= 1) {
            creationProgress -= 1f;
            CreateShape();
        }
        destructionProgress += Time.deltaTime * DestructionSpeed;
        while (destructionProgress >= 1)
        {
            destructionProgress -= 1f;
            DestroyShape();
        }
    }
    private void DestroyShape()
    {
        if (creatingShapes.Count > 0) {
            int index = Random.Range(0, creatingShapes.Count);
            //Destroy(creatingShapes[index].gameObject);
            //shapeFactory.Reclaim(creatingShapes[index]);
            creatingShapes[index].Recycle();
            int lastIndex = creatingShapes.Count - 1;
            creatingShapes[index] = creatingShapes[lastIndex];
            creatingShapes.RemoveAt(lastIndex);
        }
    }
    void CreateShape() {
        //CreatingShape instace = shapeFactory.GetRandom();
        //GameLevel.Current.ConfigureSpawn(instace);
        creatingShapes.Add(GameLevel.Current.SpawnShape());
    }
    void BeginNewGame() {
        Random.state = mainRandomState;
        int seed = Random.Range(0,int.MaxValue)^(int)Time.unscaledTime;
        mainRandomState = Random.state; 
        Random.InitState(seed);
        creationSpeedSlider.value=CreationSpeed = 0;
        destructionSpeedSlider.value =DestructionSpeed= 0;
        for (int i = 0; i < creatingShapes.Count; i++)
        {
            //shapeFactory.Reclaim(creatingShapes[i]);
            creatingShapes[i].Recycle();
        }
        creatingShapes.Clear();
    }
    public override void Save(GameDataWriter writer) {
        writer.Write(creatingShapes.Count);
        writer.Write(Random.state);
        writer.Write(CreationSpeed);
        writer.Write(creationProgress);
        writer.Write(DestructionSpeed);
        writer.Write(destructionProgress);
        writer.Write(loadedLevelBuildIndex);
        GameLevel.Current.Save(writer);
        for (int i = 0; i < creatingShapes.Count; i++)
        {
            writer.Write(creatingShapes[i].OriginalFactory.FactoryId);
            writer.Write(creatingShapes[i].ShapeID);
            writer.Write(creatingShapes[i].MaterialID);
            creatingShapes[i].Save(writer);
        }
    }

    public override void Load(GameDataReader reader)
    {
        int version = reader.Version;
        if (version > saveVersion)
        {
            Debug.LogError("Unsuppoerted future save version" + version);
            return;
        }
        StartCoroutine(LoadGame(reader));
    }
    IEnumerator LoadGame(GameDataReader reader) {
        int version = reader.Version;
        int count = version <= 0 ? -version : reader.ReadInt();
        if (version >= 3) {
            Random.State state = reader.ReadRandomState();
            if (!reseedOnLoad) {
                Random.state = state;
            }
            creationSpeedSlider.value= CreationSpeed = reader.ReadFloat();
            creationProgress = reader.ReadFloat();
            destructionSpeedSlider.value= DestructionSpeed = reader.ReadFloat();
            destructionProgress = reader.ReadFloat();
        }
        yield return LoadLevel(version<2?1:reader.ReadInt());
        if (version >= 3) {
            GameLevel.Current.Load(reader);
        }
        for (int i = 0; i < count; i++)
        {
            int factoryId = version >= 5 ? reader.ReadInt() : 0;
            int shapedID = version > 0 ? reader.ReadInt() : 0;
            int materialID = version > 0 ? reader.ReadInt() : 0;
            CreatingShape instace = shapeFactories[factoryId].Get(shapedID, materialID);
            instace.Load(reader);
            creatingShapes.Add(instace);
        }
    }
    IEnumerator LoadLevel(int levelBuildIndex)
    {
        //SceneManager.LoadScene("Level 1",LoadSceneMode.Additive);
        //yield return null;//空语句占一帧时间弥补加载场景的时间
        enabled = false;
        if (loadedLevelBuildIndex > 0) {
            yield return SceneManager.UnloadSceneAsync(loadedLevelBuildIndex);
        }
        yield return SceneManager.LoadSceneAsync(levelBuildIndex, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIndex));
        loadedLevelBuildIndex = levelBuildIndex;
        enabled = true;
    }
    //void Save() {
    //    using(
    //    var writer =
    //        new BinaryWriter(File.Open(savePath, FileMode.Create))
    //    ){
    //        writer.Write(objects.Count);
    //        for (int i = 0; i < objects.Count; i++)
    //        {
    //            Transform t = objects[i];
    //            writer.Write(t.localPosition.x);
    //            writer.Write(t.localPosition.y);
    //            writer.Write(t.localPosition.z); 
    //        }
    //    }       
    //}
    //private void load()
    //{
    //    BeginNewGame();
    //    using(
    //    var reader=new BinaryReader(File.Open(savePath,FileMode.Open))
    //    ){
    //        int count = reader.ReadInt32();
    //        for (int i = 0; i < count; i++)
    //        {
    //            Vector3 p;
    //            p.x = reader.ReadSingle();
    //            p.y = reader.ReadSingle();
    //            p.z = reader.ReadSingle();
    //            Transform t = Instantiate(prefab);
    //            t.localPosition = p;
    //            objects.Add(t);
    //        }
    //    }
    //}
}
