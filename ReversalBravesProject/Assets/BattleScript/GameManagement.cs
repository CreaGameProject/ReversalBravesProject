using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum MouseButtonDown
{
    MBD_LEFT = 0,
    MBD_RIGHT,
    MBD_MIDDLE,
}

public class GameManagement : MonoBehaviour
{

    public int EnemyNum;//残りの敵の数
    public int PlayerNum;//残りの味方の数(初期4)
    enum Turn { ALLY = 0, ENEMY } //}
    private int junban;           //}ターン管理用  
    public int TurnNum;          //}

    private int CharaLEVEL;
    private int CharaHP;
    private int CharaATTACK;
    private int CharaDEFENCE;
    private int CharaEXP;


    public Text StatusText; //[Text]をいれる
    public GameObject Panel; //[Panel]をいれる
    private bool One;    //Update関数の制限用
    private float distance = 100f;
    private CharaStatus Charaste; //ステータス置く用


    float fadeSpeed = 0.015f;        //透明度が変わるスピードを管理
    float red, green, blue, alfa;        //}パネルの色、不透明度を管理
    float red2, green2, blue2, alfa2;    //}
    public bool isFadeIn = false;   //フェードイン処理の開始、完了を管理するフラグ
    bool isFadeOut = false;  //フェードアウト処理の開始、完了を管理するフラグ
    public Image YourTurn;  //[Your faze]いれる
    public Image EnemyTurn;  //[Enemy faze]いれる

    enum Occupation{
        NULL, SOLDIER, GUARDIAN, ARCHER, WIZARD
    }


    //以下書き加え
    BattleMapManager battleMapManager;
    int waitFrame = 0;

    //



    [SerializeField]    // privateなメンバもインスペクタで編集したいときに付ける
    private GameObject focusObj = null; // 注視点となるオブジェクト

    private Vector3 oldPos; // マウスの位置を保存する変数

    // 注視点オブジェクトが未設定の場合、新規に生成する
    void setupFocusObject(string name)
    {
        GameObject obj = this.focusObj = new GameObject(name);
        obj.transform.position = Vector3.zero;

        return;
    }



    void Start()
    {

        junban = (int)Turn.ENEMY;
        TurnNum = 0;

        //
        battleMapManager = GameObject.Find("BattleMapManager").GetComponent<BattleMapManager>();
        //

        //red = YourTurn.color.r;
        //green = YourTurn.color.g;
        //blue = YourTurn.color.b;
        //alfa = YourTurn.color.a;
        //alfa = 0;

        //red2 = EnemyTurn.color.r;
        //green2 = EnemyTurn.color.g;
        //blue2 = EnemyTurn.color.b;
        //alfa2 = EnemyTurn.color.a;
        //alfa2 = 0;




        // 注視点オブジェクトの有無を確認

        this.setupFocusObject("CameraFocusObject");

        // 注視点オブジェクトをカメラの親にする
        Transform trans = this.transform;
        transform.parent = this.focusObj.transform;



        One = true;


        return;

    }

