namespace SqlKata
{
    public class IncrementClause : InsertClause
    {
        public string Column { get; set; }
        public int Value { get; set; } = 1;
    }
}
