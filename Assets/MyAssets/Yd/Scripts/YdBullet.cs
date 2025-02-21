using System.Collections;
using UnityEngine;

public class YdBullet : MonoBehaviour
{

    // ------------------------------------
    // 外部から参照される Publicフィールド変数  
    // ------------------------------------
    public Transform ExplosionsParentTransform; // 爆発エフェクトを子に配置するオブジェクト

    // ------------------------------------
    // Inspectorに表示するフィールド変数
    //  TODO: 外部から参照されないが、Inspectorに表示するためにpublicにしている変数は
    //  授業では出てこなかった　[SerializeField] private に変える
    // ------------------------------------
    public float destroyDelayTime = 1.0f;       // 弾の生存期間

    public GameObject explosionEffectPrefab;    // 爆発エフェクトプレファブ
    public float effectDuration = 1.0f;         // エフェクトを削除するまでの時間    

    public AudioClip explosionSE;               // 爆発音


    // ------------------------------------
    // Privateフィールド変数
    // ------------------------------------
    bool hasCollided = false;           // 衝突フラグ

    // コンポーネント参照用
    AudioSource audioSource;


    // ------------------------------------
    // Start is called before the first frame update
    // ------------------------------------
    void Start()
    {
        // オーディオソースを取得しておく
        audioSource = GetComponent<AudioSource>();

        // 一定時間後に消滅する
        StartCoroutine(DestroyAfterTime());
    }


    // ------------------------------------
    // 一定時間経過後に弾を消滅させる
    // ------------------------------------
    IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(destroyDelayTime);
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

        // 弾の動きを止めて非表示
        // （爆発エフェクトのためオブジェクトはまだ削除しない）
        stopMovement();
        hideBullet();
        // 敵内部にとどまって連続ダメージを与えないように無効にする
        disableDamage();

        // 爆発対象に当たった場合は爆発エフェクトを再生
        if (IsTriggered(other))
        {
            // 爆発エフェクト再生のコルーチンを呼び出す
            StartCoroutine(playExplosionEffect());
        }
        else
        {
            // 爆発しない場合は即時消滅
            Destroy(gameObject);
        }
    }


    // 弾の動きを止める
    protected virtual void stopMovement()
    {
        // 移動と回転をリセット
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }


    // 弾を隠す
    protected virtual void hideBullet()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }


    // 弾のダメージを無効化
    protected virtual void disableDamage()
    {
        // 弾についているコライダーを無効化して
        // 衝突判定でダメージを与えるのを止める
        GetComponent<Collider>().enabled = false;
    }


    // 衝突して爆発する対象か否かを判定する
    protected virtual bool IsTriggered(Collider other)
    {
        bool isTriggered = false;
        if (other.tag == "YdEnemy") isTriggered = true;

        return isTriggered;
    }


    // 爆発処理のコルーチン
    IEnumerator playExplosionEffect()
    {
        // エフェクトを現在のオブジェクトの位置と回転で生成
        GameObject effectInstance = Instantiate(explosionEffectPrefab, transform.position, transform.rotation);

        // Hierarchy上でちらばらないように指定のオブジェクトの子として配置
        if (ExplosionsParentTransform != null)
        {
            effectInstance.transform.parent = ExplosionsParentTransform;
        }

        // 爆発サウンドを再生
        if ((explosionSE != null) && (audioSource != null))
        {
            audioSource.PlayOneShot(explosionSE);
        }

        // 一定時間後にエフェクトを削除
        yield return new WaitForSeconds(effectDuration);
        Destroy(effectInstance);
        // エフェクト再生が終ったら弾も削除
        Destroy(gameObject);
    }

}
