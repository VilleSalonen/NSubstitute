using System;
using System.Linq;
using NSubstitute.Exceptions;
using NUnit.Framework;

namespace NSubstitute.Acceptance.Specs
{
    [TestFixture]
    public class ReceivedCalls
    {
        private IEngine _engine;
        const int Rpm = 7000;

        [SetUp]
        public void SetUp()
        {
            _engine = Substitute.For<IEngine>();
        }

        [Test]
        public void Check_when_call_was_received()
        {
            _engine.Rev();

            _engine.Received().Rev();
        }

        [Test]
        public void Throw_when_expected_call_was_not_received()
        {
            Assert.Throws<ReceivedCallsException>(() =>
                    _engine.Received().Idle()
                );
        }

        [Test]
        public void Check_call_was_received_with_expected_argument()
        {
            _engine.RevAt(Rpm);

            _engine.Received().RevAt(Rpm);
        }

        [Test]
        public void Throw_when_expected_call_was_received_with_different_argument()
        {
            _engine.RevAt(Rpm);

            Assert.Throws<ReceivedCallsException>(() =>
                    _engine.Received().RevAt(Rpm + 2)
                );
        }

        [Test]
        public void Check_that_a_call_was_not_received()
        {
            _engine.RevAt(Rpm);

            _engine.DidNotReceive().RevAt(Rpm + 2);
        }

        [Test]
        public void Throw_when_a_call_was_not_expected_to_be_received()
        {
            _engine.RevAt(Rpm);

            Assert.Throws<ReceivedCallsException>(() => _engine.DidNotReceive().RevAt(Rpm));
        }

        [Test]
        public void Check_call_received_with_any_arguments()
        {
            _engine.RevAt(Rpm);

            _engine.ReceivedWithAnyArgs().RevAt(Rpm + 100);
        }

        [Test]
        public void Throw_when_call_was_expected_with_any_arguments()
        {
            Assert.Throws<ReceivedCallsException>(() => _engine.ReceivedWithAnyArgs().FillPetrolTankTo(10));
        }

        [Test]
        public void Check_call_was_not_received_with_any_combination_of_arguments()
        {
            _engine.DidNotReceiveWithAnyArgs().FillPetrolTankTo(10);
        }

        [Test]
        public void Throw_when_call_was_not_expected_to_be_received_with_any_combination_of_arguments()
        {
            _engine.RevAt(Rpm);

            Assert.Throws<ReceivedCallsException>(() => _engine.DidNotReceiveWithAnyArgs().RevAt(0));
        }

        [Test]
        public void Get_all_received_calls()
        {
            _engine.Rev();
            _engine.RevAt(Rpm);

            var calls = _engine.ReceivedCalls();
            var callNames = calls.Select(x => x.GetMethodInfo().Name);
            Assert.That(callNames, Has.Member("Rev"));
            Assert.That(callNames, Has.Member("RevAt"));
        }

        [Test]
        public void Should_receive_call_even_when_call_is_stubbed_to_throw_an_exception()
        {
            _engine.GetCapacityInLitres().Returns(x => { throw new InvalidOperationException(); });

            var exceptionThrown = false;
            try { _engine.GetCapacityInLitres(); }
            catch { exceptionThrown = true; }

            _engine.Received().GetCapacityInLitres();
            Assert.That(exceptionThrown, "An exception should have been thrown for this to actually test whether calls that throw exceptions are received.");
        }

        [Test]
        public void Should_receive_call_when_a_callback_for_call_throws_an_exception()
        {
            _engine.When(x => x.Rev()).Do(x => { throw new InvalidOperationException(); });

            var exceptionThrown = false;
            try { _engine.Rev(); }
            catch { exceptionThrown = true; }

            _engine.Received().Rev();
            Assert.That(exceptionThrown, "An exception should have been thrown for this to actually test whether calls that throw exceptions are received.");
        }

        [Test]
        public void Check_when_call_was_received_repeatedly()
        {
            _engine.Rev();
            _engine.Rev();
            _engine.Rev();

            _engine.Received(3).Rev();
        }

        [Test]
        public void Check_call_was_not_received_by_making_sure_it_was_called_zero_times()
        {
            _engine.Received(0).Rev();
        }

        [Test]
        public void Throw_when_expected_call_was_not_received_enough_times()
        {
            _engine.Idle();
            _engine.Idle();
            _engine.Idle();
            _engine.Idle();

            Assert.Throws<ReceivedCallsException>(() => _engine.Received(5).Idle());
        }

        [Test]
        public void Throw_when_expected_call_was_received_too_many_times()
        {
            _engine.Idle();
            _engine.Idle();
            _engine.Idle();

            Assert.Throws<ReceivedCallsException>(() => _engine.Received(2).Idle());
        }

        [Test]
        public void Throw_when_expected_call_was_not_received_exactly_zero_times()
        {
            _engine.Idle();
            Assert.Throws<ReceivedCallsException>(() => _engine.Received(0).Idle());
        }

        [Test]
        public void Check_call_was_received_a_specifc_number_of_times_with_expected_argument()
        {
            const int differentRpm = Rpm + 2134;
            _engine.RevAt(Rpm);
            _engine.RevAt(differentRpm);
            _engine.RevAt(Rpm);

            _engine.Received(2).RevAt(Rpm);
        }

        [Test]
        public void Throw_when_expected_call_was_not_received_a_specific_number_of_times_with_expected_argument()
        {
            _engine.RevAt(Rpm);
            _engine.RevAt(Rpm);

            Assert.Throws<ReceivedCallsException>(() => _engine.Received(2).RevAt(Rpm + 2));
        }

        [Test]
        public void Check_call_received_a_specific_number_of_times_with_any_arguments()
        {
            _engine.RevAt(1);
            _engine.RevAt(2);
            _engine.RevAt(3);

            _engine.ReceivedWithAnyArgs(3).RevAt(Rpm + 100);
        }

        [Test]
        public void Throw_when_call_was_not_received_a_specific_number_of_times_with_any_arguments()
        {
            _engine.RevAt(1);
            _engine.RevAt(2);
            _engine.RevAt(3);
            _engine.RevAt(4);

            Assert.Throws<ReceivedCallsException>(() => _engine.ReceivedWithAnyArgs(2).RevAt(0));
        }

        public interface IEngine
        {
            void Start();
            void Rev();
            void Stop();
            void Idle();
            void RevAt(int rpm);
            void FillPetrolTankTo(int percent);
            float GetCapacityInLitres();
            event Action Started;
        }
    }
}