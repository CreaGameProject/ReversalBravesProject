using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ITweenE : MonoBehaviour
{

	
	void Start () {

        Vector3 enemypos = transform.position;

        GameObject player = GameObject.Find("player");
        Vector3 playerpos = player.GetComponent<Transform>().position;

        Vector3 vec = playerpos - enemypos;

        float r = 0.5f;
        float atan = Mathf.Atan2(vec.z, vec.x);
        vec.z = r * Mathf.Sin(atan);
        vec.x = r * Mathf.Cos(atan);

        //iTweenというアプリで今いる位置からx座標-2の位置に0.5秒間で移動する
        iTween.MoveBy(gameObject, iTween.Hash("x", vec.x,"z",vec.z ,"delay", 0.5f));
        //iTweenというアプリで今いる位置からx座標2の位置に0.5秒間から1秒間の間に移動する
        iTween.MoveBy(gameObject, iTween.Hash("x", -vec.x,"z",-vec.z, "delay", 1f));



    }
	
	
}
