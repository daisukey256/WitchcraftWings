using System.Linq; // Enumerableで使う
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        public Sprite sprite;
        public string gameTitle;
        public string sceneName;
        public TsScreenDirection screenDirection;
    }

    [SerializeField] private TsCarouselView _carouselView;
    [SerializeField] private _bannaaerData[] _gameBannaaers;

    private bool _isSetup;

    private void Start()
    {
        Setup();

        // 画面を縦に設定（各ゲームから戻ってきたときにもタイトルが縦表示になるように）
        Screen.orientation = ScreenOrientation.Portrait;
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

        // シーン切り替え
        SceneManager.LoadScene(sceneName);
    }

}
