using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPickerController : MonoBehaviour
{
    public Text PlayerNameText;
    public Image CharacterImage;

    public GameObject CharacterPanel;

    public Button PreviousCharacterButton;
    public Button NextCharacterButton;
    public Button PickCharacterButton;

    private LobbyPlayerConnection _player;

    private int _currentCharIdx = 0;
    private List<ECharacter> _characters = new List<ECharacter>() {
        ECharacter.White, ECharacter.Blue, ECharacter.Black, ECharacter.Red, ECharacter.Pink, ECharacter.Yellow
    };

    private void Awake()
    {
        Clear();
    }

    public void RegisterPlayer(LobbyPlayerConnection player)
    {
        _player = player;
        PlayerNameText.text = player.PlayerName;

        CharacterPanel.SetActive(true);
        UpdateCharPreview();
    }

    public void AssignSlotToPlayer(LobbyPlayerConnection player)
    {
        SetReadOnly(false);
    }

    public void OnPreviousCharacterButtonClick()
    {
        if (_player == null)
            return;

        _currentCharIdx--;
        if (_currentCharIdx < 0) _currentCharIdx = _characters.Count - 1;

        _player.CharacterChanged(_characters[_currentCharIdx]);
        UpdateCharPreview();
    }

    public void OnNextCharacterButtonClick()
    {
        if (_player == null)
            return;

        _currentCharIdx++;
        if (_currentCharIdx >= _characters.Count) _currentCharIdx = 0;

        _player.CharacterChanged(_characters[_currentCharIdx]);
        UpdateCharPreview();
    }

    public void OnPickCharacterButtonClick()
    {
        if (_player == null)
            return;

        SetReadOnly(true);
        _player.CharacterSelected(_characters[_currentCharIdx]);

        PickCharacter();
    }

    public void PickCharacter() {
        var image = CharacterPanel.GetComponent<Image>();
        var color = image.color;
        color.a = 0;
        image.color = color;
    }

    public void UpdateCharPreview()
    {
        if (_player == null)
            return;

        CharacterImage.sprite = ResourcesManager.Instance.GetCharacterSprite(_characters[_currentCharIdx]);
    }

    public void Clear()
    {
        _player = null;
        _currentCharIdx = 0;

        PlayerNameText.text = "Waiting...";
        CharacterPanel.SetActive(false);

        SetReadOnly(true);
    }

    public void SetName(string name)
    {
        PlayerNameText.text = name;
    }

    public void SetCharacter(ECharacter character)
    {
        _currentCharIdx = _characters.FindIndex(item => item == character);
        UpdateCharPreview();
    }

    private void SetReadOnly(bool readOnly)
    {
        PreviousCharacterButton.gameObject.SetActive(!readOnly);
        NextCharacterButton.gameObject.SetActive(!readOnly);
        PickCharacterButton.gameObject.SetActive(!readOnly);
    }
}
