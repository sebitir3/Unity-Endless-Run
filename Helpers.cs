using UnityEngine;

public static class Helpers
{
    //Modify the x axis of a vector 3
    public static void SetPositionX(Transform t, float x)
    {
        t.position = new Vector3(x, t.position.y, t.position.z);
    }

    //Modify the y axis of a vector 3
    public static void SetPositionY(Transform t, float y)
    {
        t.position = new Vector3(t.position.x, y, t.position.z);
    }

    public static void SetPositionZ(Transform t, float z)
    {
        t.position = new Vector3(t.position.x, t.position.y, z);
    }

    //What is being divided is called the dividend
    //What the dividend is being divided by is called the divisor
    //modulo takes these in as parameters, where x is dividend and m is divisor
    public static int Mod(int x, int m)
    {
        //returns the result of x % m, adds m to that remainder to keep it positive
        //then finds the remainder on the positive value
        return (x % m + m) % m;
    }
}
