// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.Collections.Generic;

namespace Bercetech.Games.Fleepas.Collections
{
  internal sealed class _EmptyReadOnlyDictionary<TKey, TValue>
  {
    public static readonly _ReadOnlyDictionary<TKey, TValue> Instance =
      new _ReadOnlyDictionary<TKey, TValue>(new Dictionary<TKey, TValue>());
  }
}
