using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<Transform> _transOfPosItems;
    private List<Transform> availablePositions;
    [SerializeField] private GameObject _prefabItem;
    [SerializeField] private int _initialSize = 3;
    private Queue<GameObject> _fruitsPool = new Queue<GameObject>();
    [SerializeField] private List<GameObject> _fruits = new List<GameObject>();
    [SerializeField] private GameObject _parentGo;

    public List<GameObject> Fruits { get => _fruits; set => _fruits = value; }

    void Awake()
    {
        for (int i = 0; i < _initialSize; i++)
        {
            GameObject obj = Instantiate(_prefabItem);
            obj.GetComponent<Fruit>().GameManager = this;
            obj.transform.SetParent(_parentGo.transform);
            obj.SetActive(false);
            _fruitsPool.Enqueue(obj);
        }
    }

    void Start()
    {
        ResetAvailablePositions();

        for (int i = 0; i < _initialSize; i++)
            CreateNewItem();
    }

    // Hàm để reset danh sách các vị trí có sẵn
    private void ResetAvailablePositions()
    {
        availablePositions = new List<Transform>(_transOfPosItems);
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
        fruitGo.transform.position = GetRandomPosition().position + new Vector3(0, 3.6f, 0);

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
