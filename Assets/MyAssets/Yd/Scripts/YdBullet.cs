using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YdBullet : MonoBehaviour
{
    public float destroyAfterTime = 5.0f;   // 弾の生存期間

    public GameObject explosionEffect;  // 爆発エフェクト
    public float effectDuration = 1.0f; // エフェクトを削除するまでの時間    

    public AudioClip explosionSE;       // 爆発音

    bool hasCollided = false;           // 衝突フラグ

    // コンポーネント参照用
    AudioSource audioSource; 

    // Start is called before the first frame update
    void Start()
    {
        // オーディオソースを取得しておく
        audioSource = GetComponent<AudioSource>();

        // 一定時間後に消滅する
        StartCoroutine(DestroyAfterTime());
    }

    // Update is called once per frame
    void Update()
    {

    }

    // 一定時間経過後に弾を消滅させる
    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(destroyAfterTime);
        // なにかに衝突していなければ消滅
        if (!hasCollided) { 
            Destroy(gameObject); 
        }
    }

    // なにかと衝突した場合
    private void OnTriggerEnter(Collider other)
    {
        // すでに衝突していたらなにもしない
        if (hasCollided) return;

        hasCollided = true;

        // 敵に当たった場合は爆発エフェクト
        if (other.tag == "YdEnemy")
        {
            StartCoroutine(playExplosion());
        }
        else 
        {
            // 敵以外にあたった場合は即時消滅
            Destroy(gameObject);
        }
    }

    // 爆発処理のコルーチン
    IEnumerator playExplosion()
    {
        // エフェクトを現在のオブジェクトの位置と回転で生成
        GameObject effectInstance = Instantiate(explosionEffect, transform.position, transform.rotation);

        // 爆発サウンドを再生
        if ((explosionSE != null) && (audioSource != null))
        {
            audioSource.PlayOneShot(explosionSE);
        }

        // 一定時間後にエフェクトと弾を削除
        yield return new WaitForSeconds(effectDuration);
        Destroy(effectInstance);
        Destroy(gameObject);
    }


}
