using System;

namespace LaubPlusCo.Foundation.HelixTemplating.Manifest
{
  public class ManifestTypeInstantiator
  {
    internal T CreateInstance<T>(string typeAttribute)
    {
      var type = Type.GetType(typeAttribute);
      if (type == null) throw new ArgumentException("Cannot find type " + typeAttribute);
      var instance = (T)Activator.CreateInstance(type);
      if (instance != null) return instance;
      throw new ArgumentException($"Cannot cast {typeAttribute} as {typeof(T).FullName}");
    }
  }
}