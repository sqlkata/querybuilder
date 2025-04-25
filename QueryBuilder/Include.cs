namespace SqlKata
{
    public class Include
    {
        public string Name { get; set; }
        public Query Query { get; set; }
        public string ForeignKey { get; set; }
        public string LocalKey { get; set; }
        public bool IsMany { get; set; }

        public Include Clone()
        {
            var clone = new Include();
            clone.Name = this.Name;
            clone.Query = this.Query.Clone();
            clone.ForeignKey = this.ForeignKey;
            clone.LocalKey = this.LocalKey;
            clone.IsMany = this.IsMany;
            return clone;
        }
    }
}
