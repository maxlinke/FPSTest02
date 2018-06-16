using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerPrefKeybindObserver : IPlayerPrefObserver{

	void NotifyKeybindsChanged();

}
