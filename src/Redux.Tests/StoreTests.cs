﻿using NUnit.Core;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Reactive.Linq;

namespace Redux.Tests
{
    [TestFixture]
    public class StoreTests
    {
        [Test]
        public void Should_push_initial_state()
        {
            var sut = new Store<int>(1, Reducers.PassThrough);
            var mockObserver = new MockObserver<int>();

            sut.Subscribe(mockObserver);

            CollectionAssert.AreEqual(new[] { 1 }, mockObserver.Values);
        }

        [Test]
        public void Should_push_state_on_dispatch()
        {
            var sut = new Store<int>(1, Reducers.Replace);
            var mockObserver = new MockObserver<int>();

            sut.Subscribe(mockObserver);
            sut.Dispatch(new FakeAction<int>(2));

            CollectionAssert.AreEqual(new[] { 1, 2 }, mockObserver.Values);
        }

        [Test]
        public void Should_only_push_the_last_state_before_subscription()
        {
            var sut = new Store<int>(1, Reducers.Replace);
            var mockObserver = new MockObserver<int>();

            sut.Dispatch(new FakeAction<int>(2));
            sut.Subscribe(mockObserver);

            CollectionAssert.AreEqual(new[] { 2 }, mockObserver.Values);
        }
                
        [Test]
        public void Middleware_should_be_called_for_each_action_dispatched()
        {
            var numberOfCalls = 0;
            Middleware<int> spyMiddleware = store => next => action =>
            {
                numberOfCalls++;
                return next(action);
            };

            var sut = new Store<int>(1, Reducers.Replace, spyMiddleware);
            var mockObserver = new MockObserver<int>();
            
            sut.Subscribe(mockObserver);
            sut.Dispatch(new FakeAction<int>(2));

            Assert.AreEqual(1, numberOfCalls);
            CollectionAssert.AreEqual(new[] { 1, 2 }, mockObserver.Values);
        }

        [Test]
        public void Should_push_state_to_end_of_queue_on_nested_dispatch()
        {
            var sut = new Store<int>(1, Reducers.Replace);
            var mockObserver = new MockObserver<int>();
            sut.Subscribe(val =>
            {
                if (val < 5)
                {
                    sut.Dispatch(new FakeAction<int>(val + 1));
                }
                mockObserver.OnNext(val);
            });

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, mockObserver.Values);
        }

        [Test]
        public void GetState_should_return_initial_state()
        {
            var sut = new Store<int>(1, Reducers.Replace);

            Assert.AreEqual(1, sut.GetState());
        }

        [Test]
        public void GetState_should_return_the_latest_state()
        {
            var sut = new Store<int>(1, Reducers.Replace);

            sut.Dispatch(new FakeAction<int>(2));

            Assert.AreEqual(2, sut.GetState());
        }
    }
}
