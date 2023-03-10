using System.Runtime.CompilerServices;

namespace Fun.Collections;

/// <summary>
/// A unified 5-dimensional collection.
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
/// or to yield results computed by a function,
/// there will NOT be an underlying storage actually storing the values.
/// Similarly, if arrays or lists are used as the underlying data sources, the unified collection only holds a reference through the closures.
/// </para>
/// 
/// <para>To clarify that the unified collection only holds a reference, consider the following example for 2D collection.</para>
/// <code>
/// int[][] underlyingArray = new int[] { new int[] { 0, 10 }, new int[] { 1, 11, 111, 1111 }, new int[] { 2, 22, 222 } };
/// UniJaggedD2&lt;int> jagged = new(underlyingArray);
/// Assert(underlyingArray[1][0] == 1 and jagged[1, 0] == 1);
/// underlyingArray[1][0] = 42;
/// Assert(underlyingArray[1][0] == 42 and jagged[1, 0] == 42);
/// </code>
/// 
/// <para>To further illustrate, consider the following with underlying list of lists for 2D collection.</para>
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
/// <item>Length1(): length of the collections in the first dimension.</item>
/// <item>Length2(i): length of the i-th collection.</item>
/// <item>Length3(i,j): length of the (i,j)-th collection.</item>
/// <item>Length4(i,j,k): length of the (i,j,k)-th collection.</item>
/// <item>Length5(i,j,k,l): length of the (i,j,k,l)-th collection.</item>
/// <item>this[i]: i-th D4 collection.</item>
/// <item>this[i,j,k,l,m]: element at the (i,j,k,l,m)-th position.</item>
/// <item>GetOrNone(i,j,k,l,m): returns Some of the element at the (i,j,k,l,m)-th position if indices are valid; None otherwise.</item>
/// <item>AsEnumerable(): returns the collection as IEnumerable; particularly useful for linq calls over the collection.</item>
/// </list>
/// 
/// </summary>
public class UniJaggedD5<T>
{
    // data
    internal readonly Func<int, int, int, int, int, T> Get;
    /// <summary>
    /// Length of the jagged collection in the first dimension.
    /// 
    /// <code>
    /// var array = new int[2][][][][]
    /// {
    ///    new int[1][][][]
    ///    {
    ///        new int[2][][]
    ///        {
    ///            new int[3][]
    ///            {
    ///                new int[2] { 0, 1 },
    ///                new int[3] { 2, 3, 4 },
    ///                Array.Empty&lt;int>(),
    ///            },
    ///            new int[1][]
    ///            {
    ///                new int[4] { 0, 1, 2, 3 },
    ///            }
    ///        }
    ///    },
    ///    Array.Empty&lt;int[][][]>()
    /// };
    /// UniJaggedD5&lt;int> jagged = new(array);
    /// 
    /// Assert(jagged.Length1() == 2);
    /// </code>
    /// </summary>
    public readonly Func<int> Length1;
    /// <summary>
    /// Length of the jagged collection in the second dimension.
    /// 
    /// <code>
    /// var array = new int[2][][][][]
    /// {
    ///    new int[1][][][]
    ///    {
    ///        new int[2][][]
    ///        {
    ///            new int[3][]
    ///            {
    ///                new int[2] { 0, 1 },
    ///                new int[3] { 2, 3, 4 },
    ///                Array.Empty&lt;int>(),
    ///            },
    ///            new int[1][]
    ///            {
    ///                new int[4] { 0, 1, 2, 3 },
    ///            }
    ///        }
    ///    },
    ///    Array.Empty&lt;int[][][]>()
    /// };
    /// UniJaggedD5&lt;int> jagged = new(array);
    /// 
    /// Assert(jagged.Length2(1) == 0);
    /// </code>
    /// </summary>
    public readonly Func<int, int> Length2;
    /// <summary>
    /// Length of the jagged collection in the second dimension.
    /// 
    /// <code>
    /// var array = new int[2][][][][]
    /// {
    ///    new int[1][][][]
    ///    {
    ///        new int[2][][]
    ///        {
    ///            new int[3][]
    ///            {
    ///                new int[2] { 0, 1 },
    ///                new int[3] { 2, 3, 4 },
    ///                Array.Empty&lt;int>(),
    ///            },
    ///            new int[1][]
    ///            {
    ///                new int[4] { 0, 1, 2, 3 },
    ///            }
    ///        }
    ///    },
    ///    Array.Empty&lt;int[][][]>()
    /// };
    /// UniJaggedD5&lt;int> jagged = new(array);
    /// 
    /// Assert(jagged.Length3(0, 0) == 2);
    /// </code>
    /// </summary>
    public readonly Func<int, int, int> Length3;
    /// <summary>
    /// Length of the jagged collection in the second dimension.
    /// 
    /// <code>
    /// var array = new int[2][][][][]
    /// {
    ///    new int[1][][][]
    ///    {
    ///        new int[2][][]
    ///        {
    ///            new int[3][]
    ///            {
    ///                new int[2] { 0, 1 },
    ///                new int[3] { 2, 3, 4 },
    ///                Array.Empty&lt;int>(),
    ///            },
    ///            new int[1][]
    ///            {
    ///                new int[4] { 0, 1, 2, 3 },
    ///            }
    ///        }
    ///    },
    ///    Array.Empty&lt;int[][][]>()
    /// };
    /// UniJaggedD5&lt;int> jagged = new(array);
    /// 
    /// Assert(jagged.Length4(0, 0, 1) == 1);
    /// </code>
    /// </summary>
    public readonly Func<int, int, int, int> Length4;
    /// <summary>
    /// Length of the jagged collection in the second dimension.
    /// 
    /// <code>
    /// var array = new int[2][][][][]
    /// {
    ///    new int[1][][][]
    ///    {
    ///        new int[2][][]
    ///        {
    ///            new int[3][]
    ///            {
    ///                new int[2] { 0, 1 },
    ///                new int[3] { 2, 3, 4 },
    ///                Array.Empty&lt;int>(),
    ///            },
    ///            new int[1][]
    ///            {
    ///                new int[4] { 0, 1, 2, 3 },
    ///            }
    ///        }
    ///    },
    ///    Array.Empty&lt;int[][][]>()
    /// };
    /// UniJaggedD5&lt;int> jagged = new(array);
    /// 
    /// Assert(jagged.Length5(0, 0, 0, 1) == 3);
    /// </code>
    /// </summary>
    public readonly Func<int, int, int, int, int> Length5;
    /// <summary>
    /// Safely gets Some of the element at the (i,j,k,l,m)-th position; returns None if the indices are invalid.
    /// 
    /// <code>
    /// var array = new int[2][][][][]
    /// {
    ///    new int[1][][][]
    ///    {
    ///        new int[2][][]
    ///        {
    ///            new int[3][]
    ///            {
    ///                new int[2] { 0, 1 },
    ///                new int[3] { 2, 3, 4 },
    ///                Array.Empty&lt;int>(),
    ///            },
    ///            new int[1][]
    ///            {
    ///                new int[4] { 0, 1, 2, 3 },
    ///            }
    ///        }
    ///    },
    ///    Array.Empty&lt;int[][][]>()
    /// };
    /// UniJaggedD5&lt;int> jagged = new(array);
    /// 
    /// Assert(jagged.GetOrNone(0, 0, 1, 0, 2) == Some(2));
    /// Assert(jagged.GetOrNone(0, 0, 1, 0, 4).IsNone);
    /// Assert(jagged.GetOrNone(3, 0, 0, 0, 0).IsNone);
    /// </code>
    /// 
    /// For other methods on the resulting optional, see <see cref="Opt{T}"/>.
    /// </summary>
    public readonly Func<int, int, int, int, int, Opt<T>> GetOrNone;
    /// <summary>
    /// Converts the unified jagged collection to IEnumerable yielding its underlying values.
    /// Particularly useful for linq calls over the collection.
    /// 
    /// <para>See below for two-dimensional exmaple.</para>
    /// <inheritdoc cref="UniJaggedD2{T}.AsEnumerable"/>
    /// </summary>
    public readonly Func<IEnumerable<IEnumerable<IEnumerable<IEnumerable<IEnumerable<T>>>>>> AsEnumerable;
    // data - constant
    /// <summary>
    /// <inheritdoc cref="UniJaggedD2{T}.UnderlyingScalar"/>
    /// </summary>
    public readonly Opt<T> UnderlyingScalar;
    /// <summary>
    /// <inheritdoc cref="UniJaggedD2{T}.UnderlyingScalar"/>
    /// </summary>
    public bool HasUnderlyingScalar => UnderlyingScalar.IsSome;


