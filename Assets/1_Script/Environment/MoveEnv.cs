using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class MoveEnv : MonoBehaviour
{
    [SerializeField] private List<Transform> obSets = new List<Transform>();
    [SerializeField] private List<Transform> wheels = new List<Transform>();
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    private Vector3 resetPos;
    private Vector3 moveDir;

    private void Awake()
    {
        resetPos = new Vector3(-5f, 0f, -180f);
        moveDir = Vector3.forward;
    }

    private void Update()
    {
        foreach(Transform obSet in obSets)
        {
            if (obSet.position.z > 180)
                obSet.position = resetPos;
            else
            {
                float moveDistance = moveSpeed * Time.deltaTime;
                obSet.position += moveDir * moveDistance;
            }    
        }

        foreach(Transform wheel in wheels)
        {
            float rotationAmount = rotateSpeed * Time.deltaTime;
            wheel.Rotate(Vector3.right, rotationAmount, Space.Self);
        }
    }
}
