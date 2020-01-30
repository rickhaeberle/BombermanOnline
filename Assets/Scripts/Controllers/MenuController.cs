using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public GameObject MainMenuPanel;
    public GameObject OnlinePanel;
    public GameObject LanPanel;
    public GameObject CreditsPanel;
    public GameObject LobbyPanel;

    public InputField PlayerNameInput;

    public InputField OnlineRoomNameInput;
    public GameObject AvailableInternetRoomsPanel;

    public InputField LanRoomNameInput;
    public GameObject AvailableLanRoomsPanel;

    public Button JoinRoomButtonPrefab;

    public Image CharactersImage;

    public float TimeToChangeMainMenuPlayer = 3f;
    public float TimeToUpdateMatchList = 2f;

    private float _timeToChangeMainMenuPlayerTimer;
    private float _timeToUpdateMatchListTimer;

    private int _characterSpriteIdx = 0;

    private bool _atMainMenu = false;
    private bool _searchingInternetMatch = false;
    private bool _searchingLanMatch = false;

    private List<ECharacter> _characters = new List<ECharacter>() {
        ECharacter.White, ECharacter.Blue, ECharacter.Black, ECharacter.Red, ECharacter.Pink, ECharacter.Yellow
    };

    private void Awake()
    {
        if (string.IsNullOrEmpty(PlayerSettings.PlayerName))
        {
            PlayerSettings.PlayerName = $"Player {Random.Range(1000, 10000)}";
        }

        PlayerNameInput.text = PlayerSettings.PlayerName;

        GoToHomeMenu();
    }

    private void Update()
    {
        if (_atMainMenu)
        {
            _timeToChangeMainMenuPlayerTimer += Time.deltaTime;
            if (_timeToChangeMainMenuPlayerTimer >= TimeToChangeMainMenuPlayer)
            {
                _timeToChangeMainMenuPlayerTimer = 0;

                _characterSpriteIdx++;
                if (_characterSpriteIdx >= _characters.Count) _characterSpriteIdx = 0;

                CharactersImage.sprite = ResourcesManager.Instance.GetCharacterSprite(_characters[_characterSpriteIdx]);
            }
        }

        if (_searchingInternetMatch)
        {
            _timeToUpdateMatchListTimer += Time.deltaTime;
            if (_timeToUpdateMatchListTimer >= TimeToUpdateMatchList)
            {
                _timeToUpdateMatchListTimer = 0;

                MatchMakerManager.SearchRoomOnline();
                UpdateInternetMathesPanel();
            }
        }

        if (_searchingLanMatch)
        {
            _timeToUpdateMatchListTimer += Time.deltaTime;
            if (_timeToUpdateMatchListTimer >= TimeToUpdateMatchList)
            {
                _timeToUpdateMatchListTimer = 0;
                UpdateLanMathesPanel();
            }
        }
    }

    private void GoToPanel(GameObject panel)
    {
        _atMainMenu = MainMenuPanel == panel;

        MainMenuPanel.SetActive(MainMenuPanel == panel);
        OnlinePanel.SetActive(OnlinePanel == panel);
        LanPanel.SetActive(LanPanel == panel);
        CreditsPanel.SetActive(CreditsPanel == panel);
        LobbyPanel.SetActive(LobbyPanel == panel);
    }

    private void UpdateInternetMathesPanel()
    {
        foreach (Transform child in AvailableInternetRoomsPanel.transform)
        {
            Destroy(child.gameObject);
        }

        List<Room> rooms = MatchMakerManager.GetOnlineRooms();
        foreach (Room room in rooms)
        {
            Button button = Instantiate(JoinRoomButtonPrefab, AvailableInternetRoomsPanel.transform, false);
            button.GetComponentInChildren<Text>().text = room.Name;
            button.onClick.AddListener(() =>
            {
                button.interactable = false;
                OnOnlineJoinRoomClick(room.NetworkID);
            });
        }
    }

    private void UpdateLanMathesPanel()
    {
        foreach (Transform child in AvailableLanRoomsPanel.transform)
        {
            Destroy(child.gameObject);
        }

        List<Room> rooms = MatchMakerManager.GetLocalRooms();
        foreach (Room room in rooms)
        {
            Button button = Instantiate(JoinRoomButtonPrefab, AvailableLanRoomsPanel.transform, false);
            button.GetComponentInChildren<Text>().text = $"{room.Name} ({room.Address})";
            button.onClick.AddListener(() =>
            {
                button.interactable = false;
                OnLanJoinRoomClick(room.Address);
            });
        }
    }

    public void GoToHomeMenu()
    {
        GoToPanel(MainMenuPanel);

        OnlineRoomNameInput.text = string.Empty;
        LanRoomNameInput.text = string.Empty;
    }

    #region MainMenu
    public void OnMainMenuOnlineButtonClick()
    {
        GoToPanel(OnlinePanel);

        _searchingInternetMatch = true;
        _timeToUpdateMatchListTimer = TimeToUpdateMatchList;

        UpdateInternetMathesPanel();
    }

    public void OnMainMenuLanButtonClick()
    {
        GoToPanel(LanPanel);

        MatchMakerManager.StartSearchRoomLocal();

        _searchingLanMatch = true;
        _timeToUpdateMatchListTimer = TimeToUpdateMatchList;

        UpdateLanMathesPanel();
    }

    public void OnMainMenuCreditsButtonClick()
    {
        GoToPanel(CreditsPanel);
    }

    public void OnMainMenuQuitButtonClick()
    {
        Application.Quit();
    }
    #endregion

    #region Online
    public void OnOnlineCreateMatchButtonClick()
    {
        _searchingInternetMatch = false;

        string name = OnlineRoomNameInput.text;
        if (string.IsNullOrEmpty(name))
            name = "Match " + Random.Range(100000, 1000000);

        MatchMakerManager.CreateRoomOnline(name);

        GoToPanel(LobbyPanel);
    }

    public void OnOnlineBackButtonClick()
    {
        _searchingInternetMatch = false;
        MatchMakerManager.StopBroadcasting();

        GoToPanel(MainMenuPanel);
    }

    private void OnOnlineJoinRoomClick(NetworkID networkID)
    {
        _searchingInternetMatch = false;
        GoToPanel(LobbyPanel);

        MatchMakerManager.StartOnlineMatch(networkID);
    }
    #endregion

    #region Lan
    public void OnLanCreateRoomButtonClick()
    {
        _searchingLanMatch = false;

        string name = LanRoomNameInput.text;
        if (string.IsNullOrEmpty(name))
            name = "Match " + Random.Range(100000, 1000000);

        MatchMakerManager.StopSearchRoomLocal();
        MatchMakerManager.CreateRoomLocal(name);

        GoToPanel(LobbyPanel);
    }

    public void OnLanBackButtonClick()
    {
        _searchingLanMatch = false;
        MatchMakerManager.StopSearchRoomLocal();

        GoToPanel(MainMenuPanel);
    }

    private void OnLanJoinRoomClick(string address)
    {
        _searchingLanMatch = false;
        GoToPanel(LobbyPanel);

        MatchMakerManager.StartLocalMatch(address);
    }
    #endregion

    #region Credits
    public void OnCreditsBackButtonClick()
    {
        GoToPanel(MainMenuPanel);
    }
    #endregion

    public void OnPlayerNameChanged()
    {
        PlayerSettings.PlayerName = PlayerNameInput.text;
    }
}
