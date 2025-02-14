using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YdEnemyController : MonoBehaviour
{
    public int scoreValue;          // 倒した時の得点
    public int life = 1;            // ライフ
    public float moveSpeed = 1.0f;  // 動く速さ
    public float moveWidth = 0.3f;  // 動きの幅
    public float maxDelay = 2f;     // 横移動開始をずらす最大時間
    public bool isBoss = false;     // ボスフラグ

    public Animator animator;   // 敵を動かすアニメータ

    float initialPosX;  // 初期位置X座標

    // Start is called before the first frame update
    void Start()
    {
        // 敵を左右に動かす
        StartCoroutine(MoveEnemiesPeriodically());

        // 現在のアニメーションの再生位置をランダムにずらす
        animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, Random.Range(0f, 1f));
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator MoveEnemiesPeriodically()
    {
        // 現在の位置を記録
        initialPosX = transform.position.x;

        // ランダムな位相シフトを生成
        float phaseShift = Random.Range(0f, 2f * Mathf.PI);
        //// ランダムな遅延時間を生成
        //float delay = Random.Range(0f, maxDelay);
        //yield return new WaitForSeconds(delay);  // 遅延時間を待つ

        while (true)
        {
            // sin関数を使って左右に動かす
            float x = initialPosX + Mathf.Sin(Time.time * moveSpeed + phaseShift) * moveWidth;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);

            yield return null;  // 次のフレームまで待つ
        }
    }


    // 衝突・被弾処理
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("OnTriggerEnter");

        // 弾に当たった
        if(other.tag == "YdBullet")
        {
            life--;
            if(life < 0)
            {
                // スコアを加算
                YdGameManager.instance.AddScore(scoreValue);

                // ボスだったらゲームクリア
                if (isBoss) YdGameManager.gameState = YdGameState.GameClear;

                // 自分自身を削除
                Destroy(gameObject);

            }
        }
    }

}
