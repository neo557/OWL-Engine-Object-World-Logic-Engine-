using DOESUE.Math;

namespace OWL_Engine.InputMove
{
    public class InputHelper
    {
        public static IntVector3 ReadDirection()
        {
            var key = Console.ReadKey(true).Key;

            return key switch
            {
                ConsoleKey.UpArrow => new IntVector3(0, 0, 1),
                ConsoleKey.DownArrow => new IntVector3(0, 0, -1),
                ConsoleKey.LeftArrow => new IntVector3(-1, 0, 0),
                ConsoleKey.RightArrow => new IntVector3(1, 0, 0),
                _ => new IntVector3(0, 0, 0)
            };
        }
    }
}
