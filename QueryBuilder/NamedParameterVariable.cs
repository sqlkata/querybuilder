namespace SqlKata;

public class NamedParameterVariable(string variable, object value)
{
    public object Value { get; set; } = value;
    public string Variable { get; set; } = variable;
}
