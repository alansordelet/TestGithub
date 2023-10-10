using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TransitionCamera : MonoBehaviour
{

    [SerializeField]
    public CinemachineVirtualCamera currentCamera;

    [SerializeField]
    public CinemachineVirtualCamera nextCamera;
    // Start is called before the first frame update
    void Start()
    {
        //currentCamera = GetComponent<CinemachineVirtualCamera>();
        nextCamera.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        currentCamera.gameObject.SetActive(false);
        nextCamera.gameObject.SetActive(true);
    } 

    private void OnTriggerExit(Collider other)
    {
        currentCamera.gameObject.SetActive(true);
        nextCamera.gameObject.SetActive(false);
    }
}
