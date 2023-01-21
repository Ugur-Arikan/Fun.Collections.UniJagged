using System.Runtime.CompilerServices;

namespace Fun.Collections;

/// <summary>
/// A unified 2-dimensional collection.
/// Can be considered as a union of common collections.
/// 
/// <para>
/// Takes a functional approach; i.e., the following closures are defined and stored on construction:
/// <list type="bullet">
/// <item>closure to get the jagged lengths in all dimensions;</item>
/// <item>closure to get element at the given indices (also the bounds-checked version GetOrNone, in addition to direct access by indices);</item>
/// <item>closure to get the collection as IEnumerable.</item>
/// </list>
/// The allocation is limited by these closures.
/// The data is not re-allocated, or memoized.
/// </para>
/// <para>
/// For instance, if a unified collection is defined to always return a contant value,
/// and have a length of 1000 collections in the first dimension,
/// and 1000 elements in the second dimension for each of the collection;
/// there will NOT be an underlying array storing the same value 1_000_000 times.
/// </para>
/// 
/// <para>To clarify that the unified collection only holds a reference, consider the following example.</para>
/// <code>
/// int[][] underlyingArray = new int[] { new int[] { 0, 10 }, new int[] { 1, 11, 111, 1111 }, new int[] { 2, 22, 222 } };
/// UniJaggedD2&lt;int> jagged = new(underlyingArray);
/// Assert(underlyingArray[1][0] == 1 and jagged[1, 0] == 1);
/// underlyingArray[1][0] = 42;
/// Assert(underlyingArray[1][0] == 42 and jagged[1, 0] == 42);
/// </code>
/// 
/// <para>To further illustrate, consider the following with underlying list of lists.</para>
/// <code>
/// List&lt;List&lt;char>> underlyingList = new() { new() { 'a', 'b', 'c', 'd' }, new() { 'u', 'v' } };
/// UniJaggedD2&lt;char> jagged = new(underlyingList);
/// 
/// Assert(underlyingList.Count == 2 and jagged.Length1() == 2);
/// underlyingList.Add(new() { 'x', 'y', 'z' });
/// Assert(underlyingList.Count == 3 and jagged.Length1() == 3);
/// 
/// Assert(underlyingList[0].Count == 4 and jagged.Length2(0) == 4);
/// underlyingList[0].Add('e');
/// Assert(underlyingList[0].Count == 5 and jagged.Length2(0) == 5);
/// </code>
/// 
/// <para>The unified jagged collection exposes a unified interface for the following methods:</para>
/// <list type="bullet">
/// <item>Length1(): length in the first dimension (i.e., number of 1D collections).</item>
/// <item>Length2(i): length of the i-th collection in the second dimension (i.e., number of elements in the i-th 1D collection).</item>
/// <item>this[i]: i-th D1 collection.</item>
/// <item>this[i,j]: element at the (i,j)-th position (i.e., j-th element of the i-th 1D collection).</item>
/// <item>GetOrNone(i, j): returns Some of the element at the (i,j)-th position if indices are valid; None otherwise.</item>
/// <item>AsEnumerable(): returns the collection as IEnumerable; particularly useful for linq calls over the collection.</item>
/// </list>
/// 
/// </summary>
public class UniJaggedD2<T>
{
    // data
    internal readonly Func<int, int, T> Get;
    /// <summary>
    /// Length of the jagged array in the first dimension; i.e., number of 1D collections.
    /// 
    /// <code>
    /// var underlyingList = new List&lt;List&lt;int>> { new() { 1 }, new() { 2, 3 } };
    /// UniJaggedD2&lt;int> jagged = new(underlyingList);
    /// Assert(jagged.Length1() == 2);
    /// 
    /// Func&lt;int, int, bool> underlyingFun = (i, j) => (i + j) % 2 == 0;
    /// UniJaggedD2&lt;bool> jagged = new(underlyingFun, length1: Some(4));
    /// Assert(jagged.Length1() == 4);
    /// 
    /// UniJaggedD2&lt;Agent> bool = new(underlyingFun); // omitted optional argument 'length1' defaults to None -> no limit
    /// Assert(jagged.Length1() == int.MaxValue);
    /// 
    /// </code>
    /// </summary>
    public readonly Func<int> Length1;
    /// <summary>
    /// Length of the jagged array in the first dimension; i.e., number of 1D collections.
    /// 
    /// <code>
    /// var underlyingList = new List&lt;List&lt;int>> { new() { 1 }, new() { 2, 3, 4 } };
    /// UniJaggedD2&lt;int> jagged = new(underlyingList);
    /// Assert(jagged.Length1() == 2);
    /// Assert(jagged.Length2(0) == 1);
    /// Assert(jagged.Length2(1) == 3);
    /// 
    /// Func&lt;int, int, bool> underlyingFun = (i, j) => (i + j) % 2 == 0;
    /// UniJaggedD2&lt;bool> upperTriangular = new(underlyingFun, length1: Some(3), getLength2: Some&lt;Func&lt;int, int>>(i => i + 1));
    /// Assert(jagged.Length1() == 3);
    /// Assert(jagged.Length2(0) == 1);
    /// Assert(jagged.Length2(1) == 2);
    /// Assert(jagged.Length2(2) == 3);
    /// 
    /// UniJaggedD2&lt;Agent> bool = new(underlyingFun, length1: Some(2)); // omitted optional argument 'length2' defaults to None -> no limit
    /// Assert(jagged.Length2(0) == jagged.Length2(1) == int.MaxValue);
    /// 
    /// </code>
    /// </summary>
    public readonly Func<int, int> Length2;
    /// <summary>
    /// Safely gets Some of the element at the (i,j)-th position; returns None if the indices are invalid.
    /// 
    /// <code>
    /// var underlyingArray = new int[] { new int[] { 0 }, new int[] { 1, 2, 3 } };
    /// UniJaggedD2&lt;int> jagged = new(underlyingArray);
    /// Assert(jagged.Length1() == 2);
    /// Assert(jagged.Length2(0) == 1);
    /// Assert(jagged.Length2(1) == 3);
    /// 
    /// Assert(jagged.GetOrNone(0, 0) == Some(0));
    /// Assert(jagged.GetOrNone(1, 2) == Some(3));
    /// Assert(jagged.GetOrNone(-1, 0).IsNone);
    /// Assert(jagged.GetOrNone(2, 0).IsNone);
    /// Assert(jagged.GetOrNone(0, 1).IsNone);
    /// Assert(jagged.GetOrNone(1, -1).IsNone);
    /// Assert(jagged.GetOrNone(1, 3).IsNone);
    /// </code>
    /// 
    /// For other methods on the resulting optional, see <see cref="Opt{T}"/>.
    /// </summary>
    public readonly Func<int, int, Opt<T>> GetOrNone;
    /// <summary>
    /// Converts the unified jagged collection to IEnumerable yielding its underlying values.
    /// Particularly useful for linq calls over the collection.
    /// 
    /// <code>
    /// Func&lt;int, int, bool> underlyingFun = (i, j) => (i + j) % 2 == 0;
    /// UniJaggedD2&lt;bool> jagged = new(underlyingFun, length1: Some(2), getLength2: Some&lt;Func&lt;int, int>>(_ => 2));
    /// 
    /// bool anyEvens = jagged.AsEnumerable().SelectMany(x => x).Aggregate(false, (x, y) => x || y);
    /// Assert(anyEvens == true); 
    /// 
    /// var enumerable = jagged.AsEnumerable();
    /// 
    /// int i = 0;
    /// for (var values in enumerable)
    /// {
    ///     int j = 0;
    ///     for (var value in values)
    ///     {
    ///         bool expectedValue = (i + j) % 2 == 0;
    ///         Assert(value == expectedValue);
    ///         j++;
    ///     }
    ///     i++;
    /// }
    /// </code>
    /// </summary>
    public readonly Func<IEnumerable<IEnumerable<T>>> AsEnumerable;
    // data - constant
    /// <summary>
    /// The unified collection might be constructed with a constant scalar value; hence, returning the scalar for all indices.
    /// If this is the case, <see cref="HasUnderlyingScalar"/> is true; and the field <see cref="UnderlyingScalar"/> equals to Some of the underlying scalar value.
    /// <para>Otherwise, HasUnderlyingScalar is false and UnderlyingScalar.IsNone.</para>
    /// 
    /// <code>
    /// // jaggedConst[i, j] = 10, for all i, j.
    /// UniJaggedD2&lt;int> jaggedConst = new(10);
    /// Assert(jaggedConst[3, 2] == 10 and jaggedConst[42, 0] == 10);
    /// Assert(jaggedConst.GetOrNone(12, 4) == Some(10));
    /// 
    /// // underlying constant can be obtained by the optional UnderlyingScalar field.
    /// Assert(jaggedConst.HasUnderlyingScalar);
    /// Assert(jaggedConst.UnderlyingScalar.IsSome);
    /// Assert(jaggedConst.UnderlyingScalar == Some(10));
    /// Assert(jaggedConst.UnderlyingScalar.Unwrap() == 10);
    /// 
    /// // jaggedAny, on the other hand, is constructed by any underlying data other than a scalar constant.
    /// UniJaggedD2&lt;int> jaggedAny = new(new int[] { int[] { 10, 11, 12 } });
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
    internal UniJaggedD2(int length1, Func<int, int> length2, Func<int, int, T> get, Func<int, int, Opt<T>> getOrNone, Func<IEnumerable<IEnumerable<T>>> asEnumerable, Opt<T> underlyingConstantValue)
    {
        Length1 = () => length1;
        Length2 = length2;
        Get = get;
        GetOrNone = getOrNone;
        AsEnumerable = asEnumerable;
        UnderlyingScalar = underlyingConstantValue;
    }
    /// <summary>
    /// 2-dimensional jagged collection with optional lengths, which always yields the same constant value.
    /// 
    /// <code>
    /// var agentSmith = GetSmith();
    /// UniJaggedD2&lt;Agent> jagged = new(agentSmith);
    /// Assert(jagged.Length1() == int.MaxValue);   // since length1 is omitted
    /// Assert(jagged.Length2(42) == int.MaxValue); // since length2 is omitted
    /// Assert(jagged[0][0] == agentSmith);
    /// Assert(jagged[42][42] == agentSmith);
    /// Assert(jagged.GetOrNone(100, 42) == Some(agentSmith));
    /// 
    /// UniJaggedD2&lt;Agent> jagged = new(agentSmith, Some(50));
    /// Assert(jagged.Length1() == 50);
    /// Assert(jagged.Length2(42) == int.MaxValue); // since length2 is omitted
    /// Assert(jagged[0][0] == agentSmith);
    /// Assert(jagged[42][142] == agentSmith);
    /// Assert(jagged.GetOrNone(100, 2).IsNone);
    /// 
    /// UniJaggedD2&lt;Agent> jagged = new(agentSmith, Some(50), Some&lt;Func&lt;int, int>>(_ => 2));
    /// Assert(jagged.Length1() == 50);
    /// Assert(jagged.Length2(42) == 2);
    /// Assert(jagged[0][0] == agentSmith);
    /// Assert(jagged[42][1] == agentSmith);
    /// Assert(jagged.GetOrNone(0, 2).IsNone);
    /// Assert(jagged.GetOrNone(50, 0).IsNone);
    /// 
    /// UniJaggedD2&lt;Agent> jagged = new(agentSmith, Some(50), Some&lt;Func&lt;int, int>>(i => i));
    /// Assert(jagged.Length1() == 50);
    /// Assert(jagged.Length2(0) == 0);
    /// Assert(jagged.Length2(42) == 42);
    /// Assert(jagged[42][1] == agentSmith);
    /// Assert(jagged.GetOrNone(1, 0) == Some(agentSmith));
    /// Assert(jagged.GetOrNone(0, 0).IsNone);
    /// Assert(jagged.GetOrNone(50, 0).IsNone);
    /// </code>
    /// </summary>
    /// <param name="constantValue">Constant value that every position of the jagged array will return.</param>
    /// <param name="length1">Optional length of the jagged array; will default to None (int.MaxValue) when omitted.</param>
    /// <param name="getLength2">Optional function (i -> length) to get length of the i-th collection; when omitted will default to None which will yield to a length of int.MaxValue for any index.</param>
    public UniJaggedD2(T constantValue, Opt<int> length1 = default, Opt<Func<int, int>> getLength2 = default)
    {
        var len1 = () => length1.UnwrapOr(int.MaxValue);
        var len2 = getLength2.UnwrapOr(i => int.MaxValue);

        Length1 = len1;
        Length2 = len2;

        Get = (_, _) => constantValue;
        GetOrNone = (_, _) => Some(constantValue);

        AsEnumerable = () => Range(len1()).Select(i => Range(len2(i)).Select(_ => constantValue));

        UnderlyingScalar = Some(constantValue);
    }
    /// <summary>
    /// 2-dimensional jagged collection with optional lengths, values of which are determined by the <paramref name="getValue"/> function.
    /// 
    /// <code>
    /// static int GetDistance(int from, int to) { return Math.Abs(to - from); }
    /// 
    /// UniJaggedD2&lt;int> distances = new(GetDistance);
    /// Assert(distances.Length1() == int.MaxValue);    // since length1 is omitted
    /// Assert(distances.Length2(100) == int.MaxValue); // since length2 is omitted
    /// Assert(distances[1, 2] == 1);
    /// Assert(distances[10, 5] == 5);
    /// 
    /// UniJaggedD2&lt;int> distancesUpTo4 = new(GetDistance, Some(4));
    /// Assert(distancesUpTo4.Length1() == 4);
    /// Assert(distancesUpTo4[3, 1] == 2);
    /// // Assert(distancesUpTo4[5, 1] == 4); // out-of-bounds, throws!
    /// Assert(distancesUpTo4.GetOrNone(5, 1).IsNone);
    /// 
    /// UniJaggedD2&lt;int> distancesUpTo4 = new(GetDistance, Some(4), Some&lt;Func&lt;int, int>>(i => 2*i));
    /// Assert(distancesUpTo4.Length1() == 4);
    /// Assert(distancesUpTo4.Length2(2) == 4);
    /// Assert(distancesUpTo4[3, 5] == 2);
    /// // Assert(distancesUpTo4[3, 6] == 3); // out-of-bounds, throws!
    /// // Assert(distancesUpTo4[4, 0] == 4); // out-of-bounds, throws!
    /// Assert(distancesUpTo4.GetOrNone(3, 6).IsNone);  // since Length2(3) is 6, index 6 is out of bounds.
    /// Assert(distancesUpTo4.GetOrNone(4, 0).IsNone);  // since Length1() is 4, index 4 is out of bounds.
    /// </code>
    /// </summary>
    /// <param name="getValue">Function (index -> value) that returns the value of the element at the given index-th position.</param>
    /// <param name="length1">Optional length of the jagged array; will default to None (int.MaxValue) when omitted.</param>
    /// <param name="getLength2">Optional function (i -> length) to get length of the i-th collection; when omitted will default to None which will yield to a length of int.MaxValue for any index.</param>
    public UniJaggedD2(Func<int, int, T> getValue, Opt<int> length1 = default, Opt<Func<int, int>> getLength2 = default)
    {
        var len1 = () => length1.UnwrapOr(int.MaxValue);
        var len2 = getLength2.UnwrapOr(i => int.MaxValue);

        Length1 = len1;
        Length2 = len2;

        Get = (i, j) => getValue(i, j);
        GetOrNone = (i, j) =>
        {
            if (i > -1 && i < len1())
            {
                if (j > -1 && j < len2(i))
                    return Some(getValue(i, j));
            }
            return None<T>();
        };

        AsEnumerable = () => Range(len1()).Select(i => Range(len2(i)).Select(j => getValue(i, j)));

        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 2-dimensional jagged collection lengths and values of which are determined by the underlying <paramref name="array"/>.
    /// 
    /// <code>
    /// var array = new char[][] { new char[] { 'a', 'b', 'c' }, new char[] { 'd' } };
    /// UniJaggedD2&lt;char> jagged = new(array);
    /// Assert(jagged.Length1() == 2);
    /// Assert(jagged.Length2(0) == 3);
    /// Assert(jagged.Length2(1) == 1);
    /// Assert(jagged[0, 2] == 'c');
    /// Assert(jagged.GetOrNone(1, 0) == Some('d'));
    /// Assert(jagged.GetOrNone(0, 3).IsNone);
    /// Assert(jagged.GetOrNone(2, 0).IsNone);
    /// </code>
    /// </summary>
    /// <param name="array">Underlying array of the unified jagged collection.</param>
    public UniJaggedD2(T[][] array)
    {
        int len1() => array.Length;
        int len2(int i) => array[i].Length;

        Length1 = len1;
        Length2 = len2;

        Get = (i, j) => array[i][j];
        GetOrNone = (i, j) =>
        {
            if (i > -1 && i < len1())
            {
                if (j > -1 && j < len2(i))
                    return Some(array[i][j]);
            }
            return None<T>();
        };

        AsEnumerable = () => Range(len1()).Select(i => array[i]);

        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 2-dimensional jagged collection lengths and values of which are determined by the underlying <paramref name="array"/>.
    /// 
    /// <code>
    /// var array = new char[2, 3] { { 'a', 'b', 'c' }, { 'd', 'e', 'f' } };
    /// UniJaggedD2&lt;char> jagged = new(array);
    /// Assert(jagged.Length1() == 2);
    /// Assert(jagged.Length2(0) == 3);
    /// Assert(jagged.Length2(1) == 3);
    /// Assert(jagged[0, 2] == 'c');
    /// Assert(jagged.GetOrNone(1, 0) == Some('d'));
    /// Assert(jagged.GetOrNone(0, 3).IsNone);
    /// Assert(jagged.GetOrNone(2, 0).IsNone);
    /// </code>
    /// </summary>
    /// <param name="array">Underlying rectangular array of the unified jagged collection.</param>
    public UniJaggedD2(T[,] array)
    {
        int len1() => array.Length;
        int len2(int i) => array.GetLength(1);

        Length1 = len1;
        Length2 = len2;

        Get = (i, j) => array[i, j];
        GetOrNone = (i, j) =>
        {
            if (i > -1 && i < len1())
            {
                if (j > -1 && j < len2(i))
                    return Some(array[i, j]);
            }
            return None<T>();
        };

        AsEnumerable = () => Range(len1()).Select(i => Range(len2(i)).Select(j => array[i, j]));

        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// <inheritdoc cref="UniJaggedD2{T}.UniJaggedD2(T[][])"/>
    /// </summary>
    /// <param name="array">Underlying array of the unified jagged collection.</param>
    public UniJaggedD2(List<T>[] array)
    {
        int len1() => array.Length;
        int len2(int i) => array[i].Count;

        Length1 = len1;
        Length2 = len2;

        Get = (i, j) => array[i][j];
        GetOrNone = (i, j) =>
        {
            if (i > -1 && i < len1())
            {
                if (j > -1 && j < len2(i))
                    return Some(array[i][j]);
            }
            return None<T>();
        };

        AsEnumerable = () => Range(len1()).Select(i => array[i]);

        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// <inheritdoc cref="UniJaggedD2{T}.UniJaggedD2(T[][])"/>
    /// </summary>
    /// <param name="list">Underlying list of the unified jagged collection.</param>
    public UniJaggedD2(List<T[]> list)
    {
        int len1() => list.Count;
        int len2(int i) => list[i].Length;

        Length1 = len1;
        Length2 = len2;

        Get = (i, j) => list[i][j];
        GetOrNone = (i, j) =>
        {
            if (i > -1 && i < len1())
            {
                if (j > -1 && j < len2(i))
                    return Some(list[i][j]);
            }
            return None<T>();
        };

        AsEnumerable = () => Range(len1()).Select(i => list[i]);

        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 2-dimensional jagged collection lengths and values of which are determined by the underlying <paramref name="list"/>.
    /// 
    /// <code>
    /// var list = new List&lt;List&lt;char>>() { new() { 'a', 'b', 'c' }, new() { 'd' } };
    /// UniJaggedD2&lt;char> jagged = new(list);
    /// Assert(jagged.Length1() == 2);
    /// Assert(jagged.Length2(0) == 3);
    /// Assert(jagged.Length2(1) == 1);
    /// Assert(jagged[0, 2] == 'c');
    /// Assert(jagged.GetOrNone(1, 0) == Some('d'));
    /// Assert(jagged.GetOrNone(0, 3).IsNone);
    /// Assert(jagged.GetOrNone(2, 0).IsNone);
    /// </code>
    /// </summary>
    /// <param name="list">Underlying list of the unified jagged collection.</param>
    public UniJaggedD2(List<List<T>> list)
    {
        int len1() => list.Count;
        int len2(int i) => list[i].Count;

        Length1 = len1;
        Length2 = len2;

        Get = (i, j) => list[i][j];
        GetOrNone = (i, j) =>
        {
            if (i > -1 && i < len1())
            {
                if (j > -1 && j < len2(i))
                    return Some(list[i][j]);
            }
            return None<T>();
        };

        AsEnumerable = () => Range(len1()).Select(i => list[i]);


        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// <inheritdoc cref="UniJaggedD2{T}.UniJaggedD2(T[][])"/>
    /// </summary>
    /// <param name="array">Underlying array of the unified jagged collection.</param>
    public UniJaggedD2(IList<T>[] array)
    {
        int len1() => array.Length;
        int len2(int i) => array[i].Count;

        Length1 = len1;
        Length2 = len2;

        Get = (i, j) => array[i][j];
        GetOrNone = (i, j) =>
        {
            if (i > -1 && i < len1())
            {
                if (j > -1 && j < len2(i))
                    return Some(array[i][j]);
            }
            return None<T>();
        };

        AsEnumerable = () => Range(len1()).Select(i => array[i]);

        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// <inheritdoc cref="UniJaggedD2{T}.UniJaggedD2(IList{IList{T}})"/>
    /// </summary>
    /// <param name="list">Underlying list of the unified jagged collection.</param>
    public UniJaggedD2(IList<T[]> list)
    {
        int len1() => list.Count;
        int len2(int i) => list[i].Length;

        Length1 = len1;
        Length2 = len2;

        Get = (i, j) => list[i][j];
        GetOrNone = (i, j) =>
        {
            if (i > -1 && i < len1())
            {
                if (j > -1 && j < len2(i))
                    return Some(list[i][j]);
            }
            return None<T>();
        };

        AsEnumerable = () => Range(len1()).Select(i => list[i]);

        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 2-dimensional jagged collection lengths and values of which are determined by the underlying <paramref name="list"/>.
    /// 
    /// <code>
    /// IList&lt;IList&lt;char>> list = new List&lt;List&lt;char>>() { new List&lt;char>() { 'a', 'b', 'c' }, new List&lt;char>() { 'd' } };
    /// UniJaggedD2&lt;char> jagged = new(list);
    /// Assert(jagged.Length1() == 2);
    /// Assert(jagged.Length2(0) == 3);
    /// Assert(jagged.Length2(1) == 1);
    /// Assert(jagged[0, 2] == 'c');
    /// Assert(jagged.GetOrNone(1, 0) == Some('d'));
    /// Assert(jagged.GetOrNone(0, 3).IsNone);
    /// Assert(jagged.GetOrNone(2, 0).IsNone);
    /// </code>
    /// </summary>
    /// <param name="list">Underlying list of the unified jagged collection.</param>
    public UniJaggedD2(IList<IList<T>> list)
    {
        int len1() => list.Count;
        int len2(int i) => list[i].Count;

        Length1 = len1;
        Length2 = len2;

        Get = (i, j) => list[i][j];
        GetOrNone = (i, j) =>
        {
            if (i > -1 && i < len1())
            {
                if (j > -1 && j < len2(i))
                    return Some(list[i][j]);
            }
            return None<T>();
        };

        AsEnumerable = () => Range(len1()).Select(i => list[i]);

        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 2-dimensional jagged collection lengths and values of which are determined by the underlying <paramref name="enumerable"/>.
    /// 
    /// <para>
    /// Note that underlying Count() method is used to determine Length1 and Length2 of the collections.
    /// This means that, it might require linear search if the underlying collections do not have a trivial counts.
    /// </para>
    /// 
    /// <code>
    /// IEnumerable&lt;IEnumerable&lt;char>> enumerable = new List&lt;List&lt;char>>() { new() { 'a', 'b', 'c' }, new() { 'd' } };
    /// UniJaggedD2&lt;char> jagged = new(enumerable);
    /// Assert(jagged.Length1() == 2);
    /// Assert(jagged.Length2(0) == 3);
    /// Assert(jagged.Length2(1) == 1);
    /// Assert(jagged[0, 2] == 'c');
    /// Assert(jagged.GetOrNone(1, 0) == Some('d'));
    /// Assert(jagged.GetOrNone(0, 3).IsNone);
    /// Assert(jagged.GetOrNone(2, 0).IsNone);
    /// </code>
    /// </summary>
    /// <param name="enumerable">Underlying enumerable of the unified jagged collection.</param>
    public UniJaggedD2(IEnumerable<IEnumerable<T>> enumerable)
    {
        Length1 = () => enumerable.Count();
        Length2 = i => enumerable.ElementAt(i).Count();

        Get = (i, j) => enumerable.ElementAt(i).ElementAt(j);
        GetOrNone = (i, j) => enumerable.GetOrNone(i).FlatMap(x1 => x1.GetOrNone(j));

        AsEnumerable = () => enumerable;

        UnderlyingScalar = None<T>();
    }


    // index
    /// <summary>
    /// Returns the <paramref name="i"/>-th D1 collection.
    /// <code>
    /// var array = new char[][] { new char[] { 'a', 'b', 'c' }, new char[] { 'd' } };
    /// UniJaggedD2&lt;char> jagged2 = new(array);
    /// 
    /// UniJaggedD1&lt;char> jagged1 = jagged2[0];
    /// Assert(jagged1.Length1() == jagged2.Length2(0) == 3);
    /// Assert(jagged1[2] == jagged2[0, 2] == 'c');
    /// </code>
    /// </summary>
    /// <param name="i">Index of the D1 collection to create.</param>
    public UniJaggedD1<T> this[int i]
    {
        get
        {
            var get = Get;
            var getOrNone = GetOrNone;
            var asEnumerable = AsEnumerable;
            return new(Length2(i), j => get(i, j), j => getOrNone(i, j), () => asEnumerable().ElementAt(i), UnderlyingScalar);
        }
    }
    /// <summary>
    /// Directly returns the element at the (i,j)-th position.
    /// Use <see cref="GetOrNone"/> for the bound-checked optional version.
    /// 
    /// <code>
    /// int[][] underlyingArray = new int[] { new int[] { 0, 10 }, new int[] { 1, 11, 111, 1111 }, new int[] { 2, 22, 222 } };
    /// UniJaggedD2&lt;int> jagged = new(underlyingArray);
    /// Assert(jagged[1, 0] == 1);
    /// Assert(jagged[2, 2] == 222);
    /// // var x = jagged[-1, 0]; => out-of-bounds, throws!
    /// // var x = jagged[1, 4]; => out-of-bounds, throws!
    /// </code>
    /// </summary>
    /// <param name="i">Index in the first dimension of the element to be retrieved.</param>
    /// <param name="j">Index in the second dimension of the element to be retrieved.</param>
    public T this[int i, int j]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Get(i, j);
    }
}
