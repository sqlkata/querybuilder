
namespace SqlKata
{
    public class WithVarClause : AbstractClause
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public override AbstractClause Clone()
        {
            return new WithVarClause()
            {
                Component = Component,
                Engine = Engine,
                Name = Name,
                Value = Value
            };
        }
    }
}
