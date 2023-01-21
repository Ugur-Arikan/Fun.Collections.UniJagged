# Fun.Collections.UniJagged

This library provides jagged array types enabling access by indices.
The jagged array types wraps different representations to achieve a unified representation.

Currently, from one-dimensional to eight-dimensional jagged arrays are supported.

Complete auto-generated documentation can be found here:
**[sandcastle-documentation](https://ugur-arikan.github.io/Fun.Collections.UniJagged/docs/index.html)**.

## Addressed Problem

Consider the following method that requires a two dimensional (2D) jagged array to read from.

```csharp
Path CalculateShortestPath(double[][] distanceMatrix, int origin, int destination)
{
	// the method creates the shortest path from origin to destination
    // reading required distances from teh distance matrix, where
    // distanceMatrix[i][j] -> distance from i to j.
}
```
Note that, there exist other relevant representations to enable access by indices:

**1. Other Standard Collections**

For the 2D double example above, some alternatives are `List<List<double>>`, `List<double[]>`, `IList<double>[]`, `IEnumerable<IEnumerable<double>>`, etc.

**2. Functions: On Demand Computation**

One example case for using functions is as follows. The `CalculateShortestPath` method might not require all entries of the jagged array. It might be computationally expensive to compute all distances beforehand; or memory-wise expensive to store all entries.

Consider the following on-request distance provider for instance:

```csharp
double GetEuclideanDistance(int tail, int head)
{
	var (x1, y1) = GetCoordinatesOf(tail);
    var (x2, y2) = GetCoordinatesOf(head);
	return  Math.Sqrt((Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2)));
}
```
This function, although not storing the elements, fits into the requirement; it provides indexed access to data.

**3. Functions: Immutability**

A second case for using functions could be related to the limited language support for immutability. Simply, one would prefer to pass the closure `(i, j) => distanceMatrix[i][j]` rather than passing directly the array `double[][] distanceMatrix` to prevent the consumer from mutating the array.

**4. Scalars**

Consider a collection that might have different values at different elements; or exactly the same constant/scalar value at every element. One general representation for this case is to replicate the scalar value for each entry; however, this solution would be memory inefficient. A better general representation could be to handle these variants separately.

For the above example; assume we want to call `CalculateShortestPath` method with two different distance matrices:

* the first distance matrix represents distances among physical locations; and hence, contains heterogeneous distances;
* and every element of the second distance matrix is exactly one, so that the `CalculateShortestPath` provides the path with the minimum number of edges/connections.

The first case is clear. To call `CalculateShortestPath` method to handle the second case, one needs to populate a jagged array, all entries of which are 1.

```chsarp
var distanceMatrix = new double[n][];
for (int i = 0; i < distanceMatrix.Length; i++)
{
	distanceMatrix[i] = new double[GetOutDegree(i)];
    for (int j = 0; j < distanceMatrix[i].Length; j++)
    	distanceMatrix[i][j] = 1;
}
// lots of code & memory, just to return 1.
```

## Proposed Solution

Consider now the alternative implementation of the `CalculateShortestPath` method, which is completely identical to the original, except for:

* instead of a specific `double[][]`, it expects `UniJaggedD2<double>` which can be any of many possible representations that allows access by indices;
* instead of using `distanceMatrix[i][j]` to access the (i,j)-th element, we use `distanceMatrix[i, j]`.


```csharp
Path CalculateShortestPath(UniJaggedD2<double> distanceMatrix, int origin, int destination)
{
    // distanceMatrix[i, j] -> distance from i to j.
}
```

Now the caller can freely choose the underlying data type without having to change the method signature or implementation.
For instance, all below examples are valid calls for the implementation accepting the unified jagged array.

```csharp
int origin = 0;
int destination = 10;

// array
double[][] distMatArr = ... // get the data as array of arrays
var shortestPath = CalculateShortestPath(new(distMatArr), origin, destination);

// list
List<List<double>> distMatList = ... // get the data as list of lists
var shortestPath = CalculateShortestPath(new(distMatList), origin, destination);

// scalar
UniJaggedD2<double> distMatOne = new(1); // distMatOne[i, j] = 1 for every i, j
var shortestPath = CalculateShortestPath(distMatOne, origin, destination);
// scalar - brief version
var shortestPath = CalculateShortestPath(new(1), origin, destination);

// functional - on demand computation
record Location(double X, double Y);
record DistanceProvider(Location[] Locations)
{
	public double GetEuclideanDistance(int tailIndex, int headIndex)
    {
    	var tail = Locations[tailIndex];
        var head = Locations[headIndex];
        return  Math.Sqrt((Math.Pow(tail.X - head.X, 2) + Math.Pow(tail.Y - head.Y, 2)));
    }
}
DistanceProvider provider = new(GetLocations());

UniJaggedD2<double> distMatFun = new((i,j) => provider.GetEuclideanDistance(i, j));
UniJaggedD2<double> distMatFun = new(provider.GetEuclideanDistance); // more briefly
var shortestPath = CalculateShortestPath(distMatFun, origin, destination);
```
