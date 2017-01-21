using UnityEngine;
using System.Collections;

public class CharacterVarietyController : MonoBehaviour
{
    Color[] skinColors;
    Color[] hairColors;

    SpriteRenderer bodyRenderer;
    SpriteRenderer pantsRenderer;
    SpriteRenderer shirtRenderer;
    SpriteRenderer hairRenderer;
    SpriteRenderer headRenderer;
    SpriteRenderer eyeRenderer;
    SpriteRenderer armRenderer;

    void Awake()
    {
        skinColors = new Color[3] { new Color(229f / 255f, 163f / 225f, 115f / 225f), new Color(204f / 255f, 162f / 255f, 132f / 255f), new Color(164f / 255f, 116f / 255f, 81f / 255f) };
        hairColors = new Color[2] { new Color(52f / 255f, 52f / 255f, 52f / 255f), new Color(196f / 255f, 110f / 255f, 72f / 255f) };
        bodyRenderer = transform.FindChild("Body").GetComponent<SpriteRenderer>();
        pantsRenderer = transform.FindChild("Leg").GetComponent<SpriteRenderer>();
        shirtRenderer = transform.FindChild("Body").GetComponent<SpriteRenderer>();
        hairRenderer = transform.FindChild("Hair").GetComponent<SpriteRenderer>();
        headRenderer = transform.FindChild("Head").GetComponent<SpriteRenderer>();
        eyeRenderer = transform.FindChild("Eye").GetComponent<SpriteRenderer>();
        armRenderer = transform.FindChild("Arm").GetComponent<SpriteRenderer>();
    }

    public void Set(CellType type)
    {
        // 피부색 처리
        Color skinColor = skinColors[Random.Range(0, skinColors.Length)];

        headRenderer.color = skinColor;
        armRenderer.color = skinColor;
        //

        // 티셔츠
        if(type == CellType.Normal)
        {
            string sprName = "시민/body" + Random.Range(0, 2).ToString();
            Sprite spr = Resources.Load<Sprite>(sprName);
            bodyRenderer.sprite = spr;
        }
        else if(type == CellType.Protester)
        {
            Sprite spr = Resources.Load<Sprite>("시위대/body");
            bodyRenderer.sprite = spr;
        }
        //

        // 머리카락
        hairRenderer.color = hairColors[Random.Range(0, hairColors.Length)];
        //

        // 바지
        string pantsSprName = "시민/leg" + Random.Range(0, 4).ToString();
        Sprite pantsSpr = Resources.Load<Sprite>(pantsSprName);
        pantsRenderer.sprite = pantsSpr;
        //

    }

    public void ChangeToAgent()
    {
        armRenderer.color = new Color(1, 1, 1, 1);
        headRenderer.color = new Color(1, 1, 1, 1);
        headRenderer.sprite = Resources.Load<Sprite>("국장/head");
        bodyRenderer.sprite = Resources.Load<Sprite>("국장/body");
        eyeRenderer.sprite = Resources.Load<Sprite>("국장/eye");
        hairRenderer.sprite = Resources.Load<Sprite>("국장/hair");
        armRenderer.sprite = Resources.Load<Sprite>("국장/arm");
        pantsRenderer.sprite = Resources.Load<Sprite>("국장/leg");
    }

    // 키를 랜덤하게 해서 키 큰 사람, 뚱뚱한 사람( scale.y )
    // 머리카락 색깔은 정해주는 칼라값 중 적용
    // 피부색은 갈색
    // 티셔츠는 스프라이트 교체
    // 바지도 스프라이트 교체

    // 시위대는 일단 티셔츠 교체만 하고 나중에 시위대 프리팹을 소환하는 걸로 하자

}