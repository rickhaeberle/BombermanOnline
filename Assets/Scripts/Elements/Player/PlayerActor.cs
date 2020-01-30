using UnityEngine;
using UnityEngine.Networking;

public class PlayerActor : NetworkBehaviour
{
    public GameObject Body;

    public GameObject RedPrefab;
    public GameObject BluePrefab;
    public GameObject PinkPrefab;
    public GameObject BlackPrefab;
    public GameObject WhitePrefab;
    public GameObject YellowPrefab;

    [SyncVar]
    public ECharacter Character = ECharacter.White;

    [SyncVar]
    public int MaxBombs = 1;

    [SyncVar]
    public int BombForce = 1;

    [SyncVar]
    public float BombTimer = 3f;

    [SyncVar]
    public float MoveSpeed = 3.5f;

    [SyncVar]
    public bool HasKickBuff = false;

    [SyncVar]
    public bool HasRemoteControlBuff = false;

    public float MoveBuffIncrement = 0.8f;

    private CharacterController _controller;
    private PlayerVisibility _visibility;
    private Animator _animator;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _visibility = GetComponent<PlayerVisibility>();

        SpawnCharacter();
        GameController.Instance.StartGame();

        _visibility.Disable(!hasAuthority);
    }

    private void Update()
    {
        if (GameController.Instance.IsGameOver)
            return;

        if (_animator != null)
        {
            bool isMoving = _controller.velocity.magnitude > 0.5f;
            _animator.SetBool("IsMoving", isMoving);
        }

        if (!hasAuthority)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {

            var canPlaceBomb = true;

            var hits = Physics.RaycastAll(new Ray(transform.position, transform.up * -1), 0.5f);
            foreach (var hit in hits)
            {
                var ramp = hit.collider.GetComponent<RampHitbox>();
                if (ramp != null)
                {
                    canPlaceBomb = false;
                    break;
                }
            }

            if (canPlaceBomb)
                CmdPlaceBomb();
        }

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (move != Vector3.zero)
        {
            _controller.Move((move + Vector3.down) * Time.deltaTime * MoveSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), 0.15F);
        }
    }

    private void SpawnCharacter()
    {
        GameObject prefab = null;

        switch (Character)
        {
            case ECharacter.Red:
                prefab = RedPrefab;
                break;
            case ECharacter.Blue:
                prefab = BluePrefab;
                break;
            case ECharacter.Pink:
                prefab = PinkPrefab;
                break;
            case ECharacter.Black:
                prefab = BlackPrefab;
                break;
            case ECharacter.White:
                prefab = WhitePrefab;
                break;
            case ECharacter.Yellow:
                prefab = YellowPrefab;
                break;
            default:
                prefab = WhitePrefab;
                break;
        }

        Instantiate(prefab, Body.transform);

        Vector3 relativePos = Camera.main.transform.position - transform.position;
        relativePos.y = 0;

        Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        transform.rotation = rotation;

        _animator = GetComponentInChildren<Animator>();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!HasKickBuff)
            return;

        BombHitbox bomb = hit.gameObject.GetComponentInChildren<BombHitbox>();
        if (bomb != null)
        {
            bomb.Kick(hit.moveDirection);
        }
    }

    public void TakeDamage()
    {
        if (!isServer)
            return;

        GameController.Instance.RegisterPlayerDeath(this);
    }

    public bool TakeBuff(EPowerUp buff)
    {
        if (!isServer)
            return false;

        switch (buff)
        {
            case EPowerUp.FireUp:
            {
                BombForce += 1;
                break;
            }
            case EPowerUp.FireDown:
            {
                if (BombForce > 1)
                    BombForce -= 1;

                break;
            }
            case EPowerUp.SpeedUp:
            {
                MoveSpeed += MoveBuffIncrement;
                break;
            }
            case EPowerUp.SpeedDown:
            {
                if (MoveSpeed - MoveBuffIncrement > 1f)
                    MoveSpeed -= MoveBuffIncrement;

                break;
            }
            case EPowerUp.BombUp:
            {
                MaxBombs += 1;
                break;
            }
            case EPowerUp.BombDown:
            {
                if (MaxBombs > 1)
                    MaxBombs -= 1;

                break;
            }
            case EPowerUp.Kick:
            {
                HasKickBuff = true;
                break;
            }
            case EPowerUp.Remote:
            {
                HasRemoteControlBuff = true;
                break;
            }
        }

        return true;
    }

    [Command]
    private void CmdPlaceBomb()
    {
        GameController.Instance.SpawnBomb(this, transform.position, BombTimer, BombForce);
    }
}
