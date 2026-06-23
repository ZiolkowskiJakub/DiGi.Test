using DiGi.Core.Interfaces;

namespace DiGi.Core.xUnit
{
    /// <summary>
    /// Provides an implementation of the <see cref="IUpdater{T}"/> interface for double-precision floating-point numbers, used to update a value by adding a specified addend.
    /// </summary>
    public class TestUpdater : IUpdater<double>
    {
        /// <summary>
        /// Gets or sets the value to be added to the current numeric value during an update operation.
        /// </summary>
        public double Addend { get; set; }

        /// <summary>
        /// Gets or sets the current numeric value that is managed by the updater.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestUpdater"/> class with a specified addend.
        /// </summary>
        /// <param name="addend">The value to be added to the current value during an update operation.</param>
        public TestUpdater(double addend)
        {
            Addend = addend;
        }

        /// <summary>
        /// Updates the current value by adding the addend to it.
        /// </summary>
        /// <returns>True if the update was successful.</returns>
        public bool Update()
        {
            Value = Value + Addend;
            return true;
        }
    }
}