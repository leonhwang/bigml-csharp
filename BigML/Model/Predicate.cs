using System;
using System.Collections.Generic;
using System.Json;
using LinqExpr = System.Linq.Expressions;

namespace BigML
{
    public partial class Model
    {
        /// <summary>
        /// Predicate structure to make a decision at each node.
        /// </summary>
        public class Predicate
        {
            readonly dynamic _predicate;
            private readonly string comparisonOperator;

            internal Predicate(JsonValue json)
            {
                _predicate = json;
                if (this.MissingOperator)
                {
                    comparisonOperator = this.Operator.Replace("*", "");
                } else
                {
                    comparisonOperator = this.Operator;
                }
                
            }

            internal LinqExpr.Expression Expression(Dictionary<string, LinqExpr.ParameterExpression> parameters)
            {
                if (IsSimple)
                {
                    return LinqExpr.Expression.Constant(Constant);
                }
                var value = default(double);
                var x = Value as object;
                var fieldType = "null";

                try {
                    if (Double.TryParse(Value, out value)) x = value;

                    fieldType = parameters[Field].Type.Name.ToLower();
                } catch { // Exception ex) {
                    x = 0.0;
                    //Console.Out.WriteLine("UnkownType?" + ex.ToString());
                }

                switch (Operator)
                {
                    case ">":
                    case ">*":
                        return LinqExpr.Expression.GreaterThan(parameters[Field],
                                                                              LinqExpr.Expression.
                                                                                  Constant(x));
                    case ">=":
                    case ">=*":
                        return LinqExpr.Expression.GreaterThanOrEqual(parameters[Field],

                                                                                     LinqExpr.
                                                                                         Expression.
                                                                                         Constant(x));
                    case "<>":
                    case "!=":
                    case "!=*":
                        if (fieldType == "string")
                        {
                            return LinqExpr.Expression.NotEqual(parameters[Field],
                                                                        System.Linq.Expressions.Expression.
                                                                            Constant(Value));
                        }
                        else
                        {
                            return LinqExpr.Expression.NotEqual(parameters[Field],
                                                                           LinqExpr.
                                                                               Expression.
                                                                               Constant(x));
                        }
                        
                    case "<":
                    case "<*":
                        return LinqExpr.Expression.LessThan(parameters[Field],
                                                                           System.Linq.Expressions.Expression.
                                                                               Constant(Value));
                    case "<=":
                    case "<=*":
                        return LinqExpr.Expression.LessThanOrEqual(parameters[Field],
                                                                                  System.Linq.Expressions.Expression.
                                                                                      Constant(x));
                    case "=":
                    case "=*":
                        if (fieldType == "string")
                        {
                            return LinqExpr.Expression.Equal(parameters[Field],
                                                                        System.Linq.Expressions.Expression.
                                                                            Constant(Value));
                        }
                        else
                        {
                            return LinqExpr.Expression.Equal(parameters[Field],
                                                                        System.Linq.Expressions.Expression.
                                                                            Constant(x));
                        }
                        
                    default:
                        throw new Exception("Unknown operator '" + Operator + "'");
                }
            }


            /// <summary>
            /// Is leaf node (boolean)
            /// </summary>
            public bool IsSimple
            {
                get { return _predicate.JsonType == JsonType.Boolean; }
            }

            public string OpType
            {
                get { return _predicate.opType;  }
            }

            public bool Constant
            {
                get { return _predicate; }
            }

            /// <summary>
            /// Field's id used for this decision.
            /// </summary>
            public string Field
            {
                get { return _predicate.field; }
            }

            /// <summary>
            /// Type of test used for this field.
            /// </summary>
            public string Operator
            {
                get { return _predicate.@operator; }
            }

            /// <summary>
            /// Type of test used for this field.
            /// </summary>
            public string ComparisonOperator
            {
                get { return comparisonOperator; }

            }

            /// <summary>
            /// Operator allow manage missing value.
            /// </summary>
            public bool MissingOperator
            {
                get {
                    bool isMissing;
                    isMissing = _predicate.@operator != null && ((string) _predicate.@operator).Contains("*");

                    return isMissing;
                }

            }

            /// <summary>
            /// Value of the field to make this node decision (number or string)
            /// </summary>
            public dynamic Value
            {
                get
                {
                    if (((JsonPrimitive)_predicate.value).JsonType == JsonType.Number)
                    {
                        return (float)_predicate.value;
                    }
                    else
                    {
                        return (string)_predicate.value;
                    }
                }
            }

            public string Term
            {
                get { return _predicate.term;  }
            }

        }
    }
}