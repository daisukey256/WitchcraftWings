using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TsRotateCamera : MonoBehaviour
{
    
    [SerializeField] float rotationSpeed = 0.1f;    // カメラの回転速度

    [SerializeField] private Material morningSkybox;    // 午前のSkyBox
    [SerializeField] private Material afternoonSkybox;  // 午後のSkyBox
    [SerializeField] private Material sunsetSkybox;     // 夕方のSkyBox
    [SerializeField] private Material duskSkybox;       // 日没直後のSkyBox
    [SerializeField] private Material nightSkybox;      // 夜のSkyBox

    [SerializeField] private float morningStartHour     = 6f;   // 午前の開始時間
    [SerializeField] private float afternoonStartHour   = 12f;  // 午後の開始時間
    [SerializeField] private float sunsetStartHour      = 17f;  // 夕方の開始時間
    [SerializeField] private float duskStartHour        = 18f;  // 日没の開始時間
    [SerializeField] private float nightStartHour       = 19f;  // 夜の開始時間

    [SerializeField] private float checkIntervalSec = 600f;     // SkyBoxの更新チェック間隔秒

    private float currentHour;



    // Start is called before the first frame update
    void Start()
    {
        // 現在時刻によりSkyboxを切り替えるチェック
        StartCoroutine(CheckSkyboxRoutine(checkIntervalSec));
    }

    // Update is called once per frame
    void Update()
    {
        // Y軸周りに回転させる
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }

    // 現在時刻によりSkyboxを切り替えるチェック
    IEnumerator CheckSkyboxRoutine(float checkIntervalSec)
    {
        while (true)
        {
            UpdateSkybox();
            yield return new WaitForSeconds(checkIntervalSec);
        }
    }

    void UpdateSkybox()
    {
        currentHour = DateTime.Now.Hour + DateTime.Now.Minute / 60.0f;

        if (currentHour >= morningStartHour && currentHour < afternoonStartHour)
        {
            // 午前中
            RenderSettings.skybox = morningSkybox;
        }
        else if (currentHour >= afternoonStartHour && currentHour < sunsetStartHour)
        {
            // 午後
            RenderSettings.skybox = afternoonSkybox;
        }
        else if (currentHour >= sunsetStartHour && currentHour < duskStartHour)
        {
            // 夕方
            RenderSettings.skybox = sunsetSkybox;
        }
        else if (currentHour >= duskStartHour && currentHour < nightStartHour)
        {
            // 日没
            RenderSettings.skybox = duskSkybox;
        }
        else
        {
            // 夜
            RenderSettings.skybox = nightSkybox;
        }
    }
}
