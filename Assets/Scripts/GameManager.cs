using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _txtmpPoint;
    [SerializeField] private int _point;

    void Start()
    {
        Application.targetFrameRate = 60;
        //QualitySettings.vSyncCount = 0;
    }

    public void UpdatePoint(int value)
    {
        Debug.Log($"Branch 1 merge new");
        Debug.Log($"Branch 1 merge");
        Debug.Log($"Branch 2 merge");
        _point += value;
        _txtmpPoint.text = _point.ToString();
    }
}
