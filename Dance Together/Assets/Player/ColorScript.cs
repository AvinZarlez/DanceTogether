using UnityEngine;
using System.Collections;

public class ColorScript : MonoBehaviour {
    static public Color[] colors = new Color[] {
            //Color.gray,
            //Color.black,
            //new Color(0.33f,0.5f,0.5f), //Jade Green
            //new Color(0.6f,0.4f,0.8f), //Purple
            //new Color(0.3f,0.2f,0.1f), //Brown
            //new Color(0f,0.5f,1f), //Light Blue

            Color.white,
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.magenta,
            Color.cyan,
            new Color(1f,0.5f,0f), //Orange
            new Color(0.2f,0.3f,0.1f), //Dark Green
            //new Color(0.25f,0f,1f) //Ultramarine (Bright Purple)
        };

    static public Color GetColor(int index)
    {
        return colors[index];
    }
}
