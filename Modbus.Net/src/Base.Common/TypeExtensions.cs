using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Modbus.Net
{
    /// <summary>
    ///     Extensions of Reflection
    /// </summary>
    public static class TypeExtensions
    {
        #region Public Methods

        /// <summary>
        ///     Looks for the method in the type matching the name and arguments.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName">
        ///     The name of the method to find.
        /// </param>
        /// <param name="args">
        ///     The types of the method's arguments to match.
        /// </param>
        /// <param name="isGenericMethod">
        ///     Is method Generic Method.
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if:
        ///     - The name of the method is not specified.
        /// </exception>
        public static MethodInfo GetRuntimeMethod(this Type type, string methodName, Type[] args, bool isGenericMethod)
        {
            if (ReferenceEquals(type, null))
                throw new NullReferenceException("The type has not been specified.");

            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentNullException(nameof(methodName), "The name of the method has not been specified.");


            var methods =
                type.GetRuntimeMethods()
                    .Where(methodInfo => string.Equals(methodInfo.Name, methodName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!methods.Any())
                return null; //  No methods have the specified name.

            if (isGenericMethod)
                methods = methods.Where(method => method.IsGenericMethod).ToList();
            else
                methods = methods.Where(method => !method.IsGenericMethod).ToList();

            var ans = methods.Where(method => IsSignatureMatch(method, args));

            if (ans.Count() <= 1)
                return ans.Count() == 1 ? ans.Single() : null;

            //  Oh noes, don't make me go there.
            throw new NotImplementedException("Resolving overloaded methods is not implemented as of now.");
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///     Finds out if the provided arguments matches the specified method's signature.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool IsSignatureMatch(MethodBase methodInfo, Type[] args)
        {
            Debug.Assert(!ReferenceEquals(methodInfo, null), "The methodInfo has not been specified.");


            //  Gets the parameters of the method to analyze.
            var parameters = methodInfo.GetParameters();

            var currentArgId = 0;

            foreach (var parameterInfo in parameters)
            {
                if (!ReferenceEquals(args, null) && currentArgId < args.Length)
                {
                    //  Find out if the types matchs.
                    if (parameterInfo.ParameterType == args[currentArgId])
                    {
                        currentArgId++;
                        continue; //  Yeah! Try the next one.
                    }

                    //  Is this a generic parameter?
                    if (parameterInfo.ParameterType.IsGenericParameter)
                    {
                        //  Gets the base type of the generic parameter.
                        var baseType = parameterInfo.ParameterType.GetTypeInfo().BaseType;


                        //  TODO: This is not good v and works with the most simple situation.
                        //  Does the base type match?  
                        if (args[currentArgId].GetTypeInfo().BaseType == baseType)
                        {
                            currentArgId++;
                            continue; //  Yeah! Go on to the next parameter.
                        }
                    }
                }

                //  Is this parameter optional or does it have a default value?
                if (parameterInfo.IsOptional || parameterInfo.HasDefaultValue)
                    continue; // Uhum. So let's ignore this parameter for now.

                //  No need to go further. It does not match :(
                return false;
            }

            //  Ye!
            return true;
        }

        #endregion
    }
}