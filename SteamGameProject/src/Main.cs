namespace SteamGameProject;

using Godot;
#if DEBUG
using System.Reflection;
using GoDotTest;
#endif

public partial class Main : Node2D {
  public TestEnvironment Environment = default!;

  public override void _Ready() {
#if DEBUG
    // Let GoDotTest run tests if it thinks we should. It checks for the
    // correct command line arguments and constructs a test environment.
    Environment = TestEnvironment.From(OS.GetCmdlineArgs());
    if (Environment.ShouldRunTests) {
      CallDeferred("RunTests");
      return;
    }
#endif
    // If we don't need to run tests, we can just switch to the game scene.
    GetTree().ChangeSceneToFile("res://src/Game.tscn");
  }

  private void RunTests()
    => GoTest.RunTests(Assembly.GetExecutingAssembly(), this, Environment);
}
