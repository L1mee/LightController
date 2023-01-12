namespace LightsController;

public interface ISendMode
{
    //Type T instead of byte[] to enable ArtNet byte[][]
    public void Send(byte[] payload);

    public void Quit();
}