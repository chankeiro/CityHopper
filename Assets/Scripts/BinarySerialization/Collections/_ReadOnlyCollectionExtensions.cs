// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.Collections.ObjectModel;

namespace Bercetech.Games.Fleepas.Collections
{
  internal static class _ReadOnlyCollectionExtensions
  {
    public static ReadOnlyCollection<T> AsNonNullReadOnly<T>(this T[] source)
    {
      if (source == null || source.Length == 0)
        return EmptyReadOnlyCollection<T>.Instance;

      return new ReadOnlyCollection<T>(source);
    }
  }
}
