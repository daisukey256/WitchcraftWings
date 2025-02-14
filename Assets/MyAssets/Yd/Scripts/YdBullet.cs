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
        Destroy(gameObject);
    }

}
