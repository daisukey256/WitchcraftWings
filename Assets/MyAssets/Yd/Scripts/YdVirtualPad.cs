using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class YdVirtualPad : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Vector2 startPos;   // ドラッグ開始位置
    private Vector2 movement;   // ドラッグ方向

    private CanvasGroup canvasGroup;

    void Awake() 
    { 
        //rectTransform = GetComponent<RectTransform>(); 
        canvasGroup = GetComponent<CanvasGroup>(); 
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // ドラッグ中にオブジェクトが他のレイキャストをブロックしないようにする
        canvasGroup.blocksRaycasts = false;
    }

    // ドラッグされている場合
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

    // ドラッグが終了して指が離された場合
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

    // バーチャルスティックの位置をリセット
    private void ResetPad()
    {
        startPos = Vector2.zero;
        movement = Vector2.zero;
    }

    // Handle the movement logic
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
