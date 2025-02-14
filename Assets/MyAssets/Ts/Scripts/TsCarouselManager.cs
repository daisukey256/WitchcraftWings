using System.Collections;
using System.Linq; // Enumerableで使う
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public enum TsScreenDirection
{
    Portrait,
    LandscapeLeft,
    LandscapeRight
}


public class TsCarouselManager : MonoBehaviour
{
    [System.Serializable]
    struct _bannaaerData
    {
        public Sprite sprite;                       // ゲームのバナー画像
        public string gameTitle;                    // ゲームタイトル（バナー画像の中央に表示）
        public string sceneName;                    // ゲームの初期シーン名
        public TsScreenDirection screenDirection;   // 固定するゲームの回転方向 
    }

    [SerializeField] private TsCarouselView _carouselView;      // バナーのスクロール表示部
    [SerializeField] private _bannaaerData[] _gameBannaaers;    // 表示するゲーム情報の配列

    [SerializeField] private GameObject _loadingUiPanel;        // シーン切り替え中に表示するローディング中パネル
    [SerializeField] private Slider _slider;                    // 進捗率のスライダー

    private bool _isSetup;

    private void Start()
    {
        Setup();

        // 画面を縦に設定（各ゲームから戻ってきたときにもタイトルが縦表示になるように）
        Screen.orientation = ScreenOrientation.Portrait;

        // ローディング中パネル非表示
        _loadingUiPanel.SetActive(false);
        // スクロールビューを表示
        SetScrollViewEnable(true);
    }

    private void Setup()
    {
        if (_isSetup)
            return;

        var items = Enumerable.Range(0, _gameBannaaers.Length)
            .Select(i =>
            {
                var spriteResource = _gameBannaaers[i].sprite;
                var text = _gameBannaaers[i].gameTitle;
                var sceneName = _gameBannaaers[i].sceneName;
                var screenDirection = _gameBannaaers[i].screenDirection;
                return new TsData(spriteResource, text, () => OnStartButtonClicked($"{sceneName}", screenDirection));
            })
            .ToArray();
        _carouselView.Setup(items);
        _isSetup = true;
    }

    private void Cleanup()
    {
        if (!_isSetup)
            return;

        _carouselView.Cleanup();
        _isSetup = false;
    }

    // スタートボタンに割り当てるメソッド
    public void OnStartButtonClicked(string sceneName, TsScreenDirection screenDirection)
    {
        //Debug.Log("OnStartButtonClicked: " + sceneName);

        // スクリーンの向きを設定
        switch(screenDirection)
        {
            case TsScreenDirection.Portrait:
                Screen.orientation = ScreenOrientation.Portrait;
                break;
            case TsScreenDirection.LandscapeLeft:
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                break;
            case TsScreenDirection.LandscapeRight:
                Screen.orientation = ScreenOrientation.LandscapeRight;
                break;
            default:
                Screen.orientation = ScreenOrientation.Portrait;
                break;
        }

        // スクロールビューを非表示に
        SetScrollViewEnable(false);
        // ローディング中パネル表示
        _loadingUiPanel.SetActive(true);
        // シーン切り替え
        StartCoroutine(LoadScene(sceneName));

    }

    IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        while (!async.isDone)
        {
            _slider.value = async.progress;
            yield return null;
        }

    }

    // スクロールビューの表示/非表示を設定
    public void SetScrollViewEnable(bool enable)
    {
        _carouselView.enabled = enable;
    }

}
