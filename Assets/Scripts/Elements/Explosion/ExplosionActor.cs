using UnityEngine;
using UnityEngine.Networking;

public class ExplosionActor : NetworkBehaviour
{
    public float TimeToDestroy = 0.4f;

    private void Update()
    {
        if (GameController.Instance.IsGameOver)
            return;

        if (TimeToDestroy > 0f)
        {
            TimeToDestroy -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
