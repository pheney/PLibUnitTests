using UnityEngine;
using System.Collections.Generic;
using System;
using PLib.Math;
using PLib.General;

/// <summary>
/// 2016-2-26
/// Tile map utility.
/// Includes 2d and 3d coordinates and 'maps' (rectangular arrays of integers).
/// Basic functions of maps include:
///     map.ForEach(), map.SetAll(), map.ContainsCoords(), map.GetNeighborsOf()
/// </summary>
namespace PLib.Map
{
    public static partial class PMap
    {
        ////////////////////////////
        //	Coordinates     	////
        ////////////////////////////

        #region Coordinates

        /// <summary>
        /// Extended form of a Vector (either Vector2 or Vector3).
        /// 
        /// </summary>
        public interface ICoordinate
        {
            ////  define basic operators for this structure

            /// <summary>
            /// Content comparison (using values)
            /// Returns if all the values of this object match the parameter's values.
            /// </summary>
            bool Equals(object other);

            /// <summary>
            /// Returns x ^ y ^ z
            /// Ref: https://msdn.microsoft.com/en-us/library/336aedhh(v=vs.85).aspx
            /// </summary>
            /// <returns></returns>
            int GetHashCode();

            /// <summary>
            /// Mimics vector math.
            /// Returns a new coordinate where
            ///     x = this.x - other.x
            ///     y = this.y - other.y
            ///     z = this.z - other.z
            /// </summary>
            float DistanceTo(ICoordinate other);

            Vector2 ToVector2();
            Vector3 ToVector3();
        }

        /// <summary>
        /// <para>
        /// 2016-1-14
        /// </para><para>
        /// An integer coordinate. Typically used for identifying locations in a 2d grid.
        /// If the constructor is passed float values, they will be rounded to ints using Mathf.RoundToInt()
        /// </para><para>
        /// Class vs Struct
        /// </para><para>
        /// Ref: https://msdn.microsoft.com/en-us/library/ms173109.aspx
        /// </para><para>
        /// CLASS (reference type)
        ///     1. Objects assigned to new variables refer to the original object.
        ///     2. Reference types are allocated on the heap and garbage-collected.
        ///     3. Allocations and deallocations of classes are generally more expensive than structs.
        ///     4. Arrays of classes contain pointers.
        ///     5. Allocoations and deallocation of arrays are much more expensive than structs.
        ///     6. Assignments of large classes are cheaper than structs.
        /// </para><para>
        /// STRUCT (value type)
        ///     1. Structs assigned to new variables are copied in their entirety.
        ///     2. Structs are allocated either on the stack or inline in containing types and deallocated when the stack unwinds
        ///         or the containing type gets deallocated.
        ///     3. Allocations and deallocations of structs are generally cheaper than classes.
        ///     4. Arrays of structs contain copies of values.
        ///     5. Allocoations and deallocation of arrays are much cheaper than classes.
        ///     6. Arrays of structs (usually) demonstrate better locality of reference. (They handle duplicates better.)
        ///     7. Structs are boxed and unboxed when cast to reference types or interfaces. This can have a negative impact on performance.
        ///     8. Assignments of large structs are more expensive than classes.
        /// </para><para>
        /// Guidelines for using structs (structs should meet ALL of the following conditions):
        ///     1. It logically represents a single value, similar to a primitive type, e.g., int, double, etc.
        ///     2. It has an instance size under 16 bytes.
        ///         Bool - 1 byte
        ///         Short, Char - 2 bytes
        ///         Float, Int, uInt - 4 bytes
        ///         Double, DateTime - 8 bytes
        ///         Decimal - 16 bytes
        ///         String - 2 bytes per character
        ///     3. It is immutable.
        ///     4. It will not have to be boxed frequently.
        /// </para>
        /// </summary>
        [System.Serializable]
        public struct Coord2D : ICoordinate, System.IEquatable<Coord2D>
        {
            #region fields

