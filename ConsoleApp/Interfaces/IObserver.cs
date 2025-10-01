namespace ConsoleApp.Interfaces;

public interface IObserver
{
    Task Update(ISubject subject);
}