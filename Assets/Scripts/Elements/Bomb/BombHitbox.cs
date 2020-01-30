using UnityEngine;

public class BombHitbox : MonoBehaviour {
    public BombActor Actor;

    private SphereCollider _collider;

    private void Awake() {
        _collider = GetComponent<SphereCollider>();
    }

    private void OnTriggerExit(Collider other) {
        PlayerActor player = other.GetComponent<PlayerActor>();
        if (player != null) {
            _collider.isTrigger = false;
        }
    }

    public void Kick(Vector3 direction) {
        Actor.Kick(direction);
    }

    public void Explode() {
        Actor.Explode();
    }
}
