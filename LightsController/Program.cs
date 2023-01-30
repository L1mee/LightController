// ReSharper disable RedundantUsingDirective
using System.Drawing;

namespace LightController;

internal class Program
{
    private static void Main()
    {
        var c = Controller.Instance;

        SetChannelExample();

        c.Run(SendThrough.Dmx, new byte[] { 1, 2 }, 200);

        Console.ReadKey();

        c.Quit(OnQuit.TurnOff);
    }

    private static void SetChannelExample()
    {
        var stageLight = new Fixture(1, 1, new[] { "Dimmer", "Red", "Green", "Blue", "Strobe", "Effect", "EffectSpeed" });
        var movingHead = new Fixture(2, 8, new[] { "PanRun", "PanFineTune", "TiltRun", "TiltFineTune", "Color", "Gobo", "Strobe", "Dimmer", "RunSpeed", "AutoMode", "Extra" });

        stageLight.SetDimmer(60);
        stageLight.SetColor(Color.DarkOrange);

        movingHead.SetAttribute("Dimmer", 30);
        movingHead.SetOrientation(45, 45);
    }
}