/// <summary>
/// PI. Handles key and mouse input
/// </summary>
using UnityEngine;
using System.Collections.Generic;

namespace PLib
{
    public static class PInput {
		
	//////////////////////////
	//	GUI	information		//
	//////////////////////////

	#region GUI Info

	/// <summary>
	/// Checks if the GUI was used during this frame
	/// </summary>
	/// <returns>
	/// Returns true if anything was clicked, moved, etc.
	/// </returns>
	public static bool GuiInUse () {
		return GUIUtility.hotControl != 0;
	}
	
	/// <summary>
	/// Gets the string name of the GUI element the user used.
	/// </summary>
	/// <returns>
	/// The string name of the GUI element. Note, this value must be set just before
	/// the gui element using GUI.SetNextControlName("foo")
	/// </returns>
	public static string GetNameOfGuiInUse () {
		return GUI.GetNameOfFocusedControl();	
	}
	
	/// <summary>
	/// Returns true if the Cursor is over control. For GUILayout controls only. Must be called *immediately*
	/// after the UI component to get a result.
	/// </summary>
	/// <returns><c>true</c>, if over the last referenced control, <c>false</c> otherwise.</returns>
	/// <param name="controlName">Control name.</param>
	public static bool CursorOverControl (string controlName) {	
		return Event.current.type == EventType.Repaint &&
			GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
	}

	#endregion
	
	//////////////////////////
	//	Screen and Mouse	//
	//////////////////////////

	#region Screen and Mouse

	/// <summary>
	/// Determines if the layermask contains the indicated layer.
	/// </summary>
	/// <param name="layer">The layer to check.</param>
	public static bool Contains (this LayerMask source, int layer) {
		return source.Contains(PInput.LayerToMask(layer));
	}

	/// <summary>
	/// Determines if the layermask contains all of the indicated layers.
	/// </summary>
	/// <param name="layer">The LayerMask to check.</param>
	public static bool Contains (this LayerMask source, LayerMask layers) {
		return (source ^ layers) == 0;
	}

	/// <summary>
	/// Converts a layer name into a LayerMask object for raycasting.
	/// </summary>
	/// <returns>A LayerMask object for the given layer name.</returns>
	/// <param name="layerName">Layer name.</param>
	public static LayerMask LayerNameToMask (string layerName) {
		return 1 << LayerMask.NameToLayer(layerName);	
	}
	
	/// <summary>
	/// Converts a layer  into a LayerMask object.
	/// </summary>
	/// <returns>A LayerMask object for the given layer.</returns>
	/// <param name="layerName">Layer name.</param>
	public static LayerMask LayerToMask (int layer) {
		return 1 << layer;	
	}

	/// <summary>
	/// Returns an array of the names of the layers in the LayerMask.
	/// TODO -- implement
	/// </summary>
	/// <returns>The mask to strings.</returns>
	/// <param name="mask">Mask.</param>
	public static string[] LayerMaskToStrings (LayerMask mask) {
		return new string[0];
	}

	/// <summary>
	/// Returns the RaycastHit result of the provided screen point (in pixels)
	/// indicating if the point is over an object.
	/// </summary>
	/// <returns><c>true</c>, if screen point over an object, <c>false</c> otherwise.</returns>
	/// <param name="position">Screen point.</param>
	/// <param name="hitInfo">A RaycastHit object for information on whatever is under the point.</param>
	public static bool GetScreenPointOverObject (Vector3 position, out RaycastHit hitInfo) {
		return Physics.Raycast (Camera.main.ScreenPointToRay(position), out hitInfo);
	}

	/// <summary>
	/// Returns the RaycastHit result of the provide screen point (in pixels)
	/// indicating if the point is over an object on the indicated layer.
	/// </summary>
	/// <returns><c>true</c>, if screen point over an object on the layer, <c>false</c> otherwise.</returns>
	/// <param name="position">Screen point.</param>
	/// <param name="hitInfo">A RaycastHit object for information on whatever is under the point.</param>
	/// <param name="layerName">Layer name.</param>
	public static bool GetScreenPointOverObject (Vector3 position, out RaycastHit hitInfo, string layerName) {
		return Physics.Raycast (Camera.main.ScreenPointToRay(position), 
		                        out hitInfo, Mathf.Infinity, LayerNameToMask(layerName));
	}

