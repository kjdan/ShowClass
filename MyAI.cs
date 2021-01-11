using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// 
//A class in which a part of the state machine is included, which is responsible for agent control in a project comparing the actions 
//of the classic method of controlling an opponent in games with steering using artificial neural networks.
public class MyAI : MonoBehaviour
{

    [SerializeField] private LayerMask _layerMask;
    Rigidbody m_AI;
    private MasterAgentSrcipt _opponent;
    public MasterArea m_AreaMaster;
    public GameObject[] spawnAreas;
    public TextMesh myText;

    private float _attackRange = 10f;
    private float _rayDistance = 5.0f;
    private float _stoppingDistance = 1.5f;
    public int points = 0;

    private Vector3 _way;
    private Quaternion _desiredRotation;
    private Vector3 _direction;

    private AIState _currentState;
  
    public void Start()
    {
        m_AI = GetComponent<Rigidbody>();

    }

    private void Update()
    {
        if(m_AreaMaster.playerStatesMaster[0].agentScript.myState==true)
        {
            OnStartPosition();

            m_AreaMaster.playerStatesMaster[0].agentScript.myState = false;
          
        }
        if(m_AreaMaster.playerStatesMaster[1].agentScript.myState == true)
        {
            OnStartPosition();
           
            m_AreaMaster.playerStatesMaster[1].agentScript.myState = false;
        }
        switch (_currentState)
        {
            case AIState.Search:
                {
                    if (NeedNewWay())
                    {
                        GetNewWay();
                    }

                    transform.rotation = _desiredRotation;

                    m_AI.AddForce(transform.forward * 1f * 2f, ForceMode.VelocityChange);
                    var rayColor = IsBlocked() ? Color.red : Color.green;
                    Debug.DrawRay(transform.position, _direction * _rayDistance, rayColor);

                    while (IsBlocked())
                    {

                        GetNewWay();
                    }

                    var checkOpponent = IsOpponent();
                    if (checkOpponent != null)
                    {
                        _opponent = checkOpponent.GetComponent<MasterAgentSrcipt>();
                        _currentState = AIState.GoAfter;
                    }

                    break;
                }
            case AIState.GoAfter:
                {
                    if (_opponent == null)
                    {
                        _currentState = AIState.Search;
                        return;
                    }

                     transform.LookAt(_opponent.transform);

                    m_AI.AddForce(transform.forward * 1f * 2f, ForceMode.VelocityChange);
                    if (Vector3.Distance(transform.position, _opponent.transform.position) < _attackRange)
                    {
                        _currentState = AIState.Attack;
                    }
                    break;
                }
            case AIState.Attack:
                {
                    if (_opponent != null)
                    {
                        RaycastHit hit;

                        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 10f))
                        {

                            if (hit.collider.CompareTag("playerBlue")) 
                            {
                                m_AreaMaster.CheckHit(MasterAgentSrcipt.Team.Blue);
                                points++;
                            }
                            if (hit.collider.CompareTag("playerGreen")) 
                            {
                                m_AreaMaster.CheckHit(MasterAgentSrcipt.Team.Green);
                            }

                            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 10f, Color.green);
                            
                            
                        }
                        else
                        {
                            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 10f, Color.red);
                        }
                    }

                   

                    _currentState = AIState.Search;
                    break;
                }
        }
    }

    private bool IsBlocked()
    {

        Ray ray = new Ray(transform.position, _direction);
        var hit = Physics.RaycastAll(ray, _rayDistance, _layerMask);
        return hit.Any();
    }


    private void GetNewWay()
    {
        Vector3 newPosition = (transform.position + (transform.forward * 4f)) +new Vector3(UnityEngine.Random.Range(-4.5f, 4.5f), 0f,UnityEngine.Random.Range(-4.5f, 4.5f));
        _way = new Vector3(newPosition.x, 1f, newPosition.z);
        _direction = Vector3.Normalize(_way - transform.position);
        _direction = new Vector3(_direction.x, 0f, _direction.z);
        _desiredRotation = Quaternion.LookRotation(_direction);
    }

    private bool NeedNewWay()
    {
        if (_way == Vector3.zero)
            return true;

        var distance = Vector3.Distance(transform.position, _way);
        if (distance <= _stoppingDistance)
        {
            return true;
        }

        return false;
    }

    Quaternion startingAngle = Quaternion.AngleAxis(-70, Vector3.up);
    Quaternion stepAngle = Quaternion.AngleAxis(5, Vector3.up);

    private Transform IsOpponent()
    {
        float aggroRadius = 40f;

        RaycastHit hit;
        var angle = transform.rotation * startingAngle;
        var direction = angle * Vector3.forward;
        var pos = transform.position;
        for (var i = 0; i < 24; i++)
        {
            if (Physics.Raycast(pos, direction, out hit, aggroRadius))
            {
                var drone = hit.collider.GetComponent<MasterAgentSrcipt>();
                
                if (drone != null)
                {
 
                 //   Debug.DrawRay(pos, direction * hit.distance, Color.red);

                    return drone.transform;
                }
                else
                {
                  //  Debug.DrawRay(pos, direction * hit.distance, Color.yellow);
                }
            }
            else
            {
              //  Debug.DrawRay(pos, direction * aggroRadius, Color.white);
            }
            direction = stepAngle * direction;
        }

        return null;
    }

    public void OnStartPosition()
    {
        _currentState = AIState.Search;
        System.Random rng = new System.Random();
        int randomNumber = rng.Next(0, 2);
        m_AI.transform.position = spawnAreas[randomNumber].transform.position;

        if (m_AreaMaster.playerStatesMaster[0].agentScript.team == MasterAgentSrcipt.Team.Blue)
        {
            if (m_AreaMaster.playerStatesMaster[0].agentRb.position == m_AI.position)
            {

                while (m_AreaMaster.playerStatesMaster[0].agentRb.position == m_AI.position)
                {

                    rng = new System.Random();
                    randomNumber = rng.Next(0, 2);
                    m_AI.position = spawnAreas[randomNumber].transform.position;
                }

            }
            if (m_AreaMaster.playerStatesMaster[0].agentScript.points + points == 100)
            {


                myText.text = (m_AreaMaster.playerStatesMaster[0].agentScript.team + " : " + m_AreaMaster.playerStatesMaster[0].agentScript.points + " Green : " + points);
            }
            m_AreaMaster.playerStatesMaster[0].agentScript.myState = false;


        }

         if (m_AreaMaster.playerStatesMaster[1].agentScript.team == MasterAgentSrcipt.Team.Blue)
        {
            if (m_AreaMaster.playerStatesMaster[1].agentRb.position == m_AI.position)
            {

                while (m_AreaMaster.playerStatesMaster[1].agentRb.position == m_AI.position)
                {
   
                    rng = new System.Random();
                    randomNumber = rng.Next(0, 2);
                    m_AI.position = spawnAreas[randomNumber].transform.position;
                }

            }
            if (m_AreaMaster.playerStatesMaster[1].agentScript.points + points == 100)
            {


                myText.text = (m_AreaMaster.playerStatesMaster[1].agentScript.team + " : " + m_AreaMaster.playerStatesMaster[1].agentScript.points + " Green : " + points);
            }
            m_AreaMaster.playerStatesMaster[1].agentScript.myState = false;
        }

    }

}


public enum AIState
{
    Search,
    GoAfter,
    Attack
}