    void Update()
    {

        //if (isFadeIn)
        //{
        //    if (junban == (int)Turn.ALLY)
        //    {
        //        YourFadeIn();
        //    }
        //    if (junban == (int)Turn.ENEMY)
        //    {
        //        EnemyFadeIn();
        //    }
        //}
        //if (isFadeOut)
        //{
        //    if (junban == (int)Turn.ALLY)
        //    {
        //        YourFadeOut();
        //    }
        //    if (junban == (int)Turn.ENEMY)
        //    {
        //        EnemyFadeOut();
        //    }

        //}

        /*例キャストの処理をいったんコメントアウト

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // Rayの当たったオブジェクトの情報を格納する
        RaycastHit hit = new RaycastHit();
        // オブジェクトにrayが当たった時
        if (Physics.Raycast(ray, out hit, distance))
        {
            string objectName = hit.collider.gameObject.name;// rayが当たったオブジェクトの名前を取得

            string objectTag = hit.collider.gameObject.tag;// rayが当たったオブジェクトのタグを取得


            if (One)
            {
                if (objectTag == "Ally")//味方
                {
                    Charaste = GameObject.Find(objectName).GetComponent<CharaStatus>();
                    CharaLEVEL = Charaste.Lv;
                    CharaHP = Charaste.hp;
                    CharaATTACK = Charaste.attack;  //　　       　(/ω・＼)
                    CharaDEFENCE = Charaste.defense;
                    CharaEXP = Charaste.Exp;
                    Debug.Log("みかた");
                    Panel.SetActive(true);

                    StatusText.text =
                        objectName + "\n" +
                        "Lv " + CharaLEVEL + "\n" +
                        "経験値 " + CharaEXP + "\n" +
                         "HP " + CharaHP + "\n" +
                        "攻撃力 " + CharaATTACK + "\n" +
                        "防御力 " + CharaDEFENCE;




                    One = false;

                }

                else if (objectTag == "Enemy")//敵
                {
                    Charaste = GameObject.Find(objectName).GetComponent<CharaStatus>();
                    CharaHP = Charaste.hp;
                    CharaATTACK = Charaste.attack;
                    CharaDEFENCE = Charaste.defense;
                    Debug.Log("てき");
                    Panel.SetActive(true);

                    StatusText.text =
                        objectName + "\n" +
                         "HP " + CharaHP + "\n" +
                        "攻撃力 " + CharaATTACK + "\n" +
                        "防御力 " + CharaDEFENCE;

                    One = false;
                }
                else
                {
                    Panel.SetActive(false);
                }

            }

        }
        else  //オブジェクトにRayが当たらないorRayが外れた時
        {
            Panel.SetActive(false);
            One = true;
        }

        */


        battleMapManager.mouseUpdate();

        GameObject charactor = battleMapManager.onMouseObject;// rayが当たったオブジェクトの名前を取得

        waitFrame++;
        
        if(waitFrame > 50)
        {
            if (charactor.tag == "ally")//味方
            {
                Charaste = charactor.GetComponent<CharaStatus>();
                CharaLEVEL = Charaste.Lv;
                CharaHP = Charaste.hp;
                CharaATTACK = Charaste.attack;  //　　       　(/ω・＼)
                CharaDEFENCE = Charaste.defense;
                CharaEXP = Charaste.Exp;
                //Debug.Log("みかた");
                Panel.SetActive(true);

                StatusText.text =
                    Charaste.charaName + "\n" +
                    "Lv " + CharaLEVEL + "\n" +
                    "経験値 " + CharaEXP + "\n" +
                     "HP " + CharaHP + "\n" +
                    "攻撃力 " + CharaATTACK + "\n" +
                    "防御力 " + CharaDEFENCE;




                One = false;

            }

            else if (charactor.tag == "enemy")//敵
            {
                Charaste = charactor.GetComponent<CharaStatus>();
                Occupation charaOccupation = (Occupation)Charaste.occupation;
                CharaHP = Charaste.hp;
                CharaATTACK = Charaste.attack;
                CharaDEFENCE = Charaste.defense;
                //Debug.Log("てき");
                Panel.SetActive(true);

                StatusText.text =
                    charaOccupation + "\n" +
                     "HP " + CharaHP + "\n" +
                    "攻撃力 " + CharaATTACK + "\n" +
                    "防御力 " + CharaDEFENCE;

                One = false;
            }
            else
            {
                waitFrame = 0;
                Panel.SetActive(false);
            }

        }

        // マウス関係のイベントを関数にまとめる
        this.CameraPos();
        return;

    }

    // マウス関係のイベント
    void CameraPos()
    {

        // マウスボタンが押されたタイミングで、マウスの位置を保存する
        if (Input.GetMouseButtonDown((int)MouseButtonDown.MBD_LEFT) ||
        Input.GetMouseButtonDown((int)MouseButtonDown.MBD_MIDDLE) ||
        Input.GetMouseButtonDown((int)MouseButtonDown.MBD_RIGHT))
            this.oldPos = Input.mousePosition;

        // マウスドラッグイベント
        this.mouseDragEvent(Input.mousePosition);

        return;
    }



