namespace FundLog.Domain;

public static class ArrayExtensions
{
  /// <summary>
  /// Concatenate the two arrays. Return the new array.
  /// </summary>
  public static object?[]? Concat(this object?[]? left, params object?[]? right)
  {
    var all = new List<object?>();

    if (left != null)
    {
      all.AddRange(left);
    }

    if (right != null)
    {
      all.AddRange(right);
    }

    return all.ToArray();
  }
}