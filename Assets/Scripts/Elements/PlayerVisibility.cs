using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerVisibility : MonoBehaviour {
    public bool IsElementVisible { get; private set; }

    private List<Renderer> _renderers = new List<Renderer>();

    private void Start() {
        _renderers = GetComponentsInChildren<Renderer>().ToList();
        for (var i = 0; i < _renderers.Count; i++) {
            _renderers[i].enabled = false;
            IsElementVisible = false;
        }
    }

    void Update() {

        if (_renderers.Count == 0) {
            _renderers = GetComponentsInChildren<Renderer>().ToList();

            if (_renderers.Count == 0)
                return;
        }


        bool visible = false;

        var position = transform.position;

        var hits = Physics.OverlapSphere(position, 0.3f);
        foreach (var item in hits) {
            var ramp = item.GetComponent<RampHitbox>();
            if (ramp != null) {
                var visibility = ramp.Actor.GetComponent<RampVisibility>();
                visible = visibility.IsElementVisible;
                break;
            }

            var ground = item.GetComponent<Ground>();
            if (ground != null) {
                visible = ground.GetComponentInChildren<Renderer>().enabled;
            }
        }

        for (var i = 0; i < _renderers.Count; i++) {
            _renderers[i].enabled = visible;
            IsElementVisible = visible;
        }

    }

    public void Disable(bool disable) {
        this.enabled = disable;

        if (disable) {
            for (var i = 0; i < _renderers.Count; i++) {
                _renderers[i].enabled = true;
                IsElementVisible = true;
            }
        }
    }
}
