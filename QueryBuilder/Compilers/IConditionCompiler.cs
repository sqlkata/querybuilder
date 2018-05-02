namespace SqlKata.Compilers
{
    internal interface  IConditionCompiler
    {
        string CompileRawCondition(RawCondition x);
        string CompileQueryCondition<T>(QueryCondition<T> x) where T : BaseQuery<T>;
        string CompileBasicCondition<T>(BasicCondition<T> x);
        string CompileBasicStringCondition(BasicStringCondition x);
        string CompileBasicDateCondition(BasicDateCondition x);

        string CompileNestedCondition<Q>(NestedCondition<Q> x) where Q : BaseQuery<Q>;
        string CompileTwoColumnsCondition(TwoColumnsCondition clause);
        string CompileBetweenCondition<T>(BetweenCondition<T> item);
        string CompileInCondition<T>(InCondition<T> item);
        string CompileInQueryCondition(InQueryCondition item);
        string CompileNullCondition(NullCondition item);
        string CompileExistsCondition<T>(ExistsCondition<T> item) where T : BaseQuery<T>;
    }
}