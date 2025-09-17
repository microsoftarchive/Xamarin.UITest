using System;
using System.Collections.Generic;

namespace Xamarin.UITest.Shared.Execution
{
    public class SimpleContainer : IResolver
    {
        readonly Dictionary<Type, object> _objects = new Dictionary<Type, object>();

        public void Register<T>(T instance)
        {
            if (_objects.ContainsKey(typeof(T)))
            {
                throw new Exception("Instance with type: " + typeof(T).FullName + " is already registered.");
            }

            _objects.Add(typeof(T), instance);
        }

        public T Resolve<T>() where T : class
        {
            if (!_objects.ContainsKey(typeof (T)))
            {
                throw new Exception("Instance with type: " + typeof(T).FullName + " is not registered.");
            }

            return _objects[typeof (T)] as T;
        }
    }
}