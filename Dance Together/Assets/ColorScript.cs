using UnityEngine;
using System.Collections;

public class ColorScript : MonoBehaviour {
    private Color[] colors = new Color[] {
            //Color.gray,
            //Color.black,
            //new Color(0.33f,0.5f,0.5f), //Jade Green
            //new Color(0.6f,0.4f,0.8f), //Purple
            //new Color(0.3f,0.2f,0.1f), //Brown
            //new Color(0f,0.5f,1f), //Light Blue

            Color.white,
            Color.blue,
            Color.cyan,
            Color.green,
            Color.magenta,
            Color.red,
            Color.yellow,
            new Color(1f,0.5f,0f), //Orange
            new Color(0.2f,0.3f,0.1f), //Dark Green
            new Color(0.25f,0f,1f) //Ultramarine (Bright Purple)
        };

    private List<Color> unusedColors;

    void Start ()
    {
        unusedColors;
    }

    static public Color GetNewColor()
    {
        return colors[Random.Range(0, ColorScript.colors.Length)];
    }
}
