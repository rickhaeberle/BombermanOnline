using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Floor : NetworkBehaviour {

    public GameObject GroundParent;
    public GameObject WallParent;

    public bool ShouldStartVisible = false;

    [Range(0, 1)]
    public float SpawnCrateProbability = 0.8f;

    private float _floorY = 0;

    private void Awake() {
        _floorY = transform.position.y;
    }

    private void Start() {
        SetVisibility(ShouldStartVisible);

    }

    public void SetVisibility(bool visible) {

        foreach (Transform groundObj in GroundParent.transform) {
            var renderer = groundObj.GetComponent<Renderer>();
            renderer.enabled = visible;
        }

        foreach (Transform wallObj in WallParent.transform) {
            var renderer = wallObj.GetComponent<Renderer>();
            renderer.enabled = visible;
        }
    }

    public void SpawnCrates(List<Spawner> spawners) {

        HashSet<Vector2> _cratesPossiblePositions = new HashSet<Vector2>();

        foreach (Transform groundObj in GroundParent.transform) {
            Vector2 groundGridPosition = MapToGrid(groundObj.position);
            _cratesPossiblePositions.Add(groundGridPosition);
        }

        foreach (Spawner spawner in spawners) {
            Vector2 spawnerGridPosition = MapToGrid(spawner.gameObject.transform.position);
            _cratesPossiblePositions.Remove(spawnerGridPosition);

            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    if (i == j)
                        continue;

                    Vector2 temp = new Vector2(i, j);
                    _cratesPossiblePositions.Remove(spawnerGridPosition + temp);

                }
            }
        }

        List<GameObject> crates = new List<GameObject>();

        foreach (Vector2 crateGridPosition in _cratesPossiblePositions) {
            if (Random.value > SpawnCrateProbability)
                continue;

            Vector3 position = GridToMap(crateGridPosition);
            position.y = position.y + GameController.Instance.CratePrefab.transform.position.y;

            GameObject go = Instantiate(GameController.Instance.CratePrefab, position, Quaternion.identity);
            crates.Add(go);

            NetworkServer.Spawn(go);
        }
    }

    public void DropBomb() {
        int attempts = 5;

        for (int i = 0; i < attempts; i++) {
            var groundIdx = Random.Range(0, GroundParent.transform.childCount);
            var groundObj = GroundParent.transform.GetChild(groundIdx);
            var ground = groundObj.GetComponent<Ground>();

            if (ground == null)
                continue;

            bool canPlaceBomb = ground.CanPutBomb();
            if (!canPlaceBomb)
                continue;

            ground.Blink();

            var timer = Random.Range(2f, 4f);
            var force = Random.Range(1, 5);

            StartCoroutine(DropBombRoutine(ground.transform.position, timer, force));

            return;
        }
    }

    private Vector2 MapToGrid(Vector3 position) {
        return new Vector2(position.x, position.z);
    }

    private Vector3 GridToMap(Vector2 position) {
        return new Vector3(position.x, _floorY, position.y);
    }

    IEnumerator DropBombRoutine(Vector3 position, float timer, int force) {
        yield return new WaitForSeconds(1f);
        GameController.Instance.DropBomb(position, timer, force);

    }
}
