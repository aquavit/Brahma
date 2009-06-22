﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using Brahma.Helper;

namespace Brahma.OpenGL
{
    // This internal class generates GLSL given an ExpressionProcessor.
    internal sealed class GLSLGenerator: ExpressionVisitor
    {
        // delegates of this type are used to transform specific expressions to GLSL

        internal const string EntryPoint = "main"; // Entry point of shaders generated by this class
        private const string FloatNumberFormat = "F4";

        private static readonly Dictionary<string, TransformExpression<MethodCallExpression>> _methodCallTransformations =
            new Dictionary<string, TransformExpression<MethodCallExpression>>
            {
                {
                    "System.Math.Sin",
                    delegate(GLSLGenerator sender, MethodCallExpression methodCall)
                    {
                        CheckArgumentCount("sin", 1, methodCall.Arguments.Count);

                        sender._code.Append("sin(");
                        sender.Visit(methodCall.Arguments[0]);
                        sender._code.Append(")");
                    }
                },
                {
                    "System.Math.Cos",
                    delegate(GLSLGenerator sender, MethodCallExpression methodCall)
                    {
                        CheckArgumentCount("cos", 1, methodCall.Arguments.Count);

                        sender._code.Append("cos(");
                        sender.Visit(methodCall.Arguments[0]);
                        sender._code.Append(")");
                    }
                },
                {
                    "System.Math.Sqrt",
                    delegate(GLSLGenerator sender, MethodCallExpression methodCall)
                    {
                        CheckArgumentCount("sqrt", 1, methodCall.Arguments.Count);

                        sender._code.Append("sqrt(");
                        sender.Visit(methodCall.Arguments[0]);
                        sender._code.Append(")");
                    }
                },
                {
                    "System.Math.Min",
                    delegate(GLSLGenerator sender, MethodCallExpression methodCall)
                    {
                        CheckArgumentCount("min", 2, methodCall.Arguments.Count);

                        sender._code.Append("min(");
                        sender.Visit(methodCall.Arguments[0]);
                        sender._code.Append(", ");
                        sender.Visit(methodCall.Arguments[1]);
                        sender._code.Append(")");
                    }
                },
                {
                    "System.Math.Max",
                    delegate(GLSLGenerator sender, MethodCallExpression methodCall)
                    {
                        CheckArgumentCount("max", 2, methodCall.Arguments.Count);

                        sender._code.Append("max(");
                        sender.Visit(methodCall.Arguments[0]);
                        sender._code.Append(", ");
                        sender.Visit(methodCall.Arguments[1]);
                        sender._code.Append(")");
                    }
                },
                {
                    "Brahma.Vector4.Length",
                    delegate(GLSLGenerator sender, MethodCallExpression methodCall)
                    {
                        CheckArgumentCount("length", 1, methodCall.Arguments.Count);
                        sender._code.Append("length(");
                        sender.Visit(methodCall.Arguments[0]);
                        sender._code.Append(")");
                    }
                },
                {
                    "System.Math.Log",
                    delegate(GLSLGenerator sender, MethodCallExpression methodCall)
                    {
                        CheckArgumentCount("log", 1, methodCall.Arguments.Count);
                        sender._code.Append("log(");
                        sender.Visit(methodCall.Arguments[0]);
                        sender._code.Append(")");
                    }
                },
                {
                    "System.Math.Floor",
                    delegate(GLSLGenerator sender, MethodCallExpression methodCall)
                    {
                        CheckArgumentCount("floor", 1, methodCall.Arguments.Count);
                        sender._code.Append("floor(");
                        sender.Visit(methodCall.Arguments[0]);
                        sender._code.Append(")");
                    }
                },
                {
                    typeof (DataParallelArray1DBase<float>).FullName + ".get_Item", // We need to pass in the name like this
                    IndexInto1D
                },
                {
                    typeof (DataParallelArray2DBase<float>).FullName + ".get_Item", // We need to pass in the name like this
                    IndexInto2D
                },
                {
                    typeof (DataParallelArray1DBase<Vector2>).FullName + ".get_Item", // We need to pass in the name like this
                    IndexInto1D
                },
                {
                    typeof (DataParallelArray2DBase<Vector2>).FullName + ".get_Item", // We need to pass in the name like this
                    IndexInto2D
                },
                {
                    typeof (DataParallelArray1DBase<Vector3>).FullName + ".get_Item", // We need to pass in the name like this
                    IndexInto1D
                },
                {
                    typeof (DataParallelArray2DBase<Vector3>).FullName + ".get_Item", // We need to pass in the name like this
                    IndexInto2D
                },
                {
                    typeof (DataParallelArray1DBase<Vector4>).FullName + ".get_Item", // We need to pass in the name like this
                    IndexInto1D
                },
                {
                    typeof (DataParallelArray2DBase<Vector4>).FullName + ".get_Item", // We need to pass in the name like this
                    IndexInto2D
                }
            };

