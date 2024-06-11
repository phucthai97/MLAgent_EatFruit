using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayFPS : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _fpsText;
    private float deltaTime = 0.0f;
    private float _updateInterval = 0.25f;
    private float _nextUpdateTime = 0.0f;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        if (Time.time >= _nextUpdateTime)
        {
            float fps = 1.0f / deltaTime;
            _fpsText.text = string.Format("{0:0.} fps", fps);
            _nextUpdateTime = Time.time + _updateInterval;
        }
    }
}
