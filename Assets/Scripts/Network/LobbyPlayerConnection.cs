using UnityEngine.Networking;

public class LobbyPlayerConnection : NetworkLobbyPlayer {

    [SyncVar(hook = "OnPlayerNameChanged")]
    public string PlayerName = string.Empty;

    [SyncVar(hook = "OnPlayerCharacterChanged")]
    public ECharacter Character = ECharacter.White;

    [SyncVar(hook = "OnPlayerReadyChanged")]
    public bool IsPlayerReady = false;

    public override void OnClientEnterLobby() {
        LobbyController.Instance.RegisterPlayer(slot, this);

        if (!isLocalPlayer) {
            LobbyController.Instance.SetName(slot, PlayerName);
            LobbyController.Instance.SetCharacter(slot, Character);

            if (IsPlayerReady) {
                LobbyController.Instance.PickCharacter(slot);

            }
        }
    }

    public override void OnStartLocalPlayer() {
        LobbyController.Instance.AssignSlotToPlayer(slot, this);

        string name = PlayerSettings.PlayerName;
        if (string.IsNullOrEmpty(name)) {
            name = $"Player {slot + 1}";
        }

        CmdSetPlayerName(name);
    }

    private void OnPlayerNameChanged(string name) {
        LobbyController.Instance.SetName(slot, name);
    }

    private void OnPlayerCharacterChanged(ECharacter character) {
        LobbyController.Instance.SetCharacter(slot, character);
    }

    private void OnPlayerReadyChanged(bool ready) {
        LobbyController.Instance.PickCharacter(slot);

    }

    public void CharacterChanged(ECharacter character) {
        CmdChangeChar(character);
    }

    public void CharacterSelected(ECharacter character) {
        CmdPickChar(character);
    }

    [Command]
    private void CmdSetPlayerName(string name) {
        PlayerName = name;
    }

    [Command]
    private void CmdChangeChar(ECharacter character) {
        Character = character;
    }

    [Command]
    private void CmdPickChar(ECharacter character) {
        IsPlayerReady = true;
        Character = character;
        RpcMarkAsReady();
    }

    [ClientRpc]
    private void RpcMarkAsReady() {
        if (!isLocalPlayer)
            return;

        SendReadyToBeginMessage();
    }
}
