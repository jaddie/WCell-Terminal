using System;
using System.Collections.Generic;

/// <summary>
/// provides a set of runtime validations for inputs
/// </summary>
[System.Diagnostics.DebuggerNonUserCode]
internal static partial class Check
{
	/// <summary>
	/// Verifies that value is not null and returns the value or throws ArgumentNullException
	/// </summary>
	public static T NotNull<T>(T value)
	{
		if (value == null) throw new ArgumentNullException();
		return value;
	}

	/// <summary>
	/// Verfies that the string is not null and not empty and returns the string.
	/// throws ArgumentNullException, ArgumentOutOfRangeException
	/// </summary>
	public static string NotEmpty(string value)
	{
		if (value == null) throw new ArgumentNullException();
		if (value.Length == 0) throw new ArgumentOutOfRangeException();
		return value;
	}

	/// <summary>
	/// Verfies that the collection is not null and not empty and returns the collection.
	/// throws ArgumentNullException, ArgumentOutOfRangeException
	/// </summary>
	public static T NotEmpty<T>(T value) where T : System.Collections.IEnumerable
	{
		if (value == null) throw new ArgumentNullException();
		if (!value.GetEnumerator().MoveNext()) throw new ArgumentOutOfRangeException();
		return value;
	}

	/// <summary>
	/// Verifies that the two values are the same
	/// throws ArgumentException
	/// </summary>
	public static void IsEqual<T>(T a, T b) where T : IComparable<T>
	{
		if (0 != a.CompareTo(b))
			throw new ArgumentException();
	}

	/// <summary>
	/// Verifies that the two values are NOT the same
	/// throws ArgumentException
	/// </summary>
	public static void NotEqual<T>(T a, T b) where T : IComparable<T>
	{
		if (0 == a.CompareTo(b))
			throw new ArgumentException();
	}

	/// <summary>
	/// Returns (T)value if the object provided can be assinged to a variable of type T
	/// throws ArgumentException
	/// </summary>
	public static T IsAssignable<T>(object value)
	{ return (T)IsAssignable(typeof(T), value); }

	/// <summary>
	/// Returns value if the object provided can be assinged to a variable of type toType
	/// throws ArgumentException
	/// </summary>
	public static object IsAssignable(Type toType, object fromValue)
	{
		Check.NotNull(toType);
		if (fromValue == null)
		{
			if (toType.IsValueType)
				throw new ArgumentException(String.Format("Can not set value of type {0} to null.", toType));
		}
		else
			IsAssignable(toType, fromValue.GetType());
		return fromValue;
	}

	/// <summary>
	/// Throws ArgumentException if the type fromType cannot be assigned to variable of type toType
	/// </summary>
	public static void IsAssignable(Type toType, Type fromType)
	{
		if (!Check.NotNull(toType).IsAssignableFrom(Check.NotNull(fromType)))
			throw new ArgumentException(String.Format("Can not set value of type {0} to a value of type {1}.", toType, fromType));
	}
}