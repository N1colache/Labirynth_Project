using UnityEngine;

public class ShootingAi : MonoBehaviour
{
    public float health ;
    public bool isDead;
    
    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health < 0)
        {
            isDead = true;
        }
    }
}
