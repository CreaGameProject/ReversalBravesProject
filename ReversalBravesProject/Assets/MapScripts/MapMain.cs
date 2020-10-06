using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMain : MapBase {

    protected int sceneTask;

    //マップチップを組み込む配列
    public GameObject[,] maptips = new GameObject[20, 10];



    // Use this for initialization
    void Start ()
    {

        //配置するプレハブの読み込み
        GameObject prefab = GameObject.Find("Grid");
        
        //配置元のオブジェクト指定
        GameObject stageObject = GameObject.FindWithTag("Stage");

        //タイル配置
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                Vector3 gridPos = new Vector3 (0.5f + prefab.transform.localScale.x*i,-9.5f + prefab.transform.localScale.y *j,0);

                if (prefab != null)
                {
                    //プレハブの複製
                    GameObject instantObject = (GameObject)GameObject.Instantiate(prefab, gridPos, Quaternion.identity);
                    //生成元の下に複製したプレハブをくっつける
                    instantObject.transform.parent = stageObject.transform;

                    //マップチップを配列に組み込む
                    maptips[i,9 - j] = instantObject;
                }

            }
        }

    }
	
	// Update is called once per frame
	void Update () {
		
	}


    //オブジェクトの座標軸を入力するとそれをマップ上のマス目の座標に書き換える関数
    //public GameObject SearchMaptips(Vector3 objectPosition)//オブジェクトの座標を引数として渡す
    //{
    //    for (int m = 0; m < 10; m++)//for文２つでマップ上のマス目を認識(それぞれx方向，y方向をつかさどっている)
    //    {
    //        for (int n = 0; n < 20; n++)
    //        {
    //            if (maptips[m, n].transform.position == objectPosition)//オブジェクトの座標をマップ上の座標に変換して戻り値として戻す
    //            {
    //                return maptips[m, n];//maptipsはマップ上のマス目一個一個が要素の2次元配列
    //            }                        //すなわちmaptipsのインデックス＝オブジェクトのマス目上の座標？
    //        }
    //    }
    //    return null;
    //}

}
