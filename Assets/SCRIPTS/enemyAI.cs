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


    //variables espercífiques de pratrulla
    [SerializeField] private  Transform[] waypoints; //diferents punts de patrulla (aniran d'un a s'altre punt) <min 2>

    private int totalWaypoints;//guardar es total de punts per poder fer sa volta a s'array
    private int nextPoint; //tenir una variable que guardi quin és es proxim punt en tot moment

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
        totalWaypoints = waypoints.Length; //asignar sa llargària de s'arary a sa variable
        nextPoint = 1; //sempre començarà des de sa opció 1 
        canAttack = true; //reiniciar be es can attak
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position; //sa nostra posició en tot moment
        playerInVisionRange = Physics.CheckSphere(pos, visionRange, playerLayer);
        playerInAttackRange = Physics.CheckSphere(pos, attackRange, playerLayer);

        if(!playerInVisionRange && !playerInAttackRange) //si no hi haningú a nes rango d'attak ni vision me moc tranquilament, patrol
        {
            Patrol();
        }

        if(playerInVisionRange && !playerInAttackRange) //si es player entra dins es ranfo de vision però no al d'attack, el persegeixo
        {
            Chase();
        }

        if(playerInVisionRange && playerInAttackRange) //si es player està dins s'àrea de visió i attack l'attack ( ja qu es'area d'attack està dins sa de visió)
        {
            Attack();
        }
    }

    private void Patrol()
    {
        if(Vector3.Distance(transform.position, waypoints[nextPoint].position) < 2.5f) //si l'enemic està a menos de2,5 metros des punt destí, canviam el next point al pròxim
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
            _agent.SetDestination(transform.position); //perquè s'agent s'aturi, li deim que es destí és ell mateix

            //component rigidbody de sa bala que acabam de instanciar
                    Rigidbody rigidbody = Instantiate(bullet, spawnPoint.position, Quaternion.identity).GetComponent<Rigidbody>();

                    //li donam força adalt i endavant per fer una parabòlica
                    rigidbody.AddForce(transform.forward * forwardAttackForce, ForceMode.Impulse);
                    rigidbody.AddForce(transform.up * upAttackForce, ForceMode.Impulse);

                    canAttack = false;
                    StartCoroutine("AttackCoolDown");
        }
        
    }

    private IEnumerator AttackCoolDown() //esperaré un poc entre bales i li donaré permís per atacar després
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
