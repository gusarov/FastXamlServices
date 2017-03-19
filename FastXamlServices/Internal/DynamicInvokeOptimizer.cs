using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FastXamlServices.Internal
{
	public class DynamicInvokeOptimizerTest
	{

		public static object TestStaticRef(object arg1, object arg2)
		{
			return TestStaticRefTarget((int)arg1, (string)arg2);
		}

		public static string TestStaticRefTarget(int arg1, string arg2)
		{
			return arg1 + arg2;
		}

		public static object TestStaticVal(object arg1, object arg2)
		{
			return TestStaticValTarget((int)arg1, (string)arg2);
		}

		public static int TestStaticValTarget(int arg1, string arg2)
		{
			return arg1 + 1;
		}

		public object TestInstanceRef()
		{
			return TestInstanceRefTarget();
		}

		public object TestInstanceRef(object arg1, object arg2)
		{
			return TestInstanceRefTarget((int)arg1, (string)arg2);
		}

		public string TestInstanceRefTarget()
		{
			return "";
		}

		public string TestInstanceRefTarget(int arg1, string arg2)
		{
			return arg1 + arg2;
		}

		public object TestInstanceVal(object arg1, object arg2)
		{
			return TestInstanceValTarget((int)arg1, (string)arg2);
		}

		public int TestInstanceValTarget(int arg1, string arg2)
		{
			return arg1 + 1;
		}
	}

	public static class DynamicInvokeOptimizer
	{
		#region HideObjectMembers

		/// <summary>Do not call this method.</summary>
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "b")]
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "a")]
		[Obsolete("Do not call this method", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new static bool Equals(object a, object b)
		{
			throw new InvalidOperationException("Do not call this method");
		}

		/// <summary>Do not call this method.</summary>
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "b")]
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "a")]
		[Obsolete("Do not call this method", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new static bool ReferenceEquals(object a, object b)
		{
			throw new InvalidOperationException("Do not call this method");
		}
		#endregion

		static readonly Dictionary<Type, Type> _mapDelegateToUnboundDelegate = new Dictionary<Type, Type>();

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Type GetUnboundDelegate(Delegate deleg)
		{
			if (deleg == null)
			{
				throw new ArgumentNullException(nameof(deleg));
			}
			var delegateType = deleg.GetType();
			if (!typeof(Delegate).IsAssignableFrom(delegateType))
			{
				throw new ArgumentException("delegateType is not Delegate");
			}
			Type unbound;
			if (!_mapDelegateToUnboundDelegate.TryGetValue(delegateType, out unbound))
			{
				var sig = GetDelegateSignature(deleg);
				var ret = sig.Item1;
				var args = new[] { typeof(object) }.Concat(sig.Item2).ToArray();
				_mapDelegateToUnboundDelegate[delegateType] = unbound = GetDelegateTypeFromSignature(ret, args);
			}
			return unbound;
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Tuple<Type, Type[]> GetDelegateSignature(Delegate deleg)
		{
			if (deleg == null)
			{
				throw new ArgumentNullException(nameof(deleg));
			}
			return Tuple.Create(deleg.Method.ReturnType, deleg.Method.GetParameters().Select(x => x.ParameterType).ToArray());
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Type GetDelegateTypeFromSignature(Type returnType, params Type[] args)
		{
			if (returnType == null || returnType == typeof(void))
			{
				if (args.Length == 0)
				{
					return typeof(Action);
				}
				var gen = Type.GetType("System.Action`" + args.Length + ", mscorlib", true);
				var type = gen.MakeGenericType(args);
				return type;
			}
			else
			{
				var gen = Type.GetType("System.Func`" + (args.Length + 1) + ", mscorlib", true);
				var type = gen.MakeGenericType(args.Concat(new[] { returnType }).ToArray());
				return type;
			}
		}

		static readonly Dictionary<MethodInfo, Delegate> _dic = new Dictionary<MethodInfo, Delegate>();

		public static TDelegate Compile<TTarget, TDelegate>(string methodName) where TDelegate : class
		{
			return Compile<TDelegate>(typeof(TTarget), methodName);
		}

		public static TDelegate Compile<TDelegate>(Type type, string methodName) where TDelegate : class
		{
			return Compile<TDelegate>(type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic));
		}

		/// <summary>
		/// Create lightweight method and delegate to that method.
		/// For instance methods define another first object parameter in delegate.
		/// </summary>
		/// <typeparam name="TDelegate"></typeparam>
		/// <param name="methodInfo"></param>
		/// <returns>Compiled strongly typed delegate</returns>
		/// <example>
		/// <code>
		/// class TestClass
		/// {
		///     public int TestMethod(int arg)
		///     {
		///         return arg * arg;
		///     }
		/// }
		/// 
		/// class TestInvokator
		/// {
		///     void Main()
		///     {
		///         typeof(TestClass).GetMethod("TestMethod").Compile&lt;Func&lt;object, int, int&gt;&gt;
		///     }
		/// }
		/// </code>
		/// </example>
		public static TDelegate Compile<TDelegate>(this MethodInfo methodInfo) where TDelegate : class
		{
			return (TDelegate)(object)Compile(methodInfo, typeof(TDelegate));
		}
		public static TDelegate CompileObject<TDelegate>(this MethodInfo methodInfo) where TDelegate : class
		{
			return (TDelegate)(object)CompileObject(methodInfo);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Delegate CompileObject(this MethodInfo methodInfo)
		{
			var argsList = new List<Type>();
			if (!methodInfo.IsStatic)
			{
				// next sample lead us to only one acceptable bounding object type. Wtih 'object' this signature delegate can be bound to any classes
				argsList.Add(typeof(object));
				// and such approach lead to problems, because a first who bound with Action<X, a,b,c> register this dynamic method and next call Action<Y, a,b,c> will fail.
				// TODO How about custom delegates instead of Action<int>. How about Func with return value at first argument?
				// argsList.Add(unboundDelegateType.GetGenericArguments()[0]);
			}
			argsList.AddRange(methodInfo.GetParameters().Select(x => typeof(object)));
			var args = argsList.ToArray();
			var dm = new DynamicMethod(string.Empty, typeof(object), args, typeof(DynamicInvokeOptimizer), true);
			var il = dm.GetILGenerator();
			GenerateIl(methodInfo, args, il);
			var unboundDelegateType = GetDelegateType(args.Length, methodInfo.ReturnType != typeof(void));
			var del = dm.CreateDelegate(unboundDelegateType);
			return del;

		}

		static void GenerateIl(this MethodInfo methodInfo, Type[] args, ILGenerator il)
		{

			for (int i = 0; i < args.Length; i++)
			{
				il.Emit(OpCodes.Ldarg, i);
				var pi = methodInfo.IsStatic ? methodInfo.GetParameters()[i] : (i > 0 ? methodInfo.GetParameters()[i - 1] : null);

				if (pi == null)
				{
					// instance parameter 'this' not requires cast
				}
				else if (pi.ParameterType.IsValueType)
				{
					il.Emit(OpCodes.Unbox_Any, methodInfo.GetParameters()[i].ParameterType);
				}
				else
				{
					il.Emit(OpCodes.Castclass, pi.ParameterType);
				}
			}
			il.Emit(methodInfo.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, methodInfo);
			//				if (methodInfo.ReturnType == (typeof(void)))
			//				{
			//				    il.Emit(OpCodes.Ldnull);
			//				}
			if (methodInfo.ReturnType != typeof(void))
			{
				if (methodInfo.ReturnType.IsValueType)
				{
					il.Emit(OpCodes.Box, methodInfo.ReturnType);
				}
				il.Emit(OpCodes.Ret);
			}

		}

		static Type GetDelegateType(int argsCount, bool ret)
		{
			if (ret)
			{
				switch (argsCount)
				{
					case 0:
						return typeof(Func<object>);
					case 1:
						return typeof(Func<object, object>);
					case 2:
						return typeof(Func<object, object, object>);
					case 3:
						return typeof(Func<object, object, object>);
					case 4:
						return typeof(Func<object, object, object, object>);
					default:
						throw new NotImplementedException();
				}
			}
			else
			{
				switch (argsCount)
				{
					case 0:
						return typeof(Action);
					case 1:
						return typeof(Action<object>);
					case 2:
						return typeof(Action<object, object>);
					case 3:
						return typeof(Action<object, object>);
					case 4:
						return typeof(Action<object, object, object>);
					default:
						throw new NotImplementedException();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static Delegate Compile(this MethodInfo methodInfo, Type unboundDelegateType)
		{
			if (methodInfo == null)
			{
				throw new ArgumentNullException(nameof(methodInfo));
			}
			Delegate del;
			if (!_dic.TryGetValue(methodInfo, out del))
			{
				var argsList = new List<Type>();
				if (!methodInfo.IsStatic)
				{
					// next sample lead us to only one acceptable bounding object type. Wtih 'object' this signature delegate can be bound to any classes
					argsList.Add(typeof(object));
					// and such approach lead to problems, because a first who bound with Action<X, a,b,c> register this dynamic method and next call Action<Y, a,b,c> will fail.
					// TODO How about custom delegates instead of Action<int>. How about Func with return value at first argument?
					// argsList.Add(unboundDelegateType.GetGenericArguments()[0]);
				}
				argsList.AddRange(methodInfo.GetParameters().Select(x => x.ParameterType));
				var args = argsList.ToArray();
				var dm = new DynamicMethod(string.Empty, methodInfo.ReturnType, args, typeof(DynamicInvokeOptimizer), true);
				var il = dm.GetILGenerator();
				for (int i = 0; i < args.Length; i++)
				{
					il.Emit(OpCodes.Ldarg, i);
				}
				il.Emit(methodInfo.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, methodInfo);
				//				if (methodInfo.ReturnType == (typeof(void)))
				//				{
				//				    il.Emit(OpCodes.Ldnull);
				//				}
				il.Emit(OpCodes.Ret);
				del = dm.CreateDelegate(unboundDelegateType);
				_dic.Add(methodInfo, del);
			}
			return del;
		}
	}

}
