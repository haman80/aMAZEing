using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class phraseHandler : MonoBehaviour
{
    public int numTokens = 5; //Total number of tokens
    private int revealIndex = 0; //Words up to this index have been revealed based on revealOrder
    private int tokenIndex = 0; // The nubmber of token that have been collected so far
    private int[] revealOrder; //The order in which words will be revealed after each token collection
    private bool[] wordVisability; //Which words are visable
    private int[] revealPerToken; //Number of words that will be revealed after each token collection
    private string[] wordsArray; //The array containing all the words of our phrase
    public TMPro.TMP_Text fact_on_canvas; //Text Object associated with the phrase on the canvas
    // Start is called before the first frame update
    void Start()
    {
        List<string> facts = new List<string>(); 
        //Loading the phrases from the CSV file
        using (StreamReader sr = new StreamReader("History.csv"))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    facts.Add(line);
                }
            }
        //Choosing a random fact and storing it in phrase variable
        int index = (int) Random.Range(0, facts.Count-0.001f);
        string phrase = facts[index];
        //Initializing the wordsArray and its visibility 
        wordsArray = phrase.Split(' ');
        wordVisability = new bool[wordsArray.Length];
        //Initializing the reveal order by choosing a random permutaiton 
        revealOrder = randomPermutation(wordsArray.Length);
        //Initializing the revealPerToken array
        revealPerToken = new int[numTokens];
        int q = wordsArray.Length/numTokens;
        int r = wordsArray.Length%numTokens;
        for (int i = 0; i<numTokens;i++ ){
            if (r>0){
                revealPerToken[i]=q+1;
                r=r-1;
            }
            else{
                revealPerToken[i]=q;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (tokenIndex<numTokens){
            revealNextWord();
        }
        
    }
    //Reveals the next set of words after a token has been collected
    void revealNextWord(){
        string newText = "";
        for (int i = 0; i<revealPerToken[tokenIndex]; i++){
            wordVisability[revealOrder[revealIndex]]=true;
            revealIndex=revealIndex+1;
        }
        tokenIndex=tokenIndex+1;
        for (int i = 0; i<wordsArray.Length; i++){
            if (wordVisability[i]){
                newText=newText+" " + wordsArray[i];
            }
            else{
                newText=newText+" " +"___";
            }
        }
        Debug.Log(newText);
        fact_on_canvas.text = newText;
    }
    
    //Generates a random permutation using the Fisher-Yates shuffling algorithm 
    int[] randomPermutation(int n){
        int[] array = new int[n];
        for (int i = 0; i <n; i++){
            array[i] = i;
        }
        for (int i = 0; i < (n - 1); i++)
        {
            int r = i + (int) Random.Range(0, n-i-0.001f);
            int t = array[r];
            array[r] = array[i];
            array[i] = t;
        }
        return array;
    }

}
