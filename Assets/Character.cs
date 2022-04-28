using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField]
    private float MovementSpeed;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        var targetVector = new Vector3(horizontal, 0, vertical);

        // Debug.Log("horizontal: " + horizontal + "vertical: " + vertical);

        float speed = MovementSpeed * Time.deltaTime;
        transform.position = transform.position + targetVector * speed;

    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("character collision entered");
    }
}
