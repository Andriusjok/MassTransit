﻿// Copyright 2007-2014 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.SubscriptionConnectors
{
    using System;
    using Pipeline;
    using Pipeline.Filters;
    using Pipeline.Sinks;
    using Policies;
    using Util;


    public interface ConsumerMessageConnector :
        ConsumerConnector
    {
        Type MessageType { get; }
    }


    public class ConsumerMessageConnector<TConsumer, TMessage> :
        ConsumerMessageConnector
        where TConsumer : class
        where TMessage : class
    {
        readonly IPipe<ConsumeContext<Tuple<TConsumer, ConsumeContext<TMessage>>>> _consumerPipe;

        public ConsumerMessageConnector(IPipe<ConsumeContext<Tuple<TConsumer, ConsumeContext<TMessage>>>> consumerPipe)
        {
            _consumerPipe = consumerPipe;
        }

        public Type MessageType
        {
            get { return typeof(TMessage); }
        }

        ConnectHandle ConsumerConnector.Connect<T>(IInboundPipe inboundPipe, IConsumerFactory<T> consumerFactory, IRetryPolicy retryPolicy)
        {
            var factory = consumerFactory as IConsumerFactory<TConsumer>;
            if (factory == null)
                throw new ArgumentException("The consumer factory type does not match: " + TypeMetadataCache<T>.ShortName);

            IPipe<ConsumeContext<TMessage>> pipe = Pipe.New<ConsumeContext<TMessage>>(x =>
            {
                x.Retry(retryPolicy);
                x.Filter(new ConsumerMessageFilter<TConsumer, TMessage>(factory, _consumerPipe));
            });

            return inboundPipe.Connect(pipe);
        }
    }
}