using System;

namespace Api.Generators.JavaScript.ChakraCore
{
    public static class ChakraCoreSettings
    {
        // https://github.com/Taritsyn/JavaScriptEngineSwitcher/blob/master/src/JavaScriptEngineSwitcher.ChakraCore/ChakraCoreSettings.cs

        private const int MaxStackSize32 = 492 * 1024; // like 32-bit Node.js
        private const int MaxStackSize64 = 984 * 1024; // like 64-bit Node.js

        public static int MaxStackSize { get; set; } = Environment.Is64BitProcess ? MaxStackSize64 : MaxStackSize32;
        public static UIntPtr MemoryLimit { get; set; } = Environment.Is64BitProcess ? new UIntPtr(ulong.MaxValue) : new UIntPtr(uint.MaxValue);
    }
}