	/// <summary>
	/// Returns the RaycastHit result of the provide screen point (in pixels)
	/// indicating if the point is over an object on the indicated layer.
	/// </summary>
	/// <returns><c>true</c>, if screen point over an object on the layer, <c>false</c> otherwise.</returns>
	/// <param name="position">Screen point.</param>
	/// <param name="hitInfo">A RaycastHit object for information on whatever is under the point.</param>
	/// <param name="layerNumber">Layer number.</param>
	public static bool GetScreenPointOverObject (Vector3 position, out RaycastHit hitInfo, int layerNumber) {
		return GetScreenPointOverObject (position, out hitInfo, LayerMask.LayerToName(layerNumber));
	}

	/// <summary>
	/// Returns information about the mouse position that indicates if it is over an object.
	/// <returns><c>true</c>, if mouse is over an object on the layer, <c>false</c> otherwise.</returns>
	/// <param name="hitInfo">A RaycastHit object for information on whatever is under the mouse.</param>
	/// <param name="layerNumber">Layer number.</param>
	public static bool GetMouseOverObject (out RaycastHit hitInfo) {
		return GetScreenPointOverObject (Input.mousePosition, out hitInfo);
	}
	
	/// <summary>
	/// Returns information about the mouse position that indicates if it is over an object on
	/// the indicated layer.
	/// <returns><c>true</c>, if mouse is over an object on the layer, <c>false</c> otherwise.</returns>
	/// <param name="hitInfo">A RaycastHit object for information on whatever is under the mouse.</param>
	/// <param name="layerName">Layer name.</param>
	public static bool GetMouseOverObject (out RaycastHit hitInfo, string layerName) {
		return GetScreenPointOverObject (Input.mousePosition, out hitInfo, layerName);
	}
	
	/// <summary>
	/// Returns information about the mouse position that indicates if it is over an object on
	/// the indicated layer.
	/// <returns><c>true</c>, if mouse is over an object on the layer, <c>false</c> otherwise.</returns>
	/// <param name="hitInfo">A RaycastHit object for information on whatever is under the mouse.</param>
	/// <param name="layerNumber">Layer number.</param>
	public static bool GetMouseOverObject (out RaycastHit hitInfo, int layerNumber) {
		return GetScreenPointOverObject (Input.mousePosition, out hitInfo, layerNumber);
	}

	/// <summary>
	/// Returns information about the screen point that indicates if the point is over an object
	/// with the provided tag.
	/// <returns><c>true</c>, if point is over an object with the tag, <c>false</c> otherwise.</returns>
	/// <param name="position">Screen point.</param>
	/// <param name="hitInfo">A RaycastHit object for information on whatever is under the point.</param>
	/// <param name="tag">Tag.</param>
	public static bool GetScreenPointOverTaggedObject (Vector3 position, out RaycastHit hitInfo, string tag) {
		if (GetScreenPointOverObject(position, out hitInfo)) {
			//	hit *something*
			return hitInfo.transform.tag.Equals(tag);
		}
		return false;
	}

	/// <summary>
	/// Returns information about the mouse that indicates if it is over an object with the provided tag.
	/// <returns><c>true</c>, if mouse is over an object with the tag, <c>false</c> otherwise.</returns>
	/// <param name="hitInfo">A RaycastHit object for information on whatever is under the mouse.</param>
	/// <param name="tag">Tag.</param>
	public static bool GetMouseOverTaggedObject (out RaycastHit hitInfo, string tag) {
		return GetScreenPointOverTaggedObject(Input.mousePosition, out hitInfo, tag);
	}

	#endregion

	//////////////
	//	Keys	//
	//////////////

	#region Keys
		
	private static Dictionary<string, KeyCode>	KeyDict	=	new Dictionary<string, KeyCode>();
	
	/// <summary>
	/// Define an input key. Used as DefineKey ("Eject", KeyCode.E)
	/// Use GetKey("Eject"), GetKeyDown("Eject") and GetKeyUp("Eject") to check for user input events.
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="key">Key.</param>
	public static void DefineKey (string name, KeyCode key) {
		KeyDict.Add(name, key);
	}
	
	/// <summary>
	/// Retrieves the KeyCode for the name. If the named key has not been defined then this returns null.
	///	Named keys are defined using DefineKey(string, KeyCode), e.g., DefineKey ("Turbo", KeyCode.T)
	/// </summary>
	/// <returns>The KeyCode.</returns>
	/// <param name="name">Name.</param>
	public static bool RetrieveKey (string name, out KeyCode key) {
		return KeyDict.TryGetValue(name, out key);
	}

	#region Key Input

	/// <summary>
	/// Returns true if the named key was just pushed down.
	/// </summary>
	/// <returns><c>true</c>, if key was found, and it was just pushed down, 
	/// <c>false</c> if the named key was not found, or it was found but it wasn't just pushed down.</returns>
	/// <param name="name">Name.</param>
	public static bool GetKeyDown (string name) {
		KeyCode	key;
		
		if (KeyDict.TryGetValue(name, out key)) {
			return key.Pressed();
		} else {
			return false;
		}
	}
	
