using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent
{
    public class ScopedConsoleColour : IDisposable
    {
        private ConsoleColor oldColour;

        public ScopedConsoleColour(ConsoleColor newColour)
        {
            this.oldColour = Console.ForegroundColor;

            Console.ForegroundColor = newColour;
        }

        public void Dispose()
        {
            Console.ForegroundColor = this.oldColour;
        }
    }
}