        // Add anonymous methods that transform "new" calls here
        private static readonly Dictionary<string, TransformExpression<NewExpression>> _newExpressionTransformations =
            new Dictionary<string, TransformExpression<NewExpression>>
            {
                {
                    "Brahma.Vector2",
                    delegate(GLSLGenerator sender, NewExpression newExpression)
                    {
                        sender._code.Append("vec2(");
                        if (newExpression.Arguments.Count == 2)
                        {
                            sender.Visit(newExpression.Arguments[0]);
                            sender._code.Append(", ");
                            sender.Visit(newExpression.Arguments[1]);
                        }
                        else
                            sender._code.Append("0.0, 0.0"); // default constructor
                        sender._code.Append(")");
                    }
                },
                {
                    "Brahma.Vector3",
                    delegate(GLSLGenerator sender, NewExpression newExpression)
                    {
                        sender._code.Append("vec3(");
                        if (newExpression.Arguments.Count == 3)
                        {
                            sender.Visit(newExpression.Arguments[0]);
                            sender._code.Append(", ");
                            sender.Visit(newExpression.Arguments[1]);
                            sender._code.Append(", ");
                            sender.Visit(newExpression.Arguments[2]);
                        }
                        else
                            sender._code.Append("0.0, 0.0, 0.0"); // default constructor
                        sender._code.Append(")");
                    }
                },
                {
                    "Brahma.Vector4",
                    delegate(GLSLGenerator sender, NewExpression newExpression)
                    {
                        sender._code.Append("vec4(");
                        if (newExpression.Arguments.Count == 4)
                        {
                            sender.Visit(newExpression.Arguments[0]);
                            sender._code.Append(", ");
                            sender.Visit(newExpression.Arguments[1]);
                            sender._code.Append(", ");
                            sender.Visit(newExpression.Arguments[2]);
                            sender._code.Append(", ");
                            sender.Visit(newExpression.Arguments[3]);
                        }
                        else
                            sender._code.Append("0.0, 0.0, 0.0, 0.0"); // default constructor
                        sender._code.Append(")");
                    }
                }
            };

        private static readonly Dictionary<Type, string> _typeTranslations =
            new Dictionary<Type, string> // A table of type translations
            {
                { typeof (int), "int" },
                { typeof (float), "float" },
                { typeof (double), "float" },
                { typeof (Vector2), "vec2" },
                { typeof (Vector3), "vec3" },
                { typeof (Vector4), "vec4" }
            };

        private readonly Dictionary<string, Expression> _anonTypeFieldValues =
            new Dictionary<string, Expression>();

        private readonly ExpressionProcessor _expressionProcessor;
        private StringBuilder _code;

        private GLSLGenerator(ExpressionProcessor expressionProcessor)
        {
            if (expressionProcessor == null)
                throw new ArgumentNullException("expressionProcessor");

            _expressionProcessor = expressionProcessor;
        }

        private GLSLGenerator(Expression expression)
            : this(new ExpressionProcessor(expression))
        {
        }

        private delegate void TransformExpression<T>(GLSLGenerator sender, T methodCall);

