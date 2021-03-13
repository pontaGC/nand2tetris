namespace JackCompiler.Interfaces
{
    public interface ICompilationEngine
    {
        void CompileClass();

        void CompileClassVarDec();

        void CompileSubroutine();

        void CompileParameterList();

        void CompileVarDec();

        void CompileStatements();

        void CompileDo();

        void CompileWhile();

        void CompileReturn();

        void CompileIf();

        void CompileExpression();

        void CompileTerm();

        void CompileExpressionList();
    }
}
