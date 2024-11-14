using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class ColorButton : MonoBehaviour
{
    public Color color = Color.black;
    public Color background = Color.white;
    public Sprite sprite;
    public float size = 0.8f;
    public string text = "Button";
    public bool toggle;
    public bool isPressed;
    private Color bg;
    Image button;
    Image image;
    TMP_Text tmp;


    void Awake() {
        button = GetComponent<Image>();
        image = transform.GetChild(0).GetComponent<Image>();
        image.color = color;
        image.rectTransform.sizeDelta = button.rectTransform.sizeDelta * size;
        image.sprite = sprite;
        tmp = transform.GetChild(1).GetComponent<TMP_Text>();
        Vector2 size1 = button.rectTransform.sizeDelta;
        tmp.text = text;
        tmp.fontSize = size1.y * 0.2f;
        tmp.rectTransform.anchoredPosition = new Vector2(0, size1.y * -0.6f);
        tmp.rectTransform.sizeDelta = new Vector2(size1.x, size1.y * 0.2f);
        
    }

    void Update() {
        if (isPressed) {
            image.color = background;
            button.color = color;
        }
        else {
            image.color = color;
            button.color = background;
        }
    }

    public void ButtonDown() {
        if (toggle) return;
        else isPressed = true;
    }

    public void ButtonUp() {
        if (toggle) isPressed = !isPressed;
        else isPressed = false;
    }

}
