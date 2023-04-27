using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGroup : MonoBehaviour
{
    Button[] buttons;

    public Color selectedNormal;
    public Color notSelectedNormal;

    public Color selectedHighlighted;
    public Color notSelectedHighlighted;

    private void Start(){
        buttons = GetComponentsInChildren<Button>();

        foreach (Button button in buttons){
            button.onClick.AddListener(delegate{ButtonClick(button);});
        }

        ResetButtonColors();
        buttons[0].onClick.Invoke();
    }

    private void ButtonClick(Button button){
        ResetButtonColors();
        
        var colors = button.colors;
        colors.normalColor = selectedNormal;
        colors.highlightedColor = selectedHighlighted;
        button.colors = colors;
    }

    private void ResetButtonColors(){
        foreach (Button button in buttons){
            var colors = button.colors;
            colors.normalColor = notSelectedNormal;
            colors.highlightedColor = notSelectedHighlighted;
            button.colors = colors; 
        }
    }
}
