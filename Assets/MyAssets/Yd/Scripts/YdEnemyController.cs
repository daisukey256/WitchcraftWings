using System.Collections;
using UnityEngine;

public class YdEnemyController : MonoBehaviour
{
    // ------------------------------------
    // Inspectorに表示するフィールド変数
    //  TODO: 外部から参照されないが、Inspectorに表示するためにpublicにしている変数は
    //  授業では出てこなかった　[SerializeField] private に変える
    // ------------------------------------
    public int scoreValue;          // 倒した時の得点
    public int life = 1;            // ライフ
    public bool isBoss = false;     // ボスフラグ
    public float moveSpeed = 1.0f;  // 動く速さ
    public float moveWidth = 0.3f;  // 動きの幅
    public Animator animator;       // 敵を動かすアニメータ

    // ------------------------------------
    // Privateフィールド変数
    // ------------------------------------
    float initialPosX;          // 初期位置X座標
    Collider lastHitCollider;   // 命中したした弾のコライダー 


    // ------------------------------------
    // Start is called before the first frame update
    // ------------------------------------
    void Start()
    {
        // 敵を左右に動かす
        StartCoroutine(MoveEnemiesPeriodically());

        // 現在のアニメーションの再生位置をランダムにずらす
        animator.Play(animator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, Random.Range(0f, 1f));
    }


    // ------------------------------------
    // 敵を左右に動かすコルーチン
    // ------------------------------------
    IEnumerator MoveEnemiesPeriodically()
    {
        // 現在の位置を記録
        initialPosX = transform.position.x;

        // ランダムな位相シフトを生成
        float phaseShift = Random.Range(0f, 2f * Mathf.PI);

        while (true)
        {
            // sin関数を使って左右に動かす
            float x = initialPosX + Mathf.Sin(Time.time * moveSpeed + phaseShift) * moveWidth;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);

            yield return null;  // 次のフレームまで待つ
        }
    }

    // ------------------------------------
    // 被弾処理
    // ------------------------------------
    void ApplyDamage(Collider other)
    {
        //Debug.Log("OnTriggerEnter");

        // 弾に当たった
        // 　多重呼び出し防止で以前に当たった弾は処理しない
        if (other.tag == "YdBullet" && other != lastHitCollider)
        {
            lastHitCollider = other;

            // 敵のライフを減らす
            life--;

            // ライフがなくなったら撃破
            if (life <= 0)
            {
                // スコアを加算
                YdGameManager.TotalScore += scoreValue;

                // ボスだったらゲームクリア
                if (isBoss) YdGameManager.GameState = YdGameState.GameClear;

                // 自分自身を削除
                Destroy(gameObject);

            }
        }
    }

    // ------------------------------------
    // 衝突
    // ------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("OnTriggerEnter");

        // 弾に当たった
        ApplyDamage(other);
    }

    // ------------------------------------
    // 攻撃範囲内に取り込まれた場合
    // ------------------------------------
    private void OnTriggerStay(Collider other)
    {
        // 弾に当たった
        ApplyDamage(other);
    }


}
