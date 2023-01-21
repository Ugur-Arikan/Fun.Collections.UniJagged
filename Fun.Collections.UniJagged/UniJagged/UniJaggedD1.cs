using System.Runtime.CompilerServices;

namespace Fun.Collections;

/// <summary>
/// A unified 1-dimensional collection.
/// Can be considered as a union of common collections.
/// 
/// <para>
/// Takes a functional approach; i.e., the following closures are defined and stored on construction:
/// <list type="bullet">
/// <item>closure to get the length;</item>
/// <item>closure to get element at the given index (also the bounds-checked version GetOrNone, in addition to direct access by index);</item>
/// <item>closure to get the collection as IEnumerable.</item>
/// </list>
/// The allocation is limited by these closures.
/// The data is not re-allocated, or memoized.
/// </para>
/// <para>
/// For instance, if a unified collection is defined to always return a contant value, and have a length of 1000 elements;
/// there will not be an underlying array storing the same value 1000 times.
/// </para>
/// 
/// <para>To clarify that the unified collection only holds a reference, consider the following example.</para>
/// <code>
/// int[] underlyingArray = new int[] { 0, 1, 2 };
/// UniJaggedD1&lt;int> jagged = new(underlyingArray);
/// Assert(underlyingArray[1] == 1 and jagged[1] == 1);
/// underlyingArray[1] = 42;
/// Assert(underlyingArray[1] == 42 and jagged[1] == 42);
/// </code>
/// 
/// <para>To further illustrate, consider the following with underlying list.</para>
/// <code>
/// List&lt;char> underlyingList = new() { 'a', 'b', 'c' };
/// UniJaggedD1&lt;char> jagged = new(underlyingList);
/// Assert(underlyingList.Count == 3 and jagged.Length1() == 3);
/// underlyingList.Add('d');
/// Assert(underlyingList.Count == 4 and jagged.Length1() == 4);
/// </code>
/// 
/// <para>The unified jagged collection exposes a unified interface for the following methods:</para>
/// <list type="bullet">
/// <item>Length1(): length in the first dimension.</item>
/// <item>this[i]: element at the i-th position.</item>
/// <item>GetOrNone(i): returns Some of the element at the i-th position if the index is valid; None otherwise.</item>
/// <item>AsEnumerable(): returns the collection as IEnumerable; particularly useful for linq calls over the collection.</item>
/// </list>
/// 
/// </summary>
public class UniJaggedD1<T>
{
    // data
    internal readonly Func<int, T> Get;
    /// <summary>
    /// Length of the jagged array.
    /// 
    /// <code>
    /// var underlyingList = new List&lt;int> { 10, 11, 12 };
    /// UniJaggedD1&lt;int> jagged = new(underlyingList);
    /// Assert(jagged.Length1() == 3);
    /// 
    /// Func&lt;int, bool> underlyingFun = i => i % 2 == 0;
    /// UniJaggedD1&lt;bool> jagged = new(underlyingFun, length1: Some(4));
    /// Assert(jagged.Length1() == 4);
    /// 
    /// Func&lt;int, bool> underlyingFun = i => i % 2 == 0;
    /// UniJaggedD1&lt;Agent> bool = new(underlyingFun); // omitted optional argument 'length1' defaults to None -> no limit
    /// Assert(jagged.Length1() == int.MaxValue);
    /// 
    /// </code>
    /// </summary>
    public readonly Func<int> Length1;
    /// <summary>
    /// Safely gets the element at the i-th position; returns None if the index is invalid.
    /// 
    /// <code>
    /// var underlyingArray = new int[] { 10, 11, 12 };
    /// UniJaggedD1&lt;int> jagged = new(underlyingArray);
    /// 
    /// Assert(jagged.GetOrNone(1) == Some(11));
    /// Assert(jagged.GetOrNone(-1).IsNone);
    /// Assert(jagged.GetOrNone(2).IsNone);
    /// </code>
    /// 
    /// For other methods on the resulting optional, see <see cref="Opt{T}"/>.
    /// </summary>
    public readonly Func<int, Opt<T>> GetOrNone;
    /// <summary>
    /// Converts the unified collection to IEnumerable yielding its underlying values.
    /// Particularly useful for linq calls over the collection.
    /// 
    /// <code>
    /// Func&lt;int, bool> underlyingFun = i => i % 2 == 0;
    /// UniJaggedD1&lt;bool> jagged = new(underlyingFun, length1: Some(4));
    /// 
    /// bool anyEvens = jagged.AsEnumerable().Aggregate(false, (x, y) => x || y);
    /// Assert(anyEvens == true);
    /// 
    /// var enumerable = jagged.AsEnumerable();
    /// 
    /// int counter = 0;
    /// for (var isEven in enumerable)
    /// {
    ///     Assert(isEven == counter % 2 == 0);
    ///     counter++;
    /// }
    /// </code>
    /// </summary>
    public readonly Func<IEnumerable<T>> AsEnumerable;
    // data - constant
    /// <summary>
    /// The unified collection might be constructed with a constant scalar value; hence, returning the scalar for all indices.
    /// If this is the case, <see cref="HasUnderlyingScalar"/> is true; and the field <see cref="UnderlyingScalar"/> equals to Some of the underlying scalar value.
    /// <para>Otherwise, HasUnderlyingScalar is false and UnderlyingScalar.IsNone.</para>
    /// 
    /// <code>
    /// // jaggedConst[i] = 10, for all i.
    /// UniJaggedD1&lt;int> jaggedConst = new(10);
    /// Assert(jaggedConst[3] == 10 and jaggedConst[42] == 10);
    /// Assert(jaggedConst.GetOrNone(12) == Some(10));
    /// 
    /// // underlying constant can be obtained by the optional UnderlyingScalar field.
    /// Assert(jaggedConst.HasUnderlyingScalar);
    /// Assert(jaggedConst.UnderlyingScalar.IsSome);
    /// Assert(jaggedConst.UnderlyingScalar == Some(10));
    /// Assert(jaggedConst.UnderlyingScalar.Unwrap() == 10);
    /// 
    /// // jaggedAny, on the other hand, is constructed by any underlying data other than a scalar constant.
    /// UniJaggedD1&lt;int> jaggedAny = new(new int[] { 10, 11, 12 });
    /// Assert(jaggedAny.HasUnderlyingScalar == false);
    /// Assert(jaggedAny.UnderlyingScalar.IsNone);
    /// // var scalar = jaggedAny.UnderlyingScalar.Unwrap(); // throws!
    /// </code>
    /// </summary>
    public readonly Opt<T> UnderlyingScalar;
    /// <summary>
    /// <inheritdoc cref="UnderlyingScalar"/>
    /// </summary>
    public bool HasUnderlyingScalar => UnderlyingScalar.IsSome;


