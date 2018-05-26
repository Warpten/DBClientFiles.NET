namespace DBClientFiles.NET.Data
{
    public interface ISimpleMember
    {
    }

    /// <summary>
    /// This is a basic interface that is designed to help users wrap around simple data types
    /// in their records. Generally speaking, you would define your sub-type with a
    /// constructor taking a <see cref="System.IO.BinaryReader"/> as its only argument, but there
    /// are situations when wrapping around simple types where there is simply no point.
    /// 
    /// This interface alleviates the need to declare that constructor by enforcing the presence of an underlying
    /// value type that contains the masked value. As such, you can forget about declaring a constructor.
    /// </summary>
    /// <typeparam name="T">The POD type to use. To keep it fast, avoid marshalling.</typeparam>
    public interface ISimpleMember<T> : ISimpleMember
        where T : struct
    {
        T Value { get; set; }

        T2 GetValue<T2>();
    }
}
