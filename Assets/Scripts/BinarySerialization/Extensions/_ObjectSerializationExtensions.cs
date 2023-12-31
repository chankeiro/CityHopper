// Copyright 2022 Niantic, Inc. All Rights Reserved.

using System.IO;

using Bercetech.Games.Fleepas.BinarySerialization;

namespace Bercetech.Games.Fleepas.Extensions
{
  internal static class _ObjectSerializationExtensions
  {
    public static byte[] SerializeToArray(this object obj)
    {
      if (obj == null)
        return null;

      using (var stream = new MemoryStream())
      {
        GlobalSerializer.Serialize(stream, obj);
        return stream.ToArray();
      }
    }

    public static T DeserializeFromArray<T>(this byte[] byteArray)
    where
    T: class
    {
      if (byteArray == null)
        return null;

      using (var stream = new MemoryStream(byteArray))
      {
        var obj = (T)GlobalSerializer.Deserialize(stream);
        return obj;
      }
    }
  }
}
