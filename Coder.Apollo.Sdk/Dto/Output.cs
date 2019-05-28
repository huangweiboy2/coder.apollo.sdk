using System;
using System.Collections.Generic;

namespace Coder.Apollo.Sdk.Dto
{
    public class ApolloConfigOutput : OutputBase<ICollection<ApolloConfig>>
    {
        public ApolloConfigOutput(Exception exception, bool hasChanged, ICollection<ApolloConfig> data) : base(exception, hasChanged, data)
        {
        }
    }

    public class ApolloConfigNotificationOutput : OutputBase<ICollection<ApolloConfigNotification>>
    {
        public ApolloConfigNotificationOutput(Exception exception, bool hasChanged, ICollection<ApolloConfigNotification> data) : base(exception, hasChanged, data)
        {
        }
    }

    public class OutputBase<T> where T : class
    {
        public Exception Exception { get; set; }

        public bool HasChanged { get; set; }

        public bool HasData => this.Data != null;
        public T Data { get; set; }

        public OutputBase(Exception exception, bool hasChanged, T data)
        {
            Exception = exception;
            HasChanged = hasChanged;
            Data = data;
        }
    }
}