using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TsCredits : MonoBehaviour
{
    public TextMeshProUGUI creditsText; // クレジット表示テキスト
    public TextAsset creditsTextFile;   // クレジット文書のテキストファイル
    public GameObject creditPanel;      // クレジット表示パネル
    public GameObject creditButton;     // クレジット表示ボタン

    private void Awake()
    {
        // クレジットテキストを設定
        creditsText.text = creditsTextFile.text;
        // 自身を非表示に設定
        //gameObject.SetActive(false);
        creditPanel.SetActive(false);
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
        //gameObject.SetActive(false);
        creditPanel.SetActive(false);

        // クレジット表示ボタンを再表示
        creditButton.SetActive(true);
    }


    // クレジット表示ボタンが押された時の処理
    public void OnShowCreditsButtonClicked()
    {
        // 邪魔にならないようにクレジット表示ボタンを隠す
        creditButton.SetActive(false);

        // クレジット表示パネルを表示するように設定
        creditPanel.SetActive(true);
    }

}
