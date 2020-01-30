using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager : MonoBehaviour
{
    public static ResourcesManager Instance;

    private Dictionary<ECharacter, Character> _characterResources = new Dictionary<ECharacter, Character>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    public Sprite GetCharacterSprite(ECharacter character)
    {
        Character characterResource = new Character();
        if (_characterResources.ContainsKey(character))
        {
            characterResource = _characterResources[character];
        }

        if (characterResource.Sprite == null)
        {
            characterResource.Sprite = Resources.Load<Sprite>($"Images/Characters/{character}");
        }

        _characterResources[character] = characterResource;

        return characterResource.Sprite;

    }
}
