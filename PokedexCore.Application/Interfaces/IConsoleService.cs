namespace PokedexCore.Application.Interfaces
{
    public interface IConsoleService
    {
        Task ShowSpinnerAsync(string message, CancellationToken cancellationToken);
        void Write(string message);
        void WriteLine(string message = "");
    }
}