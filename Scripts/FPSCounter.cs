using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FPSCounter : MonoBehaviour
{
    string label = "";
    [SerializeField] private GUIStyle m_style;
    float count;

    IEnumerator Start()
    {

        GUI.depth = 2;
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            count = (1 / Time.deltaTime);
            label = "FPS :" + (Mathf.Round(count));
            yield return new WaitForSeconds(0.5f);
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 105, 100, 25), label, m_style);
    }
}
