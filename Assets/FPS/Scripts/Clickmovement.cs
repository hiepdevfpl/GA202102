using UnityEngine;
using UnityEngine.AI;

public class Clickmovement : MonoBehaviour
{
    private NavMeshAgent navagent;
    private void Start()
    {
        navagent = GetComponent<NavMeshAgent>();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                navagent.SetDestination(hit.point);
            }
        }
    }
}
