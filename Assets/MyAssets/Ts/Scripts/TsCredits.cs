using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TsCredits : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _creditsText;     // クレジット表示テキスト
    [SerializeField] private TextAsset _creditsTextFile;       // クレジット文書のテキストファイル
    [SerializeField] private GameObject _creditPanel;          // クレジット表示パネル
    [SerializeField] private GameObject _creditButton;         // クレジット表示ボタン

    [SerializeField] private TsCarouselManager _tsCarouselManager;    // 

    private void Awake()
    {
        // クレジットテキストを設定
        _creditsText.text = _creditsTextFile.text;
        // 自身を非表示に設定
        _creditPanel.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // 閉じるボタンが押された時の処理
    public void OnCloseButtonClicked()
    {
        // クレジット表示パネルを非表示に設定
        _creditPanel.SetActive(false);

        // クレジット表示ボタンを再表示
        _creditButton.SetActive(true);

        // 後ろに表示されるバナーのスクロールビューを再表示
        _tsCarouselManager.SetScrollViewEnable(true);
    }


    // クレジット表示ボタンが押された時の処理
    public void OnShowCreditsButtonClicked()
    {
        // 後ろに表示されるバナーのスクロールビューを隠す
        _tsCarouselManager.SetScrollViewEnable(false);

        // 邪魔にならないようにクレジット表示ボタンを隠す
        _creditButton.SetActive(false);

        // クレジット表示パネルを表示するように設定
        _creditPanel.SetActive(true);
    }

}
