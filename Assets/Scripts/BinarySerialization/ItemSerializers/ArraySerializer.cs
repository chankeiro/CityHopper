// Copyright 2022 Niantic, Inc. All Rights Reserved.

namespace Bercetech.Games.Fleepas.BinarySerialization.ItemSerializers
{
  public static class ArraySerializer<T>
  {
    public static readonly IItemSerializer<T[]> Instance;

    static ArraySerializer()
    {
      if (typeof(T).IsSealed)
        Instance = ArrayOfSealedSerializer<T>.Instance;
      else
        Instance = ArrayOfUnsealedSerializer<T>.Instance;
    }
  }
}
