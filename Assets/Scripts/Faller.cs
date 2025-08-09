using UnityEngine;

public class Faller : MonoBehaviour
{
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
        if (other.CompareTag("Player"))
        {
            PlayerDeath.IsInvulnerable = false;
            var playerDeath = other.GetComponent<PlayerDeath>();
            playerDeath?.FallAndDie();
            playerDeath?.Die();
        }    
    }
}
