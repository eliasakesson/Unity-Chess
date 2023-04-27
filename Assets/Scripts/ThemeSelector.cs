using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThemeSelector : MonoBehaviour
{
    BoardUI boardUI;

    public Theme[] themes;
    public GameObject themePrefab;

    List<Image> buttonImages = new List<Image>();

    private void Start(){
        boardUI = FindObjectOfType<BoardUI>();

        DisplayThemes();

        GetComponentInChildren<Button>(true).onClick.Invoke();
    }

    private void DisplayThemes(){
        foreach (Theme theme in themes)
        {
            GameObject themeButton = Instantiate(themePrefab, transform);

            Image[] images = themeButton.GetComponentsInChildren<Image>(true);
            buttonImages.Add(images[0]);
            images[1].color = theme.lightColor;
            images[2].color = theme.darkColor;
            images[3].color = theme.darkColor;
            images[4].color = theme.lightColor;
            
            themeButton.GetComponent<Button>().onClick.AddListener(() => {
                boardUI.theme = theme;
                boardUI.ResetAllSquareColors();

                foreach (Image buttonImage in buttonImages){
                    buttonImage.enabled = false;
                }

                images[0].enabled = true;
            });
        }
    }
}
