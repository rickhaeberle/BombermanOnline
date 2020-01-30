using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

[RequireComponent(typeof(NetworkDiscovery))]
public class MatchMakerManager : NetworkLobbyManager
{
    public static event Action OnServerDisconnectEvent;

    private static NetworkDiscovery _discovery
    {
        get
        {
            return singleton.GetComponent<NetworkDiscovery>();
        }
    }

    private static NetworkMatch _match
    {
        get
        {
            return singleton.GetComponent<NetworkMatch>() ?? singleton.gameObject.AddComponent<NetworkMatch>();
        }
    }

    #region Lan
    public static void CreateRoomLocal(string name)
    {
        _discovery.Initialize();

        _discovery.broadcastData = name;
        _discovery.StartAsServer();

        singleton.StartHost();
    }

    public static void CloseRoomLocal()
    {
        _discovery.StopBroadcast();
        singleton.StopHost();
    }

    public static void StartSearchRoomLocal()
    {
        _discovery.Initialize();
        _discovery.StartAsClient();
    }

    public static void StopSearchRoomLocal()
    {
        _discovery.StopBroadcast();
    }

    public static List<Room> GetLocalRooms()
    {
        List<Room> rooms = new List<Room>();

        List<NetworkBroadcastResult> matches = _discovery.broadcastsReceived.Values.ToList();
        foreach (NetworkBroadcastResult match in matches)
        {
            string name = Encoding.Unicode.GetString(match.broadcastData);

            Room room = new Room();
            room.Name = name;
            room.Address = match.serverAddress;
            rooms.Add(room);
        }

        return rooms;
    }

    public static void StartLocalMatch(string address)
    {
        _discovery.StopBroadcast();

        singleton.networkAddress = address;
        singleton.StartClient();
    }

    public static void StopBroadcasting()
    {
        if (_discovery.running)
            _discovery.StopBroadcast();

    }
    #endregion

    #region Online
    public static void CreateRoomOnline(string name)
    {
        singleton.StartMatchMaker();

        _match.CreateMatch(name, 2, true, "", "", "", 0, 0, singleton.OnMatchCreate);
    }

    public static void CloseRoomOnline()
    {
        singleton.StopMatchMaker();
    }

    public static void SearchRoomOnline()
    {
        _match.ListMatches(0, 10, "", true, 0, 0, singleton.OnMatchList);

    }

    public static List<Room> GetOnlineRooms()
    {
        List<Room> rooms = new List<Room>();

        List<MatchInfoSnapshot> matches = singleton.matches;
        if (matches != null)
        {
            foreach (MatchInfoSnapshot match in matches)
            {
                Room room = new Room();
                room.Name = match.name;
                room.NetworkID = match.networkId;
                rooms.Add(room);

            }
        }

        return rooms;
    }

    public static void StartOnlineMatch(NetworkID networkID)
    {
        _match.JoinMatch(networkID, "", "", "", 0, 0, singleton.OnMatchJoined);
    }

    #endregion

    public static void Disconnect()
    {
        if (_discovery.running)
            _discovery.StopBroadcast();

        singleton.StopHost();
    }

    public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
    {
        var lobbyPlayerController = lobbyPlayer.GetComponent<LobbyPlayerConnection>();
        var gamePlayerController = gamePlayer.GetComponent<GamePlayerConnection>();

        gamePlayerController.Slot = lobbyPlayerController.slot;
        gamePlayerController.PlayerName = lobbyPlayerController.PlayerName;
        gamePlayerController.Character = lobbyPlayerController.Character;

        return true;
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);

        GameManager.Instance.GoToMenu();
        Disconnect();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        GameManager.Instance.GoToMenu();
    }

}
