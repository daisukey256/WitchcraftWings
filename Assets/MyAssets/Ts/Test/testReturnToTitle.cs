using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class testReturnToTitle : MonoBehaviour
{
    // リターンボタンに割り当てるメソッド
    public void OnReturnToTitleButtonClicked()
    {
        Debug.Log("OnReturnToTitleButtonClicked:");
        SceneManager.LoadScene("Title");
    }
}
