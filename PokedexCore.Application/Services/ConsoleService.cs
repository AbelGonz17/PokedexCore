using PokedexCore.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Application.Services
{
    public class ConsoleService : IConsoleService
    {
        private readonly char[] _spinnerChars = new[] { '|', '/', '-', '\\' };
        private readonly int _spinnerDelay = 180;

        public async Task ShowSpinnerAsync(string message, CancellationToken cancellationToken)
        {
            try
            {
                var i = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.Write($"\r{message} {_spinnerChars[i++ % _spinnerChars.Length]}");
                    await Task.Delay(_spinnerDelay, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                // Ignorar cancelación esperada
            }
            finally
            {
                Console.WriteLine(); // salto de línea al terminar
            }
        }

        public void WriteLine(string message = "")
        {
            Console.WriteLine(message);
        }

        public void Write(string message)
        {
            Console.Write(message);
        }
    }
}

   