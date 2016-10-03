﻿#region Copyright

// ****************************************************************************
// <copyright file="BindingMemberVisitor.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Interfaces.Parse;
using MugenMvvmToolkit.Binding.Interfaces.Parse.Nodes;
using MugenMvvmToolkit.Binding.Parse.Nodes;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Parse
{
    public class BindingMemberVisitor : IExpressionVisitor
    {
        #region Fields

        private readonly List<string> _lamdaParameters;
        private readonly IList<KeyValuePair<string, BindingMemberExpressionNode>> _members;
        private readonly Dictionary<IExpressionNode, IExpressionNode> _staticNodes;
        private bool _isMulti;
        private readonly bool _ignoreLambda;

        //To reduce object creation in the TryGetMemberName method.
        private List<IExpressionNode> _nodes;

        #endregion

        #region Constructors

        public BindingMemberVisitor()
        {
            _members = new List<KeyValuePair<string, BindingMemberExpressionNode>>();
            _lamdaParameters = new List<string>();
            _staticNodes = new Dictionary<IExpressionNode, IExpressionNode>();
        }

        private BindingMemberVisitor(BindingMemberVisitor innerVisitor, IEnumerable<string> lambdaParameters, IDataContext context)
            : this()
        {
            _ignoreLambda = true;
            _members = innerVisitor._members;
            _staticNodes = innerVisitor._staticNodes;
            if (innerVisitor._lamdaParameters != null)
                _lamdaParameters.AddRange(innerVisitor._lamdaParameters);
            _lamdaParameters.AddRange(lambdaParameters);
            Context = context;
            BindingExtensions.CheckDuplicateLambdaParameter(_lamdaParameters);
        }

        #endregion

        #region Properties

        public IList<KeyValuePair<string, BindingMemberExpressionNode>> Members => _members;

        public bool IsMulti => _isMulti;

        public IDataContext Context { get; set; }

        public bool IsPostOrder => false;

        #endregion

        #region Implementation of IExpressionVisitor

        public IExpressionNode Visit(IExpressionNode node)
        {
            var lambdaNode = node as ILambdaExpressionNode;
            if (lambdaNode != null && !_ignoreLambda)
                return VisitLambda(lambdaNode);

            var methodCall = node as IMethodCallExpressionNode;
            if (methodCall != null)
                return VisitMethodCall(methodCall);

            var relativeSource = node as IRelativeSourceExpressionNode;
            if (relativeSource != null)
                return VisitRelativeSource(relativeSource);
            return VisitExpression(node);
        }

        #endregion

        #region Methods

        public void Clear()
        {
            _members.Clear();
            _isMulti = false;
            _lamdaParameters.Clear();
            _staticNodes.Clear();
            if (_nodes != null)
                _nodes.Clear();
        }

        private IExpressionNode VisitMethodCall(IMethodCallExpressionNode methodCall)
        {
            _isMulti = true;
            if (methodCall.Target != null)
                return methodCall;
            BindingMemberExpressionNode member = GetOrAddBindingMember(string.Empty, (s, i) => new BindingMemberExpressionNode(string.Empty, s, i));
            return new MethodCallExpressionNode(member, methodCall.Method, methodCall.Arguments, methodCall.TypeArgs).Accept(this);
        }

        private IExpressionNode VisitRelativeSource(IRelativeSourceExpressionNode rs)
        {
            string memberName = rs.Type + rs.Path + rs.Level.ToString() + rs.ElementName;
            return GetOrAddBindingMember(memberName, (s, i) => new BindingMemberExpressionNode(rs, s, i));
        }

        private IExpressionNode VisitExpression(IExpressionNode node)
        {
            if (_nodes == null)
                _nodes = new List<IExpressionNode>();
            else
                _nodes.Clear();

            string memberName = node.TryGetMemberName(true, true, _nodes);
            if (memberName == null)
            {
                _isMulti = true;
                return node;
            }
            if (_nodes[0] is ResourceExpressionNode)
                return GetResourceMember(node, memberName, _nodes);

            IBindingPath path = BindingServiceProvider.BindingPathFactory(memberName);
            if (path.IsEmpty)
                return GetOrAddBindingMember(memberName, (s, i) => new BindingMemberExpressionNode(memberName, s, i));
            string firstMember = path.Parts[0];
            if (_lamdaParameters.Contains(firstMember))
                return node;
            return GetOrAddBindingMember(memberName, (s, i) => new BindingMemberExpressionNode(memberName, s, i));
        }

        private IExpressionNode VisitLambda(ILambdaExpressionNode node)
        {
            _isMulti = true;
            node.Accept(new BindingMemberVisitor(this, node.Parameters, Context));
            return node.Clone();
        }

        private IExpressionNode GetResourceMember(IExpressionNode node, string memberName, IList<IExpressionNode> nodes)
        {
            IExpressionNode staticValue;
            if (_staticNodes.TryGetValue(node, out staticValue))
                return staticValue;

            IBindingPath path = BindingServiceProvider.BindingPathFactory(memberName);
            string firstMember = path.Parts[0];
            Type type = BindingServiceProvider.ResourceResolver.ResolveType(firstMember, Context, false);
            var resourceMember = (ResourceExpressionNode)nodes[0];
            if (resourceMember.Dynamic && type == null)
            {
                memberName = BindingExtensions.MergePath(path.Parts.Skip(1).ToArray());
                return GetOrAddBindingMember("$" + path.Path, (s, i) => new BindingMemberExpressionNode(firstMember, memberName, s, i));
            }

            bool dynamicMember = false;
            IExpressionNode firstMemberNode = nodes[1];
            if (!_staticNodes.TryGetValue(firstMemberNode, out staticValue))
            {
                if (type == null)
                {
                    var value = BindingServiceProvider
                        .ResourceResolver
                        .ResolveObject(firstMember, Context, true)
                        .Value;
                    var dynamicObject = value as IDynamicObject;
                    if (dynamicObject == null || path.Parts.Count <= 1)
                        staticValue = new ConstantExpressionNode(value);
                    else
                    {
                        staticValue = new ConstantExpressionNode(dynamicObject.GetMember(path.Parts[1], Empty.Array<object>()));
                        dynamicMember = true;
                    }
                }
                else
                    staticValue = new ConstantExpressionNode(type, typeof(Type));
                _staticNodes[firstMemberNode] = staticValue;
                if (dynamicMember)
                    _staticNodes[nodes[2]] = staticValue;
            }
            if (firstMemberNode == node || (dynamicMember && node == nodes[2]))
                return staticValue;
            return node;
        }

        private BindingMemberExpressionNode GetOrAddBindingMember(string memberName,
            Func<string, int, BindingMemberExpressionNode> getNode)
        {
            KeyValuePair<string, BindingMemberExpressionNode> bindingMember =
                _members.SingleOrDefault(pair => pair.Key == memberName);
            if (bindingMember.Value == null)
            {
                bindingMember = new KeyValuePair<string, BindingMemberExpressionNode>(memberName,
                    getNode(GetParameterName(), _members.Count));
                _members.Add(bindingMember);
            }
            return bindingMember.Value;
        }

        private string GetParameterName()
        {
            return "param_" + _members.Count.ToString();
        }

        #endregion
    }
}
