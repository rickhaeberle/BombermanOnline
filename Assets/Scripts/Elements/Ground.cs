using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class Ground : NetworkBehaviour {

    public Renderer Renderer { get; private set; }

    private Color _color;
    private Color _blinkColor = Color.red;

    [SyncVar(hook = "OnBlinkStart")]
    public bool Blinking = false;

    private float _blinkTime = 1f;
    private float _blinkTimeTimer = 0f;

    private float _blinkInterval = 0.2f;
    private float _blinkIntervalTimer = 0f;

    private void Awake() {
        Renderer = GetComponent<Renderer>();
        _color = Renderer.material.color;
    }

    private void Update() {
        if (Blinking) {

            _blinkTimeTimer += Time.deltaTime;
            if (_blinkTimeTimer >= _blinkTime) {
                Blinking = false;
                Renderer.material.color = _color;

            } else {

                _blinkIntervalTimer += Time.deltaTime;
                if (_blinkIntervalTimer >= _blinkInterval) {
                    _blinkIntervalTimer = 0;

                    var current = Renderer.material.color;
                    Renderer.material.color = current == _color ? _blinkColor : _color;

                }
            }
        }
    }

    public void SetVisibility(bool visible) {
        Renderer.enabled = visible;
    }

    public bool CanPutBomb() {
        var hits = Physics.OverlapSphere(transform.position, 0.3f).ToList();
        foreach (var hit in hits) {
            var crate = hit.GetComponent<CrateHitbox>();
            if (crate != null)
                return false;

        }

        return true;

    }

    public void Blink() {
        Blinking = true;

    }

    public void OnBlinkStart(bool blink) {
        if (blink) {
            _blinkTimeTimer = 0f;
            _blinkIntervalTimer = 0f;

            Renderer.material.color = _blinkColor;

        } else {
            Renderer.material.color = _color;

        }

        Blinking = blink;
    }
}
