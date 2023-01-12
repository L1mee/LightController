namespace LightsController;

public class ArtNet : ISendMode, ISender
{
    public void Send(byte[] data)
    {
        //make ArtNet Packet and call Send via IO
        throw new NotImplementedException();
    }

    public void Quit()
    {
        //close ArtNet IO
        throw new NotImplementedException();
    }
}