﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * The dialog for displaying/selecting the list of Cast devices.
 */
public class CastListDialog : MonoBehaviour {

  private const int SEARCH_TIMEOUT_SECONDS = 5;
  private delegate void Action();

  /**
   * Prefab for the cast list elements.
   */
  [Tooltip("Default: CastListElementPrefab")]
  public GameObject listElementPrefab;

  /**
   * GameObject that contains the "searching for cast devices" state for the dialog.
   */
  [Tooltip("Default: CastListDialog->SearchingElements")]
  public GameObject searchingElements;

  /**
   * GameObject that contains the "list not found" state for the dialog.
   */
  [Tooltip("Default: CastListDialog->ListNotFoundElements")]
  public GameObject listNotFoundElements;

  /**
   * GameObject that contains the "list found" state for the dialog.
   */
  [Tooltip("Default: CastListDialog->ListFoundElements")]
  public GameObject listFoundElements;

  /**
   * GameObject that contains (and formats) the list of cast buttons.
   */
  [Tooltip("Default: CastListDialog->ListFoundElements->ScrollView->ContentPanel")]
  public GameObject contentPanel;

  /**
   * The callback for closing the cast list.
   */
  public UICallback closeButtonTappedCallback;

  /**
   * Currently displayed list of buttons - one for each cast device.
   */
  private List<GameObject> currentButtons = new List<GameObject>();

  /**
   * A reference to the cast button for animating the icon.
   */
  private CastButtonFrame castButtonFrame;

  /**
   * Sets the CastButtonFrame reference.
   */
  public void SetCastButtonFrame(CastButtonFrame frame) {
    castButtonFrame = frame;
  }

  /**
   * Shows the list of cast devices, or shows the "searching for cast devices" state.
   */
  public void Show() {
    gameObject.SetActive(true);
    if (currentButtons.Count == 0) {
      searchingElements.SetActive(true);
      listNotFoundElements.SetActive(false);
      listFoundElements.SetActive(false);
      StartCoroutine(ShowNotFoundCoroutine());
    } else {
      searchingElements.SetActive(false);
      listNotFoundElements.SetActive(false);
      listFoundElements.SetActive(true);
    }
  }

  private IEnumerator ShowNotFoundCoroutine() {
    float startTime = Time.realtimeSinceStartup;
    while (Time.realtimeSinceStartup < startTime + SEARCH_TIMEOUT_SECONDS) {
      yield return null;
    }
    if (currentButtons.Count == 0) {
      ShowNotFoundState();
    }
  }

  /**
   * Hides the list of cast devices.
   */
  public void Hide() {
    gameObject.SetActive(false);
  }

  /**
   * Shows the dialog when no devices have been found.
   */
  private void ShowNotFoundState() {
    if (gameObject.activeInHierarchy && currentButtons.Count == 0) {
      searchingElements.SetActive(false);
      listNotFoundElements.SetActive(true);
      listFoundElements.SetActive(false);
    }
  }

  /**
   * Populates the list of casts with devices, and sets up callbacks
   * for selecting said devices.
   */
  public void PopulateList(CastRemoteDisplayManager manager) {
    foreach (var button in currentButtons) {
      Destroy(button);
    }
    currentButtons.Clear();

    List<CastDevice> devices = manager.GetCastDevices();
    foreach (CastDevice listDevice in devices) {
      GameObject newButton = Instantiate(listElementPrefab) as GameObject;
      CastListButton button = newButton.GetComponent<CastListButton>();
      button.nameLabel.text = listDevice.deviceName;
      button.statusLabel.text = listDevice.status;
      string deviceId = listDevice.deviceId;
      button.button.onClick.AddListener(() => {
        manager.SelectCastDevice(deviceId);
        castButtonFrame.ShowConnecting();
      });
      newButton.transform.SetParent(contentPanel.transform, false);
      currentButtons.Add(newButton);
    }

    searchingElements.SetActive(false);
    listNotFoundElements.SetActive(false);
    listFoundElements.SetActive(true);
  }

  /**
   * Triggers the callback for closing the cast list.  Set as the OnClick function for
   * CloseButton.
   */
  public void OnCloseButtonTapped() {
    closeButtonTappedCallback();
  }
}
