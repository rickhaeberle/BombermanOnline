using UnityEngine;

public class CrateHitbox : MonoBehaviour {

    public CrateActor Actor;

    public void Destroyed() {
        Actor.Explode();
    }
}
