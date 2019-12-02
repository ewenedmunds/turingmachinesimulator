using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StateScript : MonoBehaviour
{
    public (StateScript, char)[] transitions = new (StateScript, char)[0];
    public bool isActive = false;
    public bool isGoingToBeActive = false;
    public bool isAccepting = false;
    public bool isStartState = false;

    private Camera mainCamera;

    public GameObject linePrefab;
    public GameObject charPrefab;

    private float clickTimer = 0.4f;
    private int clicks = 0;

    private void Start()
    {

        mainCamera = Camera.main;
    }

    public void UpdateState(char nextChar, bool epsilonTransition=false)
    {
        if (isActive || epsilonTransition)
        {
            isActive = false;

            foreach ((StateScript x, char y) in transitions)
            {
                if (y == nextChar)
                {
                    if (x.isGoingToBeActive == false)
                    {
                        x.isGoingToBeActive = true;
                        //We know that this is the first time we've visited this node this time - try and epsilon transition from here
                        x.UpdateState('ε',true);
                    }
                    x.isGoingToBeActive = true;
                }
            }
        }
    }

    public void LateUpdateState()
    {
        if (isGoingToBeActive)
        {
            isActive = true;
        }
        isGoingToBeActive = false;
    }

    public void Update()
    {
        if (isActive)
        {
            transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            transform.GetChild(1).gameObject.SetActive(false);
        }

        for(int i=0; i < transform.GetChild(3).childCount; i++)
        {
            Destroy(transform.GetChild(3).GetChild(i).gameObject);
        }

        //Draw on transitions
        foreach((StateScript x, char y) in transitions)
        {
            GameObject newLine = Instantiate(linePrefab);
            newLine.transform.parent = transform.GetChild(3);
            newLine.GetComponent<LineRenderer>().SetPosition(0, transform.position);
            newLine.GetComponent<LineRenderer>().SetPosition(1, x.gameObject.transform.position);
            newLine.transform.position = new Vector3(newLine.transform.position.x, newLine.transform.position.y, -20);

            GameObject tranText = Instantiate(charPrefab);
            tranText.transform.parent = transform.GetChild(3);
            tranText.transform.position = (x.transform.position + transform.position)/2;
            tranText.transform.GetChild(0).GetComponent<TextMeshPro>().text = y.ToString();
        }

        //Click Timer
        if (clickTimer > 0)
        {
            clickTimer -= Time.deltaTime;
            if (clickTimer <= 0)
            {
                clicks = 0;
            }
        }
    }

    public void AddTransition(StateScript newState, char charTransition)
    {
        (StateScript, char)[] newList = new (StateScript, char)[transitions.Length + 1];
        for(int i=0; i<transitions.Length; i++)
        {
            newList[i] = transitions[i];
        }

        newList[transitions.Length] = (newState, charTransition);
        transitions = newList;
    }

    private void OnMouseEnter()
    {
        GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0.8f);
    }

    private void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 1f);
    }

    private void OnMouseDown()
    {
        clicks += 1;
        if (clickTimer <= 0)
        {
            clickTimer = 0.4f;
        }
        if (clicks == 2)
        {
            isAccepting = !isAccepting;
            transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeInHierarchy);
        }
        if (clicks == 3)
        {
            isStartState = !isStartState;
            isAccepting = !isAccepting;
            transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeSelf);
            transform.GetChild(4).gameObject.SetActive(!transform.GetChild(4).gameObject.activeSelf);
        }
    }

    private void OnMouseDrag()
    {
        transform.position = mainCamera.ScreenToWorldPoint( Input.mousePosition );
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }
}
