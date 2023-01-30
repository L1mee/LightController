namespace LightController;

public interface ISender
{
    public void SetUniverseOut(IEnumerable<byte> universes);

    public bool Start();

    public void Send(Data payload);

    public void Quit();
}