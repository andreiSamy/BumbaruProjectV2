using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    public Animator animator;
    public CharacterController controller;
    private Vector3 direction;
    public float speed = 8f;
    public float jumpForce = 10f;
    public float gravity = -20f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public Transform model;

    [SerializeField] private Transform spawnedObjectPrefab;
    private Transform spawnObjectTransform;

    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(new MyCustomData { _int = 56, _bool = true, },
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + "; newValue " + newValue._int + "; " + newValue._bool + "; " + newValue.message);
        };
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            spawnObjectTransform = Instantiate(spawnedObjectPrefab);
            spawnObjectTransform.GetComponent<NetworkObject>().Spawn(true);


            randomNumber.Value = new MyCustomData { _int = 10, _bool = false, message = "All your base are belong to us!" };
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Destroy(spawnObjectTransform.gameObject);
        }

        float hInput = Input.GetAxis("Horizontal");
        direction.x = hInput * speed;


        animator.SetFloat("speed", Mathf.Abs(hInput));
        bool isGrounded = Physics.CheckSphere(groundCheck.position, 0.15f, groundLayer);

        direction.y += gravity * Time.deltaTime;
        if (isGrounded)
        {

            if (Input.GetButtonDown("Jump"))
            {
                direction.y = jumpForce;

            }
        }
        if (hInput != 0)
        {
            Quaternion newRotation = Quaternion.LookRotation(new Vector3(hInput, 0, 0));
            model.rotation = newRotation;
        }


        controller.Move(direction * Time.deltaTime);


    }


}
