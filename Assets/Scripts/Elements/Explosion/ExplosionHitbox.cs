using UnityEngine;

public class ExplosionHitbox : MonoBehaviour {

    public ExplosionActor Actor;

    private bool _destroyedCrate = false;

    private void OnTriggerEnter(Collider other) {
        if (!Actor.isServer)
            return;

        PlayerHitbox player = other.GetComponentInChildren<PlayerHitbox>();
        if (player != null) {
            player.TakeDamage();
            return;
        }

        CrateHitbox crate = other.GetComponentInChildren<CrateHitbox>();
        if (crate != null) {
            _destroyedCrate = true;
            crate.Destroyed();
            return;
        }

        if (!_destroyedCrate) {
            BombHitbox bomb = other.GetComponentInChildren<BombHitbox>();
            if (bomb != null) {
                bomb.Explode();
                return;
            }

            PowerUpHitbox powerup = other.GetComponentInChildren<PowerUpHitbox>();
            if (powerup != null) {
                powerup.Destroy();
                return;
            }
        }
    }
}
