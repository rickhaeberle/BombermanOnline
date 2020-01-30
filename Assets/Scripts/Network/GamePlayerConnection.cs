using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class GamePlayerConnection : NetworkBehaviour {

    [SyncVar]
    public int Slot;

    [SyncVar]
    public string PlayerName;

    [SyncVar]
    public ECharacter Character;

    private void Start() {
        if (!isLocalPlayer)
            return;

        StartCoroutine(WaitForReady());
    }

    IEnumerator WaitForReady() {
        while (GameController.Instance == null || !GameController.Instance.gameObject.activeSelf) {
            yield return new WaitForSeconds(0.2f);
        }

        TellServerImConnected();
    }

    public void TellServerImConnected() {
        CmdRegisterMyPlayer();
    }

    public void TellClientsGameEnded(string winner, bool timeup) {
        RpcEndGame(winner, timeup);
    }

    [Command]
    private void CmdRegisterMyPlayer() {
        GameController.Instance.RegisterPlayer(this);

    }

    [ClientRpc]
    private void RpcEndGame(string winner, bool timeup) {
        if (!isLocalPlayer)
            return;

        GameController.Instance.EndGame(winner, timeup);
    }
}
