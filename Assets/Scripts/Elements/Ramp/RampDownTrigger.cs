using UnityEngine;

public class RampDownTrigger : MonoBehaviour
{
    public RampActor Actor;

    private void OnTriggerEnter(Collider other)
    {
        PlayerHitbox player = other.GetComponent<PlayerHitbox>();
        if (player != null && player.Actor.hasAuthority) 
        {
            Actor.GoToDownFloor();
        }
    }
}
