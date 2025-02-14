using System.Collections;
using UnityEngine;

// 機体の傾きの回転方向
public enum YdRotationDirection
{
    Left, 
    Right, 
    Center
}

public class YdPlayerController : MonoBehaviour
{
    // ------------------------------------
    // 定数
    // ------------------------------------
    const int DEFAULT_LIFE = 3;                  // プレイヤーLife

    // ------------------------------------
    // 外部から参照される Publicフィールド変数  
    // ------------------------------------
    public int Life = DEFAULT_LIFE;              // プレイヤーのライフ

    // ------------------------------------
    // Inspectorに表示するフィールド変数
    //  TODO: 外部から参照されないが、Inspectorに表示するためにpublicにしている変数は
    //  授業では出てこなかった　[SerializeField] private に変える
    // ------------------------------------
    public float speedZ        = 8f;            // 前進スピード
    public float accelerationZ = 5f;            // トップスピードにいくための加速度

    public float rotationAngle = 45.0f;         // プレイヤーの傾斜角度
    public float rotationSpeed = 5.0f;          // プレイヤーの傾斜速度

    public float shotForce = 2000.0f;           // 発射するパワー
    public float shootInterval = 0.2f;          // 連射間隔

    public float sidewayMovemLimit    = 5.5f;   // 横移動幅の制限値
    public float sidewayMovementSpeed = 4.5f;   // 横移動速度

    public float floatingHeight = 1f;           // 浮いている高さ

    public float knockbackForce = 13f;          // 衝突時のノックバックの勢い
    public float knockbackDuration = 0.5f;      // ノックバック状態継続時間

    public Transform muzzleTransform;           // 銃口のトランスフォーム
    public GameObject bulletPrefab;             // 弾のプレファブ
    public Transform bulletsParentTransform;    // 弾のインスタンスを格納するオブジェクトのトランスフォーム
    public GameObject broom;                    // 箒のオブジェクト

    public Transform cockpitTransform;          // プレイヤー内のZ軸回転する機体部分のトランスフォーム
    public GameObject playerCharBody;           // プレイヤーキャラクターのボディ

    public YdCameraController cameraController; // カメラコントローラ TODO:イベントシステムを使うなどで疎結合にする

    public YdVirtualPad virtualPad;             // バーチャルパッド

    public AudioClip shotSE;                    // 発射音
    public AudioClip damegeSE;                  // ダメージ音

    public AudioSource shotAudioSource;         // 射撃音用 
    public AudioSource damegeAudioSource;       // ダメージ音用

    // [追加] ボイス追加
    public AudioSource voiceAudioSource;        // ボイス用
    public AudioClip voiceDamage;               // ダメージ時ボイス
    public AudioClip voiceOve;                  // ゲームオーバー時ボイス
    public AudioClip voiceClear;                // ゲームクリア時ボイス
    public AudioClip voiceFinish;               // フィニッシュ時ボイス
    public AudioClip voiceBye;                  // お別れボイス

    // [追加] 演出時間調整
    public float gameOverWaitTime  = 3f;       // ゲームオーバー演出待ち時間
    public float gameClearWaitTime = 3f;       // ゲームクリア演出待ち時間


    // ------------------------------------
    // Privateフィールド変数
    // ------------------------------------
    // ダメージに関する変数
    bool isKnockback = false;   // ノックバック中かどうか

    // プレイヤーの操作に関する変数
    float axisH; //横軸の値
    bool isCurrentlyShooting;       // 射撃中かどうか

    Vector3 moveDirection = Vector3.zero;   // プレイヤーの移動方向ベクトル

    Quaternion originalRotation;    // プレイヤー内の機体の元の回転
    Quaternion leftRotation;        // 左回転のゴール
    Quaternion rightRotation;       // 右回転のゴール

    Quaternion originalPlayeRot;    // プレイヤーの元の回転

    // TODO:ダメージアニメーションの再生後にキャラの位置が変わる問題の暫定対応
    Vector3 originalPlayerCharBodyPos;      // キャラクター本体の元の位置
    Quaternion originalPlayerCharBodyRot;   // キャラクター本体の元の回転

    // [追加]ボス戦中
    bool isBossBattle = false;      // ボス戦中かどうか
    bool isPlayGameClear = false;   // ゲームクリア演出をプレイ中
    bool isHovering = false;        // 前進を止めて空中で止まる

