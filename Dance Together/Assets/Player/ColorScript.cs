using UnityEngine;
using System.Collections;

public class ColorScript : MonoBehaviour {
    static public Color[] colors = new Color[] {

            Color.white, // White
        new Color(0.66f,0.66f,0.66f), // Grey
        new Color(0.5f,0.25f,0.0f), // Wood
        new Color(1f,0.0f,0.0f), // Red
        new Color(1f,0.5f,0.0f), // Orange
        new Color(1f,0.85f,0.0f), // Yellow
        new Color(0.0f,1f,0.13f), // Green
        new Color(0.0f,0.5f,1.0f), // Ocean
        new Color(0.0f,0.13f,1.0f), // Blue
        new Color(0.75f,0.0f,1.0f), // Orchid
        new Color(0.0f,0.5f,0.07f), // Forest
        new Color(0.0f,0.07f,0.5f), // Midnight
        new Color(0.25f,0.0f,0.5f), // Purple
        new Color(0.5f,0.0f,0.0f), // Wine
        new Color(0.5f,0.5f,0.0f), // Olive
        new Color(0.25f,0.13f,0.0f), // Brown
        new Color(0.33f,0.33f,0.33f), //Charcoal
        new Color(0.0f,0.0f,0.0f) // Black
        };

    static public string GetColorName(int c)
    {
        switch (c)
        {
            case 0:
                return "White";
            case 1:
                return "Grey";
            case 2:
                return "Wood";
            case 3:
                return "Red";
            case 4:
                return "Orange";
            case 5:
                return "Yellow";
            case 6:
                return "Green";
            case 7:
                return "Ocean";
            case 8:
                return "Blue";
            case 9:
                return "Orchid";
            case 10:
                return "Forest";
            case 11:
                return "Midnight";
            case 12:
                return "Purple";
            case 13:
                return "Wine";
            case 14:
                return "Olive";
            case 15:
                return "Brown";
            case 16:
                return "Charcoal";
            default: //17
                return "Black";
        }
    }

    static public Color GetColor(int index)
    {
        return colors[index];
    }
}
