using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameController : NetworkBehaviour {
    public static GameController Instance;

    public GameObject InGamePanel;
    public Text TimerText;

    public GameObject EndGamePanel;
    public Text EndGameText;

    public GameObject BombPrefab;
    public GameObject CratePrefab;

    public GameObject PlayerActorPrefab;

    public AudioSource BackgroundMusicSource;

    public AudioClip WinnerSfx;
    public AudioClip DrawSfx;
    public AudioClip TimeUpSfx;

    public List<Floor> Floors;
    public List<Spawner> Spawners;

    [SyncVar]
    public float Timer = 300f;

    public float SuddenDeathStartTimer = 60f;
    public float SuddentDeathBombInterval = 4f;
    public int SuddentDeathIncrementEvery = 8;

    private int _suddentDeathBombsCount = 1;
    private int _suddentDeathIncrementCounter = 0;

    public bool IsGameOver { get; private set; }

    private List<GamePlayerConnection> _players = new List<GamePlayerConnection>();

    private Dictionary<GamePlayerConnection, PlayerActor> _playersActor = new Dictionary<GamePlayerConnection, PlayerActor>();
    private Dictionary<PlayerActor, List<BombActor>> _playerBombs = new Dictionary<PlayerActor, List<BombActor>>();

    private Floor _currentFloor;
    private Vector3 _initialCamPosition;

    private bool _suddentDeathStarted = false;
    private float _suddentDeathBombIntervalTimer = 0f;

    private void Awake() {
        Instance = this;
        MatchMakerManager.StopBroadcasting();

        IsGameOver = true;

        _initialCamPosition = Camera.main.transform.position;
    }

    private void Update() {
        if (IsGameOver)
            return;

        if (isServer)
            UpdateServer();

        UpdateClient();
    }

    private void UpdateClient() {
        TimerText.text = Timer.ToString("F0");

        if (Timer < SuddenDeathStartTimer) {
            TimerText.color = Color.red;
            BackgroundMusicSource.pitch = 1.1f;
        }
    }

    private void UpdateServer() {
        Timer -= Time.deltaTime;

        if (Timer <= 0) {
            IsGameOver = true;

            _players.ForEach(item => {
                item.TellClientsGameEnded(string.Empty, true);
            });

            return;
        }

        if (_suddentDeathStarted) {

            _suddentDeathBombIntervalTimer += Time.deltaTime;
            if (_suddentDeathBombIntervalTimer >= SuddentDeathBombInterval) {
                _suddentDeathBombIntervalTimer = 0;

                _suddentDeathIncrementCounter++;
                if (_suddentDeathIncrementCounter >= SuddentDeathIncrementEvery) {
                    _suddentDeathBombsCount++;
                    _suddentDeathIncrementCounter = 0;
                }

                for (int i = 0; i < _suddentDeathBombsCount; i++) {
                    var floor = Floors[Random.Range(0, Floors.Count)];
                    floor.DropBomb();

                }
            }

        } else if (Timer <= SuddenDeathStartTimer) {
            _suddentDeathStarted = true;

        }
    }

    public void StartGame() {
        IsGameOver = false;
        GoToFloor(Floors.First());

        InGamePanel.SetActive(true);

        EndGamePanel.SetActive(false);
        EndGameText.text = string.Empty;

        _suddentDeathStarted = false;
    }

    public void GoToFloor(Floor floor) {
        if (_currentFloor == floor)
            return;

        _currentFloor = floor;

        int idx = Floors.FindIndex(item => item == floor);
        for (int i = 0; i < Floors.Count; i++) {
            Floors[i].SetVisibility(i == idx);
        }

        ChangeCameraPosition(idx);

        GameManager.Instance.ChangeFloor(idx);
    }

    private void ChangeCameraPosition(int floor) {
        Vector3 newPosition = Vector3.zero;
        newPosition.x = _initialCamPosition.x;
        newPosition.y = _initialCamPosition.y + (5 * floor);
        newPosition.z = _initialCamPosition.z + (-3.5f * floor);

        Camera.main.transform.position = newPosition;
    }

    public void EndGame(string winner, bool timeup) {
        BackgroundMusicSource.Pause();

        IsGameOver = true;

        InGamePanel.SetActive(false);
        TimerText.text = string.Empty;

        EndGamePanel.SetActive(true);

        if (timeup) {
            EndGameText.text = "TIME UP!";
            SFXPlayerManager.Instance.Play(TimeUpSfx, 0.9f);

        } else if (!string.IsNullOrEmpty(winner)) {
            EndGameText.text = $"{winner}\nWINS!";
            SFXPlayerManager.Instance.Play(WinnerSfx, 0.9f);

        } else {
            EndGameText.text = "TIE";
            SFXPlayerManager.Instance.Play(DrawSfx, 0.9f);
        }

    }

    public void OnQuitGameButtonClick() {
        MatchMakerManager.Disconnect();
    }

    //SERVER
    public void RegisterPlayer(GamePlayerConnection connection) {
        _players.Add(connection);

        var lobby = MatchMakerManager.singleton as NetworkLobbyManager;
        if (_players.Count == lobby.minPlayers) {

            Floors.ForEach(floor => {
                floor.SpawnCrates(Spawners);
            });

            _players.ForEach(item => {
                SpawnPlayer(item);
            });
        }
    }

    public void RegisterPlayerDeath(PlayerActor actor) {
        if (IsGameOver)
            return;

        GamePlayerConnection connection = null;

        foreach (var conn in _playersActor.Keys) {
            if (_playersActor[conn] == actor) {
                connection = conn;
                break;
            }
        }

        if (connection != null) {
            _playersActor.Remove(connection);
            NetworkServer.Destroy(actor.gameObject);

            string winner = null;

            if (_playersActor.Keys.Count == 1) {
                winner = _playersActor.Keys.First().PlayerName;
                IsGameOver = true;

            } else if (_playersActor.Keys.Count == 0) {
                IsGameOver = true;

            }

            if (IsGameOver) {
                _players.ForEach(item => {
                    item.TellClientsGameEnded(winner, false);
                });

            }
        }
    }

    public void SpawnPlayer(GamePlayerConnection connection) {
        Spawner spawner = Spawners[connection.Slot];

        Vector3 spawnPosition = spawner.transform.position;
        spawnPosition.y = 0;

        GameObject go = Instantiate(GameController.Instance.PlayerActorPrefab, spawnPosition, Quaternion.identity);

        var player = go.GetComponent<PlayerActor>();
        player.Character = connection.Character;

        _playersActor[connection] = player;
        _playerBombs[player] = new List<BombActor>();

        NetworkServer.SpawnWithClientAuthority(go, connection.gameObject);
    }

    public void SpawnBomb(PlayerActor player, Vector3 position, float timer, int range) {
        if (!isServer)
            return;

        var bombs = _playerBombs[player];

        if (player.HasRemoteControlBuff && bombs.Any(item => item.IsRemote)) {
            bombs.First().Explode();
            return;
        }

        if (bombs.Count >= player.MaxBombs)
            return;

        GameObject go = Instantiate(BombPrefab, new Vector3(Mathf.RoundToInt(position.x), position.y + BombPrefab.transform.position.y, Mathf.RoundToInt(position.z)), BombPrefab.transform.rotation);
        var bomb = go.GetComponent<BombActor>();
        bomb.SetRange(range);

        if (player.HasRemoteControlBuff) {
            bomb.SetRemote();
        } else {
            bomb.SetTimer(timer);
        }

        bombs.Add(bomb);

        NetworkServer.Spawn(go);
    }

    public void DropBomb(Vector3 position, float timer, int range) {
        if (!isServer)
            return;

        GameObject go = Instantiate(BombPrefab, new Vector3(Mathf.RoundToInt(position.x), position.y + BombPrefab.transform.position.y, Mathf.RoundToInt(position.z)), BombPrefab.transform.rotation);
        var bomb = go.GetComponent<BombActor>();
        bomb.SetRange(range);
        bomb.SetTimer(timer);

        NetworkServer.Spawn(go);
    }

    public void RemoveBomb(BombActor bomb) {
        foreach (var player in _playerBombs.Keys) {
            _playerBombs[player].Remove(bomb);
        }
    }
}