        private static void CheckArgumentCount(string function, int expectedArgumentCount, int actualArgumentCount) // Utility method for method call transformers
        {
            if (expectedArgumentCount != actualArgumentCount)
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Incorrect number of arguments to {0}(): {1}, expected {2}.", function, expectedArgumentCount, actualArgumentCount));
        }

        private static void IndexInto1D(GLSLGenerator sender, MethodCallExpression methodCall)
        {
            CheckArgumentCount("get_Item", 1, methodCall.Arguments.Count);
            // This should only be indexing on a data-parallel array which is a parameter
            if (!((methodCall.Object is ParameterExpression) &&
                  (typeof (DataParallelArrayBase).IsAssignableFrom(methodCall.Object.Type)) &&
                  (sender._expressionProcessor.QueryParameters.Contains(methodCall.Object as ParameterExpression))))
                throw new NotSupportedException("Indexing inside a GPU query is allowed only on data-parallel array type query parameters");

            sender._code.Append(string.Format(CultureInfo.InvariantCulture, "texture2D({0}, vec2(", ((ParameterExpression)methodCall.Object).Name)); // Uniform name and vec2 constructor
            sender.Visit(methodCall.Arguments[0]); // first parameter for the vec2 constructor, the actual index
            // Normalize by multiplying it with (1 / texture's width). The component is already de-normalized (see VisitMemberAccess)
            sender._code.Append(string.Format(CultureInfo.InvariantCulture, " * _brahma_invDimensions[{0}].x",
                                              sender._expressionProcessor.QueryParameters.IndexOf(methodCall.Object as ParameterExpression)));

            sender._code.Append(", 0.0))"); // Finish vec2 constructor and close texture2D braces

            // HLSL seems to let us get away with sloppiness in swizzling, GLSL doesn't
            if (methodCall.Type == typeof (float))
                sender._code.Append(".x");
            if (methodCall.Type == typeof (Vector2))
                sender._code.Append(".xy");
            if (methodCall.Type == typeof (Vector3))
                sender._code.Append(".xyz");
        }

        private static void IndexInto2D(GLSLGenerator sender, MethodCallExpression methodCall)
        {
            CheckArgumentCount("get_Item", 2, methodCall.Arguments.Count);
            // This should only be indexing on a data-parallel array which is a parameter
            if (!((methodCall.Object is ParameterExpression) &&
                  (typeof (DataParallelArrayBase).IsAssignableFrom(methodCall.Object.Type)) &&
                  (sender._expressionProcessor.QueryParameters.Contains(methodCall.Object as ParameterExpression))))
                throw new NotSupportedException("Indexing inside a GPU query is allowed only on data-parallel array type query parameters");

            int index = sender._expressionProcessor.QueryParameters.IndexOf(methodCall.Object as ParameterExpression);

            sender._code.Append(string.Format(CultureInfo.InvariantCulture, "texture2D({0}, vec2(", ((ParameterExpression)methodCall.Object).Name)); // Uniform name and vec2 constructor
            sender.Visit(methodCall.Arguments[0]); // first parameter for the vec2 constructor
            // Normalize by multiplying it with (1 / texture's width). The component is already de-normalized (see VisitMemberAccess)
            sender._code.Append(string.Format(CultureInfo.InvariantCulture, " * _brahma_invDimensions[{0}].x, ", index));

            sender.Visit(methodCall.Arguments[1]); // second parameter for the vec2 constructor
            // Normalize by multiplying it with (1 / texture's width). The component is already de-normalized (see VisitMemberAccess)
            sender._code.Append(string.Format(CultureInfo.InvariantCulture, " * _brahma_invDimensions[{0}].y", index));

            sender._code.Append("))");

            // HLSL seems to let us get away with sloppiness in swizzling, GLSL doesn't
            if (methodCall.Type == typeof (float))
                sender._code.Append(".x");
            if (methodCall.Type == typeof (Vector2))
                sender._code.Append(".xy");
            if (methodCall.Type == typeof (Vector3))
                sender._code.Append(".xyz");
        }

        // Add anonymous that transform method calls here. We to add more method support, preferably everything GLSL supports

