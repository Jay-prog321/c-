using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

public class TowerUI : MonoBehaviour
{
    public GButton Reset;
    public Transition Victory;
    public Transition Defeated;

    GComponent mainview;
    GameBoard board;
    Game towerGame;
    const int GamePage = 1;
    GButton life;
    GButton speed;
    GButton coin;
    void Start()
    {
        //动态生成UI
        towerGame = FindObjectOfType<Game>();
        board = FindObjectOfType<GameBoard>();
        UIPackage.AddPackage("Tower");
        mainview = UIPackage.CreateObject("Tower", "Game").asCom;
        GRoot.inst.SetContentScaleFactor(2400, 1080, UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);
        mainview.fairyBatching = true;
        mainview.AddRelation(GRoot.inst, RelationType.Size);
        GRoot.inst.AddChild(mainview);
        //创建预制体
        Object prefab1 = Resources.Load("Prefabs/show1");
        Object prefab2 = Resources.Load("Prefabs/Enemy Grenaier");
        GameObject go1 = (GameObject)Object.Instantiate(prefab1);
        GameObject go2 = (GameObject)Object.Instantiate(prefab2);
        Animator a2 = go2.GetComponentInChildren<Animator>();
        a2.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animation/Cube");
        a2.SetBool("playmove", true);
        a2.SetBool("defaultmove", false);
        //获取空白图形（放置开始界面模型）
        GGraph holder1 = mainview.GetChild("holder1").asGraph;
        GGraph holder2 = mainview.GetChild("holder2").asGraph;
        Vector3 holder1Pos = new Vector3(0,0,150);
        Vector3 holder2Pos = new Vector3(0,0,150);
        //设置预制体位置信息
        go1.transform.localPosition = holder1Pos;
        go2.transform.position = holder2Pos;
        go1.transform.localScale=new Vector3(300, 300, 300);
        go2.transform.localScale = new Vector3(700, 700, 700);
        go1.transform.localEulerAngles = new Vector3(-20,-5,0);
        go2.transform.localEulerAngles = new Vector3(10, 200, 0);
        //创建GoWrapper
        GoWrapper wrapper1 = new GoWrapper(go1);
        GoWrapper wrapper2 = new GoWrapper(go2);
        holder1.SetNativeObject(wrapper1);
        holder2.SetNativeObject(wrapper2);
        //获取控制器
        Controller changePage = mainview.GetController("c1");
        //获取按钮
        GButton Pause = mainview.GetChild("btn_1").asButton;
        GButton ShowPath = mainview.GetChild("btn_2").asButton;
        GButton ShowGrid = mainview.GetChild("btn_3").asButton;
        GButton Wall = mainview.GetChild("btn_4").asButton;
        GButton Laser = mainview.GetChild("btn_5").asButton;
        GButton Mortar = mainview.GetChild("btn_6").asButton;
        Reset = mainview.GetChild("btn_7").asButton;
        GButton StartGame = mainview.GetChild("btn_8").asButton;
        GButton ExitApp = mainview.GetChild("btn_9").asButton;
        GButton StartAgain = mainview.GetChild("btn_10").asButton;
        GButton ExitGame = mainview.GetChild("btn_11").asButton;
        speed = mainview.GetChild("btn_12").asButton;
        life = mainview.GetChild("btn_13").asButton;
        coin = mainview.GetChild("btn_14").asButton;
        //获取动效
        Victory = mainview.GetTransition("victory");
        Defeated = mainview.GetTransition("Reset");
        //按钮事件
        Pause.onClick.Add(()=> {
        Time.timeScale = Time.timeScale > 0f ? 0f : towerGame.playSpeed;
            towerGame.IsStartCount = true;
        });
        ShowPath.onClick.Add(() => {
            board.ShowPaths = !board.ShowPaths;
        });
        ShowGrid.onClick.Add(() => {
            board.ShowGrid = !board.ShowGrid;
        });
        Wall.onClick.Add(() => {
            towerGame.IsShaftToTower = false;
        });
        Laser.onClick.Add(() => {
            towerGame.selectedTowerType = TowerType.Laser;
            towerGame.IsShaftToTower = true;
        });
        Mortar.onClick.Add(() => {
            towerGame.selectedTowerType = TowerType.Mortar;
            towerGame.IsShaftToTower = true;
        });
        Reset.onClick.Add(() => {
            Defeated.SetPaused(false);
            towerGame.BeginNewGame();
        });
        StartGame.onClick.Add(() => {
            changePage.selectedIndex = GamePage;
            wrapper1.visible = false;
            wrapper2.visible = false;
            towerGame.IsGameStart = true;
            Time.timeScale = 0;
        });
        ExitApp.onClick.Add(() => {
            Application.Quit();
        });
        StartAgain.onClick.Add(() => {
            Victory.SetPaused(false);
            Defeated.SetPaused(false);
            towerGame.BeginNewGame();
        });
        ExitGame.onClick.Add(() => {
            Application.Quit();
        });
        speed.onClick.Add(()=> {
            if (towerGame.playSpeed ==0.5f) { towerGame.playSpeed = 0f; }
            towerGame.playSpeed +=1;
            if (towerGame.playSpeed > 3) { towerGame.playSpeed = 0.5f; }
        });
    }
    private void Update()
    {
        life.title =""+ towerGame.playerHealth;
        speed.title = towerGame.playSpeed + "X";
        if (towerGame.IsStartCount) { coin.title = (int)board.PlayerCoin + ""; }
    }
}
