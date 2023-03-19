namespace SteamGameProject;

using Godot;
using GoDotTest;
using Shouldly;

public class GameTest : TestClass {
  public GameTest(Node testScene) : base(testScene) { }

  [Test]
  public void TwoPlusTwo() => (2 + 2).ShouldBe(4);
}