        private static string TranslateType(Type type)
        {
            if (!_typeTranslations.ContainsKey(type))
                throw new TranslationException(string.Format(CultureInfo.InvariantCulture, "Cannot translate {0} to an equivalent GLSL type", type.FullName));

            return _typeTranslations[type];
        }

        private static string GetStringRepresentation(ConstantExpression constant)
        {
            if ((constant.Type == typeof (float)) || (constant.Type == typeof (double)))
                return Convert.ToSingle(constant.Value).ToString(FloatNumberFormat, CultureInfo.InvariantCulture); // Fix the number of decimals

            return constant.Value.ToString();
        }

        // Reflected (and modified) code from System.Linq.Expressions.NewExpression.GetPropertyNoThrow
        private static PropertyInfo GetProperty(MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            Type declaringType = method.DeclaringType; // Find the type that declares this method
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public;
            bindingAttr |= method.IsStatic
                               ? BindingFlags.Static
                               : BindingFlags.Instance; // Set the Binding flags based on if this is static, non-public etc.
            foreach (PropertyInfo info in declaringType.GetProperties(bindingAttr))
            {
                if (info.CanRead && (method == info.GetGetMethod(true))) // Get the getter
                    return info;
                if (info.CanWrite && (method == info.GetSetMethod(true))) // Get the setter
                    return info;
            }

            // We couldn't find a PropertyInfo for this method, throw an exception
            // Is this the right exception to throw? Sounds about right to me...
            throw new MemberAccessException(string.Format(CultureInfo.InvariantCulture, "Could not find property info for method {0}.{1}", method.DeclaringType.FullName, method.Name));
        }

