
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;//バトルシーンの終わり方(GameManagerClassとのつながり)によってはいらなくなる

public class BattleMapManager : MonoBehaviour
{
    /*############################################################################################################################
     *      【 伝言板 】
     * ・方向の指定方法：未選択を0とし、右方向から反時計回りに1,2,3,4  (中野)
     * ・味方の魔導士だけ他キャラも回復させられるバフあるからそれ書いておきたいけれどどこに書こう？（野戸）
     * ↑相当複雑になると思うのであまりお勧めはできないけど、CharaActionWait関数とCharaStatusクラス内の関数で場合分けして書けばできないこともないかも。(中野)（9/16 確認）
     * ・キャラが移動して攻撃を選択した後，攻撃する敵を選ばないといけないけどそれはどこに書くの？（野戸）
     * ↑CharaActionWait内でwhile文でマウス入力を受け付けてCharaStatus.Attack()の引数にonMouseObjectを指定してください(中野)（9/15　確認）
     * ・敵がいるのに攻撃コマンド選択出来たらおかしいからコマンドを表示させる前に攻撃範囲内に敵がいるかの判定がいる（野戸）(9/16　確認)
     * ・↑の判定をする奴をPlayerBattleCommandに書くよ。それからここで魔導士が回復をかけられるキャラが居るかどうかも判定できるように味方キャラの判定もできるようにしとく（野戸）(9/16確認)
     * ・　↑MouseButtonCheckをうまく書き換えたらいける？それとも簡単にするならコライダーの距離間で決める？
     *       ↑Mapchips書き換えの時に同時に判定できると思う。異なる関数間の処理になるので、フィールド配列が必要かも(中野)（9/16 確認）
     * ・CharaAliveJudge()って生存判定であってるよな。何をするん？味方キャラが全滅したかとかを確認するん？（野戸）
     *   ↑キャラの全滅を判断するのはGamejudge()。CharaAliveJudgeは攻撃を受けたキャラのHPが０だった時に戦闘不能にする処理。具体的にはDestroyもしくはGameObjectを無効化する。
     * ・ private int[] activecharaprepos = new int[2];って何の配列？(野戸)
     * ・↑で言った攻撃範囲内に敵がいるかの判定，maptips書き換えの時にできるって言ったけどそれどこに書いている？
     *     ↑activatecharapreposは移動をキャンセルしたときに帰ってくる場所(中野)
     * ・積極的にコメントアウト使っていこうよ by Planner（スクリプト何書いてるのか全然わからないんすけど（#^ω^））
     * ・maptips[judgex, judgey].GetComponent<Renderer>().material.color = Color.blue;←が二か所に書かれてるけど大丈夫なん？最初にマップ全部真っ青になってそれから色変わらずに移動範囲だけ青になる，みたいなスクリプトに見えるんやけど（野戸）
     * ・ int[,] search = new int[maprange[0] * maprange[1], 4];//2次元目は{x,y,折れ線距離, 格納されていれば１}って書いてるけどそもそもint[,] search ってなんの配列？（野戸）
     *   ↑主に迂回折れ線距離を扱う際の一時的な配列。上下左右に新たなsearchを配置し、マップの2次元配列に値を書き込んでいく(中野)
     *     詳しくはここにまとめてある→https://docs.google.com/drawings/d/1H-qGMfDX8D8gdYCEZb_pLmfYeZMLPBtGbXTX8XLqtks/edit
     * ・gameobject.tagを取得したときにgameobject自体がnullであるとnull参照となりエラーが出る恐れがある。(中野)
     * ・できればCharaStatusクラスにキャラをDestroyする関数を作ってください。攻撃のアニメーションの後に実行する必要があるため、このクラス内で呼び出します。(中野)（9/17確認）
     * ・↑作りました。    public void CharaDestroy(GameObject chara)という関数です。(キャラを引数で渡すとそのキャラのHPを参照しHP<=0ならばDestroyする関数)（野戸）(9/18　確認)
     * ・一応青色のマスを元に戻す処理を書きました。合っているか確認お願いします。MouseButtonCheck()のメソッドの最後の方に書いています。（野戸）(9/18 書き直しました　中野)
     * ・味方勝利でバトルが終わるとき、Save関連の関数呼んでからシーン遷移していただけるとめっちゃ嬉しいです。その際、Saveに時間がかかると思われるのでSave中の表記をポップアップで出したいです。(フリーズでバグと思われたくない)Unity上で実行してから検討(伊達) 
     * ・PlayerBattleCommandのコマンドとマウスの当たり判定は統合時に記述
     * ・Color型の変数を用意しました。maptipの色変更に利用してください。
     * ・5,5がなぜか移動不可→Bombスペースでした。→Bombの実装見送り
     *
     *
     *
     *
     *
     *   
     *   
     *   
     ###########################################################################################################################*/

    ////コマンドであるUIを
    //[SerializeField]
    //GameObject FightCommand;
    //[SerializeField]
    //GameObject HealCommand;
    //[SerializeField]
    //GameObject WaitCommand;
    [SerializeField]
    GameObject Canvas;

    public int turn = 0;
    public int judgementTurn;

