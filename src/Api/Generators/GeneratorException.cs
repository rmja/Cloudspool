using System;

namespace Api.Generators
{
    public class GeneratorException : Exception
    {
        public GeneratorException(string message) : base(message)
        {
        }
    }
}