            public Vector2 position;
            public int x
            {
                get
                {
                    return (int)position.x;
                }
            }
            public int y
            {
                get
                {
                    return (int)position.y;
                }
            }

            #endregion
            #region constructors

            public Coord2D(float xCoord, float yCoord)
            {
                this.position = new Vector2(Mathf.RoundToInt(xCoord), Mathf.RoundToInt(yCoord));
            }

            public Coord2D(Vector2 position) : this(position.x, position.y) { }

            #endregion
            #region ICoordinate

            /// <summary>
            /// Content comparison (using values)
            /// Returns if the x/y values of this object match the parameter's x/y values.
            /// </summary>
            public override bool Equals(object other)
            {
                //  check run-time types and null values
                if (other == null || GetType() != other.GetType()) return false;

                //  do value-by-value comparison
                return this.GetHashCode().Equals(((Coord2D)other).GetHashCode());
            }

            /// <summary>
            /// Returns x ^ y
            /// Ref: https://msdn.microsoft.com/en-us/library/336aedhh(v=vs.85).aspx
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return this.position.GetHashCode();
            }

            /// <summary>
            /// Mimics vector math.
            /// Returns a new coordinate where
            ///     x = left.x - right.x
            ///     y = left.y - right.y
            /// </summary>
            public float DistanceTo(ICoordinate other)
            {
                Coord2D o = (Coord2D)other;
                return (this.position - o.position).sqrMagnitude;
            }

            public Vector2 ToVector2()
            {
                return this.position;
            }

            public Vector3 ToVector3()
            {
                return this.position.ToVector3();
            }

            #endregion
            #region IEquatable

            bool System.IEquatable<Coord2D>.Equals(Coord2D other)
            {
                return this.position == other.position;
            }

            #endregion
        }

        /// <summary>
        /// 2016-2-26
        /// An integer coordinate. Typically used for identifying locations in a 3d grid.
        /// If the constructor is passed float values, they will be rounded to ints using Mathf.RoundToInt()
        [System.Serializable]
        public struct Coord3D : ICoordinate, System.IEquatable<Coord3D>
        {
            #region fields

            public Vector3 position;
            public int x
            {
                get
                {
                    return (int)position.x;
                }
            }
            public int y
            {
                get
                {
                    return (int)position.y;
                }
            }
            public int z
            {
                get
                {
                    return (int)position.z;
                }
            }

            #endregion
            #region constructors

            public Coord3D(float xCoord, float yCoord, float zCoord)
            {
                this.position = new Vector3(Mathf.RoundToInt(xCoord), Mathf.RoundToInt(yCoord), Mathf.RoundToInt(zCoord));
            }

            public Coord3D(Vector3 position) : this(position.x, position.y, position.z) { }

            #endregion
            #region ICoordinate

            /// <summary>
            /// Content comparison (using values)
            /// Returns if the x/y values of this object match the parameter's x/y values.
            /// </summary>
            public override bool Equals(object other)
            {
                //  check run-time types and null values
                if (other == null || GetType() != other.GetType()) return false;

                //  do value-by-value comparison
                return this.GetHashCode().Equals(((Coord3D)other).GetHashCode());
            }

            /// <summary>
            /// Returns x ^ y ^ z
            /// Ref: https://msdn.microsoft.com/en-us/library/336aedhh(v=vs.85).aspx
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return this.position.GetHashCode();
            }

            /// <summary>
            /// Mimics vector math.
            /// Returns a new coordinate where
            ///     x = left.x - right.x
            ///     y = left.y - right.y
            /// </summary>
            public float DistanceTo(ICoordinate other)
            {
                Coord3D o = (Coord3D)other;
                return (this.position - o.position).sqrMagnitude;
            }

            public Vector2 ToVector2()
            {
                return this.position.ToVector2();
            }

            public Vector3 ToVector3()
            {
                return this.position;
            }

            #endregion
            #region IEquatable

            bool System.IEquatable<Coord3D>.Equals(Coord3D other)
            {
                return this.position == other.position;
            }

