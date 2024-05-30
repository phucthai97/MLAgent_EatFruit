using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;

    public GameManager GameManager { set => _gameManager = value; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.gameObject.GetComponent<Player>();
        player.HandleReWard(0.2f);
        _gameManager.ReturnToPool(gameObject);
        _gameManager.CreateNewItem();
    }
}
