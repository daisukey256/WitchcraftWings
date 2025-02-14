using System.Collections;
using UnityEngine;

public class YdCameraController : MonoBehaviour
{
    public GameObject mainCamera;   // メインカメラ
    public GameObject frontCamera;  // フロントカメラ


    Vector3 diff;   // Playerとの距離さ

    public GameObject target;   // カメラが追尾するターゲット
    public float followSpeed;   // 追尾スピード

    // [追加] ダメージを受けた際にカメラを揺らす
    public float ShakeDuration = 0.6f;  // 揺れる時間
    public float ShakeMagnitude = 0.4f;  // 揺れの強さ

    //Transform cameraTransform;          // カメラ位置
    Vector3 originalCameraPos;          // カメラの元の位置

    // Start is called before the first frame update
    void Start()
    {
        // 初期状態でのターゲットとカメラ位置の距離を取得
        diff = target.transform.position - transform.position;

        // メインカメラを有効にする
        SwitchToMainCamera();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Updateの後に処理される
    // ※Playerの位置計算が確実に終わったであろうタイミング
    // 
    private void LateUpdate()
    {
        // 線形補完関数でスムーズに移動
        // 追いつくまでにかける時間は一定なので、距離が離れているほど早く、近いほどゆっくり）
        transform.position = Vector3.Lerp(
            transform.position,                 // スタート位置（現在位置）
            target.transform.position - diff,   // ゴール位置（ターゲットから一定間隔離れた位置）
            Time.deltaTime * followSpeed        // スタート・ゴール間の位置の割合(0.0～1.0)
        );
    }

    // フロントカメラを有効にする
    public void SwitchToFrontCamera()
    {
        mainCamera.SetActive(false);
        frontCamera.SetActive(true);
    }

    // メインカメラを有効にする
    public void SwitchToMainCamera()
    {
        mainCamera.SetActive(true);
        frontCamera.SetActive(false);
    }

    // [追加] ダメージを受けた際にカメラを揺らす
    public void ShakeCamera()
    {
        StartCoroutine(ShakeCameraCoroutine(ShakeDuration, ShakeMagnitude));
    }


    IEnumerator ShakeCameraCoroutine(float shakeDuration, float shakeMagnitude)
    {
        float elapsed = 0.0f;   // 経過時間

        // 現在のカメラ位置を取得
        //originalCameraPos = cameraTransform.localPosition;
        originalCameraPos = transform.localPosition;

        // 指定時間カメラをランダムに揺らす
        while (elapsed < shakeDuration)
        {
            // カメラの揺れ幅をランダムに決める
            float dx = Random.Range(-1f, 1f) * shakeMagnitude;
            float dy = Random.Range(-1f, 1f) * shakeMagnitude;

            // カメラの位置を設定
            //cameraTransform.localPosition = new Vector3(
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
        //cameraTransform.localPosition = originalCameraPos;
        transform.localPosition = originalCameraPos;
    }
}
