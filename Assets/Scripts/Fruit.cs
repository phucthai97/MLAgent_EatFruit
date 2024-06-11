using UnityEngine;

public class Fruit : MonoBehaviour
{
    [SerializeField] private ObjectPooling _objectPooling;

    public ObjectPooling ObjectPooling { set => _objectPooling = value; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        AgentPlayer agentPlayer = other.GetComponent<AgentPlayer>();
        if (agentPlayer != null)
        {
            agentPlayer.HandleReWard(0.2f);
            _objectPooling.ReturnToPool(gameObject);
            _objectPooling.CreateNewItem();
        }
        else
        {
            ControllerPlayer controllerPlayer = other.GetComponent<ControllerPlayer>();
            Debug.Log($"ControllerPlayer touch");
            controllerPlayer.AddReward();
            _objectPooling.ReturnToPool(gameObject);
            _objectPooling.CreateNewItem();
        }

    }
}
