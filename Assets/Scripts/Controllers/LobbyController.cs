using System.Collections.Generic;
using UnityEngine;

public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance = null;

    public List<CharacterPickerController> Slots = new List<CharacterPickerController>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
    }

    public void RegisterPlayer(int slot, LobbyPlayerConnection player)
    {
        Slots[slot].RegisterPlayer(player);
    }

    public void AssignSlotToPlayer(int slot, LobbyPlayerConnection player)
    {
        Slots[slot].AssignSlotToPlayer(player);
    }

    public void SetName(int slot, string name)
    {
        Slots[slot].SetName(name);
    }

    public void SetCharacter(int slot, ECharacter character)
    {
        Slots[slot].SetCharacter(character);
    }

    public void PickCharacter(int slot) 
    {
        Slots[slot].PickCharacter();
    }

    public void OnQuitLobbyButtonClick() {
        MatchMakerManager.Disconnect();
    }
}
