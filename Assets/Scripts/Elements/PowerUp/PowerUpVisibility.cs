using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowerUpVisibility : MonoBehaviour
{
    public bool IsElementVisible { get; private set; }

    private List<Renderer> _renderers = new List<Renderer>();

    private void Start()
    {
        _renderers = GetComponentsInChildren<Renderer>().ToList();
        for (var i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].enabled = false;
            IsElementVisible = false;
        }
    }

    void Update()
    {
        if (_renderers.Count == 0) {
            _renderers = GetComponentsInChildren<Renderer>().ToList();

            if (_renderers.Count == 0)
                return;
        }

        bool visible = false;

        var position = transform.position;

        var hits = Physics.RaycastAll(position, Vector3.down, 1.5f);
        foreach (var item in hits)
        {
            var ground = item.collider.GetComponent<Ground>();
            if (ground != null)
            {
                visible = ground.Renderer.enabled;
                break;
            }
        }

        for (var i = 0; i < _renderers.Count; i++)
        {
            _renderers[i].enabled = visible;
            IsElementVisible = visible;
        }
    }
}
