using UnityEngine;

public class DelegateExample : MonoBehaviour
{     
    //The keyword delegate defines the DoMath as a blueprint of a method which takes in two float parameters, a and b
    // Notice this blueprint also requires a return type -- a float
    // Delegates follow this structure:  delegate returnType DelegateName(parameters
    delegate float DoMath(float a, float b);
    
    //Here we have a regular function that adds two float numbers together and returns their combined total
    private float Add(float a, float b)
    {
        return a + b;
    }
    
    // Likewise, we have a function that subtracts two float numbers and returns their difference
    private float Subtract(float a, float b)
    {
        return a - b;
    }
    
    // Use this for initialization
    void Start ()
    {
        // Here we define a variable of type DoMath and pass in our Add function
        DoMath doMathVariable = new DoMath(Add);
        
        // here we call our doMathVariable like a function - this prints the total of 6 + 4 to console 
        Debug.Log(doMathVariable(6, 4));
        
        //Here's where things get interesting. We are adding to the delegate variable a new function
        doMathVariable += Subtract;
        
        // What do you think happens here? Will we see 6 + 4, 6 - 4, or both?
        Debug.Log(doMathVariable(6, 4));

        // lambda expression for multiplication function
        doMathVariable += (a, b) => a * b;
        Debug.Log(doMathVariable(6, 4));
    }      
}
