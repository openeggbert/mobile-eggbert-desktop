using System;

namespace Microsoft.Devices.Sensors
{
    public abstract class SensorBase<TSensorReading> : IDisposable where TSensorReading : ISensorReading
    {
        private TSensorReading currentValue;

        public event EventHandler<SensorReadingEventArgs<TSensorReading>> CurrentValueChanged;

        public void Dispose()
        {
        }
    }
}