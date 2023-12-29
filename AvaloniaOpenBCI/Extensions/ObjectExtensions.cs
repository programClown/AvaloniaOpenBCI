﻿using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using RockLib.Reflection.Optimized;

namespace AvaloniaOpenBCI.Extensions;

[PublicAPI]
public static class ObjectExtensions
{
    /// <summary>
    ///     Cache of Types to named field getters
    /// </summary>
    readonly private static Dictionary<Type, Dictionary<string, Func<object, object>>> FieldGetterTypeCache = new();

    /// <summary>
    ///     Cache of Types to named field setters
    /// </summary>
    readonly private static Dictionary<Type, Dictionary<string, Action<object, object>>> FieldSetterTypeCache = new();

    /// <summary>
    ///     Cache of Types to named property getters
    /// </summary>
    readonly private static Dictionary<Type, Dictionary<string, Func<object, object>>> PropertyGetterTypeCache = new();

    /// <summary>
    ///     Get the value of a named private field from an object
    /// </summary>
    /// <remarks>
    ///     The field must be defined by the runtime type of <see cref="obj" /> or its first base type.
    ///     For higher inheritance levels, use <see cref="GetPrivateField{TObject,T}" /> to specify the exact defining type.
    /// </remarks>
    public static T? GetPrivateField<T>(this object obj, string fieldName)
    {
        // Check cache
        Dictionary<string, Func<object, object>> fieldGetterCache = FieldGetterTypeCache.GetOrAdd(obj.GetType());

        if (!fieldGetterCache.TryGetValue(fieldName, out Func<object, object>? fieldGetter))
        {
            // Get the field
            FieldInfo? field = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            // Try get from parent
            field ??= obj.GetType().BaseType?.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (field is null)
            {
                throw new ArgumentException($"Field {fieldName} not found on type {obj.GetType().Name}");
            }

            // Create a getter for the field
            fieldGetter = field.CreateGetter();

            // Add to cache
            fieldGetterCache.Add(fieldName, fieldGetter);
        }

        return (T?)fieldGetter(obj);
    }

    /// <summary>
    ///     Get the value of a protected property from an object
    /// </summary>
    /// <remarks>
    ///     The property must be defined by the runtime type of <see cref="obj" /> or its first base type.
    /// </remarks>
    public static object? GetProtectedProperty(this object obj, [LocalizationRequired(false)] string propertyName)
    {
        // Check cache
        Dictionary<string, Func<object, object>> fieldGetterCache = PropertyGetterTypeCache.GetOrAdd(obj.GetType());

        if (!fieldGetterCache.TryGetValue(propertyName, out Func<object, object>? propertyGetter))
        {
            // Get the field
            PropertyInfo? propertyInfo = obj.GetType()
                .GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            // Try get from parent
            propertyInfo ??= obj.GetType()
                .BaseType
                ?.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (propertyInfo is null)
            {
                throw new ArgumentException($"Property {propertyName} not found on type {obj.GetType().Name}");
            }

            // Create a getter for the field
            propertyGetter = o => propertyInfo.GetValue(o)!;

            // Add to cache
            fieldGetterCache.Add(propertyName, propertyGetter);
        }

        return (object?)propertyGetter(obj);
    }

    /// <summary>
    ///     Get the value of a protected property from an object
    /// </summary>
    /// <remarks>
    ///     The property must be defined by the runtime type of <see cref="obj" /> or its first base type.
    /// </remarks>
    public static T? GetProtectedProperty<T>(this object obj, [LocalizationRequired(false)] string propertyName)
        where T : class =>
        (T?)GetProtectedProperty(obj, propertyName);

    /// <summary>
    ///     Get the value of a named private field from an object
    /// </summary>
    /// <typeparam name="TObject">
    ///     Type of the object that defines the field, must be a base class of <see cref="obj" />
    /// </typeparam>
    /// <typeparam name="T">Type of the field</typeparam>
    public static T? GetPrivateField<TObject, T>(this TObject obj, string fieldName)
        where TObject : class
    {
        // Check cache
        Dictionary<string, Func<object, object>> fieldGetterCache = FieldGetterTypeCache.GetOrAdd(typeof(TObject));

        if (!fieldGetterCache.TryGetValue(fieldName, out Func<object, object>? fieldGetter))
        {
            // Get the field
            FieldInfo? field = typeof(TObject).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (field is null)
            {
                throw new ArgumentException(
                    $"Field {typeof(TObject).Name}.{fieldName} not found on type {obj.GetType().Name}"
                );
            }

            // Create a getter for the field
            fieldGetter = field.CreateGetter();

            // Add to cache
            fieldGetterCache.Add(fieldName, fieldGetter);
        }

        return (T?)fieldGetter(obj);
    }

    /// <summary>
    ///     Set the value of a named private field on an object
    /// </summary>
    public static void SetPrivateField(this object obj, string fieldName, object value)
    {
        // Check cache
        Dictionary<string, Action<object, object>> fieldSetterCache = FieldSetterTypeCache.GetOrAdd(obj.GetType());

        if (!fieldSetterCache.TryGetValue(fieldName, out Action<object, object>? fieldSetter))
        {
            // Get the field
            FieldInfo? field = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            // Try get from parent
            field ??= obj.GetType().BaseType?.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (field is null)
            {
                throw new ArgumentException($"Field {fieldName} not found on type {obj.GetType().Name}");
            }

            // Create a setter for the field
            fieldSetter = field.CreateSetter();

            // Add to cache
            fieldSetterCache.Add(fieldName, fieldSetter);
        }

        fieldSetter(obj, value);
    }

    /// <summary>
    ///     Set the value of a named private field on an object
    /// </summary>
    public static void SetPrivateField<T>(this object obj, string fieldName, T? value)
    {
        // Check cache
        Dictionary<string, Action<object, object>> fieldSetterCache = FieldSetterTypeCache.GetOrAdd(obj.GetType());

        if (!fieldSetterCache.TryGetValue(fieldName, out Action<object, object>? fieldSetter))
        {
            // Get the field
            FieldInfo? field = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            // Try get from parent
            field ??= obj.GetType().BaseType?.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (field is null)
            {
                throw new ArgumentException($"Field {fieldName} not found on type {obj.GetType().Name}");
            }

            // Create a setter for the field
            fieldSetter = field.CreateSetter();

            // Add to cache
            fieldSetterCache.Add(fieldName, fieldSetter);
        }

        fieldSetter(obj, value!);
    }
}