            #endregion
        }

        #endregion

        ////////////////////////////
        //	Rectangular Arrays	////
        ////////////////////////////

        #region Rectangular Arrays

        public static void ForEach<T>(this T[,] source, Action<T> action)
        {
            foreach (T t in source) action(t);
        }

        public static void ForEach<T>(this T[,,] source, Action<T> action)
        {
            foreach (T t in source) action(t);
        }

        public static void ForEach<T>(this T[,] source, Action<int, int> actionByIndex)
        {
            int x = source.GetLength(0);
            int y = source.GetLength(1);

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    actionByIndex(i, j);
                }
            }
        }

        public static void ForEach<T>(this T[,,] source, Action<int, int, int> actionByIndex)
        {
            int x = source.GetLength(0);
            int y = source.GetLength(1);
            int z = source.GetLength(2);

            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                    for (int k = 0; k < z; k++)
                    {
                        actionByIndex(i, j, k);
                    }
        }

        /// <summary>
        /// 2016-2-22
        /// Sets all values in an array to the value provided
        /// </summary>
        /// <returns>The modified array</returns>
        /// <param name="source">A array</param>
        /// <param name="value">A value to assign to all locaitons</param>
        public static T[] SetAll<T>(this T[] source, T value)
        {
            int length = source.Length;

            for (int i = 0; i < length; i++)
            {
                source[i] = value;
            }
            return source;
        }

        /// <summary>
        /// 2015-12-28
        /// Sets all values in a rectangular array to the value provided
        /// </summary>
        /// <returns>The modified rectangular array</returns>
        /// <param name="source">A rectangular array</param>
        /// <param name="value">A value to assign to all locaitons</param>
        public static T[,] SetAll<T>(this T[,] source, T value)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    source[i, j] = value;
                }
            }
            return source;
        }

        /// <summary>
        /// 2016-2-22
        /// Sets all values in a cubic array to the value provided
        /// </summary>
        /// <returns>The modified cubic array</returns>
        /// <param name="source">A cubic array</param>
        /// <param name="value">A value to assign to all locaitons</param>
        public static T[,,] SetAll<T>(this T[,,] source, T value)
        {
            int length = source.GetLength(0);
            int width = source.GetLength(1);
            int height = source.GetLength(2);

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    for (int k = 0; k < height; k++)
                    {
                        source[i, j, k] = value;
                    }
                }
            }
            return source;
        }

        /// <summary>
        /// 2015-12-28
        /// Indicates if the provided x,y (row,col) coordinates are contained
        /// within the rectangular array.
        /// 
        /// For example, (-1,-1) would return false because these are not valid indicies
        /// in any array.
        /// </summary>
        public static bool ContainsCoords<T>(this T[,] source, int row, int col)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);

            return row >= 0 && row < width && col >= 0 && col < height;
        }

        /// <summary>
        /// 2016-2-22
        /// Indicates if the provided x,y,z (row,col,height) coordinates are contained
        /// within the rectangular array.
        /// 
        /// For example, (-1,-1, 1) would return false because these are not valid indicies
        /// in any array.
        /// </summary>
        public static bool ContainsCoords<T>(this T[,,] source, int x, int y, int z)
        {
            int length = source.GetLength(0);
            int width = source.GetLength(1);
            int height = source.GetLength(2);

            return x >= 0 && x < length && y >= 0 && y < width && z > 0 && z <= height;
        }

        /// <summary>
        /// 2016-2-22
        /// Returns all the neighbors of the provided coordinates. Optional last parameter indicates the 
        /// neighbor filter:
        ///     0 -- return all neighbors
        ///     1 -- return orthogonal neighbors only
        ///     2 -- diagonal neighbors only
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static List<T> GetNeighborsOf<T>(this T[,] source, int x, int y, int filter = 0)
        {
            if (!source.ContainsCoords(x, y)) return null;

            List<T> neighbors = new List<T>();

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int indexX = x + i;
                    int indexY = y + j;

                    //  ignore coordinates that aren't on the map
                    if (!source.ContainsCoords(indexX, indexY)) continue;

                    //  ignore the center coordinate
                    if (i == 0 && j == 0) continue;

                    switch (filter)
                    {
                        case 1:
                            //  skip diagonals
                            if ((i == -1 || i == 1) && j != 0) continue;
                            break;
                        case 2:
                            //  skip orthogonals
                            if ((i == -1 || i == 1) && j == 0) continue;
                            break;
                        case 0:
                        default:
                            //  keep everything
                            break;
                    }
                    neighbors.Add(source[indexX, indexY]);
                }
            }

            return neighbors;
        }

        /// <summary>
        /// 2016-2-22
        /// Returns all the neighbors of the provided coordinates. Optional last parameter indicates the 
        /// neighbor filter:
        ///     0 -- return all neighbors
        ///     1 -- return orthogonal neighbors only
        ///     2 -- diagonal neighbors only
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static List<T> GetNeighborsOf<T>(this T[,,] source, int x, int y, int z, int filter = 0)
        {
            if (!source.ContainsCoords(x, y, z)) return null;

            List<T> neighbors = new List<T>();

            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        int indexX = x + i;
                        int indexY = y + j;
                        int indexZ = z + k;

                        //  ignore coordinates that aren't on the map
                        if (!source.ContainsCoords(indexX, indexY, indexZ)) continue;

                        //  ignore the center coordinate
                        if (i == 0 && j == 0 && k == 0) continue;

                        switch (filter)
                        {
                            case 1:
                                //  skip diagonals
                                if ((i == -1 || i == 1) && j != 0 && k != 0) continue;
                                if ((j == -1 || j == 1) && i != 0 && k != 0) continue;
                                if ((k == -1 || k == 1) && i != 0 && j != 0) continue;
                                break;
                            case 2:
                                //  skip orthogonals
                                if ((i == -1 || i == 1) && j == 0 && k == 0) continue;
                                if ((j == -1 || j == 1) && i == 0 && k == 0) continue;
                                if ((k == -1 || k == 1) && i == 0 && j == 0) continue;
                                break;
                            case 0:
                            default:
                                //  keep everything
                                break;
                        }
                        neighbors.Add(source[indexX, indexY, indexZ]);
                    }
                }
            }
            return neighbors;
        }

        #endregion

        ////////////////////////////
        //	Tiles           	////
        ////////////////////////////

        #region Tiles


        #endregion
    }

    /// <summary>
    /// 2016-2-26
    /// A* pathfinding (atatic algorithm)
    /// Create a rectangular or cubic array of AStarNodes, i.e., AStarNode[,] or AStarNode[,,]
    /// Usage: array.FindPath(start, destination)
    /// Returns: List of AStarNodes that make up the path, or null if there is no path.
    /// </summary>
    public static class AStar
    {
        /// <summary>
        /// 2016-2-22
        /// Interface for pathfinding nodes based on A*
        /// </summary>
        public interface INode : System.IComparable
        {
            /// <summary>
            /// Returns the cost to traverse this node
            /// </summary>
            /// <returns></returns>
            float GetTraverseCost();

            /// <summary>
            /// Assigns the traverse cost for this node. Returns true if the new cost is different from the previous cost.
            /// </summary>
            /// <param name="cost"></param>
            /// <returns></returns>
            bool SetTraverseCost(float cost);

            /// <summary>
            /// Returns the total cost to get to (and traverse) this node.
            /// This is the sum of this node's traverse cost, plus the total cost of the parent.
            /// </summary>
            /// <returns></returns>
            float GetTotalCost();

            /// <summary>
            /// Returns the estimated cost from this node to the destination.
            /// </summary>
            /// <returns></returns>
            float GetHeuristicCost();

            /// <summary>
            /// Updates the heuristic cost. Returns true if the new heuristic cost is
            /// different from the previous heuristic cost.
            /// </summary>
            /// <returns></returns>
            bool SetHeuristicCost(float cost);

            /// <summary>
            /// Returns a list of all nodes that are adjacent to this node
            /// </summary>
            /// <returns></returns>
            List<INode> GetNeighbors();

            /// <summary>
            /// Assigns a list of nodes to this node as neighbors. This will replace any existing neighbors. Returns true
            /// if this action changes the neighbor list.
            /// </summary>
            /// <param name="neighbors"></param>
            /// <returns></returns>
            bool SetNeighbors(List<INode> neighbors);

            /// <summary>
            /// Adds the node to this node's neighbors. Does not add duplicates. Returns true if the node is added to this
            /// node's neighbor list.
            /// </summary>
            /// <param name="neighbor"></param>
            /// <returns></returns>
            bool AddNeighbor(INode node);

            /// <summary>
            /// Removes the node from this node's neighbors, if it is a neighbor. Returns true if the operation is successful.
            /// </summary>
            /// <param name="neighbor"></param>
            /// <returns></returns>
            bool RemoveNeighbor(INode node);

            /// <summary>
            /// Assigns the parent node for this node. This is to build the path once the destination is located.
            /// </summary>
            /// <param name="node"></param>
            void SetParent(INode node);

            /// <summary>
            /// Returns the parent node for this node.
            /// </summary>
            /// <returns></returns>
            INode GetParent();
        }

        public static void UpdateHeuristicsForDestination(this AStarNode2D[,] grid, AStarNode2D destination)
        {
            foreach (AStarNode2D node in grid)
            {
                node.SetHeuristicCost(node.coordinate.DistanceTo(destination.coordinate));
            }
        }

        public static void UpdateHeuristicsForDestination(this AStarNode3D[,,] grid, AStarNode3D destination)
        {
            foreach (AStarNode3D node in grid)
            {
                node.SetHeuristicCost(node.coordinate.DistanceTo(destination.coordinate));
            }
        }

        public class AStarNode2D : INode
        {
            public PMap.ICoordinate coordinate { get; set; }
            public static INode[,] grid { get; set; }

            private float traverseCost;
            private float heuristicCost;
            private INode parent;

            #region constructors

            public AStarNode2D() { }
            public AStarNode2D(PMap.ICoordinate coordiante)
            {
                this.coordinate = coordinate;
            }
            public AStarNode2D(PMap.ICoordinate coordinate, INode[,] grid)
                : this(coordinate)
            {
                AStarNode2D.grid = grid;
            }

            #endregion
            #region Common A* operations

            public bool CoordinateEquals(AStarNode2D node)
            {
                return this.CoordinateEquals(node.coordinate);
            }

            public bool CoordinateEquals(PMap.ICoordinate coordinate)
            {
                return this.coordinate.Equals(coordinate);
            }

            #endregion
            #region INode interface

            public float GetTraverseCost()
            {
                return traverseCost;
            }

            public bool SetTraverseCost(float cost)
            {
                float original = this.traverseCost;
                this.traverseCost = cost;
                return this.traverseCost != original;
            }

            public float GetTotalCost()
            {
                return this.traverseCost + (this.GetParent() == null ? 0 : this.GetParent().GetTotalCost());
            }

            public float GetHeuristicCost()
            {
                return heuristicCost;
            }

            public bool SetHeuristicCost(float cost)
            {
                float original = this.heuristicCost;
                this.heuristicCost = cost;
                return this.heuristicCost != original;
            }

            public List<INode> GetNeighbors()
            {
                Vector2 location = coordinate.ToVector2();
                return grid.GetNeighborsOf((int)location.x, (int)location.y, 1);
            }

            public bool SetNeighbors(List<INode> neighbors)
            {
                return false;
            }

            public bool AddNeighbor(INode node)
            {
                return false;
            }

            public bool RemoveNeighbor(INode node)
            {
                return false;
            }

            public void SetParent(INode node)
            {
                this.parent = node;
            }

            public INode GetParent()
            {
                return this.parent;
            }

            #endregion
            #region IComparable (so nodes are sortable)

            public int CompareTo(object obj)
            {
                INode other = (INode)obj;
                float thisRank = this.GetHeuristicCost() + this.GetTotalCost();
                float otherRank = other.GetHeuristicCost() + other.GetTotalCost();
                return thisRank.CompareTo(otherRank);
            }

            #endregion
        }

        public class AStarNode3D : INode
        {
            public PMap.ICoordinate coordinate { get; set; }
            public static INode[,,] grid { get; set; }

            private float traverseCost;
            private float heuristicCost;
            private INode parent;

            #region constructors

            public AStarNode3D() { }
            public AStarNode3D(PMap.ICoordinate coordinate)
            {
                this.coordinate = coordinate;
            }
            public AStarNode3D(PMap.ICoordinate coordinate, INode[,,] grid) : this(coordinate)
            {
                AStarNode3D.grid = grid;
            }

            #endregion
            #region Common A* operations

            public bool CoordinateEquals(AStarNode3D node)
            {
                return this.CoordinateEquals(node.coordinate);
            }

            public bool CoordinateEquals(PMap.ICoordinate coordinate)
            {
                return this.coordinate.Equals(coordinate);
            }

            #endregion
            #region INode interface

            public float GetTraverseCost()
            {
                return traverseCost;
            }

            public bool SetTraverseCost(float cost)
            {
                float original = this.traverseCost;
                this.traverseCost = cost;
                return this.traverseCost != original;
            }

            public float GetTotalCost()
            {
                return this.traverseCost + (this.GetParent() == null ? 0 : this.GetParent().GetTotalCost());
            }

            public float GetHeuristicCost()
            {
                return heuristicCost;
            }

            public bool SetHeuristicCost(float cost)
            {
                float original = this.heuristicCost;
                this.heuristicCost = cost;
                return this.heuristicCost != original;
            }

            public List<INode> GetNeighbors()
            {
                Vector3 location = coordinate.ToVector3();
                return grid.GetNeighborsOf((int)location.x, (int)location.y, (int)location.z, 1);
            }

            public bool SetNeighbors(List<INode> neighbors)
            {
                return false;
            }

            public bool AddNeighbor(INode node)
            {
                return false;
            }

            public bool RemoveNeighbor(INode node)
            {
                return false;
            }

            public void SetParent(INode node)
            {
                this.parent = node;
            }

            public INode GetParent()
            {
                return this.parent;
            }

            #endregion
            #region IComparable (so nodes are sortable)

            public int CompareTo(object obj)
            {
                INode other = (INode)obj;
                float thisRank = this.GetHeuristicCost() + this.GetTotalCost();
                float otherRank = other.GetHeuristicCost() + other.GetTotalCost();
                return thisRank.CompareTo(otherRank);
            }

            #endregion
        }

        public class AStarNodeX : INode
        {
            private float traverseCost;
            private float heuristicCost;
            private INode parent;
            private List<INode> _neighbors;
            private List<INode> neighbors
            {
                get
                {
                    if (_neighbors == null) _neighbors = new List<INode>();
                    return _neighbors;
                }
                set
                {
                    _neighbors = value;
                }
            }

            #region constructors

            public AStarNodeX()
            {
                this.neighbors = new List<INode>();
            }

            public AStarNodeX(List<INode> neighbors)
            {
                this.neighbors = neighbors;
            }

            #endregion
            #region INode interface

            public float GetTraverseCost()
            {
                return traverseCost;
            }

            public bool SetTraverseCost(float cost)
            {
                float original = this.traverseCost;
                this.traverseCost = cost;
                return this.traverseCost != original;
            }

            public float GetTotalCost()
            {
                return this.traverseCost + (this.GetParent() == null ? 0 : this.GetParent().GetTotalCost());
            }

            public float GetHeuristicCost()
            {
                return heuristicCost;
            }

            public bool SetHeuristicCost(float cost)
            {
                float original = this.heuristicCost;
                this.heuristicCost = cost;
                return this.heuristicCost != original;
            }

            public List<INode> GetNeighbors()
            {
                return neighbors;
            }

            public bool SetNeighbors(List<INode> neighbors)
            {
                bool same = this.neighbors.Equals(neighbors);
                this.neighbors = neighbors;
                return !same;
            }

            public bool AddNeighbor(INode node)
            {
                int length = this.neighbors.Count;
                this.neighbors.AddUnique(node);
                return length == this.neighbors.Count;
            }

            public bool RemoveNeighbor(INode node)
            {
                int length = this.neighbors.Count;
                this.neighbors.Remove(node);
                return length != this.neighbors.Count;
            }

            public void SetParent(INode node)
            {
                this.parent = node;
            }

            public INode GetParent()
            {
                return this.parent;
            }

            #endregion
            #region IComparable (so nodes are sortable)

            public int CompareTo(object obj)
            {
                INode other = (INode)obj;
                float thisRank = this.GetHeuristicCost() + this.GetTotalCost();
                float otherRank = other.GetHeuristicCost() + other.GetTotalCost();
                return thisRank.CompareTo(otherRank);
            }

            #endregion
        }

        /// <summary>
        /// Returns a list of nodes that lead from start to destination. 
        /// Assumptions:
        ///     All data, including heuristic costs, has already been computed and is current.
        /// Usage: start.FindPathTo(destination)
        /// Returns: list of nodes following shortest path between start and destination
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="start"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static List<INode> FindPathTo(this INode start, INode destination)
        {
            List<INode> searched = new List<INode>();
            List<INode> edges = new List<INode>();
            List<INode> path = new List<INode>();

            //  are start and destination the same?
            if (start.Equals(destination))
            {
                path.Add(start);
                return path;
            }

            //  build the path
            edges.Add(start);
            bool pathFound = false;
            do
            {
                //  sort the edges by heuristic
                edges.Sort();

                //  select the first edge
                INode selectedEdge = edges[0];

                //  get all neighbors of the edge
                List<INode> neighbors = new List<INode>();
                neighbors = selectedEdge.GetNeighbors();

                foreach (INode node in neighbors)
                {
                    //  ignore any neighbors that have already been searched
                    if (searched.Contains(node)) continue;

                    //  ignore any neighbors that are already in the edge list
                    if (edges.Contains(node)) continue;

                    //  set the neighbor's parent to the selected edge node
                    node.SetParent(selectedEdge);

                    //  append neighbor to the edge list
                    edges.Add(node);
                }

                //  remove the edge from the search list
                edges.Remove(selectedEdge);

                //  add the node to the searched list
                searched.Add(selectedEdge);

                //  check the destination has been found
                pathFound = selectedEdge.Equals(destination);
            } while (edges.Count > 0 && !pathFound);

            if (pathFound)
            {
                INode node = destination;
                do
                {
                    path.Insert(0, node);
                    node = node.GetParent();
                } while (node != null);
                return path;
            }
            else
            {
                return null;
            }
        }

        #region Original Implmentation of A*
        /// <summary>
        /// 2016-2-22
        /// Works with Coord2D or Coord3D objects.
        /// A node for use in A* navigation algorithm.
        /// Includes a coordinate (x/y or x/y/z), a parent A* node, and heuristic data.
        ///     TraverseCost (The cost to cross this node.)
        ///     HeuristicCost (The estimated cost from this node to the destination.)
        ///     TotalCost (The accumulated cost to get from start to this node.)
        /// </summary>
        public class AStarNodeDep : System.IComparable
        {
            public PMap.ICoordinate coordinate { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public AStarNodeDep(PMap.ICoordinate coordinate)
            {
                this.coordinate = coordinate;
            }

            /// <summary>
            /// Sets the individual traverse cost of this A* node, to the value provided.
            /// </summary>
            public float traverseCost { get; set; }

            /// <summary>
            /// The parent A* node that points at this node.
            /// </summary>
            public AStarNodeDep parent { get; set; }

            /// <summary>
            /// The total cost from start to this node. (read only)
            /// If there has been no parent node assigned to this node, this will return
            /// the traverseCost of the node (since the 'parent' totalCost is effectively 0).
            /// </summary>
            public float totalCost
            {
                get { return this.traverseCost + (this.parent == null ? 0 : this.parent.totalCost); }
            }

            /// <summary>
            /// The estimated cost from this node to the destination. (read-only)
            /// </summary>
            public float heuristicCost
            {
                get;
                private set;
            }

            /// <summary>
            /// Determines this node's heuristic based on the provided coordinate. (assign-only)
            /// </summary>
            public PMap.ICoordinate destination
            {
                set
                {
                    this.heuristicCost = this.coordinate.DistanceTo(value);
                }
            }

            ////  A* operations

            public bool CoordinateEquals(AStarNodeDep node)
            {
                return this.CoordinateEquals(node.coordinate);
            }

            public bool CoordinateEquals(PMap.ICoordinate coordinate)
            {
                return this.coordinate.Equals(coordinate);
            }

            ////  define basic operators for this class

            #region IComparable (so nodes are sortable)

            public int CompareTo(object obj)
            {
                AStarNodeDep other = (AStarNodeDep)obj;
                float thisRank = this.heuristicCost + this.totalCost;
                float otherRank = other.heuristicCost + other.totalCost;
                return thisRank.CompareTo(otherRank);
            }

            #endregion
        }

        public static List<AStarNodeDep> FindPath(this AStarNodeDep[,] matrix, AStarNodeDep start, AStarNodeDep destination)
        {

            List<AStarNodeDep> edges = new List<AStarNodeDep>();
            List<AStarNodeDep> path = new List<AStarNodeDep>();

            //  are start and destination the same?
            if (start.Equals(destination))
            {
                path.Add(start);
                return path;
            }

            //  update heuristic costs for all nodes in the matrix
            foreach (AStarNodeDep node in matrix)
            {
                node.destination = destination.coordinate;
            }

            //  build the path
            edges.Add(start);
            bool pathFound = false;
            do
            {
                //  sort the edges by heuristic
                edges.Sort();

                //  select the first edge
                AStarNodeDep selectedEdge = edges[0];
                Vector2 edgeCoordinate = selectedEdge.coordinate.ToVector2();

                //  get all neighbors of the edge
                List<AStarNodeDep> neighbors = new List<AStarNodeDep>();
                neighbors = matrix.GetNeighborsOf((int)edgeCoordinate.x, (int)edgeCoordinate.y, 1);

                //  set the neighbors' parents to the edge
                foreach (AStarNodeDep node in neighbors) node.parent = selectedEdge;

                //  append all neighbors to the edge list
                edges.AddUniques(neighbors);

                //  move the edge to the searched list
                edges.Remove(selectedEdge);

                pathFound = selectedEdge.Equals(destination);
            } while (edges.Count > 0 && !pathFound);

            if (pathFound)
            {
                AStarNodeDep node = destination;
                do
                {
                    path.Add(node);
                    node = node.parent;
                } while (node != null);
                return path;
            }
            else
            {
                return null;
            }
        }
        #endregion
    }

    /// <summary>
    /// 2016-12-23
    /// Dynamic path-planning. Dynamic A*
    /// </summary>
    public static class DStar
    {

    }

    /// <summary>
    /// 2016-2-26
    /// Anytime algorithm
    /// </summary>
    public static class ARAStar
    {

    }

    /// <summary>
    /// 2016-2-26
    /// Anytime re-planning algorithm
    /// </summary>
    public static class ADStar
    {

    }

    /// <summary>
    /// 2016-2-26
    /// Path Refinement Learning Real-Time Search (PRLRTS)
    /// Ref: www.jair.org/media/2293/live-2293-3453-jair.pdf
    /// "Graph Abstraction in Real-time Heuristic Search"
    /// </summary>
    public static class PRLRTS
    {

    }
}