    readonly int[] maprange = { 20, 10 };//マップの広さ{x,y}
    public int MovePower = 3; // 移動力
    public float moveSpeed = 7f;//キャラが移動する速度
    const float side = 1;//マス目一辺のunity上の長さ,const = 定数
    readonly float[] intercept = { 0, 0 };//マップの一番左上の座標{x,y}
    private GameObject activeChara;//選択した味方キャラ
    private CharaStatus activeCharaStatus;
    private GameObject attackedChara;//攻撃されるキャラ
    private CharaStatus attackedCharaStatus;
    private int[] activeCharaPos = new int[2];//activeCharaの座標
    private Vector3 activeCharaPrePos;//移動をキャンセルした後に戻ってくる場所
    private int[] mousePos = new int[2];//マウスのマス目座標
    private int enemyTurnCount = 0;//何番目の敵のターンであるか
    float animationTime;//攻撃時のアニメーションに要する時間
    GameObject[,] charamap = new GameObject[20,10];
    private int walkDistance = 3;//移動できる最大のマス

    private Renderer Tilerenderer;//移動力表現用

    public GameObject onMouseObject ;//マップ上の何かが入る

    MapPosition mapPosition;//GetComponentで使用するためにMapPositionクラスのインスタンス生成
    int[,] obstaclemap = new int[20,10];//MapPositionにある通行可不可の判定用の配列

    enum Command
    {
        ATTACK = 1, HEAL, WAIT
    }

    Command command;//選択されたコマンドをCharaActionWaitに渡すための変数

    GameObject[,] maptips;//色が変わるタイルを敷き詰めます
    Color defaultColor = new Color(1, 1, 1, 0);//透明時の色
    Color movableColor = new Color(0, 1, 1, 100/255f);//移動可能範囲の色
    Color attackColor = new Color(1, 0, 0, 100/255f);//攻撃可能範囲の色
    Color healColor = new Color(0, 1, 0, 100 / 255f);//回復可能範囲の色

    GameObject[] enemyArray;//敵キャラを格納する配列

    GameObject[] encampmentFlag; //陣地の旗を格納する配列

   public int[,] flagproperties = new int[10, 3];//(x,y,味方 = 0,敵 = 1)
    int choosingchoice;//攻撃，回復，待機選択時に使用

    public string CurrentSceneName;


    //####################################################################################################################################    

    //インスタンス作成、配列格納、初期化など
    // Use this for initialization
    void Start()
    {
        onMouseObject = new GameObject();
        CurrentSceneName = SceneManager.GetActiveScene().name;//DataManager.csでも一応宣言していますが…もしかしたらSceneLoadManagerで宣言してもらったほうがいいかもしれません。
        mapPosition = GameObject.Find("ObjectManager").GetComponent<MapPosition>();



        encampmentFlag = GameObject.FindGameObjectsWithTag("flag");//ここで実際に格納
        for(int i = 0; i < encampmentFlag.Length; i++)
        {
            flagproperties[i, 0] = (int)encampmentFlag[i].GetComponent<Transform>().position.x;
            flagproperties[i, 1] = -(int)encampmentFlag[i].GetComponent<Transform>().position.y;
            flagproperties[i, 2] = 1;
        }

        enemyArray = GameObject.FindGameObjectsWithTag("enemy");//ここで実際に格納
        Debug.Log(enemyArray.Length);
        //Debug.Log("最初のターンスタート");
        StartCoroutine(TurnStart(true));
    }


    //onMouseObject,mousePosの更新
    public void mouseUpdate()
    {
        Vector3 mouseWorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos[0] = (int)((mouseWorldPoint.x - intercept[0]) / side);
        mousePos[1] = (int)((mouseWorldPoint.y * (-1) - intercept[1]) / side);
        if(mousePos[0] >= 0 && mousePos[0] < maprange[0] && mousePos[1] >= 0 && mousePos[1] < maprange[1])
        {
            mapPosition.ObjectInformation(ref obstaclemap, ref charamap);
            //   Debug.Log("onMouseObject");
            if (charamap[mousePos[0], mousePos[1]] != null)
            {
                onMouseObject = charamap[mousePos[0], mousePos[1]];
            }
            else
            {
                onMouseObject = GameObject.FindGameObjectWithTag("null");
            }
        }
        else
        {
            onMouseObject = GameObject.FindGameObjectWithTag("null");
        }
    }


    //ターンスタートの演出、引数のboolからActivateAlly、ActivateEnemyを呼び出す。
    private IEnumerator TurnStart(bool activateAlly)
    {
        Debug.Log("TurnSTart");
        //ターンスタートの演出
        if(activateAlly == true)
        {
            enemyTurnCount = 0;
            //GameManagement統合時
            for(int i = 0; i < enemyArray.Length; i++)
            {
                if(enemyArray[i] != null)
                {
                    enemyArray[i].GetComponent<CharaStatus>().AfterMoving(true);
                }
            }
            //
            StartCoroutine(ActivateAlly());//味方キャラ選択関数の呼び出し
        }
        else
        {
            while(true)
            {
                enemyTurnCount++;
                if (enemyArray[enemyTurnCount - 1] != null)
                {
                    ActivateEnemy(enemyTurnCount - 1);//敵キャラ選択関数の呼び出し
                    break;
                }
                else
                {
                    Debug.Log("敵がnull");
                }
            }
            //GameManagement統合時
            GameObject[] allyArray = GameObject.FindGameObjectsWithTag("ally");
            for (int i = 0; i < allyArray.Length; i++)
            {
                if (allyArray[i] != null)
                {
                    allyArray[i].GetComponent<CharaStatus>().AfterMoving(true);
                }
            }
            //
        }
        //Debug.Log("TurnStart終了");
        yield break;
    }//演出の挿入以外完成

