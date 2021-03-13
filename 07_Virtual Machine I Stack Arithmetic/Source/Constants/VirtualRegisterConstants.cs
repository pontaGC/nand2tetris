using System;
namespace VMTranslator
{
    //
    // Symbols defined by Hack machine language
    //

    public struct VirtualRegisterConstants
    {
        public static readonly string SP = @"SP";         // Stack pointer (Hack: RAM[0])
        public static readonly string Local = @"LCL";     // Base address of local segment (Hack: RAM[1])
        public static readonly string Argument = @"ARG";  // Base address of argument segment (Hack: RAM[2])
        public static readonly string This = @"THIS";     // Base address of this segment (Hack: RAM[3])
        public static readonly string That = @"THAT";     // Base address of this segment (Hack: RAM[4])

        // general register symbols
        public static readonly string GeneralRegister1 = @"R13";
        public static readonly string GeneralRegister2 = @"R14";
        public static readonly string GeneralRegister3 = @"R15";

        // pointer & temp base address
        public const int PointerBaseAddress = 3;
        public const int TempBaseAddress = 5;
    }
}