	/// <summary>
	/// Returns true if the named key was just released.
	/// </summary>
	/// <returns><c>true</c>, if key was found, and it was just released, 
	/// <c>false</c> if the named key was not found, or it was found but it wasn't just released.</returns>
	/// <param name="name">Name.</param>
	public static bool GetKeyUp (string name) {
		KeyCode	key;
		
		if (KeyDict.TryGetValue(name, out key)) {
			return key.Released();
		} else {
			return false;
		}
	}
	
	/// <summary>
	/// Returns true if the named key is currently down.
	/// </summary>
	/// <returns><c>true</c>, if key was found, and is currently down, 
	/// <c>false</c> if the named key was not found, or it was found but it's not down.</returns>
	/// <param name="name">Name.</param>
	public static bool GetKey (string name) {
		KeyCode	key;
		
		if (KeyDict.TryGetValue(name, out key)) {
			return key.Hold();
		} else {
			return false;
		}
	}

	#endregion

	#region Key Extensions

	/// <summary>
	/// Usage: if (KeyCode.E.Pressed()) { ... }
	/// </summary>
	/// <param name="key">Key.</param>
	public static bool Pressed (this KeyCode key) {
		return Input.GetKeyDown(key);
	}
	
	/// <summary>
	/// Usage: if (KeyCode.E.Up()) { ... }
	/// </summary>
	/// <param name="key">Key.</param>
	public static bool Released (this KeyCode key) {
		return Input.GetKeyUp(key);
	}
	
	/// <summary>
	/// Usage: if (KeyCode.E.Hold()) { ... }
	/// </summary>
	/// <param name="key">Key.</param>
	public static bool Hold (this KeyCode key) {
		return Input.GetKey(key);
	}

	#endregion

	#endregion

	//////////////////////////
	//	Key Combinations	//
	//////////////////////////

	#region Key Combinations
	
	private static Dictionary<string, KeyCombo>	KeyComboDict	=	new Dictionary<string, KeyCombo>();
	
	/// <summary>
	/// Define an input key combination.
	/// Used as DefineKeyCombo ("Eject", KeyCode.E, new KeyCode[] {KeyCode.LeftControl, KeyCode.LeftShift})
	/// Use GetKeyCombo("Eject"), GetKeyComboDown("Eject") and GetKeyComboUp("Eject")
	/// to check for user input events.
	/// </summary>
	/// <param name="name">Combination Name.</param>
	/// <param name="triggerKey">The final key that is pressed to trigger the event, E in the example.</param>
	/// <param name="prerequisiteKeys">The array of initial keys that must be held down prior to
	/// pressing the TriggerKey, CONTROL and SHIFT in the example.</param>
	public static void DefineKeyCombo (string name, KeyCode triggerKey, KeyCode[] prerequisiteKeys) {
		KeyCombo keyCombo = new KeyCombo();
		keyCombo.triggerKey = triggerKey;
		keyCombo.secondaryKeys = prerequisiteKeys;
		KeyComboDict.Add(name, keyCombo);
	}
	
	/// <summary>
	/// Retrieves the KeyCombo for the name. If the named key combination has not been defined
	/// then this returns null. Named key combinations are defined using 
	/// DefineKeyCombo(string, KeyCode[]), e.g., DefineKey ("Reset", new {KeyCode.CONTROL, KeyCode.R})
	/// </summary>
	/// <returns><c>true</c>, if screen point over an object, <c>false</c> otherwise.</returns>
	/// <param name="name">The Name of the key combination.</param>
	/// <param name="hitInfo">The requested KeyCombo object (if it was found).</param>
	public static bool RetrieveKeyCombo (string name, out KeyCombo keyCombo) {
		return KeyComboDict.TryGetValue(name, out keyCombo);
	}

	/// <summary>
	/// Returns a string containing the keycombo, e.g., CTRL-F5
	/// </summary>
	/// <returns>The KeyCombo as a string.</returns>
	public static string AsString (this KeyCombo source) {
		string s = "";
		for (int i = 0 ; i < source.secondaryKeys.Length  ; i++) {
			if (i > 0) s += "-";
			s += source.secondaryKeys[i].ToString();
		}
		s += " + " + source.triggerKey.ToString();
		return s;
	}

