using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurriculumManager : MonoBehaviour
{
    public static CurriculumManager Instance { get; private set; }

    // Đảm bảo rằng instance này không bị phá hủy khi chuyển scene
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public int CurrentTotalNumPos = 2;
    [SerializeField] float AverageReward = 0;
    [SerializeField] int TotalEpisode = 0;
    [SerializeField] float TotalReward = 0;

    public void Calculate(float reward)
    {
        TotalEpisode++;
        TotalReward += reward;
        AverageReward = TotalReward / TotalEpisode;
    }
}
