using UnityEngine;
using UnityEngine.Networking;

public class CrateActor : NetworkBehaviour {
    public GameObject PowerUpPrefab;

    public void Explode() {
        if (!isServer)
            return;

        SpawnPowerUp();
        NetworkServer.Destroy(gameObject);
    }

    public void SpawnPowerUp() {
        var buff = EPowerUp.None;

        var random = Random.value;

        if (random >= 0f && random < 0.11f)
            buff = EPowerUp.FireUp;

        else if (random >= 0.2f && random < 0.25f)
            buff = EPowerUp.FireDown;

        else if (random >= 0.3f && random < 0.4f)
            buff = EPowerUp.SpeedUp;

        else if (random >= 0.5f && random < 0.54f)
            buff = EPowerUp.SpeedDown;

        else if (random >= 0.6f && random < 0.7f)
            buff = EPowerUp.BombUp;

        else if (random >= 0.7f && random < 0.74f)
            buff = EPowerUp.BombDown;

        else if (random >= 0.8 && random < 0.82f)
            buff = EPowerUp.Kick;

        else if (random >= 0.9 && random < 0.92f)
            buff = EPowerUp.Remote;

        if (buff != EPowerUp.None) {
            var powerupPosition = transform.position;
            powerupPosition.y += 0.5f;

            GameObject go = Instantiate(PowerUpPrefab, powerupPosition, Quaternion.identity);
            go.GetComponent<PowerUpActor>().SetBuff(buff);

            NetworkServer.Spawn(go);
        }
    }
}