    //enemyTurnCount(何番目の敵のターンか)から、次の敵をactiveCharaに代入
    private void ActivateEnemy(int enemyNum)//アクティブ状態の敵キャラ
    {
        Debug.Log("ActivateEnemy");
        //obstaclemap = mapPosition.obstaclemap;
        mapPosition.ObjectInformation(ref obstaclemap, ref charamap);
        activeChara = enemyArray[enemyNum];
        activeCharaStatus = activeChara.GetComponent<CharaStatus>();
        activeCharaPos[0] = activeCharaStatus.posX;
        activeCharaPos[1] = activeCharaStatus.posY;
        StartCoroutine("EnemyCommandCall");//敵AIの呼び出し
    }//一応完成

    //味方を
    private IEnumerator ActivateAlly()//アクティブ状態の味方キャラ
    {
        //obstaclemap = mapPosition.obstaclemap;
        mapPosition.ObjectInformation(ref obstaclemap, ref charamap);
        Debug.Log("ActivateAlly");
        yield return null;
        bool charaChoice = false;
        while (charaChoice == false)
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseUpdate();
                //Debug.Log(onMouseObject);

                


                if (onMouseObject.tag == "ally")
                {
                    //Debug.Log("actionFlag = " + onMouseObject.GetComponent<CharaStatus>().actionFlag);
                    if (onMouseObject.GetComponent<CharaStatus>().actionFlag == true)
                    {
                        activeChara = onMouseObject;
                        activeCharaStatus = activeChara.GetComponent<CharaStatus>();
                        charaChoice = true;
                        activeCharaPos[0] = activeCharaStatus.posX;
                        activeCharaPos[1] = activeCharaStatus.posY;
                        activeCharaPrePos = activeChara.transform.position;
                    }
                }
                else
                {
                
                }
            }
            yield return null;
        }
        StartCoroutine("MouseButtonCheck");
        yield break;
    }//一応完成

    private IEnumerator EnemyCommandCall()
    {
        int[] behave = activeChara.GetComponent<EffectMappingOptimized>().EffectMapping();//戻り値には行動に必要なデータが１次元配列としてまとめられている
        //for(int xcount = 0; xcount < maprange[0]; xcount++)
        //{
        //    for(int ycount = 0; ycount < maprange[1]; ycount++)
        //    {
        //        maptips[xcount, ycount].GetComponent<SpriteRenderer>().color = new Color(activeChara.GetComponent<EffectMappingOptimized>().effectmap[xcount, ycount], 0, 0, 255);
        //        if(activeChara.GetComponent<EffectMappingOptimized>().effectmap[xcount, ycount] == 1)
        //        {
        //            maptips[xcount, ycount].GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        //        }
        //    }
        //}
        //while (true)
        //{
        //    if (Input.GetMouseButtonDown(0))
        //    {
        //        MaptipsReset();
        //        break;
        //    }
        //    yield return null;
        //}
        int[] destination = { behave[0], behave[1] };//移動先
        command = (Command)behave[2];//バトルコマンド
        int[] attackPos = { behave[3], behave[4] };//攻撃座標

        int[] way = RouteSearch(activeCharaPos, destination);
        for (int i = 0; i < 3; i++)
        {
            if (way[i] == 0)
            {
                Debug.Log("移動が完了しました");
                break;
            }
            else
            {
                activeCharaPos[0] += (int)Mathf.Cos((way[i] - 1) * Mathf.PI / 2);
                activeCharaPos[1] += (int)Mathf.Sin((way[i] - 1) * Mathf.PI / 2);
                activeCharaStatus.posX = activeCharaPos[0];
                activeCharaStatus.posY = activeCharaPos[1];
                activeChara.GetComponent<Transform>().Translate(side * Mathf.Cos((way[i] - 1) * Mathf.PI / 2), -side * Mathf.Sin((way[i] - 1) * Mathf.PI / 2), 0);
            }
            yield return new WaitForSeconds(0.3f);
        }
        mapPosition.ObjectInformation(ref obstaclemap, ref charamap);
        IEnumerator coroutine = CharaActionWait(charamap[attackPos[0], attackPos[1]], command);
        StartCoroutine(coroutine);//変数に格納したコルーチンの呼び出し

        ////待機用の仮のスクリプト
        //yield return null;
        //IEnumerator coroutine = CharaActionWait(activeChara, (Command)3);
        //StartCoroutine(coroutine);//変数に格納したコルーチンの呼び出し
        yield break;
    }//一応完成

    private IEnumerator MouseButtonCheck()//マウスの判定
    {
        //mapPosition.ObjectInformation(ref obstaclemap, ref charamap);
        Debug.Log("MouseButtonCheck");
        activeChara.transform.position = activeCharaPrePos;//帰ってきた時用に
        //searchの説明：主に迂回折れ線距離を扱う際の一時的な配列。上下左右に新たなsearchを配置し、マップの2次元配列に値を書き込んでいく
        int[,] search = new int[maprange[0] * maprange[1], 4];//2次元目は{x,y,折れ線距離, 格納されていれば１}
        search[0, 0] = activeCharaPos[0];
        search[0, 1] = activeCharaPos[1];
        int x = search[0, 0];
        int y = search[0, 1];
        search[0, 3] = 1;

        maptips = GameObject.Find("MapManager").GetComponent<MapMain>().maptips;//maptipsをMapMainからGetComponent
        defaultColor = maptips[0, 0].GetComponent<SpriteRenderer>().color;
        maptips[x, y].GetComponent<SpriteRenderer>().color = movableColor;
        int judgex;
        int judgey;

        int mappingcount = 1;

        for (int n = 0; search[n, 3] != 0; n++)
        {

            if (search[n, 2] <= 2)//折れ線距離が3以内(n=0,1,2のマスの、隣のマスが移動可能(障害物がない)か判定する)
            {

                for (int rad = 0; rad < 4; rad++)
                {
                    judgex = search[n, 0] + (int)Mathf.Cos(rad * Mathf.PI / 2);
                    judgey = search[n, 1] + (int)Mathf.Sin(rad * Mathf.PI / 2);
                   
                    if (judgex >= 0 && judgey >= 0 && judgex <= maprange[0] - 1 && judgey <= maprange[1] - 1)
                    {
                      

                        //int[,] obstaclemap = mapPosition.obstaclemap;
                        if (obstaclemap[judgex, judgey] != 1 && maptips[judgex, judgey].GetComponent<SpriteRenderer>().color == defaultColor)
                        {

                            maptips[judgex, judgey].GetComponent<SpriteRenderer>().color = movableColor;
                            search[mappingcount, 0] = judgex;
                            search[mappingcount, 1] = judgey;
                            search[mappingcount, 2] = search[n, 2] + 1;
                            search[mappingcount, 3] = 1;
                            mappingcount++;

                        }
                    }
                }
            }
        }
        bool choice = false;//while文脱出のための変数
        while (choice == false)//行動が確定しない限りマスが青のままでキャラが移動しない
        {


            if (Input.GetMouseButtonDown(0) == true)//クリックして
            {
                mouseUpdate();
                //Debug.Log("キャラの座標とクリックした座標"+activeCharaPos[0] + "," + activeCharaPos[1] + "," + mousePos[0] + "," + mousePos[1]);
                if (mousePos[0] == activeCharaPos[0] && mousePos[1] == activeCharaPos[1])
                {
                    //Debug.Log("バカヤロウ");
                    //バトルコマンド呼び出し
                    IEnumerator coroutine = PlayerBattleCommand();
                    StartCoroutine(coroutine);
                }
                else if (maptips[mousePos[0], mousePos[1]].GetComponent<SpriteRenderer>().color == movableColor)//クリックしたマスが移動範囲内のマスならば
                {
                    //キャラ移動のメソッド
                    IEnumerator coroutine = Move(mousePos);
                    StartCoroutine(coroutine);
                }
                else
                {
                    //キャラ選択に戻る
                    IEnumerator coroutine = ActivateAlly();
                    StartCoroutine(coroutine);
                }
                choice = true;
            }
            yield return null;
        }
        //マップチップの色の初期化  
        MaptipsReset();
        yield break;
    }
    
    private IEnumerator Move(int[] destination)//選択先に移動
    {
        Debug.Log("Move");
        int[] way = RouteSearch(activeCharaPos, destination);
        //Debug.Log("移動中の座標" + activeCharaPos[0] + "," + activeCharaPos[1]);
        for (int i = 0; i < 3; i++)
        {
            if(way[i] == 0)
            {
                //Debug.Log("移動が完了しました");
                break;
            }
            else
            {
                activeCharaPos[0] += (int)Mathf.Cos((way[i] - 1) * Mathf.PI / 2);
                activeCharaPos[1] += (int)Mathf.Sin((way[i] - 1) * Mathf.PI / 2);
                //Debug.Log("cos,sinの値"+(int)Mathf.Cos((way[i] - 1) * Mathf.PI / 2) + "," + (int)Mathf.Sin((way[i] - 1) * Mathf.PI / 2));
                //Debug.Log("移動中の座標"+activeCharaPos[0] + "," + activeCharaPos[1]);
                activeChara.GetComponent<Transform>().Translate(side * Mathf.Cos((way[i] - 1) * Mathf.PI / 2), -side * Mathf.Sin((way[i] - 1) * Mathf.PI / 2), 0);
            }
            yield return new WaitForSeconds(0.3f);
        }
        IEnumerator coroutine = PlayerBattleCommand();//以下2行はifで味方敵の判定はいらないのか
        StartCoroutine(coroutine);
        yield break;        
    }//完成

    private int[] RouteSearch(int[] location, int[] destination)//移動経路
    {
        obstaclemap[location[0], location[1]] = 0;//ついでにコメントアウト
        bool errorflag = false;
        bool findflag = false;
        if (location[0] == destination[0] && location[1] == destination[1])
        { errorflag = true; }

        int[] way = new int[3];
        int[,] waymap = new int[maprange[0], maprange[1]];

        //searchの説明：主に迂回折れ線距離を扱う際の一時的な配列。上下左右に新たなsearchを配置し、マップの2次元配列に値を書き込んでいく
        int[,] search = new int[maprange[0] * maprange[1], 4];
        search[0, 0] = destination[0];
        search[0, 1] = destination[1];
        search[0, 2] = 1;
        search[0, 3] = 1;
        
        waymap[destination[0], destination[1]] = search[0, 2];

        int mappingcount = 1;

        int judgey = 0;
        int judgex = 0;

        //################destinationの座標からwaymapを作成######################
        for (int i = 0; search[i, 3] == 1 && search[i, 2] < search[0, 2] + 3; i++)
        {
            if (errorflag == true)
            {
                Debug.Log("エラー発生のためループ終了");
                break;
            }

            for (int rad = 0; rad < 4; rad++)
            {
                judgex = search[i, 0] + (int)Mathf.Cos(rad * Mathf.PI / 2);
                judgey = search[i, 1] + (int)Mathf.Sin(rad * Mathf.PI / 2);
                if (judgey >= 0 && judgey < maprange[1] && judgex >= 0 && judgex < maprange[0])
                {
                    if (obstaclemap[judgex, judgey] == 0 && waymap[judgex, judgey] == 0)
                    {
                        search[mappingcount, 0] = judgex;
                        search[mappingcount, 1] = judgey;
                        search[mappingcount, 2] = search[i, 2] + 1;
                        search[mappingcount, 3] = 1;
                        waymap[judgex, judgey] = search[i, 2] + 1;
                        mappingcount++;
                        //Debug.Log("爬虫類を家に持ち込むな" + judgex + "," + judgey);
                        if (judgex == location[0] && judgey == location[1])
                        {
                            //Debug.Log("タランチュラを食わせるzo☆");
                            findflag = true;
                            break;
                        }
                    }
                }
            }
            if(findflag == true)
            {
                break;
            }
        }
        if (findflag == false)
        {
            errorflag = true;
            Debug.Log("目的にたどり着くことができません");
        }
        else
        {
            Debug.Log("目的にたどり着くことができます。");
        }

        //#####################ここからwaypointがwaymapをたどる#########################
        int[] waypoint = new int[2];
        waypoint[0] = location[0];
        waypoint[1] = location[1];
        int waymaplevel = waymap[waypoint[0], waypoint[1]];
        for (int i = 0; waypoint[0] != destination[0] || waypoint[1] != destination[1]; i++)
        {
            if (errorflag == true)
            {
                Debug.Log("エラーが発生したためループを中断します。");
                break;
            }
            if (findflag == false)
            {
                Debug.Log("目的にたどり着くことができないのでループを中断します");
                break;
            }
            for (int rad = 0; rad < 4; rad++)
            {
                judgex = waypoint[0] + (int)Mathf.Cos(rad * Mathf.PI / 2);
                judgey = waypoint[1] + (int)Mathf.Sin(rad * Mathf.PI / 2);
                if (judgey >= 0 && judgey < maprange[1] && judgex >= 0 && judgex < maprange[0])
                {
                    if (obstaclemap[judgex, judgey] == 0 && waymap[judgex, judgey] != 0)
                    {
                        if (waymaplevel > waymap[judgex, judgey])
                        {
                            waypoint[0] = judgex;
                            waypoint[1] = judgey;
                            waymaplevel = waymap[judgex, judgey];
                            way[i] = rad + 1;
                            break;
                        }
                    }
                }
            }
        }
        //obstaclemap[location[0],location[1]] = 1;//移動先がキャンセル後も障害物となるバグのためコメントアウト
        return way;
    }//完成
    
    public void CommandInput(string buttonName)
    {

        Debug.Log(buttonName);
        switch (buttonName)
        {
            case "Button01":
                choosingchoice = 1;
                Debug.Log("攻撃");
                break;

            case "Button02":
                choosingchoice = 2;
                Debug.Log("回復");
                break;

            case "Button03":
                choosingchoice = 3;
                Debug.Log("待機");
                break;

            case "Button04":
                choosingchoice = 4;
                Debug.Log("キャンセル");
                break;
        }
    }

    public IEnumerator PlayerBattleCommand()//コマンド表示→コマンド選択を待つ→それぞれの処理を呼び出す（攻撃，回復の場合はキャラを選ぶという処理もここに書いたうえで）
    {
        yield return null;
        Debug.Log("PlayerBattleCommand");

        ////攻撃範囲内に敵味方がいるかの判定(途中でわけ分からんことになったからとりあえずコメントアウト)
        //int arrayX = activeChara.GetComponent<Charastatus>().posX;//味方キャラのx座標
        //int arrayY = activeChara.GetComponent<Charastatus>().posY;//味方キャラのy座標
        //int attackDistance = activeChara.GetComponent<Charastatus>().attackDistance;//味方キャラの攻撃範囲
        //GameObject[,] charamap = GameObject.Find("ObjectManager").GetComponent<MapPosition>().charamap;

        //bool attackFlag = false;

        //for (int i = 0; i < 20; i++)
        //{
        //    for (int j = 0; j < 10; j++)
        //    {
        //        if (Mathf.Abs(i - arrayX) + Mathf.Abs(j - arrayY) <= attackDistance)
        //        {
        //            if (charamap[i, j].tag == "enemy")
        //            {
        //                attackFlag = true;
        //            }
        //        }
        //    }
        //}

        //1.コマンド表示
        //コマンドとマウスの当たり判定については後々
        Canvas.SetActive(true);

        while(choosingchoice == 0)
        {
            yield return null;
        }

        Canvas.SetActive(false);

        mapPosition.ObjectInformation(ref obstaclemap, ref charamap);
        int attackDistance = activeCharaStatus.attackDistance;//味方キャラの攻撃範囲
        
        //コマンドが選択された後の処理
        bool targetFlag = false;//クリックしたキャラが敵キャラであった場合true
        if (choosingchoice == 1)//攻撃を選択
        {
            //マスの色付け
            for (int countX = 0; countX < maprange[0]; countX++)
            {
                for (int countY = 0; countY < maprange[1]; countY++)
                {
                    if (Mathf.Abs(countX - activeCharaPos[0]) + Mathf.Abs(countY - activeCharaPos[1]) <= attackDistance)
                    {
                        maptips[countX, countY].GetComponent<SpriteRenderer>().color = attackColor;
                    }

                }
            }
            
            while (targetFlag == false)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    mapPosition.ObjectInformation(ref obstaclemap, ref charamap);
                    mouseUpdate();
                    if (Mathf.Abs(mousePos[0] - activeCharaPos[0]) + Mathf.Abs(mousePos[1] - activeCharaPos[1]) <= attackDistance)
                    {
                        if (onMouseObject != null)
                        {
                            if (onMouseObject.tag == "enemy")
                            {
                                attackedChara = onMouseObject;
                                targetFlag = true;
                            }

                            if (onMouseObject.tag == "bomb")
                            {
                                attackedChara = onMouseObject;
                                targetFlag = true;
                            }
                        }
                    }
                    if(targetFlag == false)//攻撃対象をクリックしなかった場合この関数をもう一度開始
                    {
                        yield return null;
                        choosingchoice = 0;
                        MaptipsReset();
                        StartCoroutine("PlayerBattleCommand");
                        yield break;
                    }
                    
                }
                yield return null;
            }
            MaptipsReset();            
        }

        if (choosingchoice == 2)//回復を選択
        {
            bool magicAttacker = activeCharaStatus.magicAttacker;//兵種選別用の変数の呼び出し
            if (magicAttacker == false)//魔導士以外のキャラだったら
            {
                attackedChara = activeChara;
                //activeChara.GetComponent<CharaStatus>().Heal(attackedChara);//選択している味方キャラ(onMouseObject)の回復関数を呼び出し
            }

            if (magicAttacker == true)//魔導士だったら
            {
                for (int xcount = 0; xcount <= maprange[0] - 1; xcount++)
                {
                    for (int ycount = 0; ycount <= maprange[1] - 1; ycount++)
                    {
                        if(Mathf.Abs(xcount - activeCharaPos[0]) + Mathf.Abs(ycount - activeCharaPos[1]) <= 1)
                        {
                            maptips[xcount, ycount].GetComponent<SpriteRenderer>().color = healColor;
                        }
                    }
                }

                while (targetFlag == false)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        mouseUpdate();
                        if (Mathf.Abs(mousePos[0] - activeCharaPos[0]) + Mathf.Abs(mousePos[1] - activeCharaPos[1]) <= 1)//選択したキャラが回復範囲内なら
                        {
                            if (onMouseObject.tag == "ally")
                            {
                                attackedChara = onMouseObject;
                                targetFlag = true;
                            }
                        }
                        else
                        {
                            yield return null;
                            choosingchoice = 0;
                            //マスの色消し
                            MaptipsReset();
                            StartCoroutine("PlayerBattleCommand");
                            yield break;
                        }
                    }
                    yield return null;
                }
                //マスの色消し
                MaptipsReset();
            }
        }

        if (choosingchoice == 3)//待機を選択
        {
            attackedChara = activeChara;
            //Debug.Log("activeCharaPos[0]（待機選択直後）" + activeCharaPos[0]+","+ "activeCharaPos[1]" + activeCharaPos[1]);
        }

        if (choosingchoice == 4)
        {
            
            choosingchoice = 0;
            activeCharaPos[0] = activeCharaStatus.posX;
            activeCharaPos[1] = activeCharaStatus.posY;
            yield return null;
            StartCoroutine("MouseButtonCheck");
            yield break;
        }

        attackedCharaStatus = attackedChara.GetComponent<CharaStatus>();
        activeCharaStatus.posX = activeCharaPos[0];
        activeCharaStatus.posY = activeCharaPos[1];
        //Debug.Log( "移動後の座標(activecharaStatus,activeCharaPos)"+ activeCharaStatus.posX + "," + activeCharaStatus.posY + "," + activeCharaPos[0]+","+activeCharaPos[1]);
        yield return null;
        IEnumerator coroutine = CharaActionWait(attackedChara, (Command)choosingchoice);
        StartCoroutine(coroutine);//ここで攻撃，回復，待機の実際の処理を行う関数をコルーチンで呼び出し，開始
        choosingchoice = 0;
        yield break;
    }//コマンド表示、

    //maptipsの初期化
    private void MaptipsReset()
    {
        for (int countX = 0; countX < maprange[0]; countX++)
        {
            for (int countY = 0; countY < maprange[1]; countY++)
            {
                maptips[countX, countY].GetComponent<SpriteRenderer>().color = defaultColor;
            }
        }
    }

    private IEnumerator CharaActionWait(GameObject attackedChara, Command command)//行動選択をAttack,Heal,Waitに渡し，処理を待つ
    {
        Debug.Log("CharaActionWait");
        if ((int)command == 1)
        {
                activeCharaStatus.Attack(attackedChara);//選択している味方キャラ(activeChara)の攻撃関数を呼び出し，引数として敵キャラ(attackedChara)を渡している
                yield return new WaitForSeconds(animationTime);
                if (attackedChara.tag == "bomb")
                {
                    attackedChara.GetComponent<BomStatus>().BombAttack();
                    GameObject[] enemyBomb = GameObject.FindGameObjectsWithTag("enemy");
                    for(int i= 0; i < enemyBomb.Length; i++)
                    {
                        activeCharaStatus.CharaDestroy(enemyBomb[i]);
                    }
                }
                else
                {
                    activeCharaStatus.CharaDestroy(attackedChara);
                    activeCharaStatus.CharaExperience();
                }
                yield return null;
                GameJudge();
                if (attackedChara != null)
                {
                    int[] attackedCharaPos = new int[2];
                    if(attackedCharaStatus == null){ Debug.Log("attackedCharaStatus = null"); }
                    attackedCharaPos[0] = attackedCharaStatus.posX;
                    attackedCharaPos[1] = attackedCharaStatus.posY;
                        if (Mathf.Abs(attackedCharaPos[0] - activeCharaPos[0]) + Mathf.Abs(attackedCharaPos[1] - activeCharaPos[1]) <= attackedCharaStatus.attackDistance)
                        {
                            //↑攻撃したキャラと攻撃されたキャラの距離が攻撃されたキャラの攻撃範囲内であれば反撃
                            Debug.Log("反撃発生");
                            attackedCharaStatus.Attack(activeChara);//敵の反撃：先ほどとは逆に味方キャラ(attackedChara)を引数として渡し，敵キャラの攻撃関数を呼び出している。
                            attackedCharaStatus.AfterMoving(true);
                            //actionFlagの変更とカラー変更を同じ関数でまとめて↑で呼ぶ
                            yield return new WaitForSeconds(animationTime);
                            attackedCharaStatus.CharaDestroy(activeChara);
                            GameJudge();
                        }
                }
                activeCharaStatus.LevelUp();
            
            //if (onMouseObject.tag == "bomb")//攻撃した相手が火薬だった時
            //{
            //    onMouseObject.GetComponent<BomStatus>().BomAttack();
            //    yield return new WaitForSeconds(animationTime);
            //    activeCharaStatus.CharaDestroy(attackedChara);
            //}
        }

        if ((int)command == 2)
        {
            activeCharaStatus.Heal(attackedChara);//選択している味方キャラ(onMouseObject)の回復関数を呼び出し
            yield return new WaitForSeconds(animationTime);
        }
        if ((int)command == 3)
        {
            //キャラを移動後の座標に確定させるスクリプトを記述する必要あり
            activeCharaStatus.CharaWait();
        }
        yield return null;
        IEnumerator coroutine = EncampmentChange();
        StartCoroutine(coroutine);
        yield break;
    }//完成

    //private void CharaAliveJudge(GameObject activeChara)//生存判定
    //{
    //    charaStatus = onMouseObject.GetComponent<CharaStatus>();

    //    playerHP = charaStatus.hp;

    //}


    private void GameJudge()//ゲーム続行判定
    {
       Debug.Log("GameJudge");
        GameObject[] Ally = GameObject.FindGameObjectsWithTag("ally");
        GameObject[] Enemy = GameObject.FindGameObjectsWithTag("enemy");
        if(Ally.Length == 0)
        {
            BattleEnd(false);
        }
        if(Enemy.Length == 0)
        {
            //ここでLvUp
            activeCharaStatus.LevelUp();
            BattleEnd(true);
        }
    }//完成

    private void LevelUp()//
    {
        
    }//不必要？

    private void TurnManagement()//次が敵ターンなら0、味方ターンなら1を返す
    {
       Debug.Log("TurnManagement");
        int actionFlag = 0;
        IEnumerator turnStart;

        //PlayerStatusのactionFlagがtrue状態の味方の数が0のときactivateAlly=0で返す
        //playerstatus.csでactionFlagは宣言してる。キャラが未行動の時trueで宣言してます。
        if(enemyTurnCount == 0)
        {
            GameObject[] playerMovedChara = GameObject.FindGameObjectsWithTag("ally");
            for (int i = 0; i < playerMovedChara.Length; i++)
            {
                if(playerMovedChara[i].GetComponent<CharaStatus>().actionFlag == true)
                {
                    actionFlag++;
                }
            }
            if (actionFlag >= 1)
            {
                turnStart = TurnStart(true);//味方ターン
            }
            else
            {
                turnStart = TurnStart(false);//敵ターン
            }
        }
        else
        {
            GameObject[] enemyMovedChara = GameObject.FindGameObjectsWithTag("enemy");
            for (int i = 0; i < enemyMovedChara.Length; i++)
            {
                if (enemyMovedChara[i].GetComponent<CharaStatus>().actionFlag == true)
                {
                    actionFlag++;
                }
            }
            if (actionFlag >= 1)
            {
                turnStart = TurnStart(false);//敵ターン
            }
            else
            {
                //制限ターン判定
                bool[] gameJudge = new bool[2];//{制限ターン到達でtrue, 味方勝利でtrue}
                if (turn >= judgementTurn)
                {
                    gameJudge[0] = true;
                    int playerEncamNum = 0;
                    int enemyEncamNum = 0;
                    for (int i = 0; i < encampmentFlag.Length; i++)
                    {
                        if (flagproperties[i, 2] == 0)
                        {
                            playerEncamNum++;
                        }
                        else
                        {
                            enemyEncamNum++;
                        }
                    }
                    Debug.Log(turn + "ターン目 " + "味方拠点の数:" + playerEncamNum + "敵拠点の数:" + enemyEncamNum);
                    if (playerEncamNum - enemyEncamNum > 0)
                    {
                        gameJudge[1] = true;
                    }
                    else
                    {
                        gameJudge[1] = false;
                    }
                }
                if (gameJudge[0] == true)
                {
                    BattleEnd(gameJudge[1]);
                }
                //ターン渡し
                turn++;
                Debug.Log(turn + "ターン目");
                turnStart = TurnStart(true);//味方ターン
            }
        }
        
        //if (playerMovedChara[0].actionFlag == false && playerMovedChara[1].actionFlag == false && ...)
        StartCoroutine(turnStart);
    }//多分完成

    private void BattleEnd(bool gameJudge)//味方勝利でtrue,敵勝利でfalse
    {
        Debug.Log("バトル終了 結果(trueで味方勝利)→" + gameJudge);
        //ゲーム終了、次のステージへのscene遷移もしくはGAMEOVER
        if (gameJudge == true)
        {
            ////Save関連の関数呼んでから次のシーン呼んでいただけるとありがたいです。
            //DataManagerClass.SaveChara();
            //DataManagerClass.SaveStage();
            ////SceneManagerをBattleClassから呼ぶのか一度GameManagementClassを経由するのか、そもそもSceneManagerすら使わずに直接呼ぶのか、それぞれのシーン名は後程決定？

            if (CurrentSceneName == "battle1")
            {
                SceneManager.LoadScene("story1-2");
            }
            if (CurrentSceneName == "battle2")
            {
                SceneManager.LoadScene("story2-2");
            }
            if (CurrentSceneName == "battle3")
            {
                SceneManager.LoadScene("story3-2");
            }
        }
        if (gameJudge == false)
        {
            //GameOverScene呼ぶ
            SceneManager.LoadScene("GameOver");
        }
    }//一応終了？統合の時大幅変更の可能性有


    public Sprite[] stateImage;
    private IEnumerator EncampmentChange()//陣地更新
    {
        for (int i = 0; i < encampmentFlag.Length; i++)
        {
            if(activeCharaPos[0] == flagproperties[i,0] && activeCharaPos[1] == flagproperties[i, 1])
            {
                if(activeChara.tag == "ally")
                {
                    Debug.Log("この旗は味方のもの");
                    //味方の旗の色に変える処理
                    encampmentFlag[i].GetComponent<SpriteRenderer>().sprite = stateImage[0];//UnityのobjectのInspector上でScriptのタブから、Sizeは2、Element0は味方の旗、Element1は敵の旗のspriteを設定
                    //SE
                    flagproperties[i, 2] = 0;
                }
                else
                {
                    Debug.Log("この旗は敵のもの");
                    //敵の旗の色に変える処理
                    encampmentFlag[i].GetComponent<SpriteRenderer>().sprite = stateImage[1];
                    //SE
                    flagproperties[i, 2] = 1;
                }
            }
        }
        //gameJudge = gameObject.GetComponent<GameManagement>().EncampmentJudge();
        yield return null;
        TurnManagement();
        yield break;
    }//SE、旗の色を変える処理、GameManagementクラスの関数を持ってくる
}
