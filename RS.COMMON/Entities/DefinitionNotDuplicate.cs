namespace RS.COMMON.Entities
{
    /// <summary>
    /// Two values of a Tanımlar list that look alike but are different (R12a-2), so they are not suggested as
    /// duplicates again. Kind is the list ("companies", "corporations" …); FirstId is the smaller id.
    /// </summary>
    public class DefinitionNotDuplicate
    {
        public int Id { get; set; }
        public string Kind { get; set; }
        public int FirstId { get; set; }
        public int SecondId { get; set; }
    }
}