        // This method is private, meant only for use from the static overloaded Generate(...) method
        private string Generate()
        {
            // Begin the GLSL generation here. We need to construct the code in 4 parts
            // Parameters - Samplers
            // Parameters - Member access, only allowed for members of type float, Vector2, Vector3 and Vector4
            // Functions we need to define (like ternary)
            // body

            var samplers = new StringBuilder();
            foreach (ParameterExpression parameter in _expressionProcessor.QueryParameters) // Query parameters are data-parallel arrays
                samplers.AppendLine(string.Format(CultureInfo.InvariantCulture, "sampler2D {0};", parameter.Name));

            var parameters = new StringBuilder();
            var addedParameters = new List<string>();
            foreach (MemberExpression memberAccess in _expressionProcessor.MemberAccess) // These are uniforms generated by member access
            {
                if (!(memberAccess.Member is FieldInfo)) // We can't access anything other than fields since a property might change its value between two runs of the kernel
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Cannot access {0} because properties/method calls are not allowed inside a query that runs on the GPU", memberAccess));

                if (addedParameters.Contains(memberAccess.Member.Name))
                    continue; // We've added this before

                parameters.AppendLine(string.Format(CultureInfo.InvariantCulture, "uniform {0} {1};", TranslateType(memberAccess.Type), memberAccess.Member.Name));
                addedParameters.Add(memberAccess.Member.Name);
            }

            // Add parameters that we need internally. The user doesn't need to know about these at all
            parameters.AppendLine("uniform float _brahma_minWidth;");
            parameters.AppendLine("uniform float _brahma_minHeight;");
            parameters.AppendLine(string.Format(CultureInfo.InvariantCulture, "uniform vec2 _brahma_invDimensions[{0}];", _expressionProcessor.QueryParameters.Count));

            var functions = new StringBuilder();
            // Append functions for the ternary operators. Doesn't matter if we don't use them, the GLSL compiler will strip 'em out.
            functions.AppendLine("float ternary(bool condition, float ifTrue, float ifFalse){ if (condition) return ifTrue; else return ifFalse; }");
            functions.AppendLine("vec2 ternary(bool condition, vec2 ifTrue, vec2 ifFalse){ if (condition) return ifTrue; else return ifFalse; }");
            functions.AppendLine("vec3 ternary(bool condition, vec3 ifTrue, vec3 ifFalse){ if (condition) return ifTrue; else return ifFalse; }");
            functions.AppendLine("vec4 ternary(bool condition, vec4 ifTrue, vec4 ifFalse){ if (condition) return ifTrue; else return ifFalse; }");

            var body = new StringBuilder();
            body.AppendLine(string.Format(CultureInfo.InvariantCulture, "void {0}()", EntryPoint));
            body.AppendLine("{");

            _code = body; // We're generating code for the body now

            // Process all local variables. The starting point here are the anonymous type allocations. We will find the trivial cases where
            // an anonymous type simply wraps another variable, and ignore it.
            foreach (NewExpression newAnonType in _expressionProcessor.NewAnonymousTypes)
            {
                PropertyInfo property;
                int index = 0;
                foreach (MemberInfo member in newAnonType.Members)
                {
                    if ((member.MemberType != MemberTypes.Method) || ((property = GetProperty(member as MethodInfo)) == null))
                        continue;

                    if ((newAnonType.Arguments[index] is ParameterExpression) && (property.Name == ((ParameterExpression)newAnonType.Arguments[index]).Name))
                    {
                        if (!(property.PropertyType.IsAnonymous() && newAnonType.Arguments[index].Type.IsAnonymous())) // If this maps an anonymous type's field to another we don't want it
                            _anonTypeFieldValues.Add(property.Name, newAnonType.Arguments[index]); // Remember this, we need it if we encounter this member again
                        index++;
                        continue; // This is trivial, the anonymous type is wrapping a parameter in a field of the same name.
                    }

                    body.Append(string.Format(CultureInfo.InvariantCulture, "{0} {1} = ", TranslateType(property.PropertyType), property.Name));
                    _anonTypeFieldValues.Add(property.Name, null); // We add null here against the property name because we don't want to recalculate it when writing code.
                    // If it's null, VisitMemberAccess will just substitute the name of the property here (which is also the local variable)

                    Visit(newAnonType.Arguments[index]);
                    body.AppendLine(";"); // End this line

                    index++;
                }
            }

            // We now need to go through _expressionProcessor.MainLambda and generate code for the return statement
            body.Append("gl_FragColor = ");

            // We always have to return a vec4, so make sure we construct a new vec4 for all cases except if the lambda's return type is a Vector4
            if (_expressionProcessor.MainLambda.Body.Type != typeof (Vector4))
                _code.Append("vec4((");

            Visit(_expressionProcessor.MainLambda.Body); // Generate code

            // pad out parameters
            if ((_expressionProcessor.MainLambda.Body.Type == typeof(float)) ||
                (_expressionProcessor.MainLambda.Body.Type == typeof(double)) ||
                (_expressionProcessor.MainLambda.Body.Type == typeof(int)))
                _code.Append(").x, 0.0, 0.0, 0.0)"); // 3 to pad
            if (_expressionProcessor.MainLambda.Body.Type == typeof (Vector2))
                _code.Append(").xy, 0.0, 0.0)"); // 2 to pad
            if (_expressionProcessor.MainLambda.Body.Type == typeof (Vector3))
                _code.Append(").xyz, 0.0)"); // 1 to pad

            body.AppendLine(";"); // End the statement, nothing to pad

            body.AppendLine("}"); // Close braces

            return samplers + "\n" +
                   parameters + "\n" +
                   functions + "\n" +
                   body; // combine all the parts and return
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            // Is this a kernel parameter?
            if (_expressionProcessor.KernelParameters.Contains(p)) // If it is, this a sampling operation
            {
                int index = _expressionProcessor.KernelParameters.IndexOf(p);
                // We don't know the dimensionality of this array, so we might as well compute both components
                _code.Append(string.Format(CultureInfo.InvariantCulture, "texture2D({0}, vec2(gl_TexCoord[0].x * _brahma_minWidth * _brahma_invDimensions[{1}].x, gl_TexCoord[0].y * _brahma_minHeight * _brahma_invDimensions[{1}].y))",
                                           _expressionProcessor.QueryParameters[index].Name, index));
            }

            // Swizzle based on the parameter's type. Remember, a texture2D gives us a vec4
            if (p.Type == typeof (float))
                _code.Append(".x"); // Only one component
            if (p.Type == typeof (Vector2))
                _code.Append(".xy");
            if (p.Type == typeof (Vector3))
                _code.Append(".xyz");
            // It could only be a vec4. In which case, we have nothing to append, we already have what we need

            return p;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            _code.Append("("); // Always add braces
            // This is a special case, since GLSL seems to let me get away with % between a float and an integer!
            if (binaryExpression.NodeType == ExpressionType.Modulo)
                _code.Append("(int)");
            Visit(binaryExpression.Left);

            switch (binaryExpression.NodeType)
            {
                case ExpressionType.Add:
                    _code.Append(" + ");
                    break;

                case ExpressionType.Subtract:
                    _code.Append(" - ");
                    break;

                case ExpressionType.Multiply:
                    _code.Append(" * ");
                    break;

                case ExpressionType.Divide:
                    _code.Append(" / ");
                    break;

                case ExpressionType.GreaterThan:
                    _code.Append(" > ");
                    break;

                case ExpressionType.LessThan:
                    _code.Append(" < ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    _code.Append(" >= ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    _code.Append(" <= ");
                    break;

                case ExpressionType.Equal:
                    _code.Append(" == ");
                    break;

                case ExpressionType.NotEqual:
                    _code.Append(" != ");
                    break;

                case ExpressionType.Modulo:
                    _code.Append(" % ");
                    break;

                case ExpressionType.OrElse:
                    _code.Append(" || ");
                    break;

                case ExpressionType.AndAlso:
                    _code.Append(" && ");
                    break;

                default:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Cannot use the operator '{0}' in queries at this time",
                                                                  binaryExpression.NodeType));
            }

