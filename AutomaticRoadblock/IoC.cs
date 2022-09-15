using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace AutomaticRoadblocks
{
    /// <summary>
    /// Lightweight implementation of an IoC container for simplification of mod dependencies to not use a heavyweight DLL dependency.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class IoC
    {
        private static readonly string PostConstructName = typeof(PostConstruct).FullName;

        private readonly List<ComponentDefinition> _components = new List<ComponentDefinition>();
        private readonly Dictionary<ComponentDefinition, object> _singletons = new Dictionary<ComponentDefinition, object>();
        private readonly object _lockState = new object();

        #region Constructors

        protected IoC()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the IoC instance.
        /// </summary>
        public static IoC Instance { get; } = new IoC();

        #endregion

        #region Methods

        /// <summary>
        /// Register a new component type.
        /// </summary>
        /// <param name="implementation">Set the implementation type.</param>
        /// <typeparam name="T">Set the type of the component.</typeparam>
        /// <returns>Returns this IoC.</returns>
        public IoC Register<T>(Type implementation)
        {
            lock (_lockState)
            {
                RegisterType<T>(implementation, false);
                return this;
            }
        }

        /// <summary>
        /// Register a new singleton type.
        /// </summary>
        /// <param name="implementation">Set the implementation type.</param>
        /// <typeparam name="T">Set the type of the component.</typeparam>
        /// <returns>Returns this IoC.</returns>
        public IoC RegisterSingleton<T>(Type implementation)
        {
            lock (_lockState)
            {
                RegisterType<T>(implementation, true);
                return this;
            }
        }

        /// <summary>
        /// Register a new instance for the given type.
        /// This should only be used in unit tests.
        /// </summary>
        /// <param name="instance">Set the instance to register.</param>
        /// <typeparam name="T">Set the type of the component.</typeparam>
        /// <returns>Returns this IoC.</returns>
        public IoC RegisterInstance<T>(object instance)
        {
            lock (_lockState)
            {
                _singletons.Add(RegisterType<T>(instance.GetType(), true), instance);
                return this;
            }
        }

        /// <summary>
        /// Unregister everything in this IoC.
        /// </summary>
        public IoC UnregisterAll()
        {
            lock (_lockState)
            {
                _components.Clear();
                _singletons.Clear();
                return this;
            }
        }

        /// <summary>
        /// Get the an instance of the registered type.
        /// </summary>
        /// <typeparam name="T">Set the component type to return.</typeparam>
        /// <returns>Returns the component instance.</returns>
        public T GetInstance<T>()
        {
            lock (_lockState)
            {
                return (T) GetInstance(typeof(T));
            }
        }

        /// <summary>
        /// Get one or more instance of the required type.
        /// </summary>
        /// <typeparam name="T">Set the instance type to return.</typeparam>
        /// <returns>Returns one or more instance(s) of the required type of found, otherwise, it will return an empty list if none found.</returns>
        public ReadOnlyCollection<T> GetInstances<T>()
        {
            lock (_lockState)
            {
                var type = typeof(T);

                return GetInstancesDefinitions(type)
                    .Select(x => (T) x.Value)
                    .ToList()
                    .AsReadOnly();
            }
        }

        /// <summary>
        /// Verify if the given type already has an existing singleton instance.
        /// This method will only work if the given type is a registered singleton type.
        /// </summary>
        /// <typeparam name="T">Set the component type.</typeparam>
        /// <returns>Returns true if an instance already exists, else false.</returns>
        /// <exception cref="IoCException">Is thrown when the given type is not registered or not a singleton.</exception>
        public bool InstanceExists<T>()
        {
            lock (_lockState)
            {
                var type = typeof(T);

                if (!TypeExists<T>())
                    throw new IoCException(type + " has not been registered");

                var definition = GetSingletonDefinitionFor(type);

                if (definition == null)
                    throw new IoCException(type + " is not registered as a singleton");

                return _singletons.ContainsKey(definition);
            }
        }

        /// <summary>
        /// Verify if the given type has already been registered.
        /// </summary>
        /// <typeparam name="T">The component type to verify.</typeparam>
        /// <returns>Returns true when the given type is known, else false.</returns>
        public bool TypeExists<T>()
        {
            lock (_lockState)
            {
                var type = typeof(T);
                return GetDefinitionsFor(type).Count > 0;
            }
        }

        #endregion

        #region Functions

        private ComponentDefinition RegisterType<T>(Type implementation, bool isSingleton)
        {
            lock (_lockState)
            {
                var type = typeof(T);
                var key = new ComponentDefinition(type, implementation, isSingleton);

                if (!type.IsAssignableFrom(implementation))
                    throw new IoCException(implementation + " does not implement given type " + type);

                if (isSingleton && GetDefinitionsFor(type).Count > 0)
                    throw new IoCException(type + " has already been registered as a singleton or component");

                _components.Add(key);
                key.AddDerivedTypes(GetDerivedTypes(key.Type));
                key.IsPrimary = implementation.CustomAttributes.Any(x => x.AttributeType == typeof(Primary));
                return key;
            }
        }

        private IEnumerable<Type> GetDerivedTypes(Type type)
        {
            lock (_lockState)
            {
                var derivedTypes = new List<Type>();

                foreach (var derivedInterface in type.GetInterfaces())
                {
                    derivedTypes.Add(derivedInterface);
                    derivedTypes.AddRange(GetDerivedTypes(derivedInterface));
                }

                return derivedTypes;
            }
        }

        private IList<ComponentDefinition> GetDefinitionsFor(Type type)
        {
            lock (_lockState)
            {
                return _components.FindAll(x => x.Defines(type));
            }
        }

        private ComponentDefinition GetSingletonDefinitionFor(Type type)
        {
            lock (_lockState)
            {
                return _components.FirstOrDefault(x => x.Type == type && x.IsSingleton);
            }
        }

        private object GetInstance(Type type)
        {
            var instances = GetInstancesDefinitions(type);

            switch (instances.Count)
            {
                case 0:
                    return null;
                case 1:
                    return instances.First().Value;
                default:
                    var instance = instances.Where(x => x.Key.IsPrimary)
                        .Select(x => x.Value)
                        .SingleOrDefault();

                    if (instance == null)
                        throw new IoCException("More than one instance has been found for " + type);

                    return instance;
            }
        }

        private IList GetInstances(Type type)
        {
            var genericType = GetGenericTypeForCollection(type);
            var instances = GetInstancesDefinitions(genericType)
                .Select(x => x.Value)
                .ToList();
            var list = CreateGenericTypeListInstance(genericType);

            instances.ForEach(x => list.Add(x));

            return list;
        }

        private List<KeyValuePair<ComponentDefinition, object>> GetInstancesDefinitions(Type type)
        {
            lock (_lockState)
            {
                var definitions = GetDefinitionsFor(type);

                // check if any definitions could be found for the given type
                if (definitions.Count == 0)
                    return new List<KeyValuePair<ComponentDefinition, object>>();

                var singletonDefinition = GetSingletonDefinitionFor(type);

                // check a singleton definition type could be found for the given type and the singleton instance is already present
                if (singletonDefinition != null && _singletons.ContainsKey(singletonDefinition))
                    return new List<KeyValuePair<ComponentDefinition, object>>
                        {new KeyValuePair<ComponentDefinition, object>(singletonDefinition, _singletons[singletonDefinition])};

                var instances = new List<KeyValuePair<ComponentDefinition, object>>();

                foreach (var definition in definitions)
                {
                    // Check if the definition is a singleton and the instance already exists, if so, return it.
                    // A 'type' can define 'subtypes' that might have been declared as a singleton on one of it's subtypes
                    // which means that the 'type' itself won't have the singleton definition, but one of it's subtype's might be one
                    if (definition.IsSingleton)
                    {
                        if (_singletons.ContainsKey(definition))
                        {
                            instances.Add(new KeyValuePair<ComponentDefinition, object>(definition, _singletons[definition]));
                            continue;
                        }
                    }

                    // otherwise, create a new instance for the definition
                    var instance = InitializeInstanceType(definition.ImplementationType);
                    InvokePostConstruct(instance);

                    // check if we need to store a singleton instance for the definition
                    if (definition.IsSingleton)
                        _singletons.Add(definition, instance);

                    instances.Add(new KeyValuePair<ComponentDefinition, object>(definition, instance));
                }

                return instances;
            }
        }

        private object InitializeInstanceType(Type type)
        {
            foreach (var constructor in type.GetConstructors())
            {
                if (!AreAllParametersRegistered(constructor))
                    continue;

                return constructor.Invoke(constructor.GetParameters()
                    .Select(parameterInfo =>
                    {
                        var parameterType = parameterInfo.ParameterType;

                        return IsCollectionParameter(parameterInfo) ? GetInstances(parameterType) : GetInstance(parameterType);
                    })
                    .ToArray());
            }

            throw new IoCException("Could not create instance for " + type);
        }

        private static void InvokePostConstruct(object instance)
        {
            var instanceType = instance.GetType();
            var types = new List<Type> {instanceType};
            types.AddRange(GetAllBaseTypes(instanceType));
            
            var postConstructMethod = types
                .SelectMany(x => x.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                .SingleOrDefault(x => x.GetCustomAttribute<PostConstruct>() != null);

            if (postConstructMethod == null)
                return;

            var methodParams = postConstructMethod.GetParameters();

            if (methodParams.Length > 0)
                throw new IoCException(PostConstructName + " doesn't allow parameterized methods");

            try
            {
                postConstructMethod.Invoke(instance, new object[] { });
            }
            catch (Exception ex)
            {
                throw new IoCException(PostConstructName + " failed with error:" + Environment.NewLine + ex.Message, ex);
            }
        }

        private static IEnumerable<Type> GetAllBaseTypes(Type type)
        {
            var baseTypes = new List<Type>();
            var baseType = type.BaseType;

            // if given type doesn't have a base type, return the empty list
            if (baseType == null) 
                return baseTypes;
            
            // otherwise, add the base type as first element in the list and request all other base types of this base type
            baseTypes.Add(baseType);
            baseTypes.AddRange(GetAllBaseTypes(baseType));

            return baseTypes;
        }

        private bool AreAllParametersRegistered(ConstructorInfo constructor)
        {
            return constructor.GetParameters()
                .All(parameterInfo =>
                {
                    var actualParameterType = parameterInfo.ParameterType;

                    if (IsCollectionParameter(parameterInfo))
                    {
                        actualParameterType = GetGenericTypeForCollection(actualParameterType);
                    }

                    return _components.FirstOrDefault(x => actualParameterType.IsAssignableFrom(x.Type)) != null;
                });
        }

        private static bool IsCollectionParameter(ParameterInfo parameter)
        {
            return IsCollectionType(parameter.ParameterType);
        }

        private static bool IsCollectionType(Type type)
        {
            return type.GetInterface(nameof(IEnumerable)) != null;
        }

        private static Type GetGenericTypeForCollection(Type type)
        {
            if (!type.IsGenericType)
                throw new IoCException("Type is not a generic type collection for " + type);

            var genericType = type.GetGenericArguments().FirstOrDefault();

            if (genericType == null)
                throw new IoCException("Could not determine generic type of collection for " + type);

            return genericType;
        }

        private static IList CreateGenericTypeListInstance(Type genericType)
        {
            var listType = typeof(List<>);
            var constructedListType = listType.MakeGenericType(genericType);

            return (IList) Activator.CreateInstance(constructedListType);
        }

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Indicates that the method must be executed after instantiation of the instance managed by this IoC.
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class PostConstruct : Attribute
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Indicates that the instance should be used in case of multiple found instance and only one instance is wanted.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class Primary : Attribute
        {
        }

        /// <summary>
        /// Internal placeholder of the IoC definition type info.
        /// This is the type that is being used as interface registration of an implementation.
        /// </summary>
        private class ComponentDefinition
        {
            private readonly List<Type> _derivedTypes = new List<Type>();

            public ComponentDefinition(Type type, Type implementationType, bool isSingleton)
            {
                Type = type;
                ImplementationType = implementationType;
                IsSingleton = isSingleton;
            }

            #region Properties

            /// <summary>
            /// Get the definition type.
            /// This is the key of the registration types.
            /// </summary>
            public Type Type { get; }

            /// <summary>
            /// Get the implementation type for this definition.
            /// </summary>
            public Type ImplementationType { get; }

            /// <summary>
            /// Get if this definition is a singleton registration.
            /// </summary>
            public bool IsSingleton { get; }

            /// <summary>
            /// Get or set if this definition is been indicated as primary component in case of multi instance conflict.
            /// </summary>
            public bool IsPrimary { get; set; }

            /// <summary>
            /// Get all derived types defined by the key interface.
            /// </summary>
            public ReadOnlyCollection<Type> DerivedTypes => _derivedTypes.AsReadOnly();

            #endregion

            #region Methods

            /// <summary>
            /// Get if this key defines (implements) the given type.
            /// This will check it's own type as well as derived types.
            /// </summary>
            /// <param name="type">Set the type to check for.</param>
            /// <returns>Returns true if this definition defines the given type, else false.</returns>
            public bool Defines(Type type)
            {
                return Type == type || DerivedTypes.Contains(type);
            }

            public void AddDerivedTypes(IEnumerable<Type> types)
            {
                foreach (var type in types)
                {
                    AddDerivedType(type);
                }
            }

            public void AddDerivedType(Type type)
            {
                if (!_derivedTypes.Contains(type))
                    _derivedTypes.Add(type);
            }

            #endregion

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ComponentDefinition) obj);
            }

            public override int GetHashCode()
            {
                return Type != null ? Type.GetHashCode() : 0;
            }

            public override string ToString()
            {
                return $"{nameof(Type)}: {Type}, {nameof(ImplementationType)}: {ImplementationType}, {nameof(IsSingleton)}: {IsSingleton}, " +
                       $"{nameof(_derivedTypes)}: {_derivedTypes.Count}";
            }

            private bool Equals(ComponentDefinition other)
            {
                return Type == other.Type;
            }
        }
    }
}