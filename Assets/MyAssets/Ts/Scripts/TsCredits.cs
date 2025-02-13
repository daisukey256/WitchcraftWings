using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TsCredits : MonoBehaviour
{
    public TextMeshProUGUI creditsText; // クレジット表示テキスト
    public TextAsset creditsTextFile;   // クレジット文書のテキストファイル

    private void Awake()
    {
        // クレジットテキストを設定
        creditsText.text = creditsTextFile.text;
        // 自身を非表示に設定
        //gameObject.SetActive(false);
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
        // 自身を非表示に設定
        gameObject.SetActive(false);
    }


    public void OnShowCreditsButtonClicked()
    {
        // 自身を表示するように設定
        gameObject.SetActive(true);
    }

}
