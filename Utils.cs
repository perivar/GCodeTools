using System;
using System.Linq;
using System.Drawing; // Point
using System.Collections.Generic;

/// <summary>
/// Description of Utils.
/// </summary>
public static class Utils
{
	private static Random rng = new Random();

	/// <summary>
	/// Shuffle a list (i.e. randomize the list elements) 
	/// </summary>
	/// <param name="list">the list to shuffle</param>
	public static void Shuffle<T>(this List<T> list)
	{
		int n = list.Count;
		while (n > 1) {
			n--;
			int k = rng.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}
	
	/// <summary>
	/// Get the elements between the two indexes.
	/// Inclusive for start index, exclusive for end index.
	/// Note! Uses GetRange which creates a shallow copy
	/// </summary>
	/// <example>
	/// var str1 = new List<char>("The morning is upon us.".ToCharArray());
	/// var str2 = str1.Slice(4, -2); // returns: morning is upon u
	/// var str3 = str1.Slice(-3, -1); // returns 'us'
	/// var str4 = str1.Slice(0, -1);  // returns 'The morning is upon us'
	/// </example>
	public static List<T> Slice<T>(this List<T> data, int start, int end)
	{
		int count = data.Count();
		
		// Get start/end indexes, negative numbers start at the end of the collection.
		if (start < 0)
			start += count;
		
		if (end < 0)
			end += count;
		
		int len = end - start;
		return data.GetRange(start, len);
	}

	/// <summary>
	/// Get the elements between the two indexes.
	/// Inclusive for start index, exclusive for end index.
	/// </summary>
	public static List<T> Slice2<T>(this List<T> data, int start, int end)
	{
		int count = data.Count();
		
		// Get start/end indexes, negative numbers start at the end of the collection.
		if (start < 0)
			start += count;
		
		if (end < 0)
			end += count;
		
		int len = end - start;
		var result = new T[len];
		Array.Copy(data.ToArray(), start, result, 0, len);
		return result.ToList();
	}

	/// <summary>
	/// Get the elements between the two indexes.
	/// Inclusive for start index, exclusive for end index.
	/// </summary>
	public static List<T> Slice3<T>(this List<T> data, int start, int end)
	{
		int count = data.Count();
		
		// Get start/end indexes, negative numbers start at the end of the collection.
		if (start < 0)
			start += count;
		
		if (end < 0)
			end += count;
		
		int len = end - start;
		return data.Skip(start).Take(len).ToList();
	}
	
	/// <summary>
	/// Deep Clone a List
	/// </summary>
	/// <param name="listToClone">list to clone</param>
	/// <returns>deep cloned list</returns>
	public static IList<T> CloneDeep<T>(this IList<T> listToClone) where T: ICloneable
	{
		return listToClone.Select(item => (T)item.Clone()).ToList();
	}
	
	/// <summary>
	/// Shallow Clone a List (Value Lists are properly clones, but not objects)
	/// </summary>
	/// <param name="listToClone">list to clone</param>
	/// <returns>Shallow cloned list</returns>
	public static List<T> Clone<T>(this List<T> listToClone)
	{
		return new List<T>(listToClone); // only works for value lists, not object lists
	}
	
	/// <summary>
	/// Returns a new array of items from an existing array and at the same time removes these items from the existing array.
	/// </summary>
	/// <param name="source">source array</param>
	/// <param name="startIndex">Index at which to start changing the array (with origin 0). </param>
	/// <param name="deleteCount">An integer indicating the number of old array elements to remove.</param>
	/// <returns>An array containing the deleted elements.</returns>
	public static List<T> Splice<T>(this List<T> source, int startIndex, int deleteCount)
	{
		var items = source.GetRange(startIndex, deleteCount);
		source.RemoveRange(startIndex,deleteCount);
		return items;
	}

	/// <summary>
	/// Removes the first occurrence of a specific object from the List<T>.
	/// </summary>
	/// <param name="list">list to modify</param>
	/// <param name="value">value to remove</param>
	public static void DeleteByValue<T> (this List<T> list, T value) {
		list.Remove(value);
	}
	
	/// <summary>
	/// Return the next element from the list based on an index
	/// </summary>
	/// <param name="list">list</param>
	/// <param name="index">index</param>
	/// <returns>next element from the list based on an index</returns>
	public static T Next<T>(this List<T> list, int index) {
		return index == list.Count - 1 ? list[0] : list[index + 1];
	}
	
	/// <summary>
	/// Return the previous element from the list based on an index
	/// </summary>
	/// <param name="list">list</param>
	/// <param name="index">index</param>
	/// <returns>previous element from the list based on an index</returns>
	public static T Previous<T>(this List<T> list, int index) {
		return index == 0 ? list[list.Count - 1] : list[index - 1];
	}
	
	/// <summary>
	/// Swap to element in a list
	/// </summary>
	/// <param name="list">list to modify</param>
	/// <param name="indexA">index of first element to swap</param>
	/// <param name="indexB">index of second element to swap</param>
	public static void Swap<T>(this List<T> list, int indexA, int indexB) {
		if(indexA > list.Count || indexB > list.Count || indexA == indexB) {
			return;
		}
		
		var tmp = list[indexA];
		list[indexA] = list[indexB];
		list[indexB] = tmp;
	}
	
	/// <summary>
	/// Rotate the list around a random position.
	/// I.e. a list of "012345abcdef" can be returned as "45abcdef0123"
	/// </summary>
	/// <param name="list">list to roll</param>
	/// <returns>the rolled list around a random index</returns>
	public static List<T> Roll<T>(this List<T> list) {
		int rand = RandomNumber(list.Count);
		var tempList = new List<T>();

		for(int i = rand; i < list.Count; i++) {
			tempList.Add(list[i]);
		}

		for(int i = 0; i < rand; i++) {
			tempList.Add(list[i]);
		}

		return tempList;
	}
	
	/// <summary>
	/// Find elements common to the lists given
	/// </summary>
	/// <param name="a">first list</param>
	/// <param name="b">second list</param>
	/// <returns>elements common to the lists given</returns>
	public static List<T> Intersect<T>(List<T> a, List<T> b) {
		return a.Intersect(b).ToList();
	}
	
	/// <summary>
	/// Return a random Point
	/// </summary>
	/// <param name="width">width</param>
	/// <param name="height">height</param>
	/// <returns>a random point</returns>
	public static Point RandomPoint(int width, int height) {
		int randomx = RandomNumber(width);
		int randomy = RandomNumber(height);
		var randomP = new Point(randomx, randomy);
		return randomP;
	}
	
	/// <summary>
	/// Return a random number between 0 and the boundary (exclusive)
	/// </summary>
	/// <param name="boundary">upper boundary</param>
	/// <returns></returns>
	public static int RandomNumber(int boundary) {
		return (int)(rng.NextDouble() * boundary);
	}
	
	/// <summary>
	/// Calculate the distance between two points using euclidean calculation
	/// </summary>
	/// <param name="p1">point 1</param>
	/// <param name="p2">point 2</param>
	/// <returns>the euclidean distance between the two points</returns>
	public static double Distance(Point p1, Point p2) {
		return Euclidean(p1.X-p2.X, p1.Y-p2.Y);
	}
	
	/// <summary>
	/// Perform an euclidean calculation of two points
	/// </summary>
	/// <param name="dx">first point</param>
	/// <param name="dy">first point</param>
	/// <returns>the euclidean result</returns>
	public static double Euclidean(int dx, int dy) {
		return Math.Sqrt(dx*dx + dy*dy);
	}
	
}
