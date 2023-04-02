using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Monkey_Controller : MonoBehaviour
{
    public bool WallGrab, Grounded, Stunned, grab, TopHang,Smash,punch;
    private Rigidbody2D rb;
    public int health;
    public float speed;
    private float timer;
    public GameObject OtherMonkey;
    private float x, y;
    public Animator anim;
    public float jumpHeight;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Smash)
        {
            if (Grounded)
            {
                rb.velocity = new Vector2(x * speed, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(x * speed * 2, rb.velocity.y);
            }
        }

        if(!Grounded && !WallGrab && !TopHang)
        {
            rb.gravityScale = 1.0f;
        }
    }
    public void FullMovement(InputAction.CallbackContext input)
    {
        GroundMovement(input);
        AirControls(input);
        MonkeSmash(input);
    }

    void GroundMovement(InputAction.CallbackContext input)
    {
        if (!Smash)
        {
            x = input.ReadValue<Vector2>().x;
            if (x < 0 && x != 0 && Grounded)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0); 
            }
            else if (x != 0 && Grounded)
            {
                transform.rotation = Quaternion.Euler(0, 0, 0); 
            }

            //Setting speed for run animation
            anim.SetFloat("Speed", Mathf.Abs(x));
        }
    }
    void AirControls(InputAction.CallbackContext input)
    {
        if (WallGrab)
        {
            y = input.ReadValue<Vector2>().y;
            rb.gravityScale = 0.0f;
            rb.velocity = new Vector2(0, y * speed);
        }
        else if(Grounded)
        {
            rb.gravityScale = 1.0f;
            y = input.ReadValue<Vector2>().y;
            Vector2 Vy = new Vector2(0, y* jumpHeight*100);
            rb.AddForce(Vy);
            if (y != 0)
            {
                Grounded = false;
            }
        }

        //Setting y value for jump animation
        anim.SetFloat("Jump", Mathf.Abs(y));
    }

    void MonkeSmash(InputAction.CallbackContext input)
    { 
        y = input.ReadValue<Vector2>().y;
        if ((TopHang || !Grounded) && y < 0)
        {
            rb.velocity = new Vector2(0, 0);
            Smash = true;
            Vector3 dir = (this.transform.position - OtherMonkey.transform.position)*speed*15;
            dir.x *= -1;
            dir.y *= -1;
            rb.AddForce(dir);
        }
    }

    public void MonkePunch(InputAction.CallbackContext input)
    {
        if (punch)
        {
            punch = false;
            OtherMonkey.GetComponent<Monkey_Controller>().TakeDamage();
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            WallGrab = false;
            Smash = false;
            Grounded = true;
        }
        else if(other.gameObject.CompareTag("Wall") && !grab)
        {
            Grounded = false;
            Smash = false;
            WallGrab = true;
        }
        else if(other.gameObject.CompareTag("Roof") && !grab)
        {
            WallGrab = false;
            TopHang = true;
        }
        else if(other.gameObject.CompareTag("Player") && Smash)
        {
            OtherMonkey.GetComponent<Monkey_Controller>().TakeDamage();
            Smash = false;
        }
    }

    void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            WallGrab = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
                punch = true;
        }

        anim.SetBool("Punch", punch);
    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            punch = false;
        }

        anim.SetBool("Punch", punch);
    }

    public void TakeDamage()
    {
        health -= 1;
    }

    public void SendBack()
    {

    }
}
