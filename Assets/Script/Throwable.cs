using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    public GameObject Monkey;
    private Rigidbody2D rb;
    bool isThrown;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isThrown)
        {
            transform.position = Vector2.MoveTowards(transform.position, Monkey.transform.position, speed * Time.deltaTime);
            transform.Rotate(0, 0, 360 * Time.deltaTime);
        }
    }

    public void Throw(GameObject m)
    {
        isThrown = true;
        Monkey = m;
        transform.GetComponent<BoxCollider2D>().isTrigger = false;
    }
    void Catch()
    {
        Monkey = null;
        isThrown = false;
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if(!isThrown)
        {
            return;
        }
        if (other.gameObject.CompareTag("Player") && other.gameObject == Monkey)
        {
            Debug.Log("HERE");
            Monkey.GetComponent<Monkey_Controller>().TakeDamage(0);
            Destroy(gameObject);
        }
        else if(!other.gameObject == Monkey)
        {
            Destroy(gameObject);
        }
    }
}
