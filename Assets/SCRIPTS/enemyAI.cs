using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour
{
    private NavMeshAgent _agent;
    public Transform player;

    private float visionRange = 20;
    private float attackRange = 10;

    private bool playerInVisionRange;
    private bool playerInAttackRange;

    [SerializeField] private LayerMask playerLayer; //unica capa que tendran en compte es enemics per ses collisions


    //variables esperc�fiques de pratrulla
    [SerializeField] private  Transform[] waypoints; //diferents punts de patrulla (aniran d'un a s'altre punt) <min 2>

    private int totalWaypoints;//guardar es total de punts per poder fer sa volta a s'array
    private int nextPoint; //tenir una variable que guardi quin �s es proxim punt en tot moment

    //atac
    [SerializeField]private GameObject bullet; //
    private float timeBetweenAttacks = 2f; //es temps entre disparo i disparo 
    private bool canAttack;
    private float upAttackForce = 5f;
    private float forwardAttackForce = 8f;

    //instantiate bullets
    public Transform spawnPoint;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

    }
    void Start()
    {
        totalWaypoints = waypoints.Length; //asignar sa llarg�ria de s'arary a sa variable
        nextPoint = 1; //sempre comen�ar� des de sa opci� 1 
        canAttack = true; //reiniciar be es can attak
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position; //sa nostra posici� en tot moment
        playerInVisionRange = Physics.CheckSphere(pos, visionRange, playerLayer);
        playerInAttackRange = Physics.CheckSphere(pos, attackRange, playerLayer);

        if(!playerInVisionRange && !playerInAttackRange) //si no hi haning� a nes rango d'attak ni vision me moc tranquilament, patrol
        {
            Patrol();
        }

        if(playerInVisionRange && !playerInAttackRange) //si es player entra dins es ranfo de vision per� no al d'attack, el persegeixo
        {
            Chase();
        }

        if(playerInVisionRange && playerInAttackRange) //si es player est� dins s'�rea de visi� i attack l'attack ( ja qu es'area d'attack est� dins sa de visi�)
        {
            Attack();
        }
    }

    private void Patrol()
    {
        if(Vector3.Distance(transform.position, waypoints[nextPoint].position) < 2.5f) //si l'enemic est� a menos de2,5 metros des punt dest�, canviam el next point al pr�xim
        {
            nextPoint++;
            if(nextPoint == totalWaypoints)
            {
                nextPoint = 0;
            }
            transform.LookAt(waypoints[nextPoint].position); //que miri cap al punt nex
        }
        _agent.SetDestination(waypoints[nextPoint].position);

    }
    private void Chase()
    {
        _agent.SetDestination(player.position); //segueix al player
        transform.LookAt(player); //mira al player
    } 
    private void Attack()
    {
        if (canAttack)
        {
            _agent.SetDestination(transform.position); //perqu� s'agent s'aturi, li deim que es dest� �s ell mateix

            //component rigidbody de sa bala que acabam de instanciar
                    Rigidbody rigidbody = Instantiate(bullet, spawnPoint.position, Quaternion.identity).GetComponent<Rigidbody>();

                    //li donam for�a adalt i endavant per fer una parab�lica
                    rigidbody.AddForce(transform.forward * forwardAttackForce, ForceMode.Impulse);
                    rigidbody.AddForce(transform.up * upAttackForce, ForceMode.Impulse);

                    canAttack = false;
                    StartCoroutine("AttackCoolDown");
        }
        
    }

    private IEnumerator AttackCoolDown() //esperar� un poc entre bales i li donar� perm�s per atacar despr�s
    {
        yield return new WaitForSeconds(timeBetweenAttacks);
        canAttack = true;
    }

    private void OnDrawGizmos() //dibuuixar rango de atac
    {
        //vision sphere
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        
        //attack sphere
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
