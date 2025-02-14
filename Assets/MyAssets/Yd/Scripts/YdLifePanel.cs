using UnityEngine;

public class YdLifePanel : MonoBehaviour
{
    // ------------------------------------
    // Inspectorに表示するフィールド変数
    //  TODO: 外部から参照されないが、Inspectorに表示するためにpublicにしている変数は
    //  授業では出てこなかった　[SerializeField] private に変える
    // ------------------------------------
    public GameObject[] icons;  // 子オブジェクトのimage達を格納(Life1～3)


    // ------------------------------------
    // 引数(int Life)の数だけicons配列の内容を表示
    // ------------------------------------
    public void UpdateLife(int life)
    {
        for (int i = 0; i < icons.Length; i++)
        {
            if (i < life) icons[i].SetActive(true);
            else icons[i].SetActive(false);
        }
    }
}
