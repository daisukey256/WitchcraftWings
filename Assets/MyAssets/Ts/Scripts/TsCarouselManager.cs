using System.Linq; // Enumerable�Ŏg��
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
    //[SerializeField] private Button _setupButton;
    //[SerializeField] private Button _cleanupButton;

    private bool _isSetup;

    private void Start()
    {
        //_setupButton.onClick.AddListener(Setup);
        //_cleanupButton.onClick.AddListener(Cleanup);

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

    // �X�^�[�g�{�^���Ɋ��蓖�Ă郁�\�b�h
    public void OnStartButtonClicked(string sceneName)
    {
        Debug.Log("OnStartButtonClicked: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

}
