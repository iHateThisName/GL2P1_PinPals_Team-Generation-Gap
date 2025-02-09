using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
// Ivar
public class PlayerManager : MonoBehaviour {

    [SerializeField] private Transform _spawnPosition;
    public Dictionary<EnumPlayerTag, GameObject> Players { get; private set; } = new Dictionary<EnumPlayerTag, GameObject>();

    public void OnPlayerJoined(PlayerInput playerInput) {
        PlayerController playerController = playerInput.gameObject.GetComponent<PlayerController>();
        playerController.DisableGravity();
        playerInput.gameObject.transform.parent.transform.position = _spawnPosition.position;

        // Create a enum that represent the player tag
        EnumPlayerTag playerTag = (EnumPlayerTag)playerInput.playerIndex + 1; // PlayerIndex starts at 0

        // Adds the player to the dictonary also checks if player already exists in the dictionary.
        if (!Players.ContainsKey(playerTag)) {
            Players.Add(playerTag, playerInput.gameObject);
        } else if (Players.ContainsKey(playerTag) && Players[playerTag].gameObject != playerInput.gameObject) {
            playerTag = GetUnusedPlayerTag((int)playerTag);
            Players[playerTag] = playerInput.gameObject;
        }

        // Tag the player with the player tag
        playerInput.gameObject.transform.GetChild(0).tag = playerTag.ToString();
        Debug.Log(playerTag.ToString() + " joined the game");

        // Layer the player to the correct player layer
        playerInput.gameObject.layer = LayerMask.NameToLayer("Player" + (int)playerTag);
        foreach (Transform child in playerInput.gameObject.transform) {
            child.gameObject.layer = LayerMask.NameToLayer("Player" + (int)playerTag);
        }

        // Enable gravity after 1 second
        //ReEnableGravity(playerController, 1);
        StartCoroutine(ReEnableGravityCoroutine(playerController, 0.5f));

        // The player should not be destroyed when a new scene is loaded, to keep the player controller scheme
        //DontDestroyOnLoad(playerInput.gameObject.transform.parent.gameObject);
        //playerInput.gameObject.transform.parent.gameObject.name = playerTag.ToString();

        Camera playerCamera = playerInput.camera;

        // Get the default culling mask of the camera
        int defaultCullingMask = playerCamera.cullingMask;

        // Combine the default culling mask with the current layer + 4(Number of max players) to get to the flipper layers
        int currentLayer = 5 + (int)playerTag;
        playerCamera.cullingMask = defaultCullingMask | 1 << currentLayer + 4;  // Defaultmasks + flippers
    }

    public void OnPlayerLeaves(PlayerInput playerInput) {
        EnumPlayerTag playerTag = (EnumPlayerTag)playerInput.playerIndex + 1; // PlayerIndex starts at 0
        Players.Remove(playerTag);
        Debug.Log(playerTag.ToString() + " left the game");
    }

    private async void ReEnableGravity(PlayerController playerController, int delayInSeconds) {
        await Task.Delay(delayInSeconds * 1000);
        playerController.EnableGravity();
    }

    private IEnumerator ReEnableGravityCoroutine(PlayerController playerController, float delayInSeconds) {
        yield return new WaitForSeconds(delayInSeconds);
        playerController.EnableGravity();
    }

    /// <summary>
    /// Finds an unused player tag starting from a given index.
    /// </summary>
    /// <param name="startIndex">The index to start searching from. Defaults to 1.</param>
    /// <returns>The first unused player tag found. If all player tags are used, returns EnumPlayerTag.Player16 and logs an error.</returns>
    /// <remarks>
    /// This method first searches for an unused player tag starting from the given index up to 16.
    /// If no unused tag is found, it searches again from 1 to 16. If still no unused tag is found,
    /// it logs an error indicating that more than 16 players have joined the game, which is not supported.
    /// </remarks>
    private EnumPlayerTag GetUnusedPlayerTag(int startIndex = 1) {
        for (int i = startIndex; i <= 16; i++) {
            if (!Players.ContainsKey((EnumPlayerTag)i)) {
                return (EnumPlayerTag)i;
            }
        }

        for (int i = 1; i <= 16; i++) {
            if (!Players.ContainsKey((EnumPlayerTag)i)) {
                return (EnumPlayerTag)i;
            }
        }
        Debug.LogError("More that 16 players joined the game. This is not supported.");
        return EnumPlayerTag.Player16;
    }
    //Einar
    //void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    // Change the position of this object after the scene reloads
    //    transform.position = _spawnPosition.position;
    //}
}
