﻿using System;
using Whitebox.Core.Application;
using Whitebox.Messages;
using Whitebox.Model;

namespace Whitebox.Core.Updaters
{
    class LifetimeScopeHandler :
        IUpdateHandler<LifetimeScopeBeginningMessage>,
        IUpdateHandler<LifetimeScopeEndingMessage>
    {
        readonly IActiveItemRepository<LifetimeScope> _lifetimeScopes;

        public LifetimeScopeHandler(IActiveItemRepository<LifetimeScope> lifetimeScopes)
        {
            if (lifetimeScopes == null) throw new ArgumentNullException("lifetimeScopes");
            _lifetimeScopes = lifetimeScopes;
        }

        public void UpdateFrom(LifetimeScopeBeginningMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            LifetimeScope parent = null;
            if (message.LifetimeScope.ParentLifetimeScopeId != null)
                _lifetimeScopes.TryGetItem(message.LifetimeScope.ParentLifetimeScopeId, out parent);

            var lifetimeScope = new LifetimeScope(message.LifetimeScope.Id, TagFor(message.LifetimeScope), parent);
            _lifetimeScopes.Add(lifetimeScope);

            if (parent != null)
                parent.ActiveChildren.Add(lifetimeScope);
        }

        static string TagFor(LifetimeScopeModel lifetimeScope)
        {
            return lifetimeScope.Tag == "System.Object" ? 
                null :
                lifetimeScope.Tag;
        }

        public void UpdateFrom(LifetimeScopeEndingMessage message)
        {
            LifetimeScope lifetimeScope;
            if (_lifetimeScopes.TryGetItem(message.LifetimeScopeId, out lifetimeScope))
            {
                if (lifetimeScope.Parent != null)
                    lifetimeScope.Parent.ActiveChildren.Remove(lifetimeScope);

                _lifetimeScopes.RemoveCompleted(lifetimeScope);
            }
        }
    }
}