            // This is a special case, since GLSL seems to let me get away with % between a float and an integer!
            if (binaryExpression.NodeType == ExpressionType.Modulo)
                _code.Append("(int)");
            Visit(binaryExpression.Right);
            _code.Append(")");

            return binaryExpression;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            // Check to see if its a case where we're trying to access Current from a data-parallel array
            if ((m.Member.DeclaringType.IsDataParallelArray1D()) &&
                (m.Member.Name == "Current") &&
                (m.Expression != null) &&
                (m.Expression is ParameterExpression))
            {
                // De-normalize the texture coordinate. It will be normalized just before tex2D'ing
                _code.Append("(gl_TexCoord[0].x * _brahma_minWidth)"); // We can afford to use only one component, because we know this is a DataParallelArray
                return m;
            }

            // Check to see if its a case where we're trying to access CurrentX or CurrentY from a data-parallel array
            if ((m.Member.DeclaringType.IsDataParallelArray2D()) &&
                (m.Expression is ParameterExpression))
            {
                switch (m.Member.Name)
                {
                    case "CurrentX":
                        // De-normalize the texture coordinate. It will be normalized just before tex2D'ing
                        _code.Append("(gl_TexCoord[0].x * _brahma_minWidth)"); // We know this is CurrentX of a DataParallArray2D
                        return m;

                    case "CurrentY":
                        // De-normalize the texture coordinate. It will be normalized just before tex2D'ing
                        _code.Append("(gl_TexCoord[0].y * _brahma_minHeight)"); // We know this is CurrentY of a DataParallArray2D
                        return m;
                }
            }

            if (_expressionProcessor.MemberAccess.Contains(m, new MemberExpressionComparer())) // Add code only if it's a valid member access
            {
                _code.Append(m.Member.Name);
                return m;
            }

            if (m.Member.DeclaringType.IsAnonymous())
            {
                if (_anonTypeFieldValues.ContainsKey(m.Member.Name)) // Do we know this member?
                {
                    if (_anonTypeFieldValues[m.Member.Name] != null) // Is it a local variable or an in-place expression (see Generate, where anonymous types are processed)
                        Visit(_anonTypeFieldValues[m.Member.Name]); // In-place expression
                    else
                    {
                        _code.Append(m.Member.Name); // Local variable
                        Visit(m.Expression);
                    }
                }
            }
            else
            {
                Visit(m.Expression); // Visit
                _code.Append(".");
                _code.Append(m.Member.Name); // Add the member's name finally :)
            }

