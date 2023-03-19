/*
Credits:
SteamManager.cs: https://tinyurl.com/4625esvf
Steamworks Docs: https://steamworks.github.io/gettingstarted/
Minimal lobby example: https://tinyurl.com/yc36h7s4
Steamworks API Reference: https://partner.steamgames.com/doc/api
*/

namespace SteamGameProject;
using System;
using Godot;
using Steamworks;

/// <summary>
/// A domain layer class that wraps Steamworks.NET. Use this instead of
/// directly interfacing with Steamworks.NET, since this provides higher level
/// commands that are more convenient to use from game logic.
/// </summary>
public class SteamManager {
  /// <summary>Steam initialization result.</summary>
  public enum InitializationResult {
    /// <summary>Steam initialization succeeded!</summary>
    Success,
    /// <summary>Steam initialization failed.</summary>
    Failure,
    /// <summary>
    /// Application must be exited so it can be restarted through Steam.
    /// </summary>
    MustRestart,
  }

  /// <summary>
  /// Steam warning message hook severity. When running through a debugger,
  /// only warnings will be sent. If you add -debug_steamapi to the
  /// command-line, then informational messages will also be sent.
  /// </summary>
  public enum WarningMessageSeverity {
    /// <summary>Normal message.</summary>
    Message = 0,
    /// <summary>Warning message.</summary>
    Warning = 1
  }

  /// <summary>
  /// True if the Steamworks API has been setup and connected to Steam.
  /// </summary>
  public bool IsActive { get; private set; }

  /// <summary>Steam AppId.</summary>
  public AppId_t AppId { get; }

  #region Callbacks
  private SteamAPIWarningMessageHook_t _warningMessageCb = null!;
  #endregion Callbacks

  /// <summary>
  /// Creates a new SteamManager instance.
  /// </summary>
  /// <param name="appId">Steam AppId.</param>
  public SteamManager(uint appId) {
    AppId = new AppId_t(appId);
  }

  /// <summary>
  /// Attempts to initialize Steamworks.
  /// </summary>
  /// <returns>True if Steamworks was successfully initialized.</returns>
  public InitializationResult Initialize() {
    if (IsActive) {
      GD.PushWarning("Steamworks is already initialized. Can't initialize again.");
      return InitializationResult.Failure;
    }

    if (!Packsize.Test()) {
      GD.PushError(
        "Packsize test failed. The wrong version of Steamworks.NET is " +
        "being run on this platform."
      );
    }

    if (!DllCheck.Test()) {
      GD.PushError(
        "DllCheck Test returned false, One or more of the Steamworks " +
        "binaries seems to be the wrong version."
      );
    }

    try {
      // If Steam is not running or the game wasn't started through Steam,
      // SteamAPI_RestartAppIfNecessary starts the Steam client and also
      // launches this game again if the User owns it. This can act as a
      // rudimentary form of DRM.
      if (SteamAPI.RestartAppIfNecessary(new AppId_t(480))) {
        return InitializationResult.MustRestart;
      }
    }
    catch (DllNotFoundException e) {
      // We catch this exception here, as it will be the first occurrence.
      GD.PushError("Cannot find Steamworks.NET DLL. Please reinstall the game.");
      GD.PushError(e.ToString());
      return InitializationResult.Failure;
    }

    // Initializes the Steamworks API.
    // If this returns false, this indicates one of the following conditions:
    // [*] The Steam client isn't running. A running Steam client is required
    //     to provide implementations of the various Steamworks interfaces.
    // [*] The Steam client couldn't determine the App ID of game. If you're
    //     running your application from the executable or debugger directly
    //     then you must have a steam_appid.txt in your game directory next
    //     to the executable, with your app ID in it and nothing else. Steam
    //     will look for this file in the current working directory. If you
    //     are running your executable from a different directory you may
    //     need to relocate the steam_appid.txt file.
    // [*] Your application is not running under the same OS user context as
    //     the Steam client, such as a different user or administration access
    //     level.
    // [*] Ensure that you own a license for the App ID on the currently
    //     active Steam account. Your game must show up in your Steam library.
    // [*] Your App ID is not completely set up, i.e. in Release State:
    //     Unavailable, or it's missing default packages.
    // Valve's documentation for this is located here:
    // https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
    try {
      GD.Print("Attempting to initialize Steamworks.");
      IsActive = SteamAPI.Init();
      if (IsActive) {
        GD.Print("Steamworks initialization succeeded.");
        // Initialize health callbacks.
        _warningMessageCb = new SteamAPIWarningMessageHook_t(
          OnWarningMessage
        );
        SteamClient.SetWarningMessageHook(_warningMessageCb);
      }
      else {
        GD.PushError("Steamworks initialization failed!");
      }
    }
    catch (Exception e) {
      GD.PushError("Steamworks initialization threw an exception.");
      GD.PushError(e.ToString());
    }

    return IsActive ? InitializationResult.Success
      : InitializationResult.Failure;
  }

  /// <summary>
  /// Attempts to shutdown Steamworks.
  /// </summary>
  /// <returns>True if Steamworks was successfully shutdown.</returns>
  public bool Shutdown() {
    if (!IsActive) {
      GD.PushWarning("Steamworks is not initialized. Can't shutdown.");
      return false;
    }

    try {
      GD.Print("Shutting down steam...");
      SteamAPI.Shutdown();
      GD.Print("Steamworks shutdown succeeded.");
      IsActive = false;
    }
    catch (Exception e) {
      GD.Print("Steamworks shutdown threw an exception.");
      GD.Print(e.ToString());
    }

    return !IsActive;
  }

  /// <summary>
  /// Runs steam callbacks. This should be called every frame from a
  /// node's _Process method.
  /// </summary>
  public bool Update() {
    if (!IsActive) { return false; }
    SteamAPI.RunCallbacks();
    return true;
  }

  #region Steam Callback Methods

  /// <summary>
  /// Called when Steamworks.NET receives a warning message from Steam.
  /// A callback will occur directly after the API function is called that
  /// generated the warning or message.
  /// https://partner.steamgames.com/doc/api/ISteamUtils#SetWarningMessageHook
  /// </summary>
  /// <param name="severity"></param>
  /// <param name="debugText"></param>
  private void OnWarningMessage(
    int severity, System.Text.StringBuilder debugText
  ) {
    var message = debugText.ToString();
    if ((WarningMessageSeverity)severity == WarningMessageSeverity.Warning) {
      GD.PushWarning(message);
    }
    else {
      GD.Print(message);
    }
  }

  #endregion Steam Callback Methods
}
