using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public PlayerActor Actor;

    public void TakeDamage()
    {
        Actor.TakeDamage();
    }

    public bool TakeBuff(EPowerUp buff)
    {
        return Actor.TakeBuff(buff);
    }
}
