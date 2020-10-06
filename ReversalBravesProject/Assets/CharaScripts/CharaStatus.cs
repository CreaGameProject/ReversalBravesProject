//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CharaStatus : MonoBehaviour
{
    
    enum AnimationState
    {
        ATTACK, STOP, WALK

    }
    public string charaName;//名前
    public bool actionFlag;//falseなら行動済み、trueなら行動してない
    static int turn;//ターン数
    public bool playerSide;//trueなら味方。falseなら敵
    public int attack ;//攻撃力
   // public int magicattack ;//魔法攻撃力
    public int heal ;//回復量
    public int maxhp ;//最大体力
    public int hp ;//現在体力
    public int defense ;//防御力
    public int posX;//X座標
    public int posY;//Y座標
    public int Exp;//経験値
    private int ExpPoint = 40;//敵を倒した時に得られる経験値
    public int Lv =1;//レベル
    public int[] actionLevel = new int[256];//行動固有値

    public int occupatoinDistance = 3;//兵士の移動距離
    public int attackDistance;//攻撃範囲

    public bool farAttacker;//遠距離，近距離の判別

    public bool magicAttacker = false;//魔法攻撃の判別
    [SerializeField]
    public int occupation;//兵種認知

    //敵用
    public int actionKind;//行動の判別用の変数
    public int saveX;//守るマスのx座標
    public int saveY;//守るマスのy座標


    Color defaultColor = new Color(1,1,1,1);//キャラの最初の色
    protected MapPosition mapPosition;//MapPositionクラスのインスタンス生成
    protected BattleMapManager battleMapManager;
    GameObject thisObject;

    protected void StartSetting()
    {
        
    }

    // Use this for initialization
    void Start()
    {
        posX = (int)this.GetComponent<Transform>().position.x;
        posY = -(int)this.GetComponent<Transform>().position.y;
        defaultColor = this.GetComponent<Renderer>().material.color;
        mapPosition = GameObject.Find("ObjectManager").GetComponent<MapPosition>();
        //Debug.Log(mapPosition);
        mapPosition.charamap[posX, posY] = gameObject;
        actionFlag = true;
        hp = maxhp;
        battleMapManager = GameObject.Find("BattleMapManager").GetComponent<BattleMapManager>();

        //defaultColor = this.GetComponent<Renderer>().material.color;
        //mapPosition = GameObject.Find("ObjectManager").GetComponent<MapPosition>();
        //Debug.Log(mapPosition);
        //mapPosition.charamap[posX, posY] = gameObject;
        //if (occupation == 1)//主人公
        //{
        //    occupatoinDistance = 3;

        //}

        //if (occupation == 2)//盾持ち
        //{
        //    occupatoinDistance = 3;
        //}

        //if (occupation == 3)//弓兵
        //{
        //    occupatoinDistance = 3;
        //}

        //if (occupation == 4)//魔法使い
        //    occupatoinDistance = 3;//基本的に味方の行動範囲は3マス
    }

    //// Update is called once per frame
    //void Update()
    //{

    //}

    public void Attack(GameObject enemyChara)// 攻撃関数，攻撃する相手を引数として渡す
    {
        CharaStatus enemyStatus = enemyChara.GetComponent<CharaStatus>();
        int enemyHP = enemyStatus.hp;
        int enemyDefense = enemyStatus.defense;

        if (actionFlag == true)
        {

            if (farAttacker == false && magicAttacker == false)
            {
                if (attack > enemyDefense)
                {
                    enemyHP -= (attack - enemyDefense);//現在hpから（攻撃から防御だけ）引いた値。例　100-(20-10)=90
                }
            }

            if (farAttacker == true && magicAttacker == false)
            {
                if (attack > enemyDefense)
                {
                    enemyHP -= (attack - enemyDefense);//現在hpから（攻撃から防御だけ）引いた値。例　100-(20-10)=90

                }
            }

            if (farAttacker == true && magicAttacker == true)
            {
                enemyHP -= attack;//魔法攻撃は防御力関係なくダメージを与えられる。例　90 - 15 + 0 = 75 
            }
            enemyStatus.hp = enemyHP;
        }
        AfterMoving(false);
    }

    public void Heal(GameObject allyChara)//回復関数，回復させる相手を引数として渡す（自分の場合は自分が引数になる）
    {
        CharaStatus allyStatus = allyChara.GetComponent<CharaStatus>();
        int charaHP = allyStatus.hp;
        int charaMaxHP = allyStatus.maxhp;

        if (actionFlag == true)
        {
            if (charaMaxHP < charaHP + heal)
            {
                charaHP = charaMaxHP;
            }

            else
            {
                charaHP = charaHP + heal;
            }
            Debug.Log(battleMapManager.turn);
            actionLevel[battleMapManager.turn] = 1;
        }
        allyStatus.hp = charaHP;
        AfterMoving(false);
    }

    public void CharaWait()
    {
        //actionFlag = false;
        //this.GetComponent<Renderer>().material.color = Color.grey;
        AfterMoving(false);
    }

    public void AfterMoving(bool flag)
    {
        actionFlag = flag;
        if(flag == true)
        {
            this.GetComponent<Renderer>().material.color = new Color (1,1,1,1);
        }
        else
        {
            this.GetComponent<Renderer>().material.color = Color.grey;
        }
    }

    public void CharaDestroy(GameObject chara)
    {
        if (chara.GetComponent<CharaStatus>().hp <= 0)
        {
            Destroy(chara);
        }
    }

    public void CharaExperience()
    {
        if (playerSide == true)
        {
            this.Exp += ExpPoint;//敵に攻撃したらそのたびに経験値が10増える。
        }

        //if (Exp >= 100)

        //{
        //    LevelUp();  //Expが100まで溜まったらレベルアップする関数を起こす。
        //}
    }

    public void LevelUp()
    {
        if (playerSide == true)
        {
            if (Exp >= 100 && Lv <= 19)//
            {
                //LevelUp();//19レベル以下であれば経験値100溜まるごとにレベルが1上がる。
                int statusUp01 = Random.Range(1, 101);//HPが上がるかの判定
                int statusUp02 = Random.Range(1, 101);//攻撃力が上がるかの判定
                int statusUp03 = Random.Range(1, 101);//防御力が上がるかの判定
                int statusUp04 = Random.Range(1, 101);//回復力が上がるかの判定

                Exp -= 100;
                Lv++;//レベルが上がる
                     //能力値が上がる

                if (occupation == 1)//主人公
                {
                    if (statusUp01 <= 50)
                    { maxhp++; }
                    //maxhp = Mathf.Clamp(maxhp, 20, 30);

                    if (statusUp02 <= 75)
                    { attack++; }
                    //attack = Mathf.Clamp(attack, 15, 30);

                    if (statusUp03 <= 60)
                    { defense++; }
                    //defense = Mathf.Clamp(defense, 1, 15);

                    if (statusUp04 <= 25)
                    { heal++; }
                    //heal = Mathf.Clamp(heal, 5, 10);
                }

                if (occupation == 2) //盾持ち
                {
                    if (statusUp01 <= 75)
                    { maxhp++; }
                    //maxhp = Mathf.Clamp(maxhp, 25, 40);

                    if (statusUp02 <= 30)
                    { attack++; }
                    //attack = Mathf.Clamp(attack, 20, 28);

                    if (statusUp03 <= 90)
                    { defense++; }
                    //defense = Mathf.Clamp(defense, 17, 35);

                    if (statusUp04 <= 30)
                    { heal++; }
                    //heal = Mathf.Clamp(heal, 1, 7);

                }
                if (occupation == 3) //弓兵
                {
                    if (statusUp01 <= 50)
                    { maxhp++; }
                    // maxhp = Mathf.Clamp(maxhp, 20, 30);

                    if (statusUp02 <= 75)
                    { attack++; }
                    //attack = Mathf.Clamp(attack, 15, 30);

                    if (statusUp03 <= 60)
                    { defense++; }
                    //defense = Mathf.Clamp(defense, 1, 15);


                    if (statusUp04 <= 25)
                    { heal++; }
                    // heal = Mathf.Clamp(heal, 5, 10);
                }
                if (occupation == 4)//魔導士
                {
                    if (statusUp01 <= 45)
                    { maxhp++; }
                    // maxhp = Mathf.Clamp(maxhp, 10, 20);

                    if (statusUp02 <= 50)
                    { attack++; }
                    //attack = Mathf.Clamp(attack, 20, 30);


                    if (statusUp03 <= 40)
                    { defense++; }
                    // defense = Mathf.Clamp(defense, 0, 8);

                    if (statusUp04 <= 75)
                    { heal++; }
                    // heal = Mathf.Clamp(heal, 5, 20);
                }
            }

        }
    }
       
}
  

    

