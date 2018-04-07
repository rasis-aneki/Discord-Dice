using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordDice.Tests
{
    public static class ExprTest
    {
        private static void AssertOneConstant(Expr.Main expr, int value)
        {
            Assert.IsNotNull(expr);
            Assert.IsNotNull(expr.Functions);
            Assert.IsTrue(expr.IsValid);
            Assert.AreEqual(expr.Functions.Count, 1);
            Assert.IsInstanceOfType(expr.Functions[0], typeof(Expr.NumberFunctions.Constant));
            var constant = (Expr.NumberFunctions.Constant)expr.Functions[0];
            Assert.AreEqual(constant.Value, value);
        }

        private static void AssertConstant(Expr.Main expr, int value)
        {
            Assert.IsNotNull(expr);
            Assert.IsNotNull(expr.Functions);
            Assert.IsTrue(expr.IsValid);
            Assert.IsTrue(expr.Functions.Count >= 1);
            Assert.IsTrue(expr.Functions.All(f => f.GetType() == typeof(Expr.NumberFunctions.Constant)));
            Assert.AreEqual(expr.Functions.OfType<Expr.NumberFunctions.Constant>().Select(f => f.Value).Sum(), value);
        }

        private static void AssertOneDice(Expr.Main expr, int count, int max)
        {
            Assert.IsNotNull(expr);
            Assert.IsNotNull(expr.Functions);
            Assert.IsTrue(expr.IsValid);
            Assert.AreEqual(expr.Functions.Count, 1);
            Assert.IsInstanceOfType(expr.Functions[0], typeof(Expr.NumberFunctions.Die));
            var die = (Expr.NumberFunctions.Die)expr.Functions[0];
            Assert.AreEqual(die.Count, count);
            Assert.AreEqual(die.Max, max);
        }

        private static void AssertInvalid(Expr.Main expr)
        {
            Assert.IsNotNull(expr);
            Assert.IsFalse(expr.IsValid);
        }

        [TestClass]
        public class InterpretTest
        {
            private IEnumerable<string> InvalidStringTestSource()
            {
                yield return "hogehoge";
            }

            [TestMethod]
            public void InvalidStringTest()
            {
                foreach (var source in InvalidStringTestSource())
                {
                    var expr = Expr.Main.Interpret(source);
                    AssertInvalid(expr);
                }
            }

            private IEnumerable<(string source, int expected)> ValidOneConstantTestSource()
            {
                yield return ("2", 2);
                yield return ("+2", 2);
                yield return ("++2", 2);
                yield return ("-2", -2);
                yield return ("--2", 2);
            }

            [TestMethod]
            public void ValidOneConstantTest()
            {
                foreach ((var source, var expected) in ValidOneConstantTestSource())
                {
                    var expr = Expr.Main.Interpret(source);
                    AssertOneConstant(expr, expected);
                }
            }

            private IEnumerable<(string source, int expected)> ValidConstantTestSource()
            {
                yield return ("2+3", 5);
                yield return ("2-5", -3);
                yield return ("10+0", 10);
                yield return ("-6+4", -2);
            }

            [TestMethod]
            public void ValidConstantTest()
            {
                foreach ((var source, var expected) in ValidConstantTestSource())
                {
                    var expr = Expr.Main.Interpret(source);
                    AssertConstant(expr, expected);
                }
            }

            private IEnumerable<(string source, (int expectedCount, int expectedMax))> ValidOneDiceTestSource()
            {
                yield return ("2d6", (2, 6));
            }

            [TestMethod]
            public void ValidOneDiceTest()
            {
                foreach ((var source, (var expectedCount, var expectedMax)) in ValidOneDiceTestSource())
                {
                    var expr = Expr.Main.Interpret(source);
                    AssertOneDice(expr, expectedCount, expectedMax);
                }
            }
        }

        [TestClass]
        public class InterpretFromLazySocketMessageAsyncTest
        {
            private IEnumerable<(string source, int expected)> ValidOneConstantTestSource()
            {
                yield return ("2", 2);
                yield return ("+2", 2);
                yield return ("++2", 2);
                yield return ("-2", -2);
                yield return ("--2", 2);
            }

            [TestMethod]
            public async Task ValidOneConstant_NoMentionTest()
            {
                foreach ((var source, var expected) in ValidOneConstantTestSource())
                {
                    var m = TestLazySocketMessage.CreateNoMentionMessage(source);
                    var expr = await Expr.Main.InterpretFromLazySocketMessageAsync(m, TestLazySocketUser.MyBot.Id);
                    AssertOneConstant(expr, expected);
                }
            }

            [TestMethod]
            public async Task ValidOneConstant_MentionedTest()
            {
                foreach ((var source, var expected) in ValidOneConstantTestSource())
                {
                    var m = TestLazySocketMessage.CreateMentionedMessage(source);
                    var expr = await Expr.Main.InterpretFromLazySocketMessageAsync(m, TestLazySocketUser.MyBot.Id);
                    AssertOneConstant(expr, expected);
                }
            }

            private IEnumerable<(string source, int expected)> ValidConstantTestSource()
            {
                yield return ("2+3", 5);
                yield return ("2-5", -3);
                yield return ("10+0", 10);
                yield return ("-6+4", -2);
            }

            [TestMethod]
            public async Task ValidConstant_NoMentionTest()
            {
                foreach ((var source, var expected) in ValidConstantTestSource())
                {
                    var m = TestLazySocketMessage.CreateNoMentionMessage(source);
                    var expr = await Expr.Main.InterpretFromLazySocketMessageAsync(m, TestLazySocketUser.MyBot.Id);
                    AssertConstant(expr, expected);
                }
            }

            [TestMethod]
            public async Task ValidConstant_MentionedTest()
            {
                foreach ((var source, var expected) in ValidConstantTestSource())
                {
                    var m = TestLazySocketMessage.CreateMentionedMessage(source);
                    var expr = await Expr.Main.InterpretFromLazySocketMessageAsync(m, TestLazySocketUser.MyBot.Id);
                    AssertConstant(expr, expected);
                }
            }

            private IEnumerable<string> InvalidStringTestSource()
            {
                yield return "hogehoge";
                yield return "1d-100";
            }

            [TestMethod]
            public async Task InvalidString_NoMentionTest()
            {
                foreach (var source in InvalidStringTestSource())
                {
                    var m = TestLazySocketMessage.CreateNoMentionMessage(source);
                    var expr = await Expr.Main.InterpretFromLazySocketMessageAsync(m, TestLazySocketUser.MyBot.Id);
                    AssertInvalid(expr);
                }
            }

            
            [TestMethod]
            public async Task InvalidString_MentionedTest()
            {
                foreach (var source in InvalidStringTestSource())
                {
                    var m = TestLazySocketMessage.CreateMentionedMessage(source);
                    var expr = await Expr.Main.InterpretFromLazySocketMessageAsync(m, TestLazySocketUser.MyBot.Id);
                    AssertInvalid(expr);
                }
            }
        }

        [TestClass]
        public class AreEquivalentTest
        {
            private IEnumerable<(string x, string y)> EquivalentTestSource()
            {
                yield return ("2", "2");
                yield return ("0", "0");
                yield return ("-3+1", "-4+2");
                yield return ("2d100", "2d100");
                yield return ("2d150", "1d150+1d150");
                yield return ("-1+1d50", "1d50-1");
                yield return ("hogehoge", "fugafuga");
            }

            [TestMethod]
            public void EquivalentTest()
            {
                foreach ((var x, var y) in EquivalentTestSource())
                {
                    var xCode = Expr.Main.Interpret(x);
                    var yCode = Expr.Main.Interpret(y);
                    Assert.IsTrue(Expr.Main.AreEquivalent(xCode, yCode));
                    Assert.IsTrue(Expr.Main.AreEquivalent(yCode, xCode));
                }
            }

            private IEnumerable<(string x, string y)> NotEquivalentTestSource()
            {
                yield return ("-2", "2");
                yield return ("1d100", "2d100");
                yield return ("1d100", "1d150");
                yield return ("1d150", "2d150-1d150");
            }

            [TestMethod]
            public void NotEquivalentTest()
            {
                foreach ((var x, var y) in NotEquivalentTestSource())
                {
                    var xCode = Expr.Main.Interpret(x);
                    var yCode = Expr.Main.Interpret(y);
                    Assert.IsFalse(Expr.Main.AreEquivalent(xCode, yCode));
                    Assert.IsFalse(Expr.Main.AreEquivalent(yCode, xCode));
                }
            }
        }
    }
}