namespace TVC.ImageServer.ImageManipulation.Operations
{
    public interface IPipeOperation<T>
    {
        T Process(T data);
    }
}
