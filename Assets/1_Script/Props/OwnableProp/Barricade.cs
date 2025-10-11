using Garage.Interfaces;
using Garage.Manager;
using Garage.Props;
using Garage.Utils;
using Unity.Netcode;
using UnityEngine;

public class Barricade : OwnableProp, IPlaceable
{
    [SerializeField] private GameObject previewPrefab;
    [SerializeField] private Transform holdingTf;

    private Vector3 holdingPosOffset;
    private Quaternion rotationOffset;

    public override void Awake()
    {
        base.Awake();
        Init();
    }
    public override void Init()
    {
        base.Init();
        rotationOffset = Quaternion.Euler(0f, 90f, 0f);
        holdingPosOffset = transform.position - holdingTf.position;
    }
    private void Update()
    {
        if (GameManagerEx.Instance.IsDay)
        {
            if (controller != null)
            {
                rigid.MovePosition(controller.GetSocket(PropType.Tire).position + holdingPosOffset);
                rigid.MoveRotation(controller.transform.rotation * rotationOffset);

                return;
            }

            if (!IsOwner)
            {
                return;
            }
            else
            {
                UpdatePropPositionServerRPC(transform.position, NetworkManager.Singleton.LocalClientId);
                UpdatePropRotateServerRPC(transform.rotation, NetworkManager.Singleton.LocalClientId);
                UpdatePropVelocityServerRPC(Vector3.zero, NetworkManager.Singleton.LocalClientId);
            }
        }
        else
        {
            rigid.MovePosition(gridPosition.Value);
            rigid.MoveRotation(Quaternion.identity);
            rigid.linearVelocity = Vector3.zero;
        }
    }

    protected override void StartInteraction(ulong newOwnerClientId)
    {
        base.StartInteraction(newOwnerClientId);

        if (GameManagerEx.Instance.IsDay)
        {
            rigid.useGravity = false;
            rigid.isKinematic = true;
            transform.GetComponent<Collider>().isTrigger = true;
            SyncStateServerRPC(true);
        }
    }

    public override void OnEndInteraction(Transform controller)
    {
        rigid.useGravity = true;
        rigid.isKinematic = false;
        transform.GetComponent<Collider>().isTrigger = false;
        SyncStateServerRPC(false);

        base.OnEndInteraction(controller);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncStateServerRPC(bool isStart)
    {
        SyncStateClientRPC(isStart);
    }

    [ClientRpc]
    private void SyncStateClientRPC(bool isStart)
    {
        rigid.useGravity = !isStart;
        rigid.isKinematic = isStart;
        transform.GetComponent<Collider>().isTrigger = isStart;
    }

    public Vector2Int GetSize()
    {
        return new Vector2Int(1, 4);
    }

    public GameObject GetPreviewPrefab()
    {
        return previewPrefab;
    }
}