    //コンポーネントの参照用
    CharacterController controller; // プレイヤーキャラクターのコントローラー
    Animator playerAnimator;        // プレイヤーキャラクターのアニメーター
    Rigidbody playerRigidbody;      // プレイヤーのRigidbody


    // ------------------------------------
    // Start is called before the first frame update
    // ------------------------------------
    void Start()
    {
        // 必要なコンポーネントを取得
        controller = GetComponent<CharacterController>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = playerCharBody.GetComponent<Animator>();

        // プレイヤー内部の回転パーツの回転位置と傾斜角度を準備
        originalRotation = cockpitTransform.rotation;
        leftRotation = Quaternion.Euler(0, 0, rotationAngle);
        rightRotation = Quaternion.Euler(0, 0, -rotationAngle);

        // TODO:暫定対応 プレイヤーボディの初期位置を保持
        originalPlayerCharBodyPos = playerCharBody.transform.localPosition;
        originalPlayerCharBodyRot = playerCharBody.transform.localRotation;
        // プレイヤーオブジェクトの初期状態の回転も保持
        originalPlayeRot = transform.rotation;

    }


    // ------------------------------------
    // Update is called once per frame
    // ------------------------------------
    void Update()
    {
        if (YdGameManager.GameState == YdGameState.Playing)
        {
            // ゲーム中

            // ノックバック中はなにもしない
            if (isKnockback) return;

            // プレイヤーを移動
            MovePlayer();

            // プレイ中にLifeが0になったらゲームオーバー
            if ((Life <= 0) && (YdGameManager.GameState == YdGameState.Playing))
            {
                // ゲームステータスをゲームオーバーに
                YdGameManager.GameState = YdGameState.GameOver;
                // ゲームオーバー処理を呼び出す
                StartCoroutine(GameOver());
            }
        }
        else if ((YdGameManager.GameState == YdGameState.GameClear) && (!isPlayGameClear))
        {
            // ゲームクリア かつ ゲームクリア演出を開始していない
            isPlayGameClear = true;
            // ゲームクリア処理を呼び出す
            StartCoroutine(GameClear());
        }

    }


    // ------------------------------------
    // プレイヤーの移動・回転
    // ------------------------------------
    void MovePlayer()
    {
        // 前進  徐々に加速しZ方向に常に前進させる
        //  (1) 加速度を決める
        float acceleratedZ = moveDirection.z + (accelerationZ * Time.deltaTime);
        //  (2) moveDirectionを0からspeedZまでの値に制限した最終的な加速度に置き換え
        moveDirection.z = Mathf.Clamp(acceleratedZ, 0, speedZ);

        // ボス戦中は前進を止める
        if (isHovering) moveDirection.z = 0;


        // 横移動と傾斜
        YdRotationDirection rotDir = YdRotationDirection.Center;
        axisH = virtualPad.Horizontal();
        // バーチャルパッドが使われていない場合は左右キーもチェック
        if (axisH == 0)
        {
            axisH = Input.GetAxisRaw("Horizontal"); //左右のキーを検知
        }

        if (axisH > 0.0f)
        {
            // 左に傾ける
            rotDir = YdRotationDirection.Left;

            // 横移動
            if (transform.position.x > sidewayMovemLimit)
                moveDirection.x = 0;
            else
                moveDirection.x = sidewayMovementSpeed;

        }
        else if (axisH < 0.0f)
        {
            // 右に傾ける
            rotDir = YdRotationDirection.Right;

            // 横移動
            if (transform.position.x < -sidewayMovemLimit)
                moveDirection.x = 0;
            else
                moveDirection.x = -sidewayMovementSpeed;

        }
        else
        {
            // 傾きを戻して中央に
            rotDir = YdRotationDirection.Center;

            moveDirection.x = 0;
        }

        // 移動
        Vector3 globalDirection = transform.TransformDirection(moveDirection);
        transform.Translate(globalDirection * Time.deltaTime);

        // プレイヤー内の機体を傾けるために回転させる
        RotateCockpit(rotDir);

        // TODO:暫定対応
        // 浮かび挙がらないように高さを制限
        transform.position = new Vector3(transform.position.x, floatingHeight, transform.position.z);

    }


