using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class ObjectPooling : MonoBehaviour
{
    [SerializeField] private int _currentTotalNumPos = 2;
    [SerializeField] private List<Transform> _transOfPosItems;
    private List<Transform> availablePositions;
    [SerializeField] private GameObject _prefabItem;
    [SerializeField] private int _initialSize = 3;
    private Queue<GameObject> _fruitsPool = new Queue<GameObject>();
    [SerializeField] private List<GameObject> _fruits = new List<GameObject>();
    [SerializeField] private GameObject _parentGo;

    public List<GameObject> Fruits { get => _fruits; set => _fruits = value; }
    EnvironmentParameters m_ResetParams;

    void Awake()
    {
        for (int i = 0; i < _initialSize; i++)
        {
            GameObject obj = Instantiate(_prefabItem);
            obj.GetComponent<Fruit>().ObjectPooling = this;
            obj.transform.SetParent(_parentGo.transform);
            obj.SetActive(false);
            _fruitsPool.Enqueue(obj);
        }
    }

    void Start()
    {
        //var envParams = Academy.Instance.EnvironmentParameters;
        //envParams.RegisterCallback("difficulty", UpdateDifficulty);
        m_ResetParams = Academy.Instance.EnvironmentParameters;
        SetResetParameters();

        ResetAvailablePositions();
        for (int i = 0; i < _initialSize; i++)
            CreateNewItem();
    }

    public void SetResetParameters()
    {
        _currentTotalNumPos = (int)m_ResetParams.GetWithDefault("difficulty", 4.0f);
    }

    void UpdateDifficulty(float value)
    {
        Debug.Log($"UpdateDifficulty {value}");
        _currentTotalNumPos = (int)value;
    }

    // Hàm để reset danh sách các vị trí có sẵn
    private void ResetAvailablePositions()
    {
        availablePositions = new List<Transform>();
        for (int i = 0; i < _currentTotalNumPos; i++)
            availablePositions.Add(_transOfPosItems[i]);
    }

    public GameObject GetFruit()
    {
        if (_fruitsPool.Count > 0)
        {
            GameObject obj = _fruitsPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(_prefabItem);
            return obj;
        }
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        _fruitsPool.Enqueue(obj);
    }

    public void CreateNewItem()
    {
        GameObject fruitGo = GetFruit();
        fruitGo.transform.position = GetRandomPosition().position + new Vector3(0, 3.4f, 0);

        if (Fruits.Count < _initialSize)
            Fruits.Add(fruitGo);
    }

    private Transform GetRandomPosition()
    {
        if (availablePositions.Count == 0)
        {
            ResetAvailablePositions();
        }

        int randomIndex = Random.Range(0, availablePositions.Count);
        Transform chosenPosition = availablePositions[randomIndex];
        availablePositions.RemoveAt(randomIndex);
        return chosenPosition;
    }
}
