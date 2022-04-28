using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField]
    private float MovementSpeed;
    [SerializeField]
    private CustomMetamaskController MMController;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        if (MMController.IsMetamaskEnabled())
        {
            Vector3 targetVector = new Vector3(horizontal, 0, vertical);
            float speed = MovementSpeed * Time.deltaTime;
            transform.position = transform.position + targetVector * speed;
        }
        else
        {
            Debug.Log("UNITY: Character::Update: Metamask not enabled");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("UNITY: character collision entered");
    }
}
