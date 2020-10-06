using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectMappingOptimized : MonoBehaviour
{
    /*メモ
     * パブリック変数→serializefield private
     * static は必要な時のみつける
     * privateかpublicか明示的に記述する
     * enum の変数に代入するときはenum名.???と代入する
     * リージョンを使って閉じる
     * デリゲイトの利用(callback, )
     * 配列の初期化方法（nullの代入）の調査
     */
    //#####################################################################################################
    //#                 以下調整可能な値
    //#####################################################################################################
    #region 調整可能値
    //行動傾向選択にかかわる値の調整
    //積極的傾向を選択するのに必要なHP割合
    public float behaveDecide_HpRatio = 0.5f;
    //陣地重視傾向を選択するのに必要な陣地割合
    public float behaveDecide_EncamRatio = 0.5f;
    //回復を使うHP割合
    public float healhp = 0.85f;
    //係数、ゲイン調整
    //攻撃選択時
    public float attack_encampment_coefficient = 0.1f;//攻撃行動選択時の陣地マップ最終合成係数
    public float attack_hate_initial_coefficient = 0.01f;//影響マップ合成係数の内、ヘイト値がターン0で持つ影響力
    public float attack_hate_final_coefficient = 0.3f;//影響マップ合成係数の内、ヘイト値の影響力が収束する値
    public float attack_hate_coefficient_rate = 0.7f;//ヘイト値が持つ影響力のターン数に対する増加率
    public float attack_coefficient_gain = 8f;//全味方HP割合に対する孤立度とダメージ割合見込みのそれぞれの影響力の増加率
    //退避選択時
    public float evacuate_encampment_coefficient = 0.1f;//退避行動選択時の陣地マップ最終合成係数
    //占領選択時
    public float capture_flagcoefficient = 0.2f;//占領行動選択時の陣地旗基本影響マップの最終合成係数
    //防衛選択時
    public float defense_flagcoefficient = 0.2f;//防衛行動選択時の陣地旗基本影響マップの最終合成係数
    #endregion

    //######################################################################################################
    //#                 ここから変数、定数の宣言
    //######################################################################################################
    #region 変数宣言
    //マップ用2次元配列宣言
    readonly static int[] maprange = { 20, 10 };          //マップの最大範囲
    int[,] obstaclemap = new int[maprange[0], maprange[1]]; //移動可能障害物も含めた障害物マップ。通行可能:0 不可能:1。ただし、どちらがx,yになるかは未決定。とりあえず[x,y]で書いた
    float[,] encampmentmap = new float[maprange[0], maprange[1]];//ステータスにバフのかかる範囲を示したマップ　敵陣地を1、味方陣地を0とする
    GameObject[,] charamap = new GameObject[maprange[0], maprange[1]];//特に使用しないがObjectInformationを呼ぶために必要

    //ゲームオブジェクト、インスタンス宣言
    MapPosition mapPosition;
    BattleMapManager battleMapManager;
    GameManagement gameManagement;
    CharaStatus enemyStatus;
    GameObject[] player;
    CharaStatus[] playerStatus;

    //陣地、ターンの変数、配列宣言
    int encampmentnum;//陣地数
    int[,] encampmentflag = new int[10, 3];//{x,y,0:味方1 :敵}
    int pencam;//味方陣地数
    int eencam;//敵陣地数
    int turn;//ターン数(0から始まるものとする)

    //敵のステータス変数宣言
    int enemyx;//敵のx座標
    int enemyy;//敵のy座標
    int enemyatk;//敵の攻撃力
    int enemydef;//敵の防御力
    int enemyhp;//敵のhp
    int enemyhpmax;//敵の最大hp
    bool eLongRangeAttack;//遠距離攻撃可能であればtrue
    bool eFarAttacker;//魔術師であればtrue
    int enemyJob;//敵の職業{標準 = 0, 警備 = 1, 索敵 = 2}
    int eprotectionposx;//職業によって守る使命を与えられているx座標
    int eprotectionposy;//職業によって守る使命を与えられているy座標

    //味方のステータス配列宣言
    int[] pPosx = new int[4];//味方のx座標配列
    int[] pPosy = new int[4];//味方のy座標配列
    int[] pAtk = new int[4];//味方の攻撃力配列
    int[] pDef = new int[4];//味方の防御力配列
    int[] pHp = new int[4];//味方の現hp配列
    int[] pHpmax = new int[4];//味方の最大hp配列
    int[,] pAct = new int[4, 256];//味方の行動固有値配列(ターンごとに格納)

    //enumの定義
    enum BattleChoice                                       //選択肢の定義
    {
        NULL, ATK, EVC, CAP, DEF                              //ATK:攻撃 EVC:退避 CAP:占領 DEF:防衛
    }
    enum BattleCommand
    {
        NULL, ATTACK, HEAL, WAIT
    }

    //結果等を格納する変数、配列の宣言
    BattleChoice battlechoice = BattleChoice.NULL;                              //行動選択結果
    BattleCommand battlecommand = 0;                            //コマンド選択結果
    public float[,] effectmap = new float[maprange[0], maprange[1]];   //結果を保存する2次元配列
    int[] destination = new int[2];                             //移動先
    int[] attackpos = new int[2];                               //攻撃先座標
    #endregion

    //######################################################################################################
    //#             変数、配列の初期化
    //######################################################################################################


    //ゲームオブジェクトの格納、インスタンスの作成など
    // Use this for initialization
    void Start() {
        mapPosition = GameObject.Find("ObjectManager").GetComponent<MapPosition>();
        battleMapManager = GameObject.Find("BattleMapManager").GetComponent<BattleMapManager>();//オブジェクト名入力
        encampmentnum = GameObject.FindGameObjectsWithTag("flag").Length;
        gameManagement = GameObject.Find("Main Camera").GetComponent<GameManagement>();
        enemyStatus = this.GetComponent<CharaStatus>();
        playerStatus = new CharaStatus[4];
        player = GameObject.FindGameObjectsWithTag("ally");
        for (int i = 0; i < player.Length; i++)
        {
            playerStatus[i] = player[i].GetComponent<CharaStatus>();
        }
    }

    //EffectMappingが呼ばれるたびにフィールド変数の値を更新する
    private void FieldUpdate()
    {
        //ゲーム全体に関して
        mapPosition.ObjectInformation(ref obstaclemap, ref charamap);
        turn = battleMapManager.turn;
        encampmentflag = battleMapManager.flagproperties;
        pencam = 0;
        eencam = 0;
        for (int i = 0; i < encampmentnum; i++)
        {
            if (encampmentflag[i, 2] == 0)
            {
                pencam++;
            }
            else
            {
                eencam++;
            }
        }
        //敵に関して
        enemyx = enemyStatus.posX;
        enemyy = enemyStatus.posY;
        enemyatk = enemyStatus.attack;
        enemydef = enemyStatus.defense;
        enemyhp = enemyStatus.hp;
        enemyhpmax = enemyStatus.maxhp;
        eLongRangeAttack = enemyStatus.farAttacker;
        enemyJob = enemyStatus.actionKind;
        eprotectionposx = enemyStatus.saveX;
        eprotectionposy = enemyStatus.saveY;
        //味方に関して                                                                ←{# 死亡判定に気を付けつつ #}
        for (int i = 0; i < 4; i++)
        {
            if (player[i] != null)
            {
                pPosx[i] = playerStatus[i].posX;
                pPosy[i] = playerStatus[i].posY;
                pAtk[i] = playerStatus[i].attack;
                pDef[i] = playerStatus[i].defense;
                pHp[i] = playerStatus[i].hp;
                pHpmax[i] = playerStatus[i].maxhp;
                for (int ti = 0; ti <= turn; ti++)
                {
                    pAct[i, ti] = playerStatus[i].actionLevel[ti];
                }
            }
            else
            {
                pPosx[i] = 0;
                pPosy[i] = 0;
                pHp[i] = 0;
            }
        }
        //結果用変数、配列に関して
        battlechoice = 0;
        battlecommand = 0;
        effectmap = new float[maprange[0], maprange[1]];
        destination = new int[2];
        attackpos = new int[2];
    }

    //################################################################################################
    //#             行動選択処理から結果の出力
    //################################################################################################
    //行動選択から各種関数を呼び出し結果を配列にまとめて出力
    public int[] EffectMapping()
    {
        FieldUpdate();//ステータス、マップ情報などの変数、配列を更新
        obstaclemap[enemyx, enemyy] = 0;                    //行動する敵自体は障害物ではないと認識
        if ((float)enemyhp / enemyhpmax >= behaveDecide_HpRatio)//行動の決定、影響マップの生成方法を決める。この決定がコマンドを直接決定するわけではない。
        {
            if ((float)eencam / encampmentnum >= behaveDecide_EncamRatio)
            {
                battlechoice = BattleChoice.ATK;//攻撃行動
            }
            else if ((float)eencam / encampmentnum < behaveDecide_EncamRatio)
            {
                battlechoice = BattleChoice.CAP;//占領行動
            }
        }
        else if ((float)enemyhp / enemyhpmax < behaveDecide_HpRatio)
        {
            if ((float)eencam / encampmentnum >= behaveDecide_EncamRatio)
            {
                battlechoice = BattleChoice.EVC;//退避行動
            }
            else if ((float)eencam / encampmentnum < behaveDecide_EncamRatio)
            {
                battlechoice = BattleChoice.DEF;//防衛行動
            }
        }

        switch (battlechoice)//行動選択から分岐し、以下の関数内でeffectmapが生成される
        {
            case BattleChoice.ATK:
                Debug.Log("攻撃");
                switch (enemyJob)//攻撃行動選択時はさらに3つに分かれる
                {
                    case 0:
                        Attack();
                        break;
                    case 1:
                        Guard();
                        break;
                    case 2:
                        Search();
                        break;
                    default:
                        Debug.Log("エラー(影響マップ):存在しないはずの選択肢が選択されました。");
                        break;
                }
                break;
            case BattleChoice.EVC:
                Debug.Log("退避");
                Evacuate();
                break;
            case BattleChoice.CAP:
                Debug.Log("占領");
                Capture();
                break;
            case BattleChoice.DEF:
                Debug.Log("防衛");
                Defense();
                break;
            case BattleChoice.NULL:
                Debug.Log("エラー(影響マップ)行動が選択されませんでした。");
                break;
            default:
                Debug.Log("エラー(影響マップ):存在しないはずの選択肢が選択されました。");
                break;
        }
        int[] test1 = DecideDestination();
        destination[0] = test1[0];//生成した影響マップから移動目的座標を決定
        destination[1] = test1[1];
        attackpos[0] = destination[0];
        attackpos[1] = destination[1];
        if (battlechoice == BattleChoice.ATK || battlechoice == BattleChoice.DEF)
        {
            int[] test2 = AttackPos();
            attackpos[0] = test2[0];//攻撃座標を決定
            attackpos[1] = test2[1];
        }
        if (attackpos[0] == destination[0] && attackpos[1] == destination[1])//攻撃行動を選択しても攻撃可能範囲に味方がない場合回復か待機が選択される
        {
            if (enemyhp / enemyhpmax <= healhp)
            {
                battlecommand = BattleCommand.HEAL;//回復
            }
            else
            {
                battlecommand = BattleCommand.WAIT;//待機
            }
        }
        else
        {
            battlecommand = BattleCommand.ATTACK;
        }
        obstaclemap[enemyx, enemyy] = 1;//敵の位置を障害物に直す

        int[] result = { destination[0], destination[1], (int)battlecommand, attackpos[0], attackpos[1] };//結果の格納

        Debug.Log(enemyx + "," + enemyy + "のターン " + destination[0] + "," + destination[1] + "," + result[2] + "," + result[3] + "," + result[4]);
        return result;
    } 

    //#################################################################################################
    //#                 ここから各行動選択時に呼び出される関数
    //#################################################################################################

    //攻撃選択時に呼び出される関数
    private void Attack()
    {
        //ローカル変数宣言
        float[] relief = new float[4];//救援可能性
        float[] normalizedRelief = new float[4];//normalized~:正規化された値
        float[] isolate = new float[4];//孤立度
        float[] normalizedIsolate = new float[4];
        float[] hate = new float[4];//ヘイト値
        float[] normalizedHate = new float[4];
        float[] prospect = new float[4];//ダメージ割合見込み
        float[] normalizedProspect = new float[4];
        float isolatecoefficient = 0;//係数の初期化
        float prospectcoefficient = 0;//係数の初期化
        float hatecoefficient = 0;//係数の初期化
        float pHpratio = 0;//プレイヤー全体のHp割合
        float[] effectcoefficient = new float[4];//影響マップ合成係数
        float[,,] attackmap = new float[4,maprange[0], maprange[1]];//影響マップ合成用にマップ行列を格納するための3次元配列
        float[,] fusedattackmap = new float[maprange[0], maprange[1]];//合成された影響マップ

        //孤立度の計算
        int aliveally = 0;//生存味方数
        int[,] distancetable = new int[4, 4];//縦軸横軸にそれぞれ味方をとったキャラ同士の距離を測る表
        for (int ally1 = 0; ally1 < 4; ally1++)
        {
            if(pHp[ally1] > 0)
            {
                aliveally++;
                for (int ally2 = 0; ally2 < 4; ally2++)
                {
                    if (pHp[ally2] > 0 && ally1 != ally2)
                    {
                        distancetable[ally1, ally2] = PolygonalDistance(pPosx[ally1], pPosy[ally1], pPosx[ally2], pPosy[ally2]);
                        relief[ally1] += pAtk[ally2] / PolygonalDistance(pPosx[ally1], pPosy[ally1], pPosx[ally2], pPosy[ally2]);
                    }
                }
            }
        }
        for (int ally = 0; ally < 4; ally++)
        {
            if (pHp[ally] > 0)
            {
                if(aliveally > 1)
                {
                    normalizedRelief[ally] = relief[ally] / (relief[0] + relief[1] + relief[2] + relief[3]);
                    isolate[ally] = 1 - normalizedRelief[ally];
                    normalizedIsolate[ally] = isolate[ally] / (aliveally - 1);
                }
                else
                {
                    normalizedIsolate[ally] = 1f;
                }
            }
        }

        //ダメージ割合見込みの計算
        int damage;
        for(int ally = 0; ally < 4; ally++)
        {
            if(pHp[ally] > 0)
            {
                if (enemyatk <= pDef[ally])
                {
                    damage = 0;
                }
                else
                {
                    damage = enemyatk - pDef[ally];
                }
                if (pHp[ally] - damage <= 0)
                {
                    prospect[ally] = 1;
                }
                else
                {
                    prospect[ally] = 1 - ((pHp[ally] - damage) / pHpmax[ally]);
                }
            }
        }
        for(int ally = 0; ally < 4; ally++)
        {
            if(pHp[ally] > 0 && prospect[ally] > 0)
            {
                normalizedProspect[ally] = prospect[ally] / (prospect[0] + prospect[1] + prospect[2] + prospect[3]);
            }
        }

        //ヘイト値の計算
        for(int ally = 0; ally < 4; ally++)
        {
            if(pHp[ally] > 0)
            {
                for (int actturn = 0; actturn <= turn; actturn++)
                {
                    hate[ally] += (float)pAct[ally, actturn] / (turn - actturn + 1);
                }
            }
        }
        for(int ally = 0; ally < 4; ally++)
        {
            if(pHp[ally] > 0 && hate[ally] > 0)
            {
                normalizedHate[ally] = hate[ally] / (hate[0] + hate[1] + hate[2] + hate[3]);
            }
        }

        //ヘイト値の影響度係数を決定
        hatecoefficient = attack_hate_final_coefficient / (1 + (attack_hate_final_coefficient / attack_hate_initial_coefficient - 1) * Mathf.Exp(-attack_hate_coefficient_rate * (turn + 1)));
        //味方全体のHP割合
        pHpratio = (pHp[0] + pHp[1] + pHp[2] + pHp[3]) / (pHpmax[0] + pHpmax[1] + pHpmax[2] + pHpmax[3]);
        //孤立度の影響度係数を決定
        isolatecoefficient = (1 - hatecoefficient) / (1 + Mathf.Exp(-1 * attack_coefficient_gain * (pHpratio - 0.5f)));
        //ダメージ割合見込みの影響度係数を決定
        prospectcoefficient = 1 - hatecoefficient - isolatecoefficient;
        //孤立度、ダメージ割合見込み、ヘイト値とそれぞれの影響度係数をかけてその和を影響マップの係数とする
        for (int ally = 0; ally < 4; ally++)
        {
            Debug.Log(normalizedHate[ally] + "," + normalizedIsolate[ally] + "," + normalizedProspect[ally]);
            effectcoefficient[ally] = isolatecoefficient * normalizedIsolate[ally] + prospectcoefficient * normalizedProspect[ally] + hatecoefficient * normalizedHate[ally];
      
        }
        for (int ally = 0; ally < 4; ally++)
        {
            if (eLongRangeAttack == true)
            {
                MapStore(ref attackmap, BEMforFarAttacker(pPosx[ally], pPosy[ally]), ally);
            }
            else
            {
                MapStore(ref attackmap, BaseEffectMapping(pPosx[ally], pPosy[ally], 0), ally);
            }
        }
        fusedattackmap = MapFusion(attackmap, pHp, effectcoefficient);
        effectmap = MixWithEncampmentMap(ReverseMap(fusedattackmap), attack_encampment_coefficient, true);
    }


    //退避選択時に呼び出される関数
    private void Evacuate()
    {
        //ローカル変数宣言
        float[,] evacuatemap = new float[maprange[0], maprange[1]];//合成された影響マップ
        float[,,] playermap = new float[4,maprange[0], maprange[1]];//影響マップ合成用にマップ行列をまとめた３次元配列
        float[] coefficient = new float[4];//係数

        for (int pnum = 0; pnum < 4; pnum++)
        {
            MapStore(ref playermap, BaseEffectMapping(pPosx[pnum], pPosy[pnum], 0), pnum);
        }
        pAtk.CopyTo(coefficient, 0);//int→floatのキャストの代用
        evacuatemap = MapFusion(playermap, pHp, coefficient);
        effectmap = MixWithEncampmentMap(evacuatemap, evacuate_encampment_coefficient, true);
    }


    //占領選択時に呼び出される関数
    private void Capture()
    {
        //ローカル変数宣言
        float[,,] capturemap = new float[pencam,maprange[0], maprange[1]];//陣地旗基本マップを合成のためにまとめたもの
        float[,] fusedcapturemap = new float[maprange[0], maprange[1]];//陣地旗基本マップを合成したもの
        float[] capturecoefficient = new float[pencam];//陣地旗合成のための係数配列
        int[] capturejudge = new int[pencam];//陣地旗合成のための判定式配列
        for (int i = 0; i < pencam; i++)
        {
            capturecoefficient[i] = 1;
            capturejudge[i] = 1;
        }//等倍合成のため判定式、係数を1で初期化

        float[,,] playermap = new float[4,maprange[0], maprange[1]];//味方基本影響マップ行列を合成するためにまとめたもの
        float[,] fusedplayermap = new float[maprange[0], maprange[1]];//味方基本マップを合成したもの
        float[] playercoefficient = {1, 1, 1, 1};//等倍合成のため係数1

        float[,,] playercapturemap = new float[2, maprange[0], maprange[1]];//最終合成のための行列をまとめたもの

        for(int pnum = 0; pnum < 4; pnum++)
        {
            MapStore(ref playermap, BaseEffectMapping(pPosx[pnum], pPosy[pnum], 0), pnum);
        }
        fusedplayermap = MapFusion(playermap, pHp, playercoefficient);

        for(int encamcount = 0, playerencamcount = 0; encamcount < pencam; encamcount++)
        {
            if(encampmentflag[encamcount, 2] == 0)//encamcount番目の陣地が味方陣地である
            {
                MapStore(ref capturemap, BaseEffectMapping(encampmentflag[encamcount, 0], encampmentflag[encamcount, 1], 1), playerencamcount);
                //↑2次元配列「陣地旗」encamcount番目の座標を用いて作成した基本影響マップを合成用の3次元配列のplayercount番目のに格納
                playerencamcount++;
            }
        }
        fusedcapturemap = MapFusion(capturemap, capturejudge, capturecoefficient);//マップ3次元配列はすべて有効、係数はすべて1

        MapStore(ref playercapturemap, fusedplayermap, 0);
        MapStore(ref playercapturemap, ReverseMap(fusedcapturemap), 1);

        int[] effectjudge = {1, 1};
        float[] effectcoefficient = {1 - capture_flagcoefficient, capture_flagcoefficient};
        effectmap = MapFusion(playercapturemap, effectjudge, effectcoefficient);
    }


    //防衛選択時に呼び出される関数
    private void Defense()
    {
        //ローカル変数宣言
        float[,,] defensemap = new float[eencam, maprange[0], maprange[1]];//陣地旗基本マップを合成のためにまとめたもの
        float[,] fuseddefensemap = new float[maprange[0], maprange[1]];//陣地旗基本マップを合成したもの
        float[] defensecoefficient = new float[eencam];//陣地旗合成のための係数配列
        int[] defensejudge = new int[eencam];//陣地旗合成のための判定式配列
        for (int i = 0; i < eencam; i++)
        {
            defensecoefficient[i] = 1;
            defensejudge[i] = 1;
        }//等倍合成のため、判定式、係数を1で初期化

        float[,,] playermap = new float[4, maprange[0], maprange[1]];//味方基本影響マップ行列を合成するためにまとめたもの
        float[,] fusedplayermap = new float[maprange[0], maprange[1]];//味方基本マップを合成したもの
        float[] playercoefficient = { 1, 1, 1, 1 };//等倍合成のため係数1

        float[,,] playerdefensemap = new float[2, maprange[0], maprange[1]];//最終合成のための行列をまとめたもの

        for (int pnum = 0; pnum < 4; pnum++)
        {
            MapStore(ref playermap, BaseEffectMapping(pPosx[pnum], pPosy[pnum], 0), pnum);
        }
        fusedplayermap = MapFusion(playermap, pHp, playercoefficient);

        for (int encamcount = 0, enemyencamcount = 0; encamcount < eencam; encamcount++)
        {
            if (encampmentflag[encamcount, 2] == 1)//encamcount番目の陣地が敵陣地である
            {
                MapStore(ref defensemap, BaseEffectMapping(encampmentflag[encamcount, 0], encampmentflag[encamcount, 1], 1), enemyencamcount);
                //↑2次元配列「陣地旗」encamcount番目の座標を用いて作成した基本影響マップを合成用の3次元配列のplayercount番目のに格納
                enemyencamcount++;
            }
        }
        fuseddefensemap = MapFusion(defensemap, defensejudge, defensecoefficient);//マップ3次元配列はすべて有効、係数はすべて1

        MapStore(ref playerdefensemap, ReverseMap(fusedplayermap), 0);
        MapStore(ref playerdefensemap, fuseddefensemap, 1);

        int[] effectjudge = { 1, 1 };
        float[] effectcoefficient = { 1 - defense_flagcoefficient, defense_flagcoefficient };
        effectmap = MapFusion(playerdefensemap, effectjudge, effectcoefficient);
    }


    //索敵選択時に呼び出される関数
    private void Search()
    {
        //ローカル変数宣言
        float[,,] searchmap = new float[4, maprange[0], maprange[1]];//合成用にマップ行列をまとめた3次元配列
        float[,] fusedsearchmap = new float[maprange[0], maprange[1]];//合成された影響マップ
        bool attackflag = false;//移動範囲内に敵が存在すればtrue
        int[] judge = new int[4];//合成するかどうかの判定式
        float[] coefficient = { 1, 1, 1, 1 };//係数

        for (int ally = 0; ally < 4; ally++)
        {
            if(PolygonalDistance(pPosx[ally], pPosy[ally], enemyx, enemyy) <= 3)
            {
                attackflag = true;
                judge[ally] = 1;
                if (eLongRangeAttack == true)
                {
                    MapStore(ref searchmap, BEMforFarAttacker(pPosx[ally], pPosy[ally]), ally);
                }
                else
                {
                    MapStore(ref searchmap, BaseEffectMapping(pPosx[ally], pPosy[ally], 0), ally);
                }
            }
        }
        if(attackflag == true)
        {
            effectmap = ReverseMap(MapFusion(searchmap, judge, coefficient));
        }
        else
        {
            effectmap = ReverseMap(BaseEffectMapping(eprotectionposx, eprotectionposy, 1));
        }
    }


    //警備選択時に呼び出される関数
    private void Guard()
    {
        effectmap = ReverseMap(BaseEffectMapping(eprotectionposx, eprotectionposy, 1));
    }


    //#############################################################################################################################################
    //#             以下マップ生成、合成で利用する関数
    //##############################################################################################################################################

    //1つの2次元配列を1つの3次元配列のn番目の行列として代入
    private void MapStore(ref float[,,] result, float[,] map, int n)
    {
        for(int xcount = 0; xcount < maprange[0]; xcount++)
        {
            for (int ycount = 0; ycount < maprange[1]; ycount++)
            {
                result[n, xcount, ycount] = map[xcount, ycount];
            }
        }
    }
    

    //1組の座標から合成前の影響マップを2次元配列で作成。
    private float[,] BaseEffectMapping(int x, int y, int mode)//mode 0:中心座標の隣からカウント mode 1:中心座標からカウント
    {
        float[,] result = new float[maprange[0], maprange[1]];
        int[,] search = new int[maprange[0] * maprange[1], 4];//{x,y,折れ線距離, 格納されていれば１}

        search[0, 0] = x;
        search[0, 1] = y;
        search[0, 3] = 1;

        if (mode == 1)
        {
            result[x, y] = 1;
            search[0, 2] = 1;
        }

        int judgex;
        int judgey;

        int mappingcount = 1;

        for (int n = 0; search[n, 3] != 0; n++)
        {
            for(int rad = 0; rad < 4; rad++)
            {
                judgex = search[n, 0] + (int)Mathf.Cos(rad * Mathf.PI / 2);
                judgey = search[n, 1] + (int)Mathf.Sin(rad * Mathf.PI / 2);
                if (judgex >= 0 && judgey >= 0 && judgex <= maprange[0] - 1 && judgey <= maprange[1] - 1)
                {
                    if (obstaclemap[judgex, judgey] != 1 || charamap[judgex, judgey] != null)
                    {
                        if (result[judgex, judgey] == 0)
                        {
                            if (judgex != x || judgey != y)
                            {
                                result[judgex, judgey] = (float)1 / (1 + search[n, 2]);
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
        }
        return result;
    }


    //遠距離キャラ用の基本影響マップ生成関数
    private float[,] BEMforFarAttacker(int x, int y)
    {
        //合成する基本影響マップの数は合計8個
        float[,,] FABEM = new float[8, maprange[0], maprange[1]];//合成用にマップ行列をまとめた配列
        int[] judge = { 1, 1, 1, 1, 1, 1, 1, 1 };//合成判定式
        float[] coefficient = { 1, 1, 1, 1, 1, 1, 1, 1 };//係数
        int rootPosx;
        int rootPosy;
        int MappingPosx;
        int MappingPosy;
        int storingcount = 0;
        for(int rad1 = 0; rad1 < 4; rad1++)
        {
            rootPosx = x + (int)Mathf.Cos(rad1 * Mathf.PI / 2);
            rootPosy = y + (int)Mathf.Sin(rad1 * Mathf.PI / 2);
            for(int rad2 = 0; rad2 < 2; rad2++)
            {
                MappingPosx = rootPosx + (int)Mathf.Cos((rad2 + rad1) * Mathf.PI / 2);
                MappingPosy = rootPosy + (int)Mathf.Sin((rad2 + rad1) * Mathf.PI / 2);
                if (MappingPosx >= 0 && MappingPosy >= 0 && MappingPosx <= maprange[0] - 1 && MappingPosy <= maprange[1] - 1)
                {
                    if (obstaclemap[MappingPosx, MappingPosy] == 0 || charamap[MappingPosx, MappingPosy])
                    {
                        MapStore(ref FABEM, BaseEffectMapping(MappingPosx, MappingPosy, 1), storingcount);
                    }
                    storingcount++;
                }
            }
        }
        return MapFusion(FABEM, judge, coefficient);
    }
    

    //4つまでの複数のマップにそれぞれ指定した定数をかけて合成する
    //(result:結果を返すマップ行列 judgen:nのマップが有効であるか(judgen>0の時有効) mapn:nのマップ行列 coefficientn:nのマップ行列にかける係数)
    //係数は合計0に正規化されている必要はない。等倍合成の場合はすべての係数に１を代入。
    private float[,] MapFusion(float[,,] map, int[] judge, float[] coefficient)
    {
        float[,] result = new float[maprange[0], maprange[1]];
        bool errorflag = false;
        if(judge.Length != coefficient.Length)
        {
            Debug.Log("エラー(影響マップ)：判定式配列と係数配列の長さが異なります。");
            errorflag = true;
        }
        float coefficientsum = 0;
        //Debug.Log(coefficientsum);

        for (int mapnum = 0; mapnum < judge.Length; mapnum++)
        {
            //Debug.Log(coefficient[mapnum]);
            if(judge[mapnum] > 0)
            {
                coefficientsum += coefficient[mapnum];
            }
        }
        
        Debug.Log(judge.Length + "個のマップを合成。係数合計：" + coefficientsum);

        for (int xcount = 0; xcount < maprange[0]; xcount++)
        {
            if (errorflag == true)
            {
                Debug.Log("(影響マップ)エラーのためループを終了");
                break;
            }

            for (int ycount = 0; ycount < maprange[1]; ycount++)
            {
                for(int mapnum = 0; mapnum < judge.Length; mapnum++)
                if (judge[mapnum] > 0)
                {
                    result[xcount, ycount] += (coefficient[mapnum] / coefficientsum) * map[mapnum, xcount, ycount];
                }
            }
        }
        return result;
    }


    //2座標間の折れ線距離を求める(BaseEffectMappingのコードをいくつか流用)
    public int PolygonalDistance(int x1, int y1, int x2, int y2)
    {
        //Debug.Log("PolygonalDistance 1つ目の座標" + x1 + "," + y1 + " 2つ目の座標" + x2 + "," + y2);
        int[,] search = new int[maprange[0] * maprange[1], 4];//{x,y,折れ線距離, 格納されていれば１}
        int[,] searchedMap = new int[maprange[0], maprange[1]];
        search[0, 0] = x1;
        search[0, 1] = y1;
        search[0, 3] = 1;
        searchedMap[x1, y1] = 1;

        int judgex;
        int judgey;

        int mappingcount = 1;

        for (int n = 0; search[n, 3] != 0; n++)
        {
            for(int rad = 0; rad < 4; rad++)
            {
                judgex = search[n, 0] + (int)Mathf.Cos(rad * Mathf.PI / 2);
                judgey = search[n, 1] + (int)Mathf.Sin(rad * Mathf.PI / 2);
                if (judgex >= 0 && judgey >= 0 && judgex <= maprange[0] - 1 && judgey <= maprange[1] - 1)
                {
                    if (obstaclemap[judgex, judgey] != 1 || charamap[judgex, judgey] != null)
                    {
                        //Debug.Log("PolygonalDistance judgex,judgey = " + judgex + "," + judgey);

                        if (searchedMap[judgex, judgey] == 0)
                        {
                            if (judgex == x2 && judgey == y2)
                            {
                                return search[n, 2] + 1;
                            }
                            search[mappingcount, 0] = judgex;
                            search[mappingcount, 1] = judgey;
                            search[mappingcount, 2] = search[n, 2] + 1;
                            search[mappingcount, 3] = 1;
                            searchedMap[judgex, judgey] = 1;
                            mappingcount++;
                        }
                    }
                }
            }
        }
        
        Debug.Log("エラー(影響マップ):１つ目の座標が2つ目の座標を発見できませんでした。");
        return 0;
    }


    //0~1で正規化されたマップ行列を反転する
    private float[,] ReverseMap(float[,] map)
    {
        float[,] result = new float[maprange[0], maprange[1]];
        for (int xcount = 0; xcount <= maprange[0] - 1; xcount++)
        {
            for (int ycount = 0; ycount <= maprange[1] - 1; ycount++)
            {
                result[xcount, ycount] = 1 - map[xcount, ycount];
            }
        }
        return result;
    }


    //1つのマップ行列を陣地マップと合計が1となる係数で合成する陣地マップは反転可
    //encampmentmapはデフォルトで敵陣地が1,味方陣地が0
    private float[,] MixWithEncampmentMap(float[,] map, float encampmentcoefficient, bool reverse)
    {
        float[,] result = new float[maprange[0], maprange[1]];
        float[,] encampmentmapcopy = new float[maprange[0], maprange[1]];
        if(reverse == true)
        {
            encampmentmapcopy = ReverseMap(encampmentmap);
        }
        else
        {
            encampmentmapcopy = encampmentmap;
        }
        for (int xcount = 0; xcount < maprange[0]; xcount++)
        {
            for (int ycount = 0; ycount < maprange[1]; ycount++)
            {
                result[xcount, ycount] = (1 - encampmentcoefficient) * map[xcount, ycount] + encampmentcoefficient * encampmentmapcopy[xcount, ycount]; 
            }
        }
        return result;
    }

    //#################################################################################################################################################
    //#             以下具体的な行動を導く関数
    //#################################################################################################################################################

    //移動先座標決定関数　2要素1次元配列を返す
    private int[] DecideDestination ()
    {
        int[] destination = {enemyx, enemyy};
        float min = effectmap[enemyx, enemyy];
        int[] minPos = { enemyx, enemyy };

        for(int i = 0; i < 3; i++)
        {
            for(int rad = 0; rad < 4; rad++)
            {
                int judgex = destination[0] + (int)Mathf.Cos(rad * Mathf.PI / 2);
                int judgey = destination[1] + (int)Mathf.Sin(rad * Mathf.PI / 2);
                if(judgex >= 0 && judgey >= 0 && judgex <= maprange[0] - 1 && judgey <= maprange[1] - 1)
                {
                    if(obstaclemap[judgex, judgey] == 0)
                    {
                        if (effectmap[judgex, judgey] < min)
                        {
                            min = effectmap[judgex, judgey];
                            minPos[0] = judgex;
                            minPos[1] = judgey;
                        }
                    }
                }
            }
            destination[0] = minPos[0];
            destination[1] = minPos[1];
        }
        return destination;
    }


    //攻撃座標決定関数　攻撃を選択したが攻撃しなかった場合はdestinationが代入される
    private int[] AttackPos()
    {
        int[] result = new int[2];
        result[0] = destination[0];
        result[1] = destination[1];
        float min = 1;
        float judgeHp;
        for(int ally = 0; ally < 4; ally++)
        {
            if(eFarAttacker == true)
            {
                judgeHp = (float)(pHp[ally] - enemyatk) / pHpmax[ally];
            }
            else
            {
                judgeHp = (float)(pHp[ally] - (enemyatk - pDef[ally])) / pHpmax[ally];
            }
            for (int rad = 0; rad < 4; rad++)
            {
                int judgex = destination[0] + (int)Mathf.Cos(rad * Mathf.PI / 2);
                int judgey = destination[1] + (int)Mathf.Sin(rad * Mathf.PI / 2);
                if(pPosx[ally] == judgex && pPosy[ally] == judgey)
                {
                    if(judgeHp < min)
                    {
                        result[0] = judgex;
                        result[1] = judgey;
                        min = judgeHp;
                    }
                }
                if (eLongRangeAttack == true)
                {
                    for(int rad2 = 0; rad2 < 2; rad2++)
                    {
                        int judgefx = judgex + (int)Mathf.Cos((rad + rad2) * Mathf.PI / 2);
                        int judgefy = judgey + (int)Mathf.Sin((rad + rad2) * Mathf.PI / 2);
                        if (pPosx[ally] == judgefx && pPosy[ally] == judgefy)
                        {
                            if (judgeHp < min)
                            {
                                result[0] = judgefx;
                                result[1] = judgefy;
                                min = judgeHp;
                            }
                        }
                    }
                }
            }
        }
        return result;
    }


    /*以下コピペ用
     
        //配列全体に何かするアレ。多分一番使う。
        for (int xcount = 0; xcount <= maprange[0] - 1; xcount++)
        {
            for (int ycount = 0; ycount <= maprange[1] - 1; ycount++)
            {
                
            }
        }
     
     */
}
