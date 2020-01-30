using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class BombActor : NetworkBehaviour
{

    public float BlinkTimeStart = 1f;
    public float KickMoveSpeed = 7.5f;

    public GameObject ExplosionEffectPrefab;

    public AudioClip ExplosionSfx;

    [SyncVar]
    private int _explosionRange = 1;

    [SyncVar]
    private float _timeToExplode = 3f;

    [SyncVar]
    private bool _started = false;

    [SyncVar]
    public bool IsRemote = false;

    private bool _kicked = false;

    private Vector3Int _kickDirection;
    private Vector3 _kickNextDestination;

    private ElementVisibility _visibility;
    private Renderer _renderer;

    private void Awake()
    {
        _visibility = GetComponent<ElementVisibility>();
        _renderer = GetComponentInChildren<Renderer>();
    }

    private void Update()
    {
        if (GameController.Instance.IsGameOver)
            return;

        if (_started)
        {
            if (_timeToExplode < BlinkTimeStart)
            {
                _renderer.material.color = Color.red;
            }

            if (_timeToExplode > 0)
            {
                _timeToExplode -= Time.deltaTime;

            }
            else
            {
                Explode();
            }
        }

        if (_kicked)
        {
            if (transform.position == _kickNextDestination)
            {

                var shouldStop = !CanMoveTowardsDirection(_kickDirection);
                if (shouldStop)
                {
                    _kicked = false;

                }
                else
                {
                    _kickNextDestination = transform.position + _kickDirection;

                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, _kickNextDestination, KickMoveSpeed * Time.deltaTime);

            }
        }
    }

    private void OnDestroy()
    {
        if (GameController.Instance.IsGameOver)
            return;

        if (!_visibility.IsElementVisible)
            return;

        SFXPlayerManager.Instance.Play(ExplosionSfx);
    }

    public void SetTimer(float timeToExplode)
    {
        _timeToExplode = timeToExplode;
        _started = true;
    }

    public void SetRange(int range)
    {
        _explosionRange = range;
    }

    public void SetRemote()
    {
        IsRemote = true;
    }

    public void Kick(Vector3 direction)
    {
        if (!isServer)
            return;

        if (_kicked)
            return;

        _kicked = true;

        direction.y = 0;
        Vector3Int adjustedDirection = Vector3Int.RoundToInt(direction);

        Vector3Int kickDirection = Vector3Int.zero;

        if (adjustedDirection.x != 0)
        {
            if (adjustedDirection.z != 0)
            {

                var horizontalHits = Physics.RaycastAll(transform.position, direction.x > 0 ? Vector3.right : Vector3.left, 1f);
                var hasHorizontalWall = horizontalHits.Any(item => item.collider.GetComponent<Wall>() != null);

                if (hasHorizontalWall)
                {
                    kickDirection.z = direction.z > 0 ? 1 : -1;

                }
                else
                {
                    kickDirection.x = direction.x > 0 ? 1 : -1;

                }

            }
            else
            {
                kickDirection = adjustedDirection;
            }

        }
        else if (adjustedDirection.z != 0)
        {
            kickDirection = adjustedDirection;

        }
        else
        {
            _kicked = false;
            return;

        }

        bool canMove = kickDirection != Vector3Int.zero && CanMoveTowardsDirection(kickDirection);
        if (canMove)
        {
            _kickDirection = kickDirection;
            _kickNextDestination = transform.position + kickDirection;

        }
        else
        {
            _kicked = false;

        }
    }

    private bool CanMoveTowardsDirection(Vector3 direction)
    {
        var collisions = Physics.RaycastAll(transform.position, direction, 1f);

        return !collisions.Any(item =>
            item.collider.GetComponent<Wall>() != null ||
            item.collider.GetComponent<CrateHitbox>() != null ||
            item.collider.GetComponent<RampHitbox>() != null ||
            item.collider.GetComponent<PlayerHitbox>() != null
        );
    }

    public void Explode()
    {
        if (!isServer)
            return;

        _started = false;

        GameController.Instance.RemoveBomb(this);
        NetworkServer.Destroy(gameObject);

        bool checkUp = true;
        bool checkRight = true;
        bool checkDown = true;
        bool checkLeft = true;

        CreateExplosion(transform.position);

        for (int range = 1; range <= _explosionRange; range++)
        {
            if (checkUp)
            {
                checkUp = PropagateExplosion(Vector3.forward, range);
            }

            if (checkRight)
            {
                checkRight = PropagateExplosion(Vector3.right, range);
            }

            if (checkDown)
            {
                checkDown = PropagateExplosion(Vector3.back, range);
            }

            if (checkLeft)
            {
                checkLeft = PropagateExplosion(Vector3.left, range);
            }
        }
    }

    private bool PropagateExplosion(Vector3 direction, int range)
    {
        var hits = Physics.RaycastAll(transform.position, direction, range);

        foreach (var hit in hits)
        {
            var wall = hit.collider.GetComponent<Wall>();
            if (wall != null)
            {
                return false;
            }

            var crate = hit.collider.GetComponent<CrateHitbox>();
            if (crate != null)
            {
                CreateExplosion(transform.position + direction * range);
                return false;
            }
        }

        CreateExplosion(transform.position + direction * range);

        return true;
    }

    private void CreateExplosion(Vector3 position)
    {
        GameObject go = Instantiate(ExplosionEffectPrefab, position, Quaternion.identity);
        NetworkServer.Spawn(go);
    }
}
