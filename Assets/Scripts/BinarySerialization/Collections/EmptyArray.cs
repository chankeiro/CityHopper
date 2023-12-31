// Copyright 2022 Niantic, Inc. All Rights Reserved.

namespace Bercetech.Games.Fleepas.Collections
{
  /// <summary>
  /// Generic class that generates a single, reusable instance of an empty array of type T.
  /// </summary>
  public static class EmptyArray<T>
  {
    public static readonly T[] Instance = new T[0];
  }
}