    // ------------------------------------
    // プレイヤー内の機体を傾けるために回転させる
    // ------------------------------------
    void RotateCockpit(YdRotationDirection rotDir)
    {
        Quaternion targetRot;
        switch (rotDir)
        {
            case YdRotationDirection.Left:
                targetRot = leftRotation;
                break;
            case YdRotationDirection.Right:
                targetRot = rightRotation;
                break;
            case YdRotationDirection.Center:
            default:
                targetRot = originalRotation;
                break;
        }

        cockpitTransform.rotation = 
            Quaternion.Slerp(cockpitTransform.rotation, targetRot, Time.deltaTime * rotationSpeed);
    }


    // ------------------------------------
    // 弾の連射ON/OFF
    // ------------------------------------
    void Shooting(bool isShooting)
    {
        const string shootMethodName = "Shoot";

        if (isShooting)
        {
            // 射撃中でなければ射撃ON
            if (!isCurrentlyShooting)
            {
                // shootInterval時間間隔で弾を打ち出す
                InvokeRepeating(shootMethodName, 0, shootInterval);
                isCurrentlyShooting = !isCurrentlyShooting;
            }
        }
        else
        {
            // 射撃なら射撃OFF
            if (isCurrentlyShooting)
            {
                //Invokeで実行しているCallメソッドを停止する
                CancelInvoke(shootMethodName);
                isCurrentlyShooting = !isCurrentlyShooting;
            }
        }
    }


    // ------------------------------------
    // 弾の発射
    // ------------------------------------
    void Shoot()
    {
        // 銃口の位置に弾のインスタンスを生成
        GameObject bullet = Instantiate(bulletPrefab, muzzleTransform.position, Quaternion.identity);

        // 弾がHierarchy上でちらばらないように指定のオブジェクトの子として配置
        bullet.transform.parent = bulletsParentTransform;

        // YdBulletプレファブのインスタンスが生成する爆発も指定のオブジェクトの
        // 子として配置するために、YdBulletクラスのPublicフィールドを介して渡す
        bullet.GetComponent<YdBullet>().ExplosionsParentTransform = bulletsParentTransform;

        // 弾を打ち出す
        Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
        bulletRigidbody.AddForce(transform.forward * shotForce);

        // 発射サウンドを再生
        PlaySoundOneShot(shotAudioSource, shotSE);

    }


    // ------------------------------------
    // 攻撃開始
    // ------------------------------------
    public void AttackStart()
    {
        // 射撃開始
        Shooting(true);
    }


    // ------------------------------------
    // キャラクターの回転をリセット
    // ------------------------------------
    void ResetPlayerRotation()
    {
        // プレイヤーの回転をリセット
        transform.rotation = originalPlayeRot;

        // プレイヤー内の機体を傾ける回転をリセット
        cockpitTransform.rotation = originalRotation;

    }


    // ------------------------------------
    // 衝突時のノックバック
    // ------------------------------------
    void Knockback()
    {
        if (!isKnockback) 
        {
            //Debug.Log("Knockback");

            // TODO:物理による回転を止められない問題の暫定対応
            // 強制的に回転をリセット
            ResetPlayerRotation();

            // 後方にノックバックで飛ばす
            isKnockback = true;
            playerRigidbody.AddForce(-transform.forward * knockbackForce, ForceMode.Impulse);

            // ダメージアニメーションを再生
            playerAnimator.SetTrigger("damaged");

            // 一定時間後にノックバック状態を解除
            Invoke("RecoverFromKnockback", knockbackDuration);
        }
    }


    // ------------------------------------
    // ノックバック状態から復帰する
    // ------------------------------------
    void RecoverFromKnockback()
    {
        //Debug.Log("RecoverFromKnockback");

        isKnockback = false;

        // TODO:物理による回転を止められない問題の暫定対応
        // 移動と回転をリセット
        playerRigidbody.velocity = Vector3.zero;
        playerRigidbody.angularVelocity = Vector3.zero;

        // TODO:ダメージアニメーションの再生後にキャラの位置が変わる問題の暫定対応
        // プレイヤーのモデルを初期様態に戻す
        playerCharBody.transform.localPosition = originalPlayerCharBodyPos;
        playerCharBody.transform.localRotation = originalPlayerCharBodyRot;
        // プレイヤーオブジェクトの回転も初期様態に戻す
        ResetPlayerRotation();

        // 射撃再開
        Shooting(true);
    }