            return m;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            _code.Append(GetStringRepresentation(c));

            return c;
        }

        protected override Expression VisitUnary(UnaryExpression unary)
        {
            if (unary.NodeType == ExpressionType.Convert)
            {
                // This is a typecast, deal with it
                if (unary.Type == typeof (int))
                {
                    // We're casting to an int
                    _code.Append("(int)(");
                    Visit(unary.Operand);
                    _code.Append(")");
                }
                else if ((unary.Type == typeof (double)) || (unary.Type == typeof (float))) // Double and floats don't need any conversion in the shader
                    Visit(unary.Operand);
                else // else whoops, that's what
                    throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture, "Cannot cast operand of type {0} to {1} at this time", unary.Operand.Type.FullName, unary.Type.FullName));
            }
            else
                Visit(unary.Operand); // Skip the unary expressions. Get straight to the meat

            return unary;
        }

        protected override Expression VisitConditional(ConditionalExpression c)
        {
            _code.Append("ternary(");
            Visit(c.Test);
            _code.Append(", ");
            Visit(c.IfTrue);
            _code.Append(", ");
            Visit(c.IfFalse);
            _code.Append(")");

            return c;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            string fullMethodName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", m.Method.DeclaringType.FullName, m.Method.Name);
            if (_methodCallTransformations.ContainsKey(fullMethodName))
                _methodCallTransformations[fullMethodName](this, m); // This will add the appropriate code
            else
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The method {0} is not supported", fullMethodName));

            return m;
        }

        protected override Expression VisitMemberInit(MemberInitExpression init)
        {
            if ((init.NewExpression.Type != typeof (Vector2)) &&
                (init.NewExpression.Type != typeof (Vector3)) &&
                (init.NewExpression.Type != typeof (Vector4)))
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Cannot create objects of type {0} inside a query that runs on the GPU. Permissible types are Vector2, Vector3 and Vector4",
                                                              init.NewExpression.Type.FullName));

            // Even if we have arguments for a NewExpression and some member inits, we'll accumulate them all here against the argument/field names
            // this means that the constructor arguments and the field names HAVE to be same, we're assuming this
            var finalArguments = new Dictionary<string, Expression>();
            // Member inits will override the constructor arguments

            if (init.NewExpression.Arguments.Count > 0) // Do we have constructor arguments?
            {
                ParameterInfo[] parameters = init.NewExpression.Constructor.GetParameters(); // Get the parameters for this constructor
                int index = 0;
                foreach (Expression argument in init.NewExpression.Arguments)
                {
                    finalArguments.Add(parameters[index].Name, argument); // Keep these, member inits will overwrite some if necessary
                    index++;
                }
            }

            foreach (MemberBinding binding in init.Bindings)
                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        if (finalArguments.ContainsKey(binding.Member.Name))
                            finalArguments[binding.Member.Name] = ((MemberAssignment)binding).Expression; // The constructor initialized it, overwrite
                        else
                            finalArguments.Add(binding.Member.Name, ((MemberAssignment)binding).Expression); // The constructor didn't initialize it, add it

                        break;
                    default:
                        throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Member bindings of type {0} are not supported",
                                                                      binding.BindingType));
                }

            IEnumerable<Type> argumentTypes = from Expression node in finalArguments.Values
                                              select node.Type;
            Visit(Expression.New(init.NewExpression.Type.GetConstructor(argumentTypes.ToArray()), finalArguments.Values.ToArray())); // Convert this to a new Expression, and Visit it so code will be generated

            return init;
        }

        protected override NewExpression VisitNew(NewExpression nex)
        {
            if (_newExpressionTransformations.ContainsKey(nex.Type.FullName))
                _newExpressionTransformations[nex.Type.FullName](this, nex);

            return nex;
        }

        // Static methods to generate the GLSL. They internally create an instance and perform the processing required.
        public static string Generate(ExpressionProcessor expressionProcessor)
        {
            return new GLSLGenerator(expressionProcessor).Generate();
        }

        public static string Generate(Expression expression)
        {
            return new GLSLGenerator(expression).Generate();
        }
    }
}