using System.Collections;
using UnityEngine;

public class YdCameraController : MonoBehaviour
{
    // ------------------------------------
    // Inspectorに表示するフィールド変数
    //  TODO: 外部から参照されないが、Inspectorに表示するためにpublicにしている変数は
    //  授業では出てこなかった　[SerializeField] private に変える
    // ------------------------------------
    public GameObject target;   // カメラが追尾するターゲット
    public float followSpeed;   // 追尾スピード

    // [追加] 演出のための正面カメラへの切り替え用
    public GameObject mainCamera;   // メインカメラ
    public GameObject frontCamera;  // フロントカメラ

    // [追加] ダメージを受けた際にカメラを揺らす
    public float ShakeDuration = 0.6f;  // 揺れる時間
    public float ShakeMagnitude = 0.4f; // 揺れの強さ


    // ------------------------------------
    // Privateフィールド変数
    // ------------------------------------
    Vector3 diff;               // Playerとの距離さ
    Vector3 originalCameraPos;  // カメラの元の位置



    // ------------------------------------
    // Start is called before the first frame update
    // ------------------------------------
    void Start()
    {
        // 初期状態でのターゲットとカメラ位置の距離を取得
        diff = target.transform.position - transform.position;

        //  [追加] メインカメラを有効にする
        SwitchToMainCamera();
    }


    // ------------------------------------
    // Updateの後に処理される
    // ※Playerの位置計算が確実に終わったであろうタイミング
    // ------------------------------------
    void LateUpdate()
    {
        // 線形補完関数でスムーズに移動
        // 追いつくまでにかける時間は一定なので、距離が離れているほど早く、近いほどゆっくり）
        transform.position = Vector3.Lerp(
            transform.position,                 // スタート位置（現在位置）
            target.transform.position - diff,   // ゴール位置（ターゲットから一定間隔離れた位置）
            Time.deltaTime * followSpeed        // スタート・ゴール間の位置の割合(0.0～1.0)
        );
    }


    // ------------------------------------
    //  [追加] フロントカメラを有効にする
    // ------------------------------------
    public void SwitchToFrontCamera()
    {
        mainCamera.SetActive(false);
        frontCamera.SetActive(true);
    }


    // ------------------------------------
    //  [追加] メインカメラを有効にする
    // ------------------------------------
    public void SwitchToMainCamera()
    {
        mainCamera.SetActive(true);
        frontCamera.SetActive(false);
    }


    // ------------------------------------
    // [追加] ダメージを受けた際にカメラを揺らす
    // ------------------------------------
    public void ShakeCamera()
    {
        StartCoroutine(ShakeCameraCoroutine(ShakeDuration, ShakeMagnitude));
    }


    // ------------------------------------
    // カメラを揺らすコルーチン
    // ------------------------------------
    IEnumerator ShakeCameraCoroutine(float shakeDuration, float shakeMagnitude)
    {
        float elapsed = 0.0f;   // 経過時間

        // 現在のカメラ位置を取得
        originalCameraPos = transform.localPosition;

        // 指定時間カメラをランダムに揺らす
        while (elapsed < shakeDuration)
        {
            // カメラの揺れ幅をランダムに決める(Time.timeScaleを0にしてポーズ中は動かさない）
            float dx = Random.Range(-1f, 1f) * shakeMagnitude * Time.timeScale;
            float dy = Random.Range(-1f, 1f) * shakeMagnitude * Time.timeScale;

            // カメラの位置を設定
            transform.localPosition = new Vector3(
                    originalCameraPos.x + dx,
                    originalCameraPos.y + dy,
                    originalCameraPos.z
            );

            // 経過時間を加算 
            elapsed += Time.deltaTime;

            yield return null;
        }

        // カメラ位置を元に戻す
        transform.localPosition = originalCameraPos;
    }
}
