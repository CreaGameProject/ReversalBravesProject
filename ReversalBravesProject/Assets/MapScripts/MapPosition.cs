using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPosition : MonoBehaviour {
    
   public int map;
    const int mapRangeX = 20;
    const int mapRangeY = 10;

    public int[,] obstaclemap;

    public int[,] obstaclemap1 = new int[10, 20]  //マップ上のマス目を配列で表示(AI用)
    {
        {1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,1},
        {1,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,1,0,0,1},
        {0,1,0,0,0,1,0,1,0,0,0,0,0,0,0,0,1,0,0,1},
        {0,0,0,0,0,0,0,1,0,0,0,0,0,1,0,0,0,0,0,0},
        {0,1,0,0,0,1,0,0,0,0,1,0,0,1,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,1,0,0,0,0,0,1,0,0,0,0,0,0},
        {1,1,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,1,0,0,1,0,1,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0},
        {0,0,0,0,0,0,1,0,0,1,0,1,0,0,0,0,0,0,0,0}
    };

    public int[,] obstaclemap2 = new int[10, 20]
        {
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,1,1,1},
            {1,1,1,1,1,1,1,0,0,0,1,1,1,1,1,0,0,0,0,1},
            {0,0,0,1,1,1,0,0,0,0,0,0,1,1,1,0,1,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0},
            {0,0,0,0,0,0,0,0,1,0,0,0,1,1,1,1,0,0,0,0},
            {0,1,0,0,1,1,1,1,0,0,0,0,0,1,0,0,0,0,0,0},
            {0,0,0,1,1,1,1,1,1,0,0,0,0,0,0,0,0,1,0,0},
            {0,0,0,1,1,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,1,1,0,0,0,0,0,0,0,1,0,0,0,0,0,0}
        };

    public int[,] obstaclemap3 = new int[10, 20]
        {
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,1},
            {0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,1,0,0,1,0},
            {0,0,0,0,0,0,0,1,1,0,1,1,1,1,0,0,0,1,1,1},
            {0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,0,1,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,1,0,1,0,1,0,1,0,0,0},
            {0,0,0,0,0,0,0,0,1,0,1,0,1,0,0,0,1,0,0,0},
            {0,0,0,0,0,0,0,0,1,0,0,0,1,1,1,1,1,0,0,0},
            {0,0,0,0,0,0,0,0,1,0,1,0,1,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,1,0,0,0,1,1,1,1,0,1}
        };

    public GameObject[,] charamap = new GameObject[10, 20];//キャラ,樽の二次元配列
    public GameObject[,] flagmap = new GameObject[10, 20];//陣地の旗の二次元配列

    GameObject[] charaObjects;//キャラを取得する配列
    int charaNum;//キャラの数
    
    //ここで各キャラの座標，HPをCharaStatusから呼び出す
    int playernx;//x座標
    int playerny;//y座標
    int playernHP;//HP

    MapMain mapMain = new MapMain {};//MapMainクラスのインスタンス生成

    void Awake()
    {
        if (map == 1)//ステージ1の時
        {
            obstaclemap = obstaclemap1;
        }

        if (map == 2)//ステージ2の時
        {
            obstaclemap = obstaclemap2;
        }

        if (map == 3)//ステージ3の時
        {
            obstaclemap = obstaclemap3;
        }

        //できればobstaclemapを転置して欲しい
        int[,] obstacleChangeMap = new int[20, 10];//転置後の新しい配列を宣言
        for (int i = 0; i < 10; i++)//for文二つで配列内の全要素を指定
        {
            for (int j = 0; j < 20; j++)
            {
                obstacleChangeMap[j, i] = obstaclemap[i, j];//obstaclemapの要素[i,j]をobstacleChangeMapの[j,i]に代入（いわゆる転置処理）
            }
        }

        obstaclemap = obstacleChangeMap;

        //キャラマップを転置
        //flagmapを転置
        GameObject[,] charaChangeMap = new GameObject[mapRangeX, mapRangeY];
        GameObject[,] flagChangeMap = new GameObject[mapRangeX, mapRangeY];
        for (int x = 0; x < mapRangeX; x++)
        {
            for (int y = 0; y < mapRangeY; y++)
            {
                charaChangeMap[x, y] = charamap[y, x];
                flagChangeMap[x, y] = flagmap[y, x];
            }

        }
        charamap = charaChangeMap;

        for (int x = 0; x < mapRangeX; x++)
        {
            for (int y = 0; y < mapRangeY; y++)
            {

            }
        }
        flagmap = flagChangeMap;

    }




    // Use this for initialization
    void Start ()
    {
        
        ////tagを使ってマップ上のキャラを取得
        //charaObjects = GameObject.FindGameObjectsWithTag("Chara");
        //charaNum = charaObjects.Length;//キャラの数を代入
        
        ////インスタンスにMapMainクラスを格納
        //mapMain = GameObject.Find("MapManager").GetComponent<MapMain>();
    }

    // Update is called once per frame
    void Update ()
    {

    }

    //public IEnumerable<IEnumerable<T>> Do<T>(IEnumerable<IEnumerable<T>> values)
    //{
    //    return IEnumerable.Range(0, values.Max(c => c.Count())).Select(i => values.Max(c => i < c.Count() ? c.ElementAt(i) : default));
    //}


    public void ObjectInformation(ref int[,] obstacleMap, ref GameObject[,] charaMap)//キャラのいる所を通行不可にするメソッド
    {
        GameObject[] ally = GameObject.FindGameObjectsWithTag("ally");
        GameObject[] enemy = GameObject.FindGameObjectsWithTag("enemy");
        GameObject[] bomb = GameObject.FindGameObjectsWithTag("bomb");
        GameObject[] chara = new GameObject[ally.Length + enemy.Length + bomb.Length];
        int i2 = 0;
        for(int i = 0; i < ally.Length; i++)
        {
            chara[i2] = ally[i];
            i2++;
        }
        for (int i = 0; i < enemy.Length; i++)
        {
            chara[i2] = enemy[i];
            i2++;
        }
        for (int i = 0; i < bomb.Length; i++)
        {
            chara[i2] = bomb[i];
            i2++;
        }
        CharaStatus[] charaStatuses = new CharaStatus[chara.Length];
        for (int xcount = 0; xcount < mapRangeX; xcount++)
        {
            for (int ycount = 0; ycount < mapRangeY; ycount++)
            {
                obstacleMap[xcount, ycount] = obstaclemap[xcount, ycount];
            }
        }

        charaMap = new GameObject[mapRangeX, mapRangeY];
        for(int i = 0; i < chara.Length; i++)
        {
            //Debug.Log(chara[i]);
            charaStatuses[i] = chara[i].GetComponent<CharaStatus>();
            int x = charaStatuses[i].posX;
            int y = charaStatuses[i].posY;
            charaMap[x, y] = chara[i];
            obstacleMap[x, y] = 1;
        }

        //for (int y = 0; y < 10; y++)
        //{
        //    for (int x = 0; x < 20; x++)
        //    {
        //        if (charamap[y, x] != null)
        //        {
        //            obstaclemap[y, x] = 1;
        //        }
        //        else
        //        {
        //            obstaclemap[y, x] = 0;
        //        }
        //    }
        //}
    }

    public GameObject MapSearchingManager(int y, int x)//そのマス目に何があるかを返すメソッド
    {
        GameObject something = charamap[y, x];//キャラの二次元配列で情報を取得
        return something;//戻り値として返す
    }

    public GameObject MapFlagSearching(int y, int x)//陣地の有無をつかさどるメソッド
    {
        GameObject flag = flagmap[y, x];
        return flag;
    }

    public Vector2 PositionChanger(GameObject chara)//Unity座標をマス目座標に変換
    {
        int mapx;//マス目のx座標
        int mapy;//マス目のy座標
        mapx = (int)chara.transform.position.x;//x座標を変換
        mapy = (int)Mathf.Abs(chara.transform.position.y - 9);//y座標を変換

        Vector2 mapPlace = new Vector2(mapx, mapy); //変換したものをVector2に格納

        return mapPlace;//戻り値として返却
    }
}
