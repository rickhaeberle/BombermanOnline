using UnityEngine;

public class PowerUpHitbox : MonoBehaviour
{
    public PowerUpActor Actor;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerHitbox>();
        if (player != null)
        {
            bool taken = player.TakeBuff(Actor.Buff);
            if (taken)
            {
                Actor.Taken(!player.Actor.hasAuthority);
            }
        }
    }

    public void Destroy()
    {
        Actor.Destroyed();
    }

}
