namespace Authorization.Domain.Common.Exceptions.Enums
{
    /// <summary>
    /// Операція, під час якої виник внутрішній виняток.
    /// Використовується як діагностичний контекст.
    ///
    /// (Operation during which an internal exception occurred.
    /// Used as diagnostic context.)
    /// </summary>
    public enum OperationType
    {
        Restore,

        Create,

        Update,

        Delete,

        Read,

        Compare
    }
}
