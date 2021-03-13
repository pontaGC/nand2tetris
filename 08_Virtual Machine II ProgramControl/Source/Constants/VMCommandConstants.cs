namespace VMTranslation
{
    public struct VMCommandConstants
    {

        //
        // Arithmetic commands
        //

        public const string Add = @"add";           // integer addition (two's complement)
        public const string Subtruction = @"sub";   // integer subtraction (two's complement)
        public const string Negation = @"neg";      // negation (two's complement)
        public const string Equality = @"eq";       // equality
        public const string Greater = @"gt";        // greater than (x > y is true)
        public const string Less = @"lt";           // less than (x < y is true)
        public const string And = @"and";           // bitwise
        public const string Or = @"or";             // bitwise
        public const string Not = @"not";           // bitwise
        
        //
        // Memory access commands
        //

        public const string Push = @"push";         // stack operation
        public const string Pop = @"pop";           // stack operation
        public const string Label = @"label";       // symbol label
        public const string Goto = @"goto";         // goto instruction
        public const string If = @"if-goto";        // if instruction
        public const string Function = @"function"; // function
        public const string Return = @"return";     // return
        public const string Call = @"call";         // call subroutines/functions
    }    

}    
