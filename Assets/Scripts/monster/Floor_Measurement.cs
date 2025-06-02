using UnityEngine;

public class Floor_Measurement : MonoBehaviour
{
    public Vector2 boxSize = new Vector2(1.0f, 1.0f);
    public LayerMask groundLayer;
    public GameObject pos;
    private GameObject MimicBox;
    public Rigidbody2D rb { get; private set; }

    public bool Groundcheck = false;
    public float direction = 1f;

    void Update()
    {
        CheckForHillAhead();
        Change_of_location();
    }

    void CheckForHillAhead()
    {
        Collider2D hit = Physics2D.OverlapBox(pos.transform.position, boxSize, 0f, groundLayer);

        if (hit != null)
        {
            //Debug.Log("sdsdsd");
            Groundcheck = true;
        }
        else
        {
            Groundcheck = false;
        }
    }
    void Change_of_location()
    {
        /*Vector3 newPosition = MimicBox.transform.position;
        if(direction == 1f)
        {
            newPosition.x *= 1f;
        }
        else
        {
            newPosition.x *= -1f;
        }
        transform.position = newPosition;*/
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(pos.transform.position, boxSize);
    }
}
