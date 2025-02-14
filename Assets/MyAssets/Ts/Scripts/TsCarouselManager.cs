using System.Linq; // Enumerableで使う
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TsCarouselManager : MonoBehaviour
{
    [System.Serializable]
    struct _bannaaerData
    {
        public Sprite sprite;
        public string gameTitle;
        public string sceneName;
    }

    [SerializeField] private TsCarouselView _carouselView;
    [SerializeField] private _bannaaerData[] _gameBannaaers;

    private bool _isSetup;

    private void Start()
    {
        Setup();
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
                return new TsData(spriteResource, text, () => OnStartButtonClicked($"{sceneName}"));
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
    public void OnStartButtonClicked(string sceneName)
    {
        //Debug.Log("OnStartButtonClicked: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

}
