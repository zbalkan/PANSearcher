﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PANSearcher.Tests
{
    [TestClass]
    public class LuhnTests
    {
        [TestMethod]
        public void Test_Valid_Mastercard()
        {
            var cardNumber = "5105105105105100";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Test_Valid_Mastercard_With_Dashes()
        {
            var cardNumber = "5105-1051-0510-5100";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Test_Invalid_Mastercard()
        {
            var cardNumber = "5105105105105101";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void Test_Invalid_Mastercard_With_Dashes()
        {
            var cardNumber = "5105-1051-0510-5101";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void Test_Valid_Visa()
        {
            var cardNumber = "4012888888881881";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Test_Valid_Visa_With_Dashes()
        {
            var cardNumber = "4012-8888-8888-1881";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Test_Invalid_Visa()
        {
            var cardNumber = "4012888888881882";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void Test_Invalid_Visa_With_Dashes()
        {
            var cardNumber = "4012-8888-8888-1882";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void Test_Valid_Amex()
        {
            var cardNumber = "371449635398431";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Test_Valid_Amex_With_Dashes()
        {
            var cardNumber = "3714-496353-98431";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void Test_Invalid_Amex()
        {
            var cardNumber = "371449635398432";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void Test_Invalid_Amex_With_Dashes()
        {
            var cardNumber = "3714-496353-98432";
            var isValid = Luhn.Validate(cardNumber);

            Assert.IsFalse(isValid);
        }
    }
}
