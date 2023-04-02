using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Monkey_Controller : MonoBehaviour
{
    public bool WallGrab, Grounded, grab,TopHang,Smash,punch,KnockBack,throwable;
    private Rigidbody2D rb;
    public int health;
    public float speed;
    private float timer;
    public GameObject OtherMonkey,pickup,PickUpSpot;
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
        if (!Smash && !KnockBack)
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

        if (!Grounded && !WallGrab)
        {
            rb.gravityScale = 1.0f;
        }

        if (KnockBack)
        {
            if (Time.time > timer)
            {
                KnockBack = false;
            }
        }
        if(grab &&pickup != null)
        {
            pickup.transform.position = PickUpSpot.transform.position;
        }
    }
    public void FullMovement(InputAction.CallbackContext input)
    {
        GroundMovement(input);
        AirControls(input);
        MonkeSmash(input);
    }

    public void FireControl(InputAction.CallbackContext input)
    {
        MonkeThrow(input);
        MonkePunch(input);
        MonkePickup(input);
    }

    void GroundMovement(InputAction.CallbackContext input)
    {
        if (!Smash && !KnockBack)
        {
            x = input.ReadValue<Vector2>().x;
            if (x < 0 && x != 0)
            {
                transform.rotation = Quaternion.Euler(0, 180, 0); 
            }
            else if (x != 0)
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
            if (y > 0)
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
        if (!Grounded && y < 0 && !Smash)
        {
            rb.velocity = new Vector2(0, 0);
            Smash = true;
            Vector3 dir = (this.transform.position - OtherMonkey.transform.position)*speed*25;
            Debug.Log(dir);

            dir.x *= -1;
            dir.y *= dir.normalized.x;
            if (dir.y > 0)
            {
                dir.y *= -1;
            }
            rb.AddForce(dir);
        }
    }

    void MonkePunch(InputAction.CallbackContext input)
    {
        if (punch && !Smash && !grab)
        {
            punch = false;
            OtherMonkey.GetComponent<Monkey_Controller>().TakeDamage(rb.velocity.x);
        }
    }

    void MonkePickup(InputAction.CallbackContext input)
    {
        if(pickup != null && !grab)
        { 
            pickup.transform.SetParent(transform);
            pickup.transform.GetComponent<BoxCollider2D>().isTrigger = true;
            pickup.transform.rotation = Quaternion.Euler(0, 0, 0);
            pickup.transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;
            pickup.transform.GetComponent<Rigidbody2D>().gravityScale = 0.0f;
            grab = true;
            throwable = false;

        }
        else if(!throwable && grab)
        {
            if(input.ReadValue<float>() <= 0)
            {
                throwable = true;
            }
        }
    }

    void MonkeThrow(InputAction.CallbackContext input)
    {
        if (pickup != null && grab && throwable)
        {
            anim.SetBool("Throw", true);
            pickup.transform.SetParent(null);
            pickup.transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
            pickup.transform.GetComponent<Throwable>().Throw(OtherMonkey);
            pickup = null;
            throwable = false;
            grab = false;
            anim.SetBool("Throw", false);
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
        else if(other.gameObject.CompareTag("Player") && Smash)
        {
            OtherMonkey.GetComponent<Monkey_Controller>().TakeDamage(rb.velocity.x);
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
            anim.SetBool("Punch", punch);
        }

        else if (other.gameObject.CompareTag("Pickup") && !Smash && !grab)
        {
            pickup = other.gameObject;
        }

    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            punch = false;
            anim.SetBool("Punch", punch);
        }

        else if (other.gameObject.CompareTag("Pickup") && !grab)
        {
            pickup = null;
        }
    }

    public void TakeDamage(float Xval)
    {
        health -= 1;
        SendBack(Xval);
    }

    public void SendBack(float xval)
    {
        rb.velocity = new Vector2(xval * speed, rb.velocity.y);
        SetStun(.125f);
    }

    public void SetStun(float time)
    {
        timer = Time.time + time;
        KnockBack = true;
    }
}
