// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.Collections.ObjectModel;

namespace Bercetech.Games.Fleepas.Collections
{
  public static class EmptyReadOnlyCollection<T>
  {
    public static readonly ReadOnlyCollection<T> Instance =
      new ReadOnlyCollection<T>(EmptyArray<T>.Instance);
  }
}
