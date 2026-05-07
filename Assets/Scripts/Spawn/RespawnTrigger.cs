using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    //[SerializeField] private Transform respawnPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        /*
        CharacterController cc = other.gameObject.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            other.transform.position = respawnPosition.position;
            cc.enabled = true;
            
        }
        */
        Character c = other.GetComponent<Character>();
        if (c != null )
        {
            c.Die();
        }

    }

    private void OnTriggerExit(Collider other)
    {

    }
}
