namespace Fun.Collections.UniJaggedMut;

internal class UniJaggedMutD1<T>
{
    // data
    readonly UniJaggedD1<T> Array;
    readonly Action<int, T> Set;
    public readonly Func<int, T, Res> TrySet;


    // ctor
    public UniJaggedMutD1(Func<int, T> getValue, Action<int, T> setValue, Opt<int> numberOfElements = default)
    {
        Array = new(getValue, numberOfElements);
        Set = setValue;
        TrySet = (i, v) =>
        {
            int length = Array.Length1();
            if (i > -1 && i < length)
            {
                setValue(i, v);
                return Ok();
            }
            else
                return Err(string.Format("Index out of bounds: index={0}, expected-range=[0,{1}).", i, length));
        };
    }
    public UniJaggedMutD1(T[] array)
    {
        Array = new(array);
        Set = (i, v) => array[i] = v;
        TrySet = array.TrySet;
    }
    public UniJaggedMutD1(Memory<T> memory)
    {
        Array = new(memory);
        Set = (i, v) => memory.Span[i] = v;
        TrySet = (i, v) => memory.TrySet(i, v);
    }
    public UniJaggedMutD1(List<T> list)
    {
        Array = new(list);
        Set = (i, v) => list[i] = v;
        TrySet = list.TrySet;
    }
    public UniJaggedMutD1(IList<T> list)
    {
        Array = new(list);
        Set = (i, v) => list[i] = v;
        TrySet = list.TrySet;
    }


    // method
    public int Length1()
        => Array.Length1();
    public Opt<T> GetOrNone(int i)
        => Array.GetOrNone(i);
    public IEnumerable<T> AsEnumerable()
        => Array.AsEnumerable();


    // index
    public T this[int i]
    {
        get => Array.Get(i);
        set => Set(i, value);
    }
}