	/// <summary>
	/// Returns true if the all the keys in an array of keys are down.
	/// </summary>
	/// <returns><c>true</c>, if all the requisite keys are down, 
	/// <c>false</c> otherwise.</returns>
	/// <param name="keys">An array of KeyCodes.</param>
	private static bool AllKeysDown (KeyCode[] keys) {
		bool requiredKeys = true;
		foreach (KeyCode k in keys) {
			requiredKeys &= (k.Hold() || k.Pressed());
		}
		return requiredKeys;
	}

	#region CombinationKey Input

	/// <summary>
	/// Returns true if the last key of the named key combination was just pushed down.
	/// </summary>
	/// <returns><c>true</c>, if key combination was found, 
	/// AND all the requisite keys are down, 
	/// AND trigger key of the combination was just pushed down, 
	/// <c>false</c> otherwise.</returns>
	/// <param name="name">Key Combination Name.</param>
	public static bool KeyComboDown (string name) {
		KeyCombo keyCombo;
		if (RetrieveKeyCombo(name, out keyCombo)) {
			return keyCombo.Pressed();
		}
		return false;
	}
	
	/// <summary>
	/// Returns true if the last key of the named key combination was just released,
	/// BUT the other keys are still down.
	/// </summary>
	/// <returns><c>true</c>, if key combination was found, 
	/// AND all the requisite keys are down, 
	/// AND trigger key of the combination was just released, 
	/// <c>false</c> otherwise.</returns>
	/// <param name="name">Key Combination Name.</param>
	public static bool KeyComboUp (string name) {
		KeyCombo keyCombo;
		if (RetrieveKeyCombo(name, out keyCombo)) {
			return keyCombo.Released();
		}
		return false;
	}
	
	/// <summary>
	/// Returns true if the all the keys of the named key combination are down.
	/// </summary>
	/// <returns><c>true</c>, if key combination was found, 
	/// AND all the requisite keys are down, 
	/// AND trigger key of the combination was just released, 
	/// <c>false</c> otherwise.</returns>
	/// <param name="name">Key Combination Name.</param>
	public static bool KeyComboHold (string name) {
		KeyCombo keyCombo;
		if (RetrieveKeyCombo(name, out keyCombo)) {
			return keyCombo.Hold();
		}
		return false;
	}

	#endregion
	
	#region Extensions for CombinationKey for Up, Down, Hold
	
	/// <summary>
	/// Returns true if the last key of the key combination was just pushed down.
	/// </summary>
	/// <returns><c>true</c>, if all the requisite keys are down, 
	/// AND trigger key of the combination was just pushed down, 
	/// <c>false</c> otherwise.</returns>
	public static bool Pressed (this KeyCombo keyCombo) {
		return AllKeysDown(keyCombo.secondaryKeys) && keyCombo.triggerKey.Pressed();
	}
	
	/// <summary>
	/// Returns true if the last key of the key combination was just released,
	/// BUT the other keys are still down.
	/// </summary>
	/// <returns><c>true</c>, if all the requisite keys are down, 
	/// AND trigger key of the combination was just released, 
	/// <c>false</c> otherwise.</returns>
	public static bool Released (this KeyCombo keyCombo) {
		return AllKeysDown(keyCombo.secondaryKeys) && keyCombo.triggerKey.Released();
	}
	
	/// <summary>
	/// Returns true if the all the keys of the key combination are down.
	/// </summary>
	/// <returns><c>true</c>, if all the requisite keys are down, 
	/// AND trigger key of the combination was just released, 
	/// <c>false</c> otherwise.</returns>
	public static bool Hold (this KeyCombo keyCombo) {
		return AllKeysDown(keyCombo.secondaryKeys) && keyCombo.triggerKey.Hold();
	}

	#endregion

	#endregion

	
	/// <summary>
	/// A key combination. Multi-key input, such as CONTROL-SHIFT-F5.
	/// SecondaryKeys represents the initial keys that must be held down, i.e., CONTROL and SHIFT.
	/// Trigger Key is the final key that triggers the event, i.e., F5.
	/// </summary>
	[System.Serializable]
	public struct KeyCombo {
		public	KeyCode		triggerKey;
		public	KeyCode[]	secondaryKeys;
	}
	
	//////////////////////
	//	Interfaces		//
	//////////////////////

	#region Inputable
	
	public static class Inputable {
		public const string SET_INPUT = "OnInputEnabled"; 
		public const string SET_MOUSE_INPUT = "OnMouseEnabled"; 
		public const string SET_KEYBOARD_INPUT = "OnKeyboardEnabled";
		
		public interface IInputable {
			void OnInputEnabled (bool enable);
		}
		public interface IDeviceInputable {
			void OnMouseEnabled (bool enable);
			void OnKeyboardEnabled (bool enable);
		}
	}
	
	#endregion
}
}