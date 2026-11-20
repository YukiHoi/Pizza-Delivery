using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;
using FoodDeliverySYS;
using System.Windows.Forms;

namespace FoodDeliverySYS.Original.Tests
{
    [Apartment(ApartmentState.STA)]
    [NonParallelizable]
    public class Tests
    {
        private frmPlaceOrder form;
        private CultureInfo previousCulture;

        [SetUp]
        public void Setup()
        {
            previousCulture = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentCulture =
                CultureInfo.GetCultureInfo("en-IE");

            // Create the form without showing it or loading Oracle data.
            form = new frmPlaceOrder(1);
        }

        [TearDown]
        public void Cleanup()
        {
            form?.Dispose();

            Thread.CurrentThread.CurrentCulture = previousCulture;
        }

        [Test]
        public void OriginalPrice_WholeNumbers_Returns15()
        {
            AddItem(10m, 2m, 3m, 1);

            Assert.That(
                GetField<TextBox>("txtTotal").Text,
                Is.EqualTo("15.00"));
        }

        [Test]
        public void OriginalPrice_Decimals_Returns32_98()
        {
            AddItem(6m, 3.5m, 6.99m, 2);

            Assert.That(
                GetField<TextBox>("txtTotal").Text,
                Is.EqualTo("32.98"));
        }

        [Test]
        public void OriginalForm_KeepsSelectedValues()
        {
            AddItem(6m, 3.5m, 6.99m, 4);

            Assert.That(GetField<int>("pizza_id"), Is.EqualTo(1));
            Assert.That(GetField<int>("crust_id"), Is.EqualTo(2));
            Assert.That(GetField<int>("size_id"), Is.EqualTo(3));
            Assert.That(GetField<int>("qty"), Is.EqualTo(4));
        }

        [Test]
        public void OriginalForm_AcceptsQuantityTen()
        {
            AddItem(10m, 2m, 3m, 10);

            Assert.That(GetField<int>("qty"), Is.EqualTo(10));

            Assert.That(
                GetField<TextBox>("txtTotal").Text,
                Is.EqualTo("150.00"));
        }

        [Test]
        public void OriginalForm_CurrentlyAcceptsQuantityZero()
        {
            // Record an existing limitation, not the desired behaviour.
            AddItem(10m, 2m, 3m, 0);

            Assert.That(GetField<int>("qty"), Is.EqualTo(0));

            Assert.That(
                GetField<TextBox>("txtTotal").Text,
                Is.EqualTo("0.00"));
        }

        // Supply options directly instead of loading them from Oracle.
        private void AddItem(
            decimal pizzaPrice,
            decimal crustPrice,
            decimal sizePrice,
            int quantity)
        {
            SetOption(
                "cboPizzas",
                "01 - Test Pizza - " + pizzaPrice);

            SetOption(
                "cboCrustType",
                "02 - Test Crust - " + crustPrice);

            SetOption(
                "cboPizzaSize",
                "03 - Test Size - " + sizePrice);

            GetField<NumericUpDown>("numQty").Value = quantity;

            // Run the original private button-click method.
            MethodInfo button = typeof(frmPlaceOrder).GetMethod(
                "btnAdd_Click",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(button, Is.Not.Null);

            button.Invoke(form, new object[] { form, EventArgs.Empty });
        }

        private void SetOption(string fieldName, string option)
        {
            ComboBox combo = GetField<ComboBox>(fieldName);

            combo.Items.Clear();
            combo.Items.Add(option);
            combo.SelectedIndex = 0;
        }

        // Read a private control or value without changing the original form.
        private T GetField<T>(string name)
        {
            FieldInfo field = typeof(frmPlaceOrder).GetField(
                name,
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null);

            return (T)field.GetValue(form);
        }
    }
}