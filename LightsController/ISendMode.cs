namespace LightsController;

public interface ISendMode<T>
{
    public void Send(T payload);

    public void Quit();
}