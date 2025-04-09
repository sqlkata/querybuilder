namespace SqlKata;

// public class NamedParameter(object value)
// {
//     public object Value { get; set; } = value;
// }

public class NamedParameterVariable(string variable, object value)
{
    public object Value { get; set; } = value;
    public string Variable { get; set; } = variable;
}