    // ctor
    internal UniJaggedD1(int length1, Func<int, T> get, Func<int, Opt<T>> getOrNone, Func<IEnumerable<T>> asEnumerable, Opt<T> underlyingConstantValue)
    {
        Length1 = () => length1;
        Get = get;
        GetOrNone = getOrNone;
        AsEnumerable = asEnumerable;
        UnderlyingScalar = underlyingConstantValue;
    }
    /// <summary>
    /// 1-dimensional jagged collection with optional length, which always yields the same constant value.
    /// 
    /// <code>
    /// var agentSmith = GetSmith();
    /// UniJaggedD1&lt;Agent> jagged = new(agentSmith);
    /// Assert(jagged.Length1() == int.MaxValue); // since length1 is omitted
    /// Assert(jagged[0] == agentSmith);
    /// Assert(jagged[42] == agentSmith);
    /// Assert(jagged.GetOrNone(100) == Some(agentSmith));
    /// 
    /// UniJaggedD1&lt;Agent> jagged = new(agentSmith, Some(50));
    /// Assert(jagged.Length1() == 50);
    /// Assert(jagged[0] == agentSmith);
    /// Assert(jagged[42] == agentSmith);
    /// Assert(jagged.GetOrNone(100).IsNone);
    /// </code>
    /// </summary>
    /// <param name="constantValue">Constant value that every position of the jagged array will return.</param>
    /// <param name="length1">Optional length of the jagged array; will default to None (int.MaxValue) when omitted.</param>
    public UniJaggedD1(T constantValue, Opt<int> length1 = default)
    {
        int length = length1.UnwrapOr(int.MaxValue);
        Get = _ => constantValue;
        GetOrNone = _ => Some(constantValue);
        Length1 = () => length;
        AsEnumerable = () => Range(length).Select(_ => constantValue);
        UnderlyingScalar = Some(constantValue);
    }
    /// <summary>
    /// 1-dimensional jagged collection with optional length, values of which are determined by the <paramref name="getValue"/> function.
    /// 
    /// <code>
    /// static int Factorial(int number) { .. }
    /// 
    /// UniJaggedD1&lt;int> factorials = new(Factorial);
    /// Assert(factorials.Length1() == int.MaxValue); // since length1 is omitted
    /// Assert(factorials[3] == 6);
    /// Assert(factorials[5] == 120);
    /// 
    /// UniJaggedD1&lt;int> factorialsUpTo4 = new(Factorial, Some(4));
    /// Assert(factorialsUpTo4.Length1() == 4);
    /// Assert(factorialsUpTo4[3] == 6);
    /// // Assert(factorialsUpTo4[5] == 120); // out-of-bounds, throws!
    /// Assert(factorialsUpTo4.GetOrNone(5).IsNone);
    /// </code>
    /// </summary>
    /// <param name="getValue">Function (index -> value) that returns the value of the element at the given index-th position.</param>
    /// <param name="length1">Optional length of the jagged array; will default to None (int.MaxValue) when omitted.</param>
    public UniJaggedD1(Func<int, T> getValue, Opt<int> length1 = default)
    {
        static int Factorial(int number) { return 12; }
        UniJaggedD1<int> factorials = new(Factorial);
        int length = length1.UnwrapOr(int.MaxValue);
        Get = getValue;
        GetOrNone = i => SomeIf(i > -1 && i < length, getValue(i));
        Length1 = () => length;
        AsEnumerable = () => Range(length).Select(i => getValue(i));
        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 1-dimensional jagged collection length and values of which are determined by the underlying <paramref name="array"/>.
    /// 
    /// <code>
    /// var array = new char[] { 'a', 'b', 'c' };
    /// UniJaggedD1&lt;char> jagged = new(array);
    /// Assert(jagged.Length1() == 3);
    /// Assert(jagged[2] == 'c');
    /// Assert(jagged.GetOrNone(0) == Some('a'));
    /// Assert(jagged.GetOrNone(3).IsNone);
    /// </code>
    /// </summary>
    /// <param name="array">Underlying array of the unified jagged collection.</param>
    public UniJaggedD1(T[] array)
    {
        Get = i => array[i];
        GetOrNone = i => array.GetOrNone(i);
        Length1 = () => array.Length;
        AsEnumerable = () => array;
        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 1-dimensional jagged collection length and values of which are determined by the underlying <paramref name="memory"/>.
    /// 
    /// <code>
    /// var memory = (new char[] { 'a', 'b', 'c' }).AsMemory();
    /// UniJaggedD1&lt;char> jagged = new(memory);
    /// Assert(jagged.Length1() == 3);
    /// Assert(jagged[2] == 'c');
    /// Assert(jagged.GetOrNone(0) == Some('a'));
    /// Assert(jagged.GetOrNone(3).IsNone);
    /// </code>
    /// </summary>
    /// <param name="memory">Underlying memory of the unified jagged collection.</param>
    public UniJaggedD1(Memory<T> memory)
    {
        Get = i => memory.Span[i];
        GetOrNone = i => memory.Span.GetOrNone(i);
        Length1 = () => memory.Length;
        AsEnumerable = () => Range(memory.Span.Length).Select(i => memory.Span[i]);
        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 1-dimensional jagged collection length and values of which are determined by the underlying <paramref name="readonlyMemory"/>.
    /// 
    /// <code>
    /// var array = new char[] { 'a', 'b', 'c' };
    /// ReadOnlyMemory&lt;char> memory = array;
    /// UniJaggedD1&lt;char> jagged = new(memory);
    /// Assert(jagged.Length1() == 3);
    /// Assert(jagged[2] == 'c');
    /// Assert(jagged.GetOrNone(0) == Some('a'));
    /// Assert(jagged.GetOrNone(3).IsNone);
    /// </code>
    /// </summary>
    /// <param name="readonlyMemory">Underlying readonly memory of the unified jagged collection.</param>
    public UniJaggedD1(ReadOnlyMemory<T> readonlyMemory)
    {
        Get = i => readonlyMemory.Span[i];
        GetOrNone = i => readonlyMemory.Span.GetOrNone(i);
        Length1 = () => readonlyMemory.Length;
        AsEnumerable = () => Range(readonlyMemory.Span.Length).Select(i => readonlyMemory.Span[i]);
        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 1-dimensional jagged collection length and values of which are determined by the underlying <paramref name="list"/>.
    /// 
    /// <code>
    /// var list = new List&lt;char>() { 'a', 'b', 'c' };
    /// UniJaggedD1&lt;char> jagged = new(list);
    /// Assert(jagged.Length1() == 3);
    /// Assert(jagged[2] == 'c');
    /// Assert(jagged.GetOrNone(0) == Some('a'));
    /// Assert(jagged.GetOrNone(3).IsNone);
    /// </code>
    /// </summary>
    /// <param name="list">Underlying list of the unified jagged collection.</param>
    public UniJaggedD1(List<T> list)
    {
        Get = i => list[i];
        GetOrNone = i => list.GetOrNone(i);
        Length1 = () => list.Count;
        AsEnumerable = () => list;
        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 1-dimensional jagged collection length and values of which are determined by the underlying <paramref name="list"/>.
    /// 
    /// <code>
    /// IList&lt;char> list = new List&lt;char>() { 'a', 'b', 'c' };
    /// UniJaggedD1&lt;char> jagged = new(list);
    /// Assert(jagged.Length1() == 3);
    /// Assert(jagged[2] == 'c');
    /// Assert(jagged.GetOrNone(0) == Some('a'));
    /// Assert(jagged.GetOrNone(3).IsNone);
    /// </code>
    /// </summary>
    /// <param name="list">Underlying list of the unified jagged collection.</param>
    public UniJaggedD1(IList<T> list)
    {
        Get = i => list[i];
        GetOrNone = i => list.GetOrNone(i);
        Length1 = () => list.Count;
        AsEnumerable = () => list;
        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 1-dimensional jagged collection length and values of which are determined by the underlying <paramref name="enumerable"/>.
    /// 
    /// <para>
    /// Note that underlying Count() method is used to determine Length1 of the collection.
    /// This means that, it might require linear search if the underlying collection does not have a trivial count.
    /// </para>
    /// 
    /// <code>
    /// IEnumerable&lt;char> enumerable = new List&lt;char>() { 'a', 'b', 'c' };
    /// UniJaggedD1&lt;char> jagged = new(enumerable);
    /// Assert(jagged.Length1() == 3);
    /// Assert(jagged[2] == 'c');
    /// Assert(jagged.GetOrNone(0) == Some('a'));
    /// Assert(jagged.GetOrNone(3).IsNone);
    /// </code>
    /// </summary>
    /// <param name="enumerable">Underlying enumerable of the unified jagged collection.</param>
    public UniJaggedD1(IEnumerable<T> enumerable)
    {
        Get = i => enumerable.ElementAt(i);
        GetOrNone = i => enumerable.GetOrNone(i);
        Length1 = () => enumerable.Count();
        AsEnumerable = () => enumerable;
        UnderlyingScalar = None<T>();
    }
    

    // index
    /// <summary>
    /// Directly returns the element at the i-th position.
    /// Use <see cref="GetOrNone"/> for the bound-checked optional version.
    /// 
    /// <code>
    /// var underlyingArray = new int[] { 10, 11, 12 };
    /// UniJaggedD1&lt;int> jagged = new(underlyingArray);
    /// 
    /// Assert(jagged[1] == 11);
    /// 
    /// // var x = jagged[-1]; => out-of-bounds, throws!
    /// // var x = jagged[3]; => out-of-bounds, throws!
    /// </code>
    /// </summary>
    /// <param name="i">Index of the element to be retrieved.</param>
    public T this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Get(i);
    }
}
