// Copyright 2022 Niantic, Inc. All Rights Reserved.

namespace Bercetech.Games.Fleepas.Collections
{
  public static class EmptyArdkReadOnlyCollection<T>
  {
    public static readonly ARDKReadOnlyCollection<T> Instance =
      EmptyArray<T>.Instance.AsArdkReadOnly();
  }
}
