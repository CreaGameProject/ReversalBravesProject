using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ITweenp : MonoBehaviour
{
    
    void Start()
    {

        /*Vector3 tmp = GameObject.Find("Ene").transform.position;
        GameObject.Find("Ene").transform.position = new Vector3(tmp.x + 10, tmp.y, tmp.z);
        float x = tmp.x;
        float y = tmp.y;
        float z = tmp.z;*/

        //GameObject player = GameObject.Find("player");//playerのオブジェクトを検索
        Vector3 playerpos = transform.position;

        GameObject enemy = GameObject.Find("Ene");
        Vector3 enemypos = enemy.GetComponent<Transform>().position;

        Vector3 vec = enemypos - playerpos;

        float r = 0.5f;
        float atan = Mathf.Atan2(vec.z, vec.x);
        vec.z = r * Mathf.Sin(atan);
        vec.x = r * Mathf.Cos(atan);

        // パターン１   
        //敵がx座標の正方向(真横)にいたら
               //iTweenというアプリで今いるから目的の位置に0.7秒間で移動する
            iTween.MoveBy(gameObject, iTween.Hash("x",vec.x, "z",vec.z,"delay", 0.7f));

            //iTweenというアプリで今いる位置からx座標-5の位置に0.7秒間から1.4秒間の間に移動する
            iTween.MoveBy(gameObject, iTween.Hash("x", -vec.x,"z",-vec.z, "delay", 1.4f));
        }
        
        
        
    }
     
   
    

