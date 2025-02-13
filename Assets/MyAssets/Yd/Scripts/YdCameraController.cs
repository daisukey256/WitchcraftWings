using UnityEngine;

public class YdCameraController : MonoBehaviour
{
    Vector3 diff;   // Playerとの距離さ

    public GameObject target;   // カメラが追尾するターゲット
    public float followSpeed;   // 追尾スピード

    // Start is called before the first frame update
    void Start()
    {
        // 初期状態でのターゲットとカメラ位置の距離を取得
        diff = target.transform.position - transform.position;
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
}
