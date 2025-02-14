using UnityEngine;
using UnityEngine.EventSystems;

public class YdVirtualPad : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    // ------------------------------------
    // Privateフィールド変数
    // ------------------------------------
    Vector2 startPos;   // ドラッグ開始位置
    Vector2 movement;   // ドラッグ方向

    CanvasGroup canvasGroup;


    // ------------------------------------
    // 初めてロードされるときに一度だけ呼び出される
    // ------------------------------------
    void Awake() 
    { 
        //rectTransform = GetComponent<RectTransform>(); 
        canvasGroup = GetComponent<CanvasGroup>(); 
    }


    // ------------------------------------
    // バーチャルスティックの位置をリセット
    // ------------------------------------
    void ResetPad()
    {
        startPos = Vector2.zero;
        movement = Vector2.zero;
    }



    // ------------------------------------
    // ドラッグ開始イベントハンドラ
    // ------------------------------------
    public void OnBeginDrag(PointerEventData eventData)
    {
        // ドラッグ中にオブジェクトが他のレイキャストをブロックしないようにする
        canvasGroup.blocksRaycasts = false;
    }


    // ------------------------------------
    // ドラッグ中イベントハンドラ
    // ------------------------------------
    public void OnDrag(PointerEventData eventData)
    {
        //if (eventData.pointerId == -1) //タッチ入力かチェック
        //{
            // ドラッグ開始後初回イベントなら開始位置を記録
            if (startPos == Vector2.zero)
            {
                startPos = eventData.position;
            }
            // ドラッグ中の移動量
            movement = eventData.position - startPos;
        //}
    }


    // ------------------------------------
    // ドラッグ終了イベントハンドラ
    // （ドラッグが終了して指が離された場合）
    // ------------------------------------
    public void OnEndDrag(PointerEventData eventData)
    {
        //if (eventData.pointerId == -1)
        //{
            // バーチャルスティックの位置をリセット
            ResetPad();
            // レイキャストブロックを元に戻す
            canvasGroup.blocksRaycasts = true;
        //}
    }


    // ------------------------------------
    // 水平方向の移動量を取得
    // ------------------------------------
    public float Horizontal()
    {
        float direction = 0;
        if (movement.x > 0.0f)
            direction = 1;
        else if (movement.x < 0.0f)
            direction = -1;
        else
            direction = 0;

        return direction;
    }


    // ------------------------------------
    // 垂直方向の移動量を取得
    // ------------------------------------
    public float Virtical()
    {
        float direction = 0;
        if (movement.y > 0.0f)
            direction = 1;
        else if (movement.y < 0.0f)
            direction = -1;
        else
            direction = 0;

        return direction;
    }

}
