using System;
using UnityEngine;

public class TsData
{
    public Sprite SpriteResource { get; }   //セルの背景テクスチャを読み込むためのキーのアクセサ
    public string Text { get; }             // セルに表示する文字列のアクセサ
    public Action Clicked { get; }          // セルがクリックされた時に実行するアクション

    // コンストラクタ
    public TsData(Sprite spriteResource, string text, Action clicked)
    {
        SpriteResource = spriteResource;
        Text = text;
        Clicked = clicked;
    }
}
