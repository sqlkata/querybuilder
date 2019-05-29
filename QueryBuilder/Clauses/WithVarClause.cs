
namespace SqlKata
{
    public class WithVarClause : AbstractClause
    {
        /// <summary>
        /// Hold the name of a sql variable
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Hold the value of a sql variable
        /// </summary>
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
