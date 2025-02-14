using System.Collections;
using Unity.VisualScripting;
using UnityChan;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class YdPlayerController : MonoBehaviour
{

    // ダメージに関する定数
    const int defaultLife = 3;                  // プレイヤーLife

    // 暫定
    const float gameOverWaitTime = 5f;          // ゲームオーバー演出待ち時間
    const float gameClearWaitTime = 10f;        // ゲームクリア演出待ち時間

    // Inspectorに表示する変数
    public float rotationAngle = 45.0f;         // プレイヤーの傾斜角度
    public float rotationSpeed = 5.0f;          // プレイヤーの傾斜速度

    public float sidewayMovemLimit = 6.0f;      // 横移動幅の制限値
    public float sidewayMovementSpeed = 2.0f;   // 横移動速度

    public float speedZ;                        // 前進スピード
    public float accelerationZ;                 // トップスピードにいくための加速度

    public float floatingHeight = 0.5f;         // 浮いている高さ

    public float KnockbackForce = 10f;          // 衝突時のノックバックの勢い
    public float KnockbackDuration = 0.5f;      // ノックバック状態継続時間

    public Transform MuzzleTransform;           // 銃口のトランスフォーム
    public GameObject BulletPrefab;             // 弾のプレファブ
    public GameObject Broom;                    // 箒のオブジェクト

    public float shotForce = 500.0f;            // 発射するパワー
    public float shootInterval = 0.5f;          // 連射間隔

    public YdVirtualPad VirtualPad;             // バーチャルパッド

    public GameObject playerBody;               // プレイヤーのボディ

    public AudioClip shotSE;                    // 発射音
    public AudioClip damegeSE;                  // ダメージ音

    public AudioSource shotAudioSource;         // 射撃音用 
    public AudioSource damegeAudioSource;       // ダメージ音用

    public YdCameraController cameraController; // カメラコントローラ

    // [追加] ボイス追加
    public AudioSource voiceAudioSource;        // ボイス用
    public AudioClip voiceDamage;               // ダメージ時ボイス
    public AudioClip voiceOve;                  // ゲームオーバー時ボイス
    public AudioClip voiceClear;                // ゲームクリア時ボイス
    public AudioClip voiceFinish;               // フィニッシュ時ボイス


    public int life = defaultLife;  // プレイヤーのライフ

    // ダメージに関する変数
    bool isKnockback = false;   // ノックバック中かどうか

    // プレイヤーの操作に関する変数
    float axisH; //横軸の値

    Transform cockpitTransform; // プレイヤー内のZ軸回転する機体部分

    Vector3 moveDirection = Vector3.zero;

    // 射撃状態
    bool isCurrentlyShooting;    // 射撃中かどうか

    Quaternion originalRotation;    // プレイヤー内の機体の元の回転
    Quaternion leftRotation;        // 左回転のゴール
    Quaternion rightRotation;       // 右回転のゴール

    Quaternion originalPlayeRot;   // プレイヤーの元の回転

    // TODO:ダメージアニメーションの再生後にキャラの位置が変わる問題の暫定対応
    Vector3 originalPlayerBodyPos;      // キャラクター本体の元の位置
    Quaternion originalPlayerBodyRot;   // キャラクター本体の元の回転

    // [追加]ボス戦中
    bool isBossBattle = false;      // ボス戦中かどうか
    bool isPlayGameClear = false;   // ゲームクリア演出をプレイ中
    bool isHovering = false;        // 前進を止めて空中で止まる

    //コンポーネントの参照用
    CharacterController controller;
    Animator playerAnimator;    // プレイヤーのアニメーター
    Rigidbody playerRigidbody;  // 


    // Start is called before the first frame update
    void Start()
    {
        // 必要なコンポーネントを取得
        controller = GetComponent<CharacterController>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = playerBody.GetComponent<Animator>();

        // プレイヤー内のZ軸回転するコックピットのtransformを取得
        cockpitTransform = transform.Find("Cockpit");
        if (cockpitTransform == null )
        {
            Debug.LogError("Cockpit Not Found");
        }

        // プレイヤー内部の回転パーツの回転位置と傾斜角度を準備
        originalRotation = cockpitTransform.rotation;
        leftRotation = Quaternion.Euler(0, 0, rotationAngle);
        rightRotation = Quaternion.Euler(0, 0, -rotationAngle);

        // TODO:暫定対応 プレイヤーボディの初期位置を保持
        originalPlayerBodyPos = playerBody.transform.localPosition;
        originalPlayerBodyRot = playerBody.transform.localRotation;
        // プレイヤーオブジェクトの初期状態の回転も保持
        originalPlayeRot = transform.rotation;


    }

    // Update is called once per frame
    void Update()
    {
        // TODO: ゲームクリア演出の呼び出しは要整理        
        if (YdGameManager.gameState == YdGameState.GameClear)
        {
            if (isPlayGameClear) return;    // 2重呼び出しガード
            isPlayGameClear = true;
            // ゲームクリア処理を呼び出す
            StartCoroutine(GameClear());

            return;
        }
        else if (YdGameManager.gameState != YdGameState.Playing)
        {
            // プレイ中でなければなにもしない
            return;
        }

        // ノックバック中はなにもしない
        if (isKnockback) return;


        // プレイヤーを移動
        MovePlayer();

        // プレイ中にLifeが0になったらゲームオーバー
        if((life <= 0) && (YdGameManager.gameState == YdGameState.Playing))
        {
            // ゲームステータスをゲームオーバーに
            YdGameManager.gameState = YdGameState.GameOver;
            // ゲームオーバー処理を呼び出す
            StartCoroutine(GameOver());
        }
    }

    // プレイヤーの移動・回転
    void MovePlayer()
    {
        // 前進
        // 徐々に加速しZ方向に常に前進させる
        //  (1) 加速度を決める
        float acceleratedZ = moveDirection.z + (accelerationZ * Time.deltaTime);
        //  (2) moveDirectionを0からspeedZまでの値に制限した最終的な加速度に置き換え
        moveDirection.z = Mathf.Clamp(acceleratedZ, 0, speedZ);

        // ボス戦中は前進を止める
        if (isHovering) moveDirection.z = 0;


        // 横移動と傾斜
        Quaternion rot;
        axisH = VirtualPad.Horizontal();
        // バーチャルパッドが使われていない場合は左右キーもチェック
        if (axisH == 0)
        {
            axisH = Input.GetAxisRaw("Horizontal"); //左右のキーを検知
        }

        if (axisH > 0.0f)
        {
            // 左に回転
            rot =
                Quaternion.Slerp(cockpitTransform.rotation, leftRotation, Time.deltaTime * rotationSpeed);

            // 横移動
            if (transform.position.x > sidewayMovemLimit)
                moveDirection.x = 0;
            else
                moveDirection.x = sidewayMovementSpeed;

        }
        else if (axisH < 0.0f)
        {
            // 右に回転
            rot =
                Quaternion.Slerp(cockpitTransform.rotation, rightRotation, Time.deltaTime * rotationSpeed);

            // 横移動
            if (transform.position.x < -sidewayMovemLimit)
                moveDirection.x = 0;
            else
                moveDirection.x = -sidewayMovementSpeed;

        }
        else
        {
            // 傾きを戻す
            rot =
                Quaternion.Slerp(cockpitTransform.rotation, originalRotation, Time.deltaTime * rotationSpeed);

            moveDirection.x = 0;
        }

        // 移動
        Vector3 globalDirection = transform.TransformDirection(moveDirection);
        transform.Translate(globalDirection * Time.deltaTime);

        // 機体を傾ける回転
        cockpitTransform.rotation = rot;

        // TODO:暫定対応
        // 浮かび挙がらないように高さを制限
        transform.position = new Vector3(transform.position.x, floatingHeight, transform.position.z);

    }


    // 弾の連射ON/OFF
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

    // 弾の発射
    void Shoot()
    {
        // 銃口の位置に弾のインスタンスを生成
        GameObject bullet = Instantiate(BulletPrefab, MuzzleTransform.position, Quaternion.identity);

        // 
        Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
        bulletRigidbody.AddForce(transform.forward * shotForce);

        // 発射サウンドを再生
        PlaySoundOneShot(shotAudioSource, shotSE);

    }


    public void AttackStart()
    {
        // 射撃開始
        Shooting(true);
    }


    // キャラクターの回転をリセット
    void ResetPlayerRotation()
    {
        // プレイヤーの回転をリセット
        //transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.rotation = originalPlayeRot;

        // プレイヤー内の機体を傾ける回転をリセット
        cockpitTransform.rotation = originalRotation;

    }

    // 衝突時のノックバック
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
            playerRigidbody.AddForce(-transform.forward * KnockbackForce, ForceMode.Impulse);

            // ダメージアニメーションを再生
            playerAnimator.SetTrigger("damaged");

            // 一定時間後にノックバック状態を解除
            Invoke("RecoverFromKnockback", KnockbackDuration);
        }
    }

    // ノックバック状態から復帰する
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
        playerBody.transform.localPosition = originalPlayerBodyPos;
        playerBody.transform.localRotation = originalPlayerBodyRot;
        // プレイヤーオブジェクトの回転も初期様態に戻す
        ResetPlayerRotation();

        // 射撃再開
        Shooting(true);
    }

    // ダメージを受けた
    void ReceiveDamage()
    {
        // 射撃停止
        Shooting(false);

        // [追加] ダメージを受けた際にカメラを揺らす
        cameraController.ShakeCamera();
        //StartCoroutine(ShakeCamera(ShakeDuration, ShakeMagnitude));

        // 跳ね返る
        Knockback();

        // ライフ減
        life--;

        // ダメージ時ボイス
        PlaySoundOneShot(voiceAudioSource, voiceDamage);
        // ダメージサウンドを再生
        PlaySoundOneShot(damegeAudioSource, damegeSE);
    }

    // ゲームオーバー時処理
    IEnumerator GameOver()
    {
        // 射撃停止
        Shooting(false);

        // キャラクタの傾きをリセット
        ResetPlayerRotation();

        // 箒に重力を付けて落とす
        Broom.AddComponent<Rigidbody>();

        // ゲームオーバーアニメーションを再生
        playerAnimator.SetBool("isOver", true);

        // 負けボイス
        PlaySoundOneShot(voiceAudioSource, voiceOve);

        // 指定した時間待機する
        yield return new WaitForSeconds(gameOverWaitTime);
     }

    // ゲームクリア時処理
    IEnumerator GameClear()
    {

        // 演出開始まで少し待つ
        yield return new WaitForSeconds(1.0f);

        // フロントカメラに切り替え
        cameraController.SwitchToFrontCamera();

        // 射撃停止
        Shooting(false);

        // キャラクタの傾きをリセット
        ResetPlayerRotation();

        // 箒に重力を付けて落とす
        Broom.AddComponent<Rigidbody>();

        // ゲームオーバーアニメーションを再生
        playerAnimator.SetBool("isClear", true);

        // 勝ちボイス
        PlaySoundOneShot(voiceAudioSource, voiceClear);

        // 指定した時間待機する
        yield return new WaitForSeconds(gameOverWaitTime);
    }


    void PlaySoundOneShot(AudioSource audioSource, AudioClip audioClip) 
    {
        // AudioSourceかAudioClipがnullならなにもしない
        if ((audioSource == null) || (audioClip == null)) return;

        audioSource.PlayOneShot(audioClip);
    }


    // 衝突処理
    void OnCollisionEnter(Collision collision)
    {
        // プレイ中でなければなにもしない
        if (YdGameManager.gameState != YdGameState.Playing) return;

        // ノックバック中は衝突処理をしない
        if (isKnockback) return;

        // 何にぶつかってもダメージを受ける
        ReceiveDamage();

    }

    // ゲート通過処理
    void OnTriggerExit(Collider other)
    {
        // ボスゲートを通過した
        if (other.tag == "YdBossGate")
        {
            // ちょっと進んでからボス戦に入る
            StartCoroutine(StartBosssBattle(0.5f));
        }
    }

    // [追加] ボス先を開始する
    IEnumerator StartBosssBattle(float waitSeconds)
    {
        if (isBossBattle) yield break;   // 二重呼び出し防止
        isBossBattle = true;

        // 指定時間待ってから前進を止めてホバリング
        yield return new WaitForSeconds(waitSeconds);
        isHovering = true;

        // GameManagerにボス戦開始を通知
        // TODO: GameManagerと疎結合にする
        YdGameManager.instance.StartBosssBattle();


        // すこし待ってボス戦ボイスを再生
        yield return new WaitForSeconds(1.0f);
        PlaySoundOneShot(voiceAudioSource, voiceFinish);
    }

}