    // マウスドラッグイベント関数
    void mouseDragEvent(Vector3 mousePos)
    {
        // マウスの現在の位置と過去の位置から差分を求める
        Vector3 diff = mousePos - oldPos;


        if (Input.GetMouseButton((int)MouseButtonDown.MBD_MIDDLE))
        {

        }
        else if (Input.GetMouseButton((int)MouseButtonDown.MBD_RIGHT))
        {

            this.cameraTranslate(-diff / 10.0f);

        }
        else if (Input.GetMouseButton((int)MouseButtonDown.MBD_LEFT))
        {

        }

        // 現在のマウス位置を、次回のために保存する
        this.oldPos = mousePos;

        return;
    }

    // カメラを移動する関数
    void cameraTranslate(Vector3 vec)
    {
        Transform focusTrans = this.focusObj.transform;
        Transform trans = this.transform;

        // カメラのローカル座標軸を元に注視点オブジェクトを移動する
        focusTrans.Translate((trans.right * vec.x) + (trans.up * vec.y));

        oldPos = transform.position; //カメラの位置を取得

        oldPos.x = Mathf.Clamp(oldPos.x,6f, 14f); //　　　　　　             ――――――――――
        oldPos.y = Mathf.Clamp(oldPos.y, -5f, -5f); //　　　　　　　　　　　|　カメラの範囲制限　|
        transform.position = new Vector3(oldPos.x, oldPos.y, -10); //        ――――――――――        
        //                                                                                  ＼_(・ω・`)要調整！
        return;
    }




    public void TurnManagement()//(ターン管理)
    {
        if (TurnNum < 5)//ターン数による

        {
            TurnChange();//ターンを変更

            TurnCheck();//ターン数を確認、どちらのターンかの確認
        }
    }
    void TurnChange()
    {
        if (junban == (int)Turn.ENEMY)
        {
            junban = (int)Turn.ALLY;
            TurnNum += 1;

        }
        if (junban == (int)Turn.ALLY)
        {
            junban = (int)Turn.ENEMY;
        }
    }
    void TurnCheck()
    {
        if (junban == (int)Turn.ALLY)
        {
            isFadeIn = true;      //「Your faze」をだす
            print("ターン" + TurnNum);
        }

        if (junban == (int)Turn.ENEMY)
        {
            isFadeIn = true;
            print("Enemy Turn");
        }
    }


    //    void YourFadeOut()
    //    {
    //        alfa -= fadeSpeed;
    //        SetAlpha();
    //        if (alfa <= 0)
    //        {
    //            isFadeOut = false;
    //            YourTurn.enabled = false;
    //        }
    //    }
    //    void YourFadeIn()
    //    {
    //        YourTurn.enabled = true;
    //        alfa += fadeSpeed;
    //        SetAlpha();
    //        if (alfa >= 1)
    //        {
    //            isFadeIn = false;
    //            isFadeOut = true;
    //        }
    //    }
    //    void EnemyFadeOut()
    //    {
    //        alfa2 -= fadeSpeed;
    //        SetAlpha();
    //        if (alfa2 <= 0)
    //        {
    //            isFadeOut = false;
    //            EnemyTurn.enabled = false;
    //        }
    //    }
    //    void EnemyFadeIn()
    //    {
    //        EnemyTurn.enabled = true;
    //        alfa2 += fadeSpeed;
    //        SetAlpha();
    //        if (alfa2 >= 1)
    //        {
    //            isFadeIn = false;
    //            isFadeOut = true;
    //        }
    //    }

    //    void SetAlpha()
    //    {
    //        if (junban == (int)Turn.ALLY)
    //        {
    //            YourTurn.color = new Color(red, green, blue, alfa);
    //        }
    //        if (junban == (int)Turn.ENEMY)
    //        {
    //            EnemyTurn.color = new Color(red2, green2, blue2, alfa2);
    //        }
    //    }
}