    // ctor
    internal UniJaggedD5(int length1, Func<int, int> length2, Func<int, int, int> length3, Func<int, int, int, int> length4, Func<int, int, int, int, int> length5, Func<int, int, int, int, int, T> get, Func<int, int, int, int, int, Opt<T>> getOrNone, Func<IEnumerable<IEnumerable<IEnumerable<IEnumerable<IEnumerable<T>>>>>> asEnumerable, Opt<T> underlyingConstantValue)
    {
        Length1 = () => length1;
        Length2 = length2;
        Length3 = length3;
        Length4 = length4;
        Length5 = length5;
        Get = get;
        GetOrNone = getOrNone;
        AsEnumerable = asEnumerable;
        UnderlyingScalar = underlyingConstantValue;
    }
    /// <summary>
    /// 5-dimensional jagged collection with optional lengths, which always yields the same constant value.
    /// 
    /// <para>See <see cref="UniJaggedD2{T}.UniJaggedD2(T, Opt{int}, Opt{Func{int, int}})"/> for two-dimensional examples.</para>
    /// </summary>
    /// <param name="constantValue">Constant value that every position of the jagged array will return.</param>
    /// <param name="length1">Optional length of the jagged array; will default to None (int.MaxValue) when omitted.</param>
    /// <param name="getLength2">Optional function (i -> length) to get length of the i-th collection; when omitted will default to None which will yield to a length of int.MaxValue for any index.</param>
    /// <param name="getLength3">Optional function (i,j -> length) to get length of the (i,j)-th collection; when omitted will default to None which will yield to a length of int.MaxValue for any indices.</param>
    /// <param name="getLength4">Optional function (i,j,k -> length) to get length of the (i,j,k)-th collection; when omitted will default to None which will yield to a length of int.MaxValue for any indices.</param>
    /// <param name="getLength5">Optional function (i,j,k,l -> length) to get length of the (i,j,k,l)-th collection; when omitted will default to None which will yield to a length of int.MaxValue for any indices.</param>
    public UniJaggedD5(T constantValue, Opt<int> length1 = default, Opt<Func<int, int>> getLength2 = default,
        Opt<Func<int, int, int>> getLength3 = default, Opt<Func<int, int, int, int>> getLength4 = default,
        Opt<Func<int, int, int, int, int>> getLength5 = default)
    {
        var len1 = () => length1.UnwrapOr(int.MaxValue);
        var len2 = getLength2.UnwrapOr(i => int.MaxValue);
        var len3 = getLength3.UnwrapOr((i, j) => int.MaxValue);
        var len4 = getLength4.UnwrapOr((i, j, k) => int.MaxValue);
        var len5 = getLength5.UnwrapOr((i, j, k, l) => int.MaxValue);

        Length1 = len1;
        Length2 = len2;
        Length3 = len3;
        Length4 = len4;
        Length5 = len5;

        Get = (_, _, _, _, _) => constantValue;
        GetOrNone = (_, _, _, _, _) => Some(constantValue);

        AsEnumerable = ()
            => Range(len1())
            .Select(i => Range(len2(i))
            .Select(j => Range(len3(i, j))
            .Select(k => Range(len4(i, j, k))
            .Select(l => Range(len5(i, j, k, l))
            .Select(m => constantValue)))));
        UnderlyingScalar = Some(constantValue);
    }
    /// <summary>
    /// 5-dimensional jagged collection with optional lengths, values of which are determined by the <paramref name="getValue"/> function.
    /// 
    /// <para>See <see cref="UniJaggedD2{T}.UniJaggedD2(Func{int, int, T}, Opt{int}, Opt{Func{int, int}})"/> for two-dimensional examples.</para>
    /// </summary>
    /// <param name="getValue">Function (indices -> value) that returns the value of the element at the given position.</param>
    /// <param name="length1">Optional length of the jagged array; will default to None (int.MaxValue) when omitted.</param>
    /// <param name="getLength2">Optional function (i -> length) to get length of the i-th collection; when omitted will default to None which will yield to a length of int.MaxValue for any index.</param>
    /// <param name="getLength3">Optional function (i,j -> length) to get length of the (i,j)-th collection; when omitted will default to None which will yield to a length of int.MaxValue for any indices.</param>
    /// <param name="getLength4">Optional function (i,j,k -> length) to get length of the (i,j,k)-th collection; when omitted will default to None which will yield to a length of int.MaxValue for any indices.</param>
    /// <param name="getLength5">Optional function (i,j,k,l -> length) to get length of the (i,j,k,l)-th collection; when omitted will default to None which will yield to a length of int.MaxValue for any indices.</param>
    public UniJaggedD5(Func<int, int, int, int, int, T> getValue, Opt<int> length1 = default, Opt<Func<int, int>> getLength2 = default,
        Opt<Func<int, int, int>> getLength3 = default, Opt<Func<int, int, int, int>> getLength4 = default,
        Opt<Func<int, int, int, int, int>> getLength5 = default)
    {
        var len1 = () => length1.UnwrapOr(int.MaxValue);
        var len2 = getLength2.UnwrapOr(i => int.MaxValue);
        var len3 = getLength3.UnwrapOr((i, j) => int.MaxValue);
        var len4 = getLength4.UnwrapOr((i, j, k) => int.MaxValue);
        var len5 = getLength5.UnwrapOr((i, j, k, l) => int.MaxValue);

        Length1 = len1;
        Length2 = len2;
        Length3 = len3;
        Length4 = len4;
        Length5 = len5;

        Get = (i, j, k, l, m) => getValue(i, j, k, l, m);
        GetOrNone = (i, j, k, l, m) =>
        {
            if (i > -1 && i < len1())
            {
                if (j > -1 && j < len2(i))
                {
                    if (k > -1 && k < len3(i, j))
                    {
                        if (l > -1 && l < len4(i, j, k))
                        {
                            if (m > -1 && m < len5(i, j, k, l))
                                return Some(getValue(i, j, k, l, m));
                        }
                    }
                }
            }
            return None<T>();
        };

        AsEnumerable = ()
            => Range(len1())
            .Select(i => Range(len2(i))
            .Select(j => Range(len3(i, j))
            .Select(k => Range(len4(i, j, k))
            .Select(l => Range(len5(i, j, k, l))
            .Select(m => getValue(i, j, k, l, m))))));
        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 5-dimensional jagged collection lengths and values of which are determined by the underlying <paramref name="array"/>.
    /// 
    /// <para>See <see cref="UniJaggedD2{T}.UniJaggedD2(T[][])"/> for two-dimensional examples.</para>
    /// </summary>
    /// <param name="array">Underlying array of the unified jagged collection.</param>
    public UniJaggedD5(T[][][][][] array)
    {
        int len1() => array.Length;
        int len2(int i) => array[i].Length;
        int len3(int i, int j) => array[i][j].Length;
        int len4(int i, int j, int k) => array[i][j][k].Length;
        int len5(int i, int j, int k, int l) => array[i][j][k][l].Length;

        Length1 = len1;
        Length2 = len2;
        Length3 = len3;
        Length4 = len4;
        Length5 = len5;

        Get = (i, j, k, l, m) => array[i][j][k][l][m];
        GetOrNone = (i, j, k, l, m) =>
        {
            if (i > -1 && i < len1())
            {
                if (j > -1 && j < len2(i))
                {
                    if (k > -1 && k < len3(i, j))
                    {
                        if (l > -1 && l < len4(i, j, k))
                        {
                            if (m > -1 && m < len5(i, j, k, l))
                                return Some(array[i][j][k][l][m]);
                        }
                    }
                }
            }
            return None<T>();
        };

        AsEnumerable = () => Range(len1()).Select(i => array[i]);
        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 5-dimensional jagged collection lengths and values of which are determined by the underlying <paramref name="array"/>.
    /// 
    /// <para>See <see cref="UniJaggedD2{T}.UniJaggedD2(T[,])"/> for two-dimensional examples.</para>
    /// </summary>
    /// <param name="array">Underlying array of the unified jagged collection.</param>
    public UniJaggedD5(T[,,,,] array)
    {
        int len1() => array.Length;
        int len2(int i) => array.GetLength(1);
        int len3(int i, int j) => array.GetLength(2);
        int len4(int i, int j, int k) => array.GetLength(3);
        int len5(int i, int j, int k, int l) => array.GetLength(4);

        Length1 = len1;
        Length2 = len2;
        Length3 = len3;
        Length4 = len4;
        Length5 = len5;

        Get = (i, j, k, l, m) => array[i, j, k, l, m];
        GetOrNone = (i, j, k, l, m) =>
        {
            if (i > -1 && i < len1())
            {
                if (j > -1 && j < len2(i))
                {
                    if (k > -1 && k < len3(i, j))
                    {
                        if (l > -1 && l < len4(i, j, k))
                        {
                            if (m > -1 && m < len5(i, j, k, l))
                                return Some(array[i, j, k, l, m]);
                        }
                    }
                }
            }
            return None<T>();
        };

        AsEnumerable = ()
            => Range(len1())
            .Select(i => Range(len2(i))
            .Select(j => Range(len3(i, j))
            .Select(k => Range(len4(i, j, k))
            .Select(l => Range(len5(i, j, k, l))
            .Select(m => array[i, j, k, l, m])))));
        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 5-dimensional jagged collection lengths and values of which are determined by the underlying <paramref name="list"/>.
    /// 
    /// <para>See <see cref="UniJaggedD2{T}.UniJaggedD2(List{List{T}})"/> for two-dimensional examples.</para>
    /// </summary>
    /// <param name="list">Underlying list of the unified jagged collection.</param>
    public UniJaggedD5(List<List<List<List<List<T>>>>> list)
    {
        int len1() => list.Count;
        int len2(int i) => list[i].Count;
        int len3(int i, int j) => list[i][j].Count;
        int len4(int i, int j, int k) => list[i][j][k].Count;
        int len5(int i, int j, int k, int l) => list[i][j][k][l].Count;

        Length1 = len1;
        Length2 = len2;
        Length3 = len3;
        Length4 = len4;
        Length5 = len5;

        Get = (i, j, k, l, m) => list[i][j][k][l][m];
        GetOrNone = (i, j, k, l, m) =>
        {
            if (i > -1 && i < len1())
            {
                if (j > -1 && j < len2(i))
                {
                    if (k > -1 && k < len3(i, j))
                    {
                        if (l > -1 && l < len4(i, j, k))
                        {
                            if (m > -1 && m < len5(i, j, k, l))
                                return Some(list[i][j][k][l][m]);
                        }
                    }
                }
            }
            return None<T>();
        };

        AsEnumerable = () => Range(len1()).Select(i => list[i]);
        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 5-dimensional jagged collection lengths and values of which are determined by the underlying <paramref name="list"/>.
    /// 
    /// <para>See <see cref="UniJaggedD2{T}.UniJaggedD2(List{List{T}})"/> for two-dimensional examples.</para>
    /// </summary>
    /// <param name="list">Underlying list of the unified jagged collection.</param>
    public UniJaggedD5(IList<IList<IList<IList<IList<T>>>>> list)
    {
        int len1() => list.Count;
        int len2(int i) => list[i].Count;
        int len3(int i, int j) => list[i][j].Count;
        int len4(int i, int j, int k) => list[i][j][k].Count;
        int len5(int i, int j, int k, int l) => list[i][j][k][l].Count;

        Length1 = len1;
        Length2 = len2;
        Length3 = len3;
        Length4 = len4;
        Length5 = len5;

        Get = (i, j, k, l, m) => list[i][j][k][l][m];
        GetOrNone = (i, j, k, l, m) =>
        {
            if (i > -1 && i < len1())
            {
                if (j > -1 && j < len2(i))
                {
                    if (k > -1 && k < len3(i, j))
                    {
                        if (l > -1 && l < len4(i, j, k))
                        {
                            if (m > -1 && m < len5(i, j, k, l))
                                return Some(list[i][j][k][l][m]);
                        }
                    }
                }
            }
            return None<T>();
        };

        AsEnumerable = () => Range(len1()).Select(i => list[i]);
        UnderlyingScalar = None<T>();
    }
    /// <summary>
    /// 5-dimensional jagged collection lengths and values of which are determined by the underlying <paramref name="enumerable"/>.
    /// 
    /// <para>
    /// Note that underlying Count() method is used to determine lengths of the collections.
    /// This means that, it might require linear search if the underlying collections do not have a trivial counts.
    /// </para>
    /// 
    /// <para>See <see cref="UniJaggedD2{T}.UniJaggedD2(IEnumerable{IEnumerable{T}})"/> for two-dimensional examples.</para>
    /// </summary>
    /// <param name="enumerable">Underlying enumerable of the unified jagged collection.</param>
    public UniJaggedD5(IEnumerable<IEnumerable<IEnumerable<IEnumerable<IEnumerable<T>>>>> enumerable)
    {
        int len1() => enumerable.Count();
        int len2(int i) => enumerable.ElementAt(i).Count();
        int len3(int i, int j) => enumerable.ElementAt(i).ElementAt(j).Count();
        int len4(int i, int j, int k) => enumerable.ElementAt(i).ElementAt(j).ElementAt(k).Count();
        int len5(int i, int j, int k, int l) => enumerable.ElementAt(i).ElementAt(j).ElementAt(k).ElementAt(l).Count();

        Length1 = len1;
        Length2 = len2;
        Length3 = len3;
        Length4 = len4;
        Length5 = len5;

        Get = (i, j, k, l, m) => enumerable.ElementAt(i).ElementAt(j).ElementAt(k).ElementAt(l).ElementAt(m);
        GetOrNone = (i, j, k, l, m) =>
        {
            if (i > -1 && i < len1())
            {
                if (j > -1 && j < len2(i))
                {
                    if (k > -1 && k < len3(i, j))
                    {
                        if (l > -1 && l < len4(i, j, k))
                        {
                            if (m > -1 && m < len5(i, j, k, l))
                                return Some(enumerable.ElementAt(i).ElementAt(j).ElementAt(k).ElementAt(l).ElementAt(m));
                        }
                    }
                }
            }
            return None<T>();
        };

        AsEnumerable = () => enumerable;
        UnderlyingScalar = None<T>();
    }


    // index
    /// <summary>
    /// Returns the <paramref name="i"/>-th D4 collection.
    /// <code>
    /// var array = new int[2][][][][]
    /// {
    ///    new int[1][][][]
    ///    {
    ///        new int[2][][]
    ///        {
    ///            new int[3][]
    ///            {
    ///                new int[2] { 0, 1 },
    ///                new int[3] { 2, 3, 4 },
    ///                Array.Empty&lt;int>(),
    ///            },
    ///            new int[1][]
    ///            {
    ///                new int[4] { 0, 1, 2, 3 },
    ///            }
    ///        }
    ///    },
    ///    Array.Empty&lt;int[][][]>()
    /// };
    /// UniJaggedD5&lt;int> jagged = new(array);
    /// 
    /// int i = 0;
    /// UniJaggedD4&lt;char> jaggedI = jagged[i];
    /// Assert(jaggedI.Length1() == jagged.Length2(i) == 2);
    /// Assert(jaggedI[0, 1, 2] == jagged[0, 0, 1, 2] == 2);
    /// </code>
    /// </summary>
    /// <param name="i">Index of the D4 collection to create.</param>
    public UniJaggedD4<T> this[int i]
    {
        get
        {
            var get = Get;
            var getOrNone = GetOrNone;
            var asEnumerable = AsEnumerable;
            var length3 = Length3;
            var length4 = Length4;
            var length5 = Length5;
            return new(Length2(i), j => length3(i, j), (j, k) => length4(i, j, k), (j, k, l) => length5(i, j, k, l),
                (j, k, l, m) => get(i, j, k, l, m),
                (j, k, l, m) => getOrNone(i, j, k, l, m),
                () => asEnumerable().ElementAt(i),
                UnderlyingScalar);
        }
    }
    /// <summary>
    /// Directly returns the element at the (i,j,k,l,m)-th position.
    /// Use <see cref="GetOrNone"/> for the bound-checked optional version.
    /// 
    /// <code>
    /// var array = new int[2][][][][]
    /// {
    ///    new int[1][][][]
    ///    {
    ///        new int[2][][]
    ///        {
    ///            new int[3][]
    ///            {
    ///                new int[2] { 0, 1 },
    ///                new int[3] { 2, 3, 4 },
    ///                Array.Empty&lt;int>(),
    ///            },
    ///            new int[1][]
    ///            {
    ///                new int[4] { 0, 1, 2, 3 },
    ///            }
    ///        }
    ///    },
    ///    Array.Empty&lt;int[][][]>()
    /// };
    /// UniJaggedD5&lt;int> jagged = new(array);
    /// 
    /// Assert(jagged[0, 0, 1, 0, 2] == 2);
    /// // var number = jagged[0, 0, 1, 0, 4]; // out-of-bounds, throws!
    /// // var number = jagged[3, 0, 0, 0, 0]; // out-of-bounds, throws!
    /// </code>
    /// </summary>
    /// <param name="i">Index in the first dimension of the element to be retrieved.</param>
    /// <param name="j">Index in the second dimension of the element to be retrieved.</param>
    /// <param name="k">Index in the third dimension of the element to be retrieved.</param>
    /// <param name="l">Index in the fourth dimension of the element to be retrieved.</param>
    /// <param name="m">Index in the fifth dimension of the element to be retrieved.</param>
    public T this[int i, int j, int k, int l, int m]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Get(i, j, k, l, m);
    }
}
