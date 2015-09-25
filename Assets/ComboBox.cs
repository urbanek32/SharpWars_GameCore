/*
// znalezione na: http://wiki.unity3d.com/index.php?title=PopupList
// zak³adam ¿e na licencji Public Domain
 * 
 * 
// Popup list created by Eric Haines
// ComboBox Extended by Hyungseok Seo.(Jerry) sdragoon@nate.com
// Refactored by zhujiangbo jumbozhu@gmail.com
// Slight edit for button to show the previously selected item AndyMartin458 www.clubconsortya.blogspot.com
// 
// -----------------------------------------------
// This code working like ComboBox Control.
// I just changed some part of code, 
// because I want to seperate ComboBox button and List.
// ( You can see the result of this code from Description's last picture )
// -----------------------------------------------
//
// === usage ======================================
using UnityEngine;
using System.Collections;
 
public class ComboBoxTest : MonoBehaviour
{
	GUIContent[] comboBoxList;
	private ComboBox comboBoxControl;// = new ComboBox();
	private GUIStyle listStyle = new GUIStyle();
 
	private void Start()
	{
		comboBoxList = new GUIContent[5];
		comboBoxList[0] = new GUIContent("Thing 1");
		comboBoxList[1] = new GUIContent("Thing 2");
		comboBoxList[2] = new GUIContent("Thing 3");
		comboBoxList[3] = new GUIContent("Thing 4");
		comboBoxList[4] = new GUIContent("Thing 5");
 
		listStyle.normal.textColor = Color.white; 
		listStyle.onHover.background =
		listStyle.hover.background = new Texture2D(2, 2);
		listStyle.padding.left =
		listStyle.padding.right =
		listStyle.padding.top =
		listStyle.padding.bottom = 4;
 
		comboBoxControl = new ComboBox(new Rect(50, 100, 100, 20), comboBoxList[0], comboBoxList, "button", "box", listStyle);
	}
 
	private void OnGUI () 
	{
		comboBoxControl.Show();
	}
}
 
*/

using System.Collections.Generic;
using UnityEngine;
using STL;

public class ComboBox
{
    private static bool forceToUnShow = false; 
    private static int useControlID = -1;
    private bool isClickedComboButton = false;
    private int selectedItemIndex = 0;
 
	private Rect rect;
	private GUIContent buttonContent;
	private GUIContent[] listContent;
	private string buttonStyle;
	//private string boxStyle;
	private GUIStyle listStyle;
    private GUIStyle boxStyle;
    Player humanPlayer;

    private static GUIContent[] emptyContent = { new GUIContent("-----") };

    public bool IsClicked() { return isClickedComboButton; }
 
    public ComboBox( Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle uniStyle ){
		this.rect = rect;
		this.buttonContent = buttonContent;
		this.listContent = listContent;
		this.buttonStyle = "button";
		this.boxStyle = uniStyle;
		this.listStyle = uniStyle;
    }
 
	public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, string buttonStyle, GUIStyle boxStyle, GUIStyle listStyle){
		this.rect = rect;
		this.buttonContent = buttonContent;
		this.listContent = listContent;
		this.buttonStyle = buttonStyle;
		this.boxStyle = boxStyle;
		this.listStyle = listStyle;
	}

    //Adds element at the end of list(array to be more precisely)
    void AddItem(GUIContent item)
    {
        List<GUIContent> content = (listContent == null) ? new List<GUIContent>() : new List<GUIContent>(listContent);
        content.Add(item);
        listContent = content.ToArray();
    }

    //Remove item from list by id
    void RemoveItem(int id)
    {
        if (listContent == null)
            return;
        if (listContent.Length > id)
        {
            List<GUIContent> content = new List<GUIContent>(listContent);
            content.RemoveAt(id);
            listContent = content.ToArray();
        }
    }

    public int Show(Pair<string, string>[] items, Player player)
    {
        List<GUIContent> lc = new List<GUIContent>();
        humanPlayer = player;

        foreach (Pair<string, string> i in items)
        {
            lc.Add(new GUIContent(i.First));
        }

        listContent = lc.ToArray();

        return Show();
    }
 
    public int Show()
    {
        if( forceToUnShow )
        {
            forceToUnShow = false;
            isClickedComboButton = false;
        }
 
        bool done = false;
        int controlID = GUIUtility.GetControlID( FocusType.Passive );       
 
        switch( Event.current.GetTypeForControl(controlID) )
        {
            case EventType.mouseUp:
            {
                if( isClickedComboButton )
                {
                    done = true;
                }
            }
            break;
        }       
 
        if( GUI.Button( rect, buttonContent, buttonStyle ) )
        {
            if( useControlID == -1 )
            {
                useControlID = controlID;
                isClickedComboButton = false;
            }
 
            if( useControlID != controlID )
            {
                forceToUnShow = true;
                useControlID = controlID;
            }
            isClickedComboButton = true;
        }
 
        if( isClickedComboButton )
        {
            GUIContent[] guiContent = (listContent == null) ? emptyContent : listContent;

            Rect listRect = new Rect( rect.x, rect.y + listStyle.CalcHeight(guiContent[0], 1.0f),
                      rect.width, listStyle.CalcHeight(guiContent[0], 1.0f) * guiContent.Length );

            GUI.Box(listRect, "", boxStyle);
            int newSelectedItemIndex = GUI.SelectionGrid( listRect, selectedItemIndex, guiContent, 1, listStyle);
            if( newSelectedItemIndex != selectedItemIndex || (selectedItemIndex == 0 && humanPlayer.SelectedObject != null && humanPlayer.SelectedObject.selectedScript != selectedItemIndex) )
            {
                selectedItemIndex = newSelectedItemIndex;
                buttonContent = guiContent[selectedItemIndex];

                //overwrite world's object script
                if (humanPlayer.SelectedObject != null && humanPlayer.SelectedObject.selectedScript != selectedItemIndex)
                {
                    humanPlayer.SelectedObject.selectedScript = selectedItemIndex;
                    humanPlayer.SelectedObject.unitScript = humanPlayer.scriptList[selectedItemIndex].Second;
                }
            }
        }
 
        if( done )
            isClickedComboButton = false;
 
        return selectedItemIndex;
    }
 
    public int SelectedItemIndex{
		get{
        	return selectedItemIndex;
		}
		set{
			selectedItemIndex = value;
		}
    }
}