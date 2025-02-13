using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YdStageGenerator : MonoBehaviour
{
    const int StageChipSize = 48;   // 1ステージチップのサイズ

    int currentChipIndex;           // 今何番目にいるのか

    public Transform character;     // ターゲットキャラクタ(Player)
    public GameObject[] stageChips; // ステージチッププレファブ配列（ランダムに生成したいステージの素）
    public int startChipIndex;      // 自動生成開始インデックス
    public int preInstantiate;      //生成先読み個数（何個ステージを維持するか
    public List<GameObject> generatedStageList = new List<GameObject>();    //生成済みステージチップ保持リスト


    // Start is called before the first frame update
    void Start()
    {
        // stargChipIndes(1)から1引いた数(0)が最先端のステージ番号
        currentChipIndex = startChipIndex - 1;
        // まずはゲーム開始と同時にpreInstantiate(5個)分だけステージを生成する
        UpdateStage(preInstantiate);
    }

    // Update is called once per frame
    void Update()
    {
        // キャラクターの位置から現在のステージチップのインデックスを計算
        // PlayerのZ位置をステージのサイズで割るとPlayerの現在のIndexがわかる
        int charaPositionIndex = (int)(character.position.z / StageChipSize);

        // 次のステージチップに入ったらステージの更新処理を行う
        //  Playerの現在のIndexにpreInstantiate(5個)足した数が、最先端のステージ番号を上回ってしまった場合
        if (charaPositionIndex + preInstantiate > currentChipIndex)
        {
            UpdateStage(charaPositionIndex + preInstantiate);
        }
    }

    // 指定のIndexまでのステージチップを生成して、管理下に置く
    void UpdateStage(int toChipIndex)
    {
        // もし引数で指定された番号が最先端のステージ番号以下であれば何かの間違いなのでなにもしない
        if (toChipIndex <= currentChipIndex) return;

        // 指定ステージチップまで作成
        for (int i = currentChipIndex + 1; i <= toChipIndex; i++)
        {
            GameObject stageObject = GenerateStage(i);

            // 生成したステージチップを管理リストに追加
            generatedStageList.Add(stageObject);
        }

        // preInstantiate(5個) +2 =7個　をリストに記載されたステージ情報が上回ってしまったら
        // ステージ保有上限内になるまで古いステージを削除
        while (generatedStageList.Count > preInstantiate + 2) DestroyOldestStage();

        currentChipIndex = toChipIndex;
    }

    // 指定のインデックス位置にStageオブジェクトをランダムに生成
    GameObject GenerateStage(int chipIndex)
    {
        int nextStageChip = Random.Range(0, stageChips.Length);

        // stageChips配列からランダムな番号のステージを選択
        // 引数にあたえられたステージ番号*サイズのz位置にあたらしく生成
        // 特に回転はしない
        GameObject stageObject = Instantiate(
            stageChips[nextStageChip],
            new Vector3(0, 0, chipIndex * StageChipSize),
            Quaternion.identity
        );

        return stageObject;
    }

    // 一番古いステージを削除
    void DestroyOldestStage()
    {
        GameObject oldStage = generatedStageList[0];
        generatedStageList.RemoveAt(0);
        Destroy(oldStage);
    }
}
