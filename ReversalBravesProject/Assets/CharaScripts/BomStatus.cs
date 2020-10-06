using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class BomStatus : MonoBehaviour {
public class BomStatus : CharaStatus
{


    ////継承前の取り敢えずの変数（継承ができ次第消去）
    //public int bomHP;
    //public int bomMaxHP = 1;
    //public int bomAttack = 20;

    //public int enemyHP;
    GameObject enemy;
    MapMain mapMain;
    //BomStatus bomStatus = new BomStatus();

    //int bombX;
    //int bombY;
    int bomDistance = 2;




    // Use this for initialization
    void Start()
    {
        //mapMain = GameObject.Find("MapManager").GetComponent<MapMain>();
        mapPosition = GameObject.Find("ObjectManager").GetComponent<MapPosition>();

        //継承時に使うための本当のスクリプト（今はエラーが出るためコメントアウト）
        maxhp = 1;
        hp = maxhp;
        attack = 20;
    }

    // Update is called once per frame
    void Update () {

        //if (bomHP <= 0)
        //{
        //    BomAttack(enemyHP);
        //    Destroy (this);
        //}
    }

    public void BombAttack()//周りのマスの情報を取得し爆発によるダメージ
    {
        for (int i = 0;i < 20;i ++)
            for(int j = 0;j < 10;j ++)
            {
                if (Mathf.Abs(i- posX) + Mathf.Abs(j - posY) <= bomDistance)
                {
                    //if ((i > 0 && i < 20) && (j > 0 && j < 10))
                    //{
                        //mapPosition.MapSearchingManager(posX, posY);
                        GameObject chara = mapPosition.charamap[i, j];

                    if (chara != null)
                    {
                        if (chara.tag == "enemy")//フラグが敵ならば
                        {
                            chara.GetComponent<CharaStatus>().hp -= attack;//ダメージを与える
                        }
                    }
                    else
                    {

                    }
                        //}
                }
            }
        //爆発のアニメーション

        Destroy(this);//爆発して消滅
    }

    //public new void Attack(GameObject enemyChara)
    //{
    //    GameObject[,] charaMap = GameObject.Find("ObjectManager").GetComponent<MapPosition>().charamap;
    //    for (int rad1 = 0; rad1 < 4; rad1++)
    //    {
    //        if(charaMap[posX + (int)Mathf.Cos(rad1 * Mathf.PI / 2), posY + (int)Mathf.Sin(rad1 * Mathf.PI / 2)] != null)
    //        {
    //            this.GetComponent<CharaStatus>().Attack(charaMap[posX + (int)Mathf.Cos(rad1 * Mathf.PI / 2), posY + (int)Mathf.Sin(rad1 * Mathf.PI / 2)]);
    //        }
    //        for(int rad2 = 0; rad2 < 2; rad2++)
    //        {
    //            if (charaMap[posX + (int)Mathf.Cos((rad1 + rad2) * Mathf.PI / 2), posY + (int)Mathf.Sin((rad1 + rad2) * Mathf.PI / 2)] != null)
    //            {
    //                this.GetComponent<CharaStatus>().Attack(charaMap[posX + (int)Mathf.Cos((rad1 + rad2) * Mathf.PI / 2), posY + (int)Mathf.Sin((rad1 + rad2) * Mathf.PI / 2)]);
    //            }
    //        }
    //    }
    //}
}
