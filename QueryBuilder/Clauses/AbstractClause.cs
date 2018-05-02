namespace SqlKata
{
    public abstract class AbstractClause
    {
        public string Engine { get; set; } = null;
        public string Component { get; set; }
        public virtual object[] GetBindings(string engine)
        {
            return new object[] { };
        }
        public abstract AbstractClause Clone();
    }

}