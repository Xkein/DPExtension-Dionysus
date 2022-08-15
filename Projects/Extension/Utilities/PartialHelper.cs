using Extension.Ext;
using PatcherYRpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Utilities
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    sealed class INILoadActionAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    sealed class SaveActionAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    sealed class LoadActionAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    sealed class UpdateActionAttribute:Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    sealed class PutActionAttribute : Attribute
    {
    }


    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    sealed class RemoveActionAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    sealed class FireActionAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    sealed class ReceiveDamageActionAttribute : Attribute
    {
    }



    static class PartialHelper
    {

        public static Action<TechnoExt> TechnoOnUpdateAction = (owner) => { };

        public static Action<TechnoExt,Pointer<AbstractClass>, int> TechnoFireAction = (owner,pTarget, weaponIndex) => { };


        static PartialHelper()
        {
            var type = typeof(TechnoExt);
            var methods = type.GetMethods().Where(m => m.GetCustomAttributes()?.Count() > 0).ToList();

            foreach (var method in methods)
            {
                if (method.GetCustomAttribute(typeof(UpdateActionAttribute)) != null)
                {
                    List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { };

                    ParameterExpression parameterExpression = Expression.Parameter(typeof(TechnoExt), "owner");
                    MethodCallExpression methodCall = Expression.Call(parameterExpression, method, parameterExpressions);

                    var parameterExpressionAll = new List<ParameterExpression>()
                    { };
                    parameterExpressionAll.Add(parameterExpression);
                    parameterExpressionAll.AddRange(parameterExpressions);
                    Expression<Action<TechnoExt>> expression = Expression.Lambda<Action<TechnoExt>>
                       (methodCall, parameterExpressionAll);
                    var lambda = expression.Compile();
                    TechnoOnUpdateAction += lambda;
                }

                if (method.GetCustomAttribute(typeof(FireActionAttribute)) != null)
                {
                    List<ParameterExpression> parameterExpressions = new List<ParameterExpression>()
                    { Expression.Parameter(typeof(Pointer<AbstractClass>), "pTarget"), Expression.Parameter(typeof(int), "weaponIndex") };

                    ParameterExpression parameterExpression = Expression.Parameter(typeof(TechnoExt), "owner");
                    MethodCallExpression methodCall = Expression.Call(parameterExpression, method, parameterExpressions);

                    var parameterExpressionAll = new List<ParameterExpression>()
                    { };
                    parameterExpressionAll.Add(parameterExpression);
                    parameterExpressionAll.AddRange(parameterExpressions);
                    Expression<Action<TechnoExt, Pointer<AbstractClass>, int>> expression = Expression.Lambda<Action<TechnoExt, Pointer<AbstractClass>, int>>
                       (methodCall, parameterExpressionAll);
                    var lambda = expression.Compile();

                    TechnoFireAction += lambda;
                }
            }
        }





        public static void PartialLoadINIConfig<T>(this Extension<T> ext, Pointer<CCINIClass> pINI)
        {
            Type type = ext.GetType();
            MethodInfo[] methods = type.GetMethods();

            foreach (var method in methods)
            {
                if(method.IsDefined(typeof(INILoadActionAttribute), false))
                {
                    method?.Invoke(ext, new object[] { pINI });
                }
            }
        }
        public static void PartialSaveToStream<T>(this Extension<T> ext, IStream stream)
        {
            Type type = ext.GetType();
            MethodInfo[] methods = type.GetMethods();

            foreach (var method in methods)
            {
                if (method.IsDefined(typeof(SaveActionAttribute), false))
                {
                    method?.Invoke(ext, new object[] { stream });
                }
            }
        }
        public static void PartialLoadFromStream<T>(this Extension<T> ext, IStream stream)
        {
            Type type = ext.GetType();
            MethodInfo[] methods = type.GetMethods();

            foreach (var method in methods)
            {
                if (method.IsDefined(typeof(LoadActionAttribute), false))
                {
                    method?.Invoke(ext, new object[] { stream });
                }
            }
        }
    }
}
