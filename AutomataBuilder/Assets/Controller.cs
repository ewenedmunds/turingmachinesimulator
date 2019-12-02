using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Controller : MonoBehaviour
{
    public string word = "";
    private string processingWord = "";

    private StateScript[] states = new StateScript[0];
    public GameObject statePrefab;

    private GameObject fromState;
    private GameObject toState;
    private int linkCount = 0;

    public TextMeshProUGUI statusText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Create new state
        if (Input.GetMouseButtonDown(2))
        {
            GameObject newState = Instantiate(statePrefab);
            newState.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            newState.transform.position = new Vector3(newState.transform.position.x, newState.transform.position.y, 0);

            newState.transform.GetChild(2).GetComponent<TextMeshPro>().text = "q" + states.Length.ToString();

            StateScript[] newList = new StateScript[states.Length + 1];
            for(int i=0; i<states.Length; i++)
            {
                newList[i] = states[i];
            }
            newList[states.Length] = newState.GetComponent<StateScript>();
            states = newList;
        }


        //Deal with next letter of the current word
        if (Input.GetKeyDown(KeyCode.Return) && processingWord.Length > 0)
        {
            NextStep();

        }
        else if (processingWord.Length == 0)
        {
            
        }

        //Link two states
        if (Input.GetMouseButtonDown(1))
        {
            GameObject objHit;

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit)
            {

                objHit = hit.transform.gameObject;
                
                if (linkCount == 0)
                {
                    linkCount = 1;
                    fromState = objHit;
                }
                else
                {
                    linkCount = 0;
                    toState = objHit;
                    CreateTransition();
                }
            }
        }
    }

    public void NextStep(){
        if (processingWord.Length > 0)
        {
            char nextLetter = processingWord[0];
            processingWord = processingWord.Substring(1, processingWord.Length - 1);

            GameObject.FindGameObjectWithTag("WordInput").GetComponent<TMP_InputField>().text = processingWord;

            foreach (StateScript state in states)
            {
                state.UpdateState(nextLetter);
            }
            foreach (StateScript state in states)
            {
                state.LateUpdateState();
            }

            if (processingWord.Length == 0){
                NextStep();
            }
        }
        else{
            statusText.text = "Status: Rejected!";
            Debug.Log("Reached the end of the word!");
            foreach (StateScript state in states){
                if (state.isActive && state.isAccepting){
                    statusText.text = "Status: Accepted!";
                }
            }
        }
    }

    private void CreateTransition()
    {
        char tranChar;
        string text = GameObject.FindGameObjectWithTag("TransitionChar").GetComponent<TMP_InputField>().text;

        if (text.Length > 0)
        {
            tranChar = text[0];
        }
        else
        {
            tranChar = 'ε';
        }

        fromState.GetComponent<StateScript>().AddTransition(toState.GetComponent<StateScript>(), tranChar);

        fromState = null;
        toState = null;
    }

    public void CheckWord()
    {
        statusText.text = "Status: Checking";

        string text = GameObject.FindGameObjectWithTag("WordInput").GetComponent<TMP_InputField>().text;
        word = text;
        processingWord = text;

        foreach(StateScript state in states)
        {
            state.isActive = false;
        }

        foreach(StateScript state in states)
        {
            if (state.isStartState)
            {
                state.isActive = true;
                state.isGoingToBeActive = true;
                state.UpdateState('ε');
            }
        }
        foreach(StateScript state in states)
        {
            state.LateUpdateState();
        }
    }

}