    // ------------------------------------
    // ダメージを受けた
    // ------------------------------------
    void ReceiveDamage()
    {
        // 射撃停止
        Shooting(false);

        // [追加] ダメージを受けた際にカメラを揺らす
        cameraController.ShakeCamera();

        // 跳ね返る
        Knockback();

        // ライフ減
        Life--;

        // ダメージ時ボイス
        PlaySoundOneShot(voiceAudioSource, voiceDamage);
        // ダメージサウンドを再生
        PlaySoundOneShot(damegeAudioSource, damegeSE);
    }


    // ------------------------------------
    // ゲームオーバー時処理
    // ------------------------------------
    IEnumerator GameOver()
    {
        // 射撃停止
        Shooting(false);

        // キャラクタの傾きをリセット
        ResetPlayerRotation();

        // 箒に重力を付けて落とす
        broom.AddComponent<Rigidbody>();

        // ゲームオーバーアニメーションを再生
        playerAnimator.SetBool("isOver", true);

        // 負けボイス
        PlaySoundOneShot(voiceAudioSource, voiceOve);

        // 指定した時間待機する
        yield return new WaitForSeconds(gameOverWaitTime);

        // ゲームステータスをゲームエンドに
        YdGameManager.GameState = YdGameState.GameEnd;
    }


    // ------------------------------------
    // ゲームクリア時処理
    // ------------------------------------
    IEnumerator GameClear()
    {
        // 演出開始まで少し待って、機体の傾きを中央に戻す
        float waitTime = 1.0f;
        float elapsedTime = 0f;
        while (elapsedTime < waitTime)
        {
            // プレイヤー内の機体の傾きを中央に戻す
            RotateCockpit(YdRotationDirection.Center);

            elapsedTime += Time.deltaTime;

            // 次のフレームまで待つ
            yield return null;
        }

        // 射撃停止
        Shooting(false);

        // キャラクタの傾きをリセット
        ResetPlayerRotation();

        // フロントカメラに切り替え
        cameraController.SwitchToFrontCamera();

        // 箒に重力を付けて落とす
        broom.AddComponent<Rigidbody>();

        // ゲームオーバーアニメーションを再生
        playerAnimator.SetBool("isClear", true);

        // 勝ちボイス
        PlaySoundOneShot(voiceAudioSource, voiceClear);

        // 指定した時間待機する
        yield return new WaitForSeconds(gameClearWaitTime);

        // ゲームステータスをゲームエンドに
        YdGameManager.GameState = YdGameState.GameEnd;
    }


    // ------------------------------------
    // 効果音を再生
    // ------------------------------------
    void PlaySoundOneShot(AudioSource audioSource, AudioClip audioClip) 
    {
        // AudioSourceかAudioClipがnullならなにもしない
        if ((audioSource == null) || (audioClip == null)) return;

        audioSource.PlayOneShot(audioClip);
    }


    // ------------------------------------
    // 衝突処理
    // ------------------------------------
    void OnCollisionEnter(Collision collision)
    {
        // プレイ中でなければなにもしない
        if (YdGameManager.GameState != YdGameState.Playing) return;

        // ノックバック中は衝突処理をしない
        if (isKnockback) return;

        // 何にぶつかってもダメージを受ける
        ReceiveDamage();

    }


    // ------------------------------------
    // ボスゲート通過処理
    // ------------------------------------
    void OnTriggerExit(Collider other)
    {
        // ボスゲートを通過した
        if (other.tag == "YdBossGate")
        {
            // ちょっと進んでからボス戦に入る
            StartCoroutine(StartBosssBattle(0.5f));
        }
    }


    // ------------------------------------
    // [追加] ボス戦を開始する
    // ------------------------------------
    IEnumerator StartBosssBattle(float waitSeconds)
    {
        if (isBossBattle) yield break;   // 二重呼び出し防止
        isBossBattle = true;

        // 指定時間待ってから前進を止めてホバリング
        yield return new WaitForSeconds(waitSeconds);
        isHovering = true;

        // GameManagerにボス戦開始を通知
        // TODO: GameManagerと疎結合にする(イベントシステムを使う等）
        YdGameManager.IsBossEncountered = true;


        // すこし待ってボス戦ボイスを再生
        yield return new WaitForSeconds(1.0f);
        PlaySoundOneShot(voiceAudioSource, voiceFinish);
    }

    // お別れボイス再生
    public void PlayVoiceBye()
    {
        // お別れボイス
        PlaySoundOneShot(voiceAudioSource, voiceBye);
    }
}
