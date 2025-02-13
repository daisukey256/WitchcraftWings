using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public enum YdGameState
{
    WaitingToStart,
    Playing,
    GameOver,
    GameClear,
    GameEnd
}

public class YdGameManager : MonoBehaviour
{
    // static変数
    public static YdGameState gameState;    // ゲームのステータスを管理するstatic変数

    // Inspectorに表示する変数
    public static YdGameManager instance;   // GameManager
    public int totalScore = 0;              // トータルスコア

    public TextMeshProUGUI statusText;      // ゲームステータステキスト
    public TextMeshProUGUI scoreText;       // スコアテキスト
    public GameObject uiPanel;              // UIパネル
    public GameObject pausePanel;           // 一時停止パネル
    public GameObject startButton;          // STARTボタン
    public GameObject closeButton;          // 一時停止ボタン

    public YdPlayerController player;       // プレイヤー

    public YdLifePanel lifePanel;           // LifeのUI更新用

    public float gameEndWaitTime = 3.0f;    // Title画面へ戻るまでの時間
    public string retrySceneName;           // リトライシーン名

    public float bgmVolume = 0.5f;          // BGMサウンドボリューム
    public AudioClip bgmSound;              // BGMサウンドクリップ
    public AudioClip gameoverSound;         // GAME OVERサウンドクリップ

    // ロ＝カル変数
    AudioSource audioSource;                // BGM用オーディオソース

    bool isPaused = false;                  // 一時停止フラグ

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            // 既にinstanceがあるのに生成されてしまった場合は破棄
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // ゲームスタート待ち状態に設定
        gameState = YdGameState.WaitingToStart;

        // スコア初期表示
        UpdateScore();

        // UIパネルを表示
        ActiveUIPanel();


        // 一時停止ボタンとパネルを非表示
        closeButton.SetActive(false);
        pausePanel.SetActive(false);

        // BGM再生
        PlayLoopSound(bgmSound);
        // BGMをフェードインしていく
        StartCoroutine(FadeInLoopSound(1.0f));
    }

    // Update is called once per frame
    void Update()
    {
        //もしステータスが"gameend"なら何もしない
        if (gameState == YdGameState.GameEnd) return;

        //常にLifeパネルを更新
        UpdateLifePanel();

        // ステータスが"gameover"なら
        if (gameState == YdGameState.GameOver)
        {
            // ステータスを"gameend”にする
            gameState = YdGameState.GameEnd;

            // mainTextを"GAME OVER"にして表示
            statusText.text = "GAME OVER";
            ActiveUIPanel();

            // GameOver BGM再生
            PlayLoopSound(gameoverSound);
            // BGMをフェードアウトしていく
            StartCoroutine(FadeOutLoopSound(gameEndWaitTime));

            // 時間差でTitleシーンに移動(ChangeSceneメソッド)
            Invoke("ChangeToRetryScene", gameEndWaitTime);

            // 以降の処理は行わない
            return;
        }
        else if (gameState == YdGameState.GameClear)
        {
            // ステータスを"gameend”にする
            gameState = YdGameState.GameEnd;

            // mainTextを"GAME CLEAR"にして表示
            statusText.text = "GAME CLEAR";
            ActiveUIPanel();

            // 時間差でTitleシーンに移動(ChangeSceneメソッド)
            Invoke("ChangeToRetryScene", gameEndWaitTime);

            // 以降の処理は行わない
            return;
        }

    }

    // スコアを更新
    void UpdateScore()
    {
        scoreText.text = "Score: " + totalScore;
    }

    // UIパネルを非表示にする
    void InactiveUIPanel()
    {
        uiPanel.SetActive(false);
    }

    // UIパネルを表示する
    void ActiveUIPanel()
    {
        uiPanel.SetActive(true);
    }

    // Lifeパネルの内容を更新
    void UpdateLifePanel()
    {
        lifePanel.UpdateLife(player.life);
    }


    // スコアを追加
    public void AddScore(int scoreValue)
    {
        totalScore += scoreValue;
        UpdateScore();
    }

    // BGMサウンドクリップをループ再生
    void PlayLoopSound(AudioClip soundClip)
    {
        if ((soundClip != null) && (audioSource != null))
        {
            audioSource.loop = true;
            audioSource.clip = soundClip;
            audioSource.volume = bgmVolume;

            // 再生中の場合は停止してから再生開始 
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            audioSource.Play();             
        }
    }

    //  BGMサウンドを停止
    void StopLoopSound()
    {
        audioSource.Stop();
    }

    //  BGMサウンドを一時停止
    void PauseLoopSound(bool isPaused)
    {
        if (isPaused)
        {
            // BGM一時停止
            audioSource.Pause();
        }
        else
        {
            // BGM一時解除
            audioSource.UnPause();
        }
    }

    // BMGをフェードイン
    public IEnumerator FadeInLoopSound(float duration)
    {
        audioSource.volume = 0;
        audioSource.Play();

        float startVolume = audioSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 1, t / duration);
            yield return null;
        }

        audioSource.volume = bgmVolume;
    }

    // BMGをフェードアウト
    IEnumerator FadeOutLoopSound(float duration)
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();
    }    

    // ゲームの一時停止
    void PauseGame()
    {
        isPaused = true;
        // タイムスケールを0にすることで一時停止
        Time.timeScale = 0;

        // BGM一時停止
        PauseLoopSound(true);
    }

    // ゲームの一時停止を解除
    void ResumeGame()
    {
        isPaused = false;
        // タイムスケールを1にすることで一時停止解除
        Time.timeScale = 1.0f;

        // BGM一時解除
        PauseLoopSound(false);
    }


    // リトライシーンに切り替え
    void ChangeToRetryScene()
    {
        // BGM停止
        StopLoopSound();

        // リトライの待機シーンに切り替え
        SceneManager.LoadScene(retrySceneName);
    }

    // スタートボタンが押された時の処理
    public void OnStartButtonClicked()
    {
        // ゲーム中状態に設定
        gameState = YdGameState.Playing;

        // mainTextを"GAME START"表示に
        statusText.text = "GAME START";

        // スタートボタンを非表示に
        if (startButton != null) startButton.SetActive(false);

        // 一時停止ボタンを表示
        closeButton.SetActive(true);

        // UIパネルを消す
        Invoke("InactiveUIPanel", 1.0f);

        // プレイヤーにゲーム開始を通知
        player.AttackStart();
    }

    // 戻るボタンが押された時の処理
    public void OnReturnToTitleButtonClicked()
    {
        // タイトルシーンに切り替え
        SceneManager.LoadScene("Title");
    }

    // ポーズボタンが押された時の処理
    public void OnPauseButtonClicked()
    {
        // ゲームを一時停止
        PauseGame();

        // 一時停止ボタンを非表示
        closeButton.SetActive(false);

        // 一時停止パネルを表示
        pausePanel.SetActive(true);
    }

    // リスタートボタンが押された時の処理
    public void OnRestartButtonClicked()
    {
        // ゲームを再開　
        // TODO要確認：再開せずにTimeScaleを0にしたままシーンを切り替えるとバグる？
        ResumeGame();

        // 一時停止パネルを非表示
        pausePanel.SetActive(false);

        // リトライシーンに切り替え
        ChangeToRetryScene();
    }

    // コンティニューボタンが押された時の処理
    public void OnContinueButtonClicked()
    {
        // ゲームの一時停止を解除
        ResumeGame();

        // 一時停止ボタンを表示
        closeButton.SetActive(true);

        // 一時停止パネルを非表示
        pausePanel.SetActive(false);
    }


}
