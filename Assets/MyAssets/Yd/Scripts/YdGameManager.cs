using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


// ゲームステータス
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
    // ------------------------------------
    // 定数
    // ------------------------------------
    const string SCORE_TEXT_LABEL = "Score : ";      // スコア表示のラベル
    const string HI_SCORE_TEXT_LABEL = "Hi-Score : ";   // ハイスコア表示のラベル
    const string HI_SCORE_KEY = "HIGH_SCORE";    // ハイスコアを記録するキー

    // ------------------------------------
    // 外部から参照される Publicフィールド変数  
    // ------------------------------------
    public static YdGameState GameState;            // ゲームのステータスを管理するstatic変数
    public static bool IsBossEncountered = false;   // ボスにエンカウントした
    public static int TotalScore = 0;               // トータルスコア

    // ------------------------------------
    // Inspectorに表示するフィールド変数
    //  TODO: 外部から参照されないが、Inspectorに表示するためにpublicにしている変数は
    //  授業では出てこなかった　[SerializeField] private に変える
    // ------------------------------------
    public TextMeshProUGUI statusText;      // ゲームステータステキスト
    public TextMeshProUGUI highScoreText;   // ハイスコアテキスト
    public TextMeshProUGUI scoreText;       // スコアテキスト
    public GameObject uiPanel;              // UIパネル
    public GameObject pausePanel;           // 一時停止パネル
    public GameObject startButton;          // STARTボタン
    public GameObject closeButton;          // 一時停止ボタン
    public GameObject clearHiScoreButton;   // Clear Hi-Scoreボタン
    public GameObject operationGuidPanel;   // 操作ガイドパネル

    public YdPlayerController player;       // プレイヤー

    public YdLifePanel lifePanel;           // LifeのUI更新用

    public string retrySceneName;           // リトライシーン名

    public float bgmVolume = 0.5f;          // BGMサウンドボリューム
    public AudioClip bgmSound;              // BGMサウンドクリップ
    public AudioClip gameoverSound;         // GAME OVERサウンドクリップ
    public AudioClip bossBosssBattleSound;  // ボス戦サウンドクリップ
    public AudioClip gemeClearSound;        // GAME CLEARサウンドクリップ 

    public float blinkScoreTextInterval = 0.5f;      // ハイスコア更新時のスコア点滅間隔

    public float bgmFadeOutTime = 3.0f;      // Title画面へ戻るまでの時間
    public float bossBgmFadeOutTime = 1.5f;  // ボス戦BGMに切り替わるときのフェードアウト時間
    public float bossBgmFadeInTime  = 0.5f;  // ボス戦BGMに切り替わるときのフェードイン時間

    public float returnToTitleDelayTime = 1.5f; // タイトルへ戻るまでの待ち時間

    // ------------------------------------
    // Privateフィールド変数
    // ------------------------------------
    AudioSource audioSource;                // BGM用オーディオソース
    bool isStateProcessing = false;         // ステート変化時の処理中フラグ
    bool isGameEndProcessing = false;       // GameEnd処理中フラグ


    // ------------------------------------
    // Start is called before the first frame update
    // ------------------------------------
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // ゲームスタート待ち状態に設定
        GameState = YdGameState.WaitingToStart;
        // ボスエンカウントフラグも初期化
        IsBossEncountered = false;
        // スコア初期化
        TotalScore = 0;
        // ステート変化時の処理中フラグ初期化
        isStateProcessing = false;
        isGameEndProcessing = false;

        // ハイスコア表示
        UpdateHiScoreText();

        // スコア初期表示
        UpdateScoreText();

        // UIパネルを表示
        ActiveUIPanel();

        // 操作ガイドを表示
        operationGuidPanel.SetActive(true);

        // 一時停止ボタンとパネルを非表示
        closeButton.SetActive(false);
        pausePanel.SetActive(false);

        // BGM再生
        PlayLoopSound(bgmSound);
        // BGMをフェードインしていく
        StartCoroutine(FadeInLoopSound(1.0f));
    }


    // ------------------------------------
    // Update is called once per frame
    // ------------------------------------
    void Update()
    {
        // プレイ中
        if (GameState == YdGameState.Playing)
        {
            //常にLifeパネルとスコアを更新
            UpdateLifePanel();
            UpdateScoreText();

            // ボスにエンカウントしていたらボス戦開始
            if (IsBossEncountered)
            {
                // 二重呼び出し防止
                IsBossEncountered = false;

                // ボス戦開始
                StartCoroutine(StartBosssBattle());
            }
        }
        // ゲームオーバー
        else if ((GameState == YdGameState.GameOver) && !isStateProcessing)
        {
            // 二重呼び出し防止
            isStateProcessing = true;

            // GameOverになってから入った得点の表示も反映
            UpdateScoreText();

            // ゲームオーバー処理
            GameOver();
        }
        // ゲームクリア
        else if ((GameState == YdGameState.GameClear) && !isStateProcessing)
        {
            // 二重呼び出し防止
            isStateProcessing = true;

            // ボスクリアの得点も表示に反映
            UpdateScoreText();

            // ゲームクリア処理
            GameClear();
        }
        // ゲーム終了
        else if ((GameState == YdGameState.GameEnd) && !isGameEndProcessing)
        {
            // 二重呼び出し防止
            isGameEndProcessing = true;

            // ゲームエンド処理
            GameEnd();
        }
    }


    // ------------------------------------
    // スコア表示を更新
    // ------------------------------------
    void UpdateScoreText()
    {
        scoreText.text = SCORE_TEXT_LABEL + TotalScore;
    }


    // ------------------------------------
    // ハイスコア表示を更新
    // ------------------------------------
    void UpdateHiScoreText()
    {
        int hiScore = PlayerPrefs.GetInt(HI_SCORE_KEY);
        if (hiScore > 0)
        {
            // ハイスコアが記録されていれば表示
            highScoreText.text = HI_SCORE_TEXT_LABEL + hiScore;

            // ハイスコア消去ボタンを表示
            clearHiScoreButton.SetActive(true);
        }
        else
        {
            // ハイスコアが表示されていない場合は表示しない
            highScoreText.text = "";
            // ハイスコア消去ボタンを非表示
            clearHiScoreButton.SetActive(false);
        }
    }


    // ------------------------------------
    // ハイスコア更新
    // ------------------------------------
    void UpdateHiScore()
    {
        // もしもPlayerPrefsに記録しておいたスコアより高いスコアだったらPlayerPrefs更新
        if (PlayerPrefs.GetInt(HI_SCORE_KEY) < TotalScore)
        {
            PlayerPrefs.SetInt(HI_SCORE_KEY, TotalScore);

            // スコアを点滅させる
            StartCoroutine(BlinkScoreText());
        }
    }


    // ------------------------------------
    // スコアを点滅させる
    // ------------------------------------
    IEnumerator BlinkScoreText()
    {
        scoreText.color = Color.green;

        while (true)
        {
            // テキストを非表示
            scoreText.alpha = 0;
            yield return new WaitForSeconds(blinkScoreTextInterval);

            // テキストを表示
            scoreText.alpha = 1;
            yield return new WaitForSeconds(blinkScoreTextInterval);
        }
    }


    // ------------------------------------
    // ゲームオーバー処理
    // ------------------------------------
    void GameOver()
    {
        // mainTextを"GAME OVER"にして表示
        statusText.text = "GAME OVER";
        ActiveUIPanel();

        // ハイスコア更新
        UpdateHiScore();

        // GameOver BGM再生
        PlayLoopSound(gameoverSound);

        // BGMをフェードアウトしていく
        StartCoroutine(FadeOutLoopSound(bgmFadeOutTime));
    }


    // ------------------------------------
    // ゲームクリア処理
    // ------------------------------------
    void GameClear()
    {
        // mainTextを"GAME CLEAR"にして表示
        statusText.text = "GAME CLEAR";
        ActiveUIPanel();

        // ハイスコア更新
        UpdateHiScore();

        // GameClear BGM再生
        PlayLoopSound(gemeClearSound);

        // BGMをフェードアウトしていく
        StartCoroutine(FadeOutLoopSound(bgmFadeOutTime));
    }


    // ------------------------------------
    // ゲームエンド処理
    // ------------------------------------
    void GameEnd()
    {
        // Titleシーンに移動(ChangeSceneメソッド)
        ChangeToRetryScene();
    }


    // ------------------------------------
    // UIパネルを非表示にする
    // ------------------------------------
    void InactiveUIPanel()
    {
        uiPanel.SetActive(false);
    }


    // ------------------------------------
    // UIパネルを表示する
    // ------------------------------------
    void ActiveUIPanel()
    {
        uiPanel.SetActive(true);
    }


    // ------------------------------------
    // Lifeパネルの内容を更新
    // ------------------------------------
    void UpdateLifePanel()
    {
        lifePanel.UpdateLife(player.Life);
    }


    // ------------------------------------
    // BGMサウンドクリップをループ再生
    // ------------------------------------
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


    // ------------------------------------
    // 効果音を再生する
    // ------------------------------------
    void PlayOneshot(AudioClip soundClip)
    {
        if ((soundClip == null) || (audioSource == null)) return;

        audioSource.PlayOneShot(soundClip);
    }


    // ------------------------------------
    //  BGMサウンドを停止
    // ------------------------------------
    void StopLoopSound()
    {
        audioSource.Stop();
    }


    // ------------------------------------
    //  BGMサウンドを一時停止
    // ------------------------------------
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


    // ------------------------------------
    // BMGをフェードイン
    // ------------------------------------
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


    // ------------------------------------
    // BMGをフェードアウト
    // ------------------------------------
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


    // ------------------------------------
    // ゲームの一時停止
    // ------------------------------------
    void PauseGame()
    {
        // タイムスケールを0にすることで一時停止
        Time.timeScale = 0;

        // BGM一時停止
        PauseLoopSound(true);
    }


    // ------------------------------------
    // ゲームの一時停止を解除
    // ------------------------------------
    void ResumeGame()
    {
        // タイムスケールを1にすることで一時停止解除
        Time.timeScale = 1.0f;

        // BGM一時解除
        PauseLoopSound(false);
    }


    // ------------------------------------
    // リトライシーンに切り替え
    // ------------------------------------
    void ChangeToRetryScene()
    {
        // BGM停止
        StopLoopSound();

        // リトライの待機シーンに切り替え
        SceneManager.LoadScene(retrySceneName);
    }


    // ------------------------------------
    // スタートボタンが押された時の処理
    // ------------------------------------
    public void OnStartButtonClicked()
    {
        // ゲーム中状態に設定
        GameState = YdGameState.Playing;

        // mainTextを"GAME START"表示に
        statusText.text = "GAME START";

        // スタートボタンを非表示に
        if (startButton != null) startButton.SetActive(false);

        // 操作ガイドを非表示
        operationGuidPanel.SetActive(false);

        // 一時停止ボタンを表示
        closeButton.SetActive(true);

        // ハイスコア消去ボタンはスタート画面でしか使わないので非表示にしておく
        clearHiScoreButton.SetActive(false);

        // UIパネルを消す
        Invoke("InactiveUIPanel", 1.0f);

        // プレイヤーにゲーム開始を通知
        player.AttackStart();
    }


    // ------------------------------------
    // 戻るボタンが押された時の処理
    // ------------------------------------
    public void OnReturnToTitleButtonClicked()
    {
        StartCoroutine(ReturnToTitleWithWait());
    }


    // ------------------------------------
    // 少しまってからタイトルへ戻る
    // ------------------------------------
    IEnumerator ReturnToTitleWithWait()
    {
        // お別れボイス再生
        player.PlayVoiceBye();

        // BGMをフェードアウトしてAudioSourceの再生が終わるまで待機
        yield return StartCoroutine(FadeOutLoopSound(returnToTitleDelayTime));

        // タイトルシーンに切り替え
        SceneManager.LoadScene("Title");
    }


    // ------------------------------------
    // ポーズボタンが押された時の処理
    // ------------------------------------
    public void OnPauseButtonClicked()
    {
        // ゲームを一時停止
        PauseGame();

        // 一時停止ボタンを非表示
        closeButton.SetActive(false);

        // UIパネルを非表示
        uiPanel.SetActive(false);

        // 一時停止パネルを表示
        pausePanel.SetActive(true);
    }


    // ------------------------------------
    // リスタートボタンが押された時の処理
    // ------------------------------------
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


    // ------------------------------------
    // コンティニューボタンが押された時の処理
    // ------------------------------------
    public void OnContinueButtonClicked()
    {
        // ゲームの一時停止を解除
        ResumeGame();

        // 一時停止ボタンを表示
        closeButton.SetActive(true);

        // 一時停止パネルを非表示
        pausePanel.SetActive(false);
    }


    // ------------------------------------
    // ハイスコアクリアボタンを押された時の処理
    // ------------------------------------
    public void OnClearHiScoreButtonClicked()
    {
        // ハイスコア記録をゼロリセット
        PlayerPrefs.SetInt(HI_SCORE_KEY, 0);

        // ハイスコア表示を更新
        UpdateHiScoreText();
    }


    // ------------------------------------
    // [追加] ボス戦開始
    // ------------------------------------
    private IEnumerator StartBosssBattle()
    {
        // BGMをフェードアウトしていく
        yield return StartCoroutine(FadeOutLoopSound(bossBgmFadeOutTime));
        // BGM切り替え
        PlayLoopSound(bossBosssBattleSound);
        // BGMをフェードインしていく
        yield return StartCoroutine(FadeInLoopSound(bossBgmFadeInTime));
    }